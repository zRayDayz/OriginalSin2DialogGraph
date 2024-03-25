namespace InjectedLogic;

public static class DataToSendSyncKeeper
{
    private static int dialogStateCounter = 0;

    // It is possible to use the SpinLock class instead of "lock" operator, but the performance gain may be too small or none at all
    // For example ConfigurableArrayPool.Bucket class uses SpinLock, but its logic in locking is very small, this case is different
    private static readonly object dataToSendLock = new object();
    private static NativeStringWrapper? dialogName;
    private static NativeStringWrapper? currentDialogOptionGuid;
    
    // The strings in this HashSet are stored for the entire game session, there is no point in disposing of them
    private static HashSet<HolderForNativeStringWrapperWithoutNeedForDisposal> uniqueNamesAndGuidsCache = new HashSet<HolderForNativeStringWrapperWithoutNeedForDisposal>(256);
    private static List<HolderForNativeStringWrapperWithoutNeedForDisposal> uniqueNamesAndGuidsForSending = new List<HolderForNativeStringWrapperWithoutNeedForDisposal>(128);
    
    // Each new NativeStringWrapper added to the HashSet will take approximately 208 bytes (considering that the string has 150 characters of one byte each, and also considering the additional data in the HashSet)
    private const int maxTotalAmountOfCachedNamesAndGuids = 50000; // ~ 10 mb 

    public static AutoResetEvent ResetEventForDataSending { get; } = new AutoResetEvent(false);

    #region For Injected logic
    // Typically, these methods are called from the same game thread, but just in case, Interlocked is used here
    public static void SetDialogPhase(DialogPhase phase)
    {
        int tempNum = (int)phase;
        Interlocked.Exchange(ref dialogStateCounter, tempNum);
        OnDialogPhaseChanged(phase);
    }

    public static DialogPhase GetDialogPhase()
    {
        return (DialogPhase)Interlocked.CompareExchange(ref dialogStateCounter,  0,  0);
    }
    #endregion
    
    #region For sending
    private static void OnDialogPhaseChanged(DialogPhase phase)
    {
        if (phase == DialogPhase.NextDialogStateFired) ResetEventForDataSending.Set();
    }
    
    public static void SetDialogName(NativeStringWrapper nativeStringWrapper)
    {
        var previousValue = Interlocked.Exchange(ref dialogName, nativeStringWrapper);
        if(previousValue != null) previousValue.Dispose();
    }

    public static NativeStringWrapper? TakeDialogName()
    {
        var previousValue = Interlocked.Exchange(ref dialogName,  null);
        return previousValue;
    }

    public static void SetCurrentDialogOptionGuid(NativeStringWrapper nativeStringWrapper)
    {
        var previousValue = Interlocked.Exchange(ref currentDialogOptionGuid, nativeStringWrapper);
        if(previousValue != null) previousValue.Dispose();
    }

    public static NativeStringWrapper? TakeCurrentDialogOptionGuild()
    {
        var previousValue = Interlocked.Exchange(ref currentDialogOptionGuid,  null);
        return previousValue;
    }

    public static bool IsNewNameAndGuid(HolderForNativeStringWrapperWithoutNeedForDisposal nativeStringWrapper)
    {
        return uniqueNamesAndGuidsCache.Contains(nativeStringWrapper) == false;
    }

    private static bool AddNameAndGuidToCache(HolderForNativeStringWrapperWithoutNeedForDisposal nativeStringWrapper)
    {
        if (uniqueNamesAndGuidsCache.Count + 1 > maxTotalAmountOfCachedNamesAndGuids) uniqueNamesAndGuidsCache.Clear();
        return uniqueNamesAndGuidsCache.Add(nativeStringWrapper);
    }
    
    // Read comment in the header
    public static void AddNameAndGuidToListForSending(HolderForNativeStringWrapperWithoutNeedForDisposal nativeStringWrapper)
    {
        if (AddNameAndGuidToCache(nativeStringWrapper) == false) return;
        lock (dataToSendLock)
        {
            uniqueNamesAndGuidsForSending.Add(nativeStringWrapper);
        }
    }

    public static void AddNamesAndGuidsToList(in List<HolderForNativeStringWrapperWithoutNeedForDisposal> list)
    {
        lock (dataToSendLock)
        {
            list.AddRange(uniqueNamesAndGuidsForSending);
            uniqueNamesAndGuidsForSending.Clear();
        }
    }
    #endregion
}