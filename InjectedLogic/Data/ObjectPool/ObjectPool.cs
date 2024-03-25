namespace InjectedLogic;

// A simpler implemented ObjectPool then Microsoft's: https://learn.microsoft.com/ru-ru/dotnet/api/microsoft.extensions.objectpool.defaultobjectpool-1?view=dotnet-plat-ext-6.0
class ObjectPool<T>
{
    public ObjectPool(int initPoolSize)
    {
        pool = new Queue<T>(initPoolSize);
    }

    public int Count { get { return pool.Count; } }

    Queue<T> pool;

    public T GetObject()
    {
        return pool.Dequeue();
    }
    public void ReturnObject(T obj)
    {
        pool.Enqueue(obj);
    }
}