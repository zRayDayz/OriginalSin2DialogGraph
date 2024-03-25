using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;

namespace DialogGraph;

/* Larian keeps localization in .xml files.
   Each file has many lines with an id and localized text, with all ids sorted in ascending order.
   For example:
    <contentList>
	    <content contentuid="h00002e6cg3610g4788g8e0eg7615ac443b6e">Some localized text.</content>
	    <content contentuid="h00007224gb454g4b8bgb762g7865d9ee3dbb">Some bigger localized text.</content>
	    <content contentuid="h0001d8b9g13d6g4605g85e9g708fe1e537c8">Some even bigger localized text.</content>
    </contentList>
   This class uses a MemoryMappedFile and Binary search to find localized text by id without the need to fully parse the localization file and load it into RAM.
   Since each line (with an id and localized text) has a different length, the binary search usually never jumps to the beginning of a line. Instead, it jumps (for example) to the middle of it, and then the class has to manually find the beginning of the line. 
   Having found the beginning of the line, it is now possible to parse an id (or a hash), then compare it to the id argument. If it matches, then the text is parsed and returned, if not, the binary search starts the next iteration of the search loop.
   
 * It is possible to use a Span (for the memoryFilePointer) and its extension method IndexOf (from the MemoryExtensions class) for convenient and fast byte sequence search.
   Alternatively, it is also possible to use the Boyer-Moore algorithm, but it is not clear whether this would provide any performance benefit at all 
 */
public unsafe class LocalizationFileParser : ILocalizationFileParser, IDisposable
{
    private bool isInitialized;
    private bool? isLocalizationFileExists = null;
    private string localizationFilePath;
    public bool? CanLocalizationBeUsed => isLocalizationFileExists;

    private const string xmlHeaderTag = "<contentList>";
    private const string xmlFooterTag = "</contentList>";
    private const string exampleHash = "h00005e6cg3610g4788g8e0eg7615ac443b6e";
    private byte[] xmlElementPartialHeader = Encoding.UTF8.GetBytes("<content contentuid=\"");
    private byte[] xmlElementFooter = Encoding.UTF8.GetBytes("</content>");

    private MemoryMappedFile memoryMappedFile;
    private MemoryMappedViewAccessor accessor;
    private long filePayloadLength;
    byte[] tempHashBytes = new byte[exampleHash.Length];
    // Dictionary for optimization, allows to skip finding the beginning of a string and parsing of id
    private Dictionary<long, HashAndItsPosition> filePositionsOfBinarySearchAndHashes = new Dictionary<long, HashAndItsPosition>(256);
    private const int maximumSizeOfFilePositionsOfBinarySearchAndHashes = 25000;   // ~ 3600 kb

    byte* memoryFilePointer = null;

    record struct HashAndItsPosition(string hash, long position);

    public LocalizationFileParser(ConfigManager configManager)
    {
        var config = configManager.Config;
        localizationFilePath = config.LocalizationFileFullPath;
    }

    private void Initialize()
    {
        if (isInitialized) throw new InvalidOperationException("Trying to initialized already initialized " + nameof(LocalizationFileParser));
        
        var fileInfo = new FileInfo(localizationFilePath);
        if (string.IsNullOrEmpty(localizationFilePath) || fileInfo.Exists == false)
        {
            isLocalizationFileExists = false;
            isInitialized = true;
            return;
        }

        memoryMappedFile = MemoryMappedFile.CreateFromFile(localizationFilePath, FileMode.Open);
        filePayloadLength = fileInfo.Length - xmlHeaderTag.Length - xmlFooterTag.Length;
        accessor = memoryMappedFile.CreateViewAccessor(xmlHeaderTag.Length, filePayloadLength, MemoryMappedFileAccess.Read);

        isLocalizationFileExists = true;
        isInitialized = true;
    }

    public string? TryCreateLocalizationStringById(string id)
    {
        if (isInitialized == false) Initialize();
        if (isLocalizationFileExists == false) return null;

        try
        {
            accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref memoryFilePointer);

            long start = 0;
            long end = filePayloadLength;
            while (start <= end)
            {
                long mid = start + (end - start) / 2;

                var foundId = FindClosestStringId(mid, out long hashPosition);
                if (foundId == null) return null;

                int comparisonResult = string.CompareOrdinal(id, foundId);
                if (comparisonResult == 0)
                {
                    string text = FindText(hashPosition);
                    return text;
                }
                else if (comparisonResult > 0)
                {
                    start = mid + 1;
                }
                else
                {
                    end = mid - 1;
                }
            }

            return null;

        }
        catch (Exception e)
        {
            var errorText = $"Error during parsing of localization file. String id: {id}" + Environment.NewLine;
            throw new ApplicationException(errorText, e);
        }
        finally
        {
            accessor.SafeMemoryMappedViewHandle.ReleasePointer();
            memoryFilePointer = null;

            if (filePositionsOfBinarySearchAndHashes.Count > maximumSizeOfFilePositionsOfBinarySearchAndHashes)
            {
                filePositionsOfBinarySearchAndHashes.Clear();
            }
        }
    }

    private string FindText(long hashPos)
    {
        long textPos = hashPos + exampleHash.Length + "\">".Length;
        long footerPos = LocateInAscendingOrder(textPos, xmlElementFooter);
        byte[] textBytes = new byte[footerPos - textPos];
        ReadByteArray(textPos, textBytes, textBytes.Length);
        
        string text = Encoding.UTF8.GetString(textBytes);
        return text;
    }

    private string? FindClosestStringId(long currentPos, out long hashPosition)
    {
        hashPosition = -1;
        if (filePositionsOfBinarySearchAndHashes.TryGetValue(currentPos, out HashAndItsPosition foundHashAndPosition))
        {
            hashPosition = foundHashAndPosition.position;
            return foundHashAndPosition.hash;
        }
        
        long positionOfElementBeginning = LocateInDescendingOrder(currentPos, xmlElementPartialHeader);
        if (positionOfElementBeginning == -1) return null;

        long positionOfHash = positionOfElementBeginning + xmlElementPartialHeader.Length;
        ReadByteArray(positionOfHash, tempHashBytes, tempHashBytes.Length);
        
        string hash = Encoding.UTF8.GetString(tempHashBytes);
        filePositionsOfBinarySearchAndHashes.Add(currentPos, new HashAndItsPosition(hash, positionOfHash));

        hashPosition = positionOfHash;
        return hash;
    }

    private long LocateInAscendingOrder(long currentPos, byte[] pattern)
    {
        var capacity = accessor.Capacity;
        if (currentPos >= capacity || currentPos < 0) return -1;
        
        long lastFoundPos = -1;
        
        byte currentByte;
        while ((currentByte = ReadByte(currentPos)) > 0)
        {
            if (pattern[0] == currentByte)
            {
                lastFoundPos = currentPos;
                currentPos++;
                var result = MatchInAscendingOrder(currentPos, pattern, 1);
                if (result) return lastFoundPos;
                else currentPos = lastFoundPos;
            }
            
            currentPos++;
            if (currentPos >= capacity) return -1;
        }

        return -1;
    }

    private long LocateInDescendingOrder(long currentPos, byte[] pattern)
    {
        var capacity = accessor.Capacity;
        if (currentPos >= capacity || currentPos < 0) return -1;
        
        long lastFoundPos = -1;
        
        byte currentByte;
        while ((currentByte = ReadByte(currentPos)) > 0)
        {
            if (pattern[0] == currentByte)
            {
                lastFoundPos = currentPos;
                currentPos++;
                var result = MatchInAscendingOrder(currentPos, pattern, 1);
                if (result) return lastFoundPos;
                else currentPos = lastFoundPos;
            }

            currentPos--;
            if (currentPos < 0) return -1;
        }
        
        return -1;
    }

    bool MatchInAscendingOrder(long currentPos, byte[] pattern, int patternPosToStartWith)
    {
        int patternLength = pattern.Length;
        if (patternPosToStartWith < 0 || patternPosToStartWith >= patternLength) throw new ArgumentException($"'{nameof(patternPosToStartWith)}' out of '{nameof(pattern)}' array bounds");

        long capacity = accessor.Capacity;
        if (currentPos >= capacity || currentPos < 0) return false;
        
        byte currentByte;
        while ((currentByte = ReadByte(currentPos)) > 0)
        {
            if (patternPosToStartWith >= patternLength) return true;

            if (currentByte != pattern[patternPosToStartWith]) return false;

            patternPosToStartWith++;
            currentPos++;
            
            if (currentPos >= capacity) return false;
        }

        return false;
    }
    
    bool MatchInDescendingOrder(long currentPos, byte[] pattern, int patternPosToStartWith)
    {
        int patternLength = pattern.Length;
        if (patternPosToStartWith < 0 || patternPosToStartWith >= patternLength) throw new ArgumentException($"'{nameof(patternPosToStartWith)}' out of '{nameof(pattern)}' array bounds");

        long capacity = accessor.Capacity;
        if (currentPos >= capacity || currentPos < 0) return false;
        
        byte currentByte;
        while ((currentByte = ReadByte(currentPos)) > 0)
        {
            if (patternPosToStartWith < 0) return true;

            if (currentByte != pattern[patternPosToStartWith]) return false;

            patternPosToStartWith--;
            currentPos--;
            
            if (currentPos < 0) return false;
        }

        return false;
    }

    private byte ReadByte(long position)
    {
        if (memoryFilePointer == null) return ReadByteInternal(position);
        else
        {
            EnsureSafeToRead(position, sizeof(byte));
            return *((byte*)(memoryFilePointer + accessor.PointerOffset + position));
        }
    }
    
    // Method is based on original ReadByte() in UnmanagedMemoryAccessor (base class of MemoryMappedViewAccessor)
    // https://stackoverflow.com/questions/7956167/how-can-i-quickly-read-bytes-from-a-memory-mapped-file-in-net
    private byte ReadByteInternal(long position)
    {
        EnsureSafeToRead(position, sizeof(byte));
 
        byte result;
        unsafe
        {
            byte* pointer = null;
 
            try
            {
                accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
                result = *((byte*)(pointer + accessor.PointerOffset + position));
            }
            finally
            {
                if (pointer != null)
                {
                    accessor.SafeMemoryMappedViewHandle.ReleasePointer();
                }
            }
        }
        return result;
    }

    private void ReadByteArray(long position, byte[] arr, int length)
    {
        if (memoryFilePointer == null) ReadByteArrayInternal(position, arr, length);
        else
        {
            EnsureSafeToRead(position, sizeof(byte) * length);
            byte* pointerWithOffset = memoryFilePointer + accessor.PointerOffset + position;
            Marshal.Copy(new IntPtr(pointerWithOffset), arr, 0, length);
        }
    }
    
    private void ReadByteArrayInternal(long position, byte[] arr, int length)
    {
        EnsureSafeToRead(position, sizeof(byte) * length);
        
        unsafe
        {
            byte* pointer = null;
 
            try
            {
                accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
                byte* pointerWithOffset = pointer + accessor.PointerOffset + position;
                Marshal.Copy(new IntPtr(pointerWithOffset), arr, 0, length);
            }
            finally
            {
                if (pointer != null)
                {
                    accessor.SafeMemoryMappedViewHandle.ReleasePointer();
                }
            }
        }

    }
    
    // Method is based on original EnsureSafeToRead() in UnmanagedMemoryAccessor (base class of MemoryMappedViewAccessor)
    private void EnsureSafeToRead(long position, int sizeOfType)
    {
        if (accessor.CanRead == false)
        {
            throw new NotSupportedException("Accessor is either closed or does not have read access");
        }

        if (position < 0) throw new ArgumentOutOfRangeException(nameof(position), "Can not be less then zero");

        long capacity = accessor.Capacity;
        if (position > capacity - sizeOfType)
        {
            if (position >= capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Can not exceed capacity");
            }
            else
            {
                throw new ArgumentException("There are not enough bytes after position to read a value", nameof(position));
            }
        }
    }
    
    
    ~LocalizationFileParser()
    {
        Dispose(false);
    }
    
    private bool disposed = false;
    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                memoryMappedFile?.Dispose();
                accessor?.Dispose();
            }
            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}