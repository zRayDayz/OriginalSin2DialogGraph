namespace InjectedLogic;

/// <summary>
/// Not in use yet
/// </summary>
public class ListPool<T>
{
    static readonly object lockObj = new object();

    ObjectPool<List<T>> objectPool;
    public ListPool(int initCount = 4, int initListLength = 32)
    {
        objectPool = new ObjectPool<List<T>>(initCount);
        for (int i = 0; i < initCount; i++)
        {
            objectPool.ReturnObject(new List<T>(initListLength));
        }
    }

    public List<T> GetObject()
    {
        lock (lockObj)
        {
            List<T> list;
            if (objectPool.Count == 0) list = new List<T>(32);
            else list = objectPool.GetObject();

            return list;
        }
    }

    public void ReturnObject(List<T> obj, bool clearList = true)
    {
        lock (lockObj)
        {
            if (clearList) obj.Clear();
            objectPool.ReturnObject(obj);
        }
    }
}