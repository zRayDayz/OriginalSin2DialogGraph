using System.Text;

namespace InjectedLogic;

public class DataSender : IDataSender
{
    private readonly ListRentWrapper<byte> dataBuffer = new ListRentWrapper<byte>(1024);
    readonly byte[] dialogNameDataKeyBytes = Encoding.UTF8.GetBytes("\"dialogName\":");
    readonly byte[] currentDialogOptionGuidDataKeyBytes = Encoding.UTF8.GetBytes("\"currentDialogOptionName\":");
    readonly byte[] namesAndGuidsDataKeyBytes = Encoding.UTF8.GetBytes("\"namesAndGuidsCombined\":");
    private NativeStringWrapper? tempDialogName;
    private NativeStringWrapper? tempCurrentDialogOptionGuid;
    private List<HolderForNativeStringWrapperWithoutNeedForDisposal> tempNamesAndGuids = new List<HolderForNativeStringWrapperWithoutNeedForDisposal>(128);

    public void OnNewServerCycle()
    {
        DataToSendSyncKeeper.ResetEventForDataSending.WaitOne();
    }

    public bool IsReadyToSendData()
    {
        // We should always send the current active dialog name
        var backupOfDialogName = tempDialogName;
        tempDialogName = DataToSendSyncKeeper.TakeDialogName();
        if (tempDialogName == null) tempDialogName = backupOfDialogName;
        else backupOfDialogName?.Dispose();

        tempCurrentDialogOptionGuid?.Dispose();
        tempCurrentDialogOptionGuid = DataToSendSyncKeeper.TakeCurrentDialogOptionGuild();
        DataToSendSyncKeeper.AddNamesAndGuidsToList(tempNamesAndGuids);
        
        return true;
    }

    public ReadOnlySpan<byte> GetDataToWrite()
    {
        dataBuffer.Clear();
        
        dataBuffer.Add((byte)'{');
        
        AddJsonDialogNameToDataBuffer(false);
        AddJsonCurrentDialogOptionGuidToDataBuffer();
        AddJsonNamesAndGuids();
        
        dataBuffer.Add((byte)'}');
        
        var dataSpan = dataBuffer.RentListArrayAsSpan();
        return dataSpan;
    }

    private bool AddJsonDialogNameToDataBuffer(bool addCommaAtTheBeginning = true)
    {
        if(tempDialogName == null) return false;
            
        if(addCommaAtTheBeginning) dataBuffer.Add((byte)',');
        
        dataBuffer.AddRange(dialogNameDataKeyBytes);
        dataBuffer.Add((byte)'"');
        ListRentWrapper<byte>.AddToList(dataBuffer, tempDialogName);
        dataBuffer.Add((byte)'"');

        return true;
    }

    private bool AddJsonCurrentDialogOptionGuidToDataBuffer(bool addCommaAtTheBeginning = true)
    {
        if (tempCurrentDialogOptionGuid == null) return false;
        
        if(addCommaAtTheBeginning) dataBuffer.Add((byte)',');
        
        dataBuffer.AddRange(currentDialogOptionGuidDataKeyBytes);
        dataBuffer.Add((byte)'"');
        ListRentWrapper<byte>.AddToList(dataBuffer, tempCurrentDialogOptionGuid);
        dataBuffer.Add((byte)'"');

        return true;
    }
    
    private bool AddJsonNamesAndGuids(bool addCommaAtTheEnd = true)
    {
        if (tempNamesAndGuids.Count == 0) return false;
        
        if(addCommaAtTheEnd) dataBuffer.Add((byte)',');
        
        dataBuffer.AddRange(namesAndGuidsDataKeyBytes);
        dataBuffer.Add((byte)'[');
        var listCount = tempNamesAndGuids.Count;
        for (int i = 0; i < listCount - 1; i++)
        {
            dataBuffer.Add((byte)'"');
            ListRentWrapper<byte>.AddToList(dataBuffer, tempNamesAndGuids[i].nativeStringWrapper);
            dataBuffer.Add((byte)'"');
            dataBuffer.Add((byte)',');
        }
        
        var lastNameAndGuid = tempNamesAndGuids[listCount - 1];
        dataBuffer.Add((byte)'"');
        ListRentWrapper<byte>.AddToList(dataBuffer, lastNameAndGuid.nativeStringWrapper);
        dataBuffer.Add((byte)'"');
        dataBuffer.Add((byte)']');

        return true;
    }
    

    public void OnDataSuccessfullySent()
    {
        dataBuffer.CloseArrayRent();
        tempNamesAndGuids.Clear();
    }

    public void OnDataUnsuccessfullySent()
    {
        dataBuffer.CloseArrayRent();
    }
}