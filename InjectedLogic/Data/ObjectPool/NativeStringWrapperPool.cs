namespace InjectedLogic;

public class NativeStringWrapperPool : INativeStringWrapperPoolObjectReturner
{
    static readonly object lockObj = new object();

    private ObjectPool<NativeStringWrapper> objectPool;
    public NativeStringWrapperPool(int initCount)
    {
        objectPool = new ObjectPool<NativeStringWrapper>(initCount);
        for (int i = 0; i < initCount; i++)
        {
            objectPool.ReturnObject(new NativeStringWrapper());
        }
    }

    public NativeStringWrapper GetObject(byte[] nativeStringBytes, int length, bool shouldUseArrayPoolWhenDisposing)
    {
        lock (lockObj)
        {
            NativeStringWrapper obj;
            if (objectPool.Count == 0) obj = new NativeStringWrapper();
            else obj = objectPool.GetObject();

            obj.RestoreDisposedObject(nativeStringBytes, length, shouldUseArrayPoolWhenDisposing);
            return obj;
        }
    }
    
    /// <summary>
    /// This method should be called only by NativeStringWrapper during disposing
    /// </summary>
    void INativeStringWrapperPoolObjectReturner.ReturnObject(NativeStringWrapper obj, object caller)
    {
        if (caller is not NativeStringWrapper) throw new ArgumentException($"Method should be called only by {nameof(NativeStringWrapper)} during disposing");
        lock (lockObj)
        {
            if (obj.IsDisposed == false) throw new ObjectDisposedException("An attempt to return an object to the pool that was not disposed");
            objectPool.ReturnObject(obj);
        }
    }
}

