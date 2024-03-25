using System.Buffers;

namespace InjectedLogic;

public class ArrayPoolKeeper<T>
{
    private ArrayPool<T> pool;
    
    public ArrayPoolKeeper()
    {
        pool = ArrayPool<T>.Shared;
    }

    public T[] GetObject (int minimumLength)
    {
        return pool.Rent(minimumLength);
    }

    public void ReturnObject(T[] array, bool clearArray = true)
    {
        pool.Return(array, clearArray);
    }
}