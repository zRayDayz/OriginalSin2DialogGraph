using System.Diagnostics;
using System.Text;
using System.IO.Hashing;

namespace InjectedLogic;

public readonly record struct HolderForNativeStringWrapperWithoutNeedForDisposal(NativeStringWrapper nativeStringWrapper);

// In .Net 6,7,8, the Marvin hash algorithm is used, for example, to calculate strings hashes. However xxHash32 and xxHash64 should be faster and more efficient. That's why the GetHashCode() method uses xxHash32
// This class could use a structure with fixed-size buffer instead of an array. However, strings always have different sizes, so we would have to create a structure with a fixed-size bugger that could accommodate a string of any size, which is often not possible
[DebuggerDisplay("{GetStringUTF8}")]
public class NativeStringWrapper : IDisposable
{
    private byte[] nativeStringBytes;

    private byte[] NativeStringBytes
    {
        get
        {
            if (IsDisposed) throw new ObjectDisposedException($"{nameof(NativeStringWrapper)}.{nameof(NativeStringBytes)}");
            return nativeStringBytes;
        }
    }

    public ReadOnlySpan<byte> NativeStringSpan
    {
        get
        {
            if (IsDisposed) throw new ObjectDisposedException($"{nameof(NativeStringWrapper)}.{nameof(NativeStringSpan)}");
            return new ReadOnlySpan<byte>(nativeStringBytes, 0, length);
        }
    }

    private int length;

    public int Length
    {
        get
        {
            if (IsDisposed) throw new ObjectDisposedException($"{nameof(NativeStringWrapper)}.{nameof(Length)}");
            return length;
        }
    }

    private bool shouldUseArrayPoolWhenDisposing;
    public bool IsDisposed { get; private set; } = true;

    public void Dispose()
    {
        if(shouldUseArrayPoolWhenDisposing) IOCContainer.Instance.ArrayPoolKeeper.Value.ReturnObject(nativeStringBytes);
        nativeStringBytes = null;
        length = 0;
        IsDisposed = true;
        var nativeStringWrapperPool = (INativeStringWrapperPoolObjectReturner)IOCContainer.Instance.NativeStringWrapperPool.Value;
        nativeStringWrapperPool.ReturnObject(this, this);
    }

    public NativeStringWrapper RestoreDisposedObject(byte[] nativeStringBytes, int length, bool shouldUseArrayPoolWhenDisposing)
    {
        if (IsDisposed == false) throw new InvalidOperationException("Attempting to restore an object that was not disposed");
        if (nativeStringBytes == null) throw new ArgumentNullException(nameof(nativeStringBytes));
        if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length), "The length of the data cannot be less then or equal to zero");
        if (nativeStringBytes.Length == 0) throw new ArgumentOutOfRangeException(nameof(length), "The length of the array cannot be equal to zero");
        if (length > nativeStringBytes.Length) throw new ArgumentOutOfRangeException(nameof(length), "The length of the data cannot exceed the length of the array");
        
        this.nativeStringBytes = nativeStringBytes;
        this.length = length;
        this.shouldUseArrayPoolWhenDisposing = shouldUseArrayPoolWhenDisposing;
        IsDisposed = false;
        return this;
    }

    private string GetStringUTF8
    {
        get
        {
            if (nativeStringBytes == null || length == 0) return String.Empty;
            return Encoding.UTF8.GetString(nativeStringBytes, 0, length);
        }
    }

    public override string ToString()
    {
        if (IsDisposed) throw new ObjectDisposedException($"{nameof(NativeStringWrapper)}.{nameof(ToString)}");
        return GetStringUTF8;
    }
    
    public override int GetHashCode()
    {
        if (IsDisposed) throw new ObjectDisposedException($"{nameof(NativeStringWrapper)}.{nameof(GetHashCode)}");
        return (int)XxHash32.HashToUInt32(nativeStringBytes);
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not NativeStringWrapper other) return false;
        return Equals(other);
    }
    
    public bool Equals(NativeStringWrapper other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(this, other)) return true;

        return EqualsInternal(other);
    }

    private bool EqualsInternal(NativeStringWrapper other)
    {
        var thisLength = length;
        var otherLength = other.length;
        if (thisLength != otherLength) return false;
        
        byte[] thisStr = nativeStringBytes;
        byte[] otherStr = other.nativeStringBytes;

        var thisStrSpan = new Span<byte>(thisStr, 0, thisLength);
        var otherStrSpan = new Span<byte>(otherStr, 0, otherLength);
        
        if (thisLength < 16) return CommonHelpers.CompareByteSpansUnrolled0To15Bytes(thisStrSpan, otherStrSpan);
        if (thisLength < 32) return CommonHelpers.CompareByteSpansUnrolled16Bytes(thisStrSpan, otherStrSpan);
        if (thisLength < 64) return CommonHelpers.CompareByteSpansUnrolled32Bytes(thisStrSpan, otherStrSpan);

        // At this point, the SequenceEqual() method will be the fastest because uses SIMD
        return thisStrSpan.SequenceEqual(otherStrSpan);
    }

    public static bool operator == (NativeStringWrapper left, NativeStringWrapper right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (ReferenceEquals(left, null)) return false;
        if (ReferenceEquals(right, null)) return false;
        return left.EqualsInternal(right);
    }
    
    public static bool operator != (NativeStringWrapper left, NativeStringWrapper right)
    {
        return !(left == right);
    }

    // Method CollectionExtensions.AddRange<T>(list, readOnlySpan) is only available in .Net 8
    public void AddToList(in List<byte> list)
    {
        var segment = new ArraySegment<byte>(NativeStringBytes, 0, Length);
        list.AddRange(segment);
    }

    public static class UnsafeActions
    {
        public static void SetNewArrayAndLength(NativeStringWrapper nativeStringWrapper, byte[] bytes, int length)
        {
            nativeStringWrapper.nativeStringBytes = bytes;
            nativeStringWrapper.length = length;
        }

        public static void SetIsDisposed(NativeStringWrapper nativeStringWrapper, bool isDisposed)
        {
            nativeStringWrapper.IsDisposed = isDisposed;
        }
    }
}
