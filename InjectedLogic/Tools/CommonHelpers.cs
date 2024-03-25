using System.Runtime.CompilerServices;

namespace InjectedLogic;

public static class CommonHelpers
{
    // https://github.com/Reloaded-Project/Reloaded.Hooks/blob/master/source/Reloaded.Hooks.Tests.Shared/Macros/Macros.cs
    public static class ASMHelpers
    {
        public static bool Is64Bit = Environment.Is64BitProcess;
        public static string _use32 = Is64Bit ? "use64" : "use32";
        public static string _eax = Is64Bit ? "rax" : "eax";
        public static string _ebx = Is64Bit ? "rbx" : "ebx";
        public static string _ecx = Is64Bit ? "rcx" : "ecx";
        public static string _edx = Is64Bit ? "rdx" : "edx";
        public static string _esi = Is64Bit ? "rsi" : "esi";
        public static string _edi = Is64Bit ? "rdi" : "edi";
        public static string _ebp = Is64Bit ? "rbp" : "ebp";
        public static string _esp = Is64Bit ? "rsp" : "esp";
        //public static string _word = Is64Bit ? "qword" : "dword";
        
        public static string _r8 = Is64Bit ? "r8" : throw new InvalidOperationException("It is not possible to use the x64 registers in an x32 application");
        public static string _r9 = Is64Bit ? "r9" : throw new InvalidOperationException("It is not possible to use the x64 registers in an x32 application");
        public static string _r10 = Is64Bit ? "r10" : throw new InvalidOperationException("It is not possible to use the x64 registers in an x32 application");
        public static string _r11 = Is64Bit ? "r11" : throw new InvalidOperationException("It is not possible to use the x64 registers in an x32 application");
        public static string _r12 = Is64Bit ? "r12" : throw new InvalidOperationException("It is not possible to use the x64 registers in an x32 application");
        public static string _r13 = Is64Bit ? "r13" : throw new InvalidOperationException("It is not possible to use the x64 registers in an x32 application");
        public static string _r14 = Is64Bit ? "r14" : throw new InvalidOperationException("It is not possible to use the x64 registers in an x32 application");
        public static string _r15 = Is64Bit ? "r15" : throw new InvalidOperationException("It is not possible to use the x64 registers in an x32 application");
    }
    
    private const int maxStringLength = 256;
    public unsafe static NativeStringWrapper? GetNativeString(IntPtr ptrToString)
    {
        int count = GetNativeStringLength(ptrToString);
        if (count == 0) return null;
        
        byte[] strBytes = IOCContainer.Instance.ArrayPoolKeeper.Value.GetObject(count);

        fixed (byte* fixedStrBytes = strBytes)
        {
            Buffer.MemoryCopy((byte*)ptrToString, fixedStrBytes, count, count);
        }
        
        var nativeStringWrapper = IOCContainer.Instance.NativeStringWrapperPool.Value.GetObject(strBytes, count, true);

        return nativeStringWrapper;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static int GetNativeStringLength(IntPtr ptrToString)
    {
        byte* strPointer = (byte*)ptrToString;
        int count = 0;
        while (*strPointer != 0x00 && count < maxStringLength)
        {
            strPointer++;
            count++;
        }
        return count;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static void CopyBytePointerDataToArray(byte* pointer, byte[] arr, int dataLength)
    {
        fixed (byte* fixedArr = arr)
        {
            Buffer.MemoryCopy(pointer, fixedArr, arr.Length, dataLength);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EnsureArrayLength<T>(ref T[] array, int targetLength)
    {
        if (array.Length >= targetLength) return;
        Array.Resize(ref array, targetLength);
    }

    #region Byte arrays fast comparison
    // The methods are based on several examples from: https://stackoverflow.com/questions/43289/comparing-two-byte-arrays-in-net/69280107
    public unsafe static bool CompareByteSpansUnrolled32Bytes(Span<byte> data1, Span<byte> data2)
    {
        if (data1 == data2)
            return true;

        int data1Length = data1.Length;
        int data2Length = data2.Length;
        if (data1Length != data2Length) return false;
        if (data1Length == 0) return true;

        fixed (byte* bytes1 = data1, bytes2 = data2) {
            int len = data1Length;
            int rem = len % (sizeof(long) * 4);
            long* b1 = (long*)bytes1;
            long* b2 = (long*)bytes2;
            long* e1 = (long*)(bytes1 + len - rem);

            while (b1 < e1) {
                if (*(b1) != *(b2) || *(b1 + 1) != *(b2 + 1) || 
                    *(b1 + 2) != *(b2 + 2) || *(b1 + 3) != *(b2 + 3))
                    return false;
                b1 += 4;
                b2 += 4;
            }

            if (rem != 0) return CompareByteSpansHelper((byte*)b1, (byte*)b2, rem);
            return true;
        }
    }
    
    public unsafe static bool CompareByteSpansUnrolled16Bytes(Span<byte> data1, Span<byte> data2)
    {
        if (data1 == data2)
            return true;

        int data1Length = data1.Length;
        int data2Length = data2.Length;
        if (data1Length != data2Length) return false;
        if (data1Length == 0) return true;

        fixed (byte* bytes1 = data1, bytes2 = data2) {
            int len = data1Length;
            int rem = len % (sizeof(long) * 2);
            long* b1 = (long*)bytes1;
            long* b2 = (long*)bytes2;
            long* e1 = (long*)(bytes1 + len - rem);

            while (b1 < e1) {
                if (*(b1) != *(b2) || *(b1 + 1) != *(b2 + 1))
                    return false;
                b1 += 2;
                b2 += 2;
            }
            
            if (rem != 0) return CompareByteSpansHelper((byte*)b1, (byte*)b2, rem);
            return true;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool CompareByteSpansUnrolled0To15Bytes(Span<byte> data1, Span<byte> data2)
    {
        if (data1 == data2)
            return true;

        int data1Length = data1.Length;
        int data2Length = data2.Length;
        if (data1Length != data2Length) return false;
        if (data1Length == 0) return true;
        
        fixed (byte* bytes1 = data1, bytes2 = data2)
        {
            return CompareByteSpansHelper(bytes1, bytes2, data1Length);
        }
    }
    
    static unsafe bool CompareByteSpansHelper(byte* x1, byte* x2, int len)
    {
        
        for (int i = 0; i < len / 8; i++, x1 += 8, x2 += 8)
        {
            if (*((long*)x1) != *((long*)x2)) return false;
        }
        
        if ((len & 4) !=0 ) 
        { 
            if (*((int*)x1) != *((int*)x2)) return false; 
            x1 += 4; 
            x2 += 4;
        }
        
        if ((len & 2) !=0 ) 
        { 
            if (*((short*)x1) != *((short*)x2)) return false; 
            x1 += 2; 
            x2 += 2;
        }
            
        if ((len & 1) != 0) 
        {
            if (*((byte*)x1) != *((byte*)x2)) return false;
        }
              
        return true;
    }
    #endregion
}