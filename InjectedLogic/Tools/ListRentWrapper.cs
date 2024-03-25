using System.Runtime.InteropServices;

namespace InjectedLogic;

// Not a particularly necessary class, but it allows to guarantee that no method of the List instance will be called while its array is being used as a Span
public class ListRentWrapper<T>
{
    private List<T> list;
    private bool isArrayRented;

    public ListRentWrapper()
    {
        list = new List<T>();
    }
    
    public ListRentWrapper(int capacity)
    {
        list = new List<T>(capacity);
    }

    public Span<T> RentListArrayAsSpan()
    {
        if (isArrayRented) throw new InvalidOperationException("Attempting to re-rent the List array while its array is already rented");
        isArrayRented = true;
        return CollectionsMarshal.AsSpan(list);
    }

    public void CloseArrayRent()
    {
        isArrayRented = false;
    }
    
    public void AddRange(IEnumerable<T> collection)
    {
        ThrowIfRented(this);
        list.AddRange(collection);
    }

    public void Add(T item)
    {
        ThrowIfRented(this);
        list.Add(item);
    }

    public void Clear()
    {
        list.Clear();
    }

    private static void ThrowIfRented<V>(ListRentWrapper<V> instance)
    {
        if (instance.isArrayRented) throw new InvalidOperationException("Attempting to call a method of the List instance while its array is rented");
    }


    public static void AddToList(in ListRentWrapper<byte> listWrapper, in NativeStringWrapper stringWrapper)
    {
        ThrowIfRented(listWrapper);
        stringWrapper.AddToList(listWrapper.list);
    }
}