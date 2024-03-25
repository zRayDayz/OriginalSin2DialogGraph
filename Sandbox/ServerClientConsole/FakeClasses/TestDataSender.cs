using System.Text;
using InjectedLogic;

namespace Sandbox;

public class TestDataSender : IDataSender
{
    
    //private NativeStringWrapper? tempDialogNameBytes = null;

    public void OnNewServerCycle()
    {
        Thread.Sleep(1500);
    }

    public bool IsReadyToSendData()
    {
        // tempDialogNameBytes = DataToSendSyncKeeper.TakeDialogNameBytes();
        // if (tempDialogNameBytes == null)
        // {
        //     Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!! tempDialogNameBytes is null !!!!!!!!!!!!!!!!!!!!!!!!");
        //     return false;
        // }
        
        return true;
    }

    public ReadOnlySpan<byte> GetDataToWrite()
    {
        //var strBytes = Encoding.UTF8.GetBytes("{ \"dialogName\": {\"innerObjKey\": 12345} }");
        var strBytes = Encoding.UTF8.GetBytes("{ \"dialogName\": \"TestDialogName\" }");
        return strBytes;
        //return tempDialogNameBytes.NativeStringSpan;
    }

    public void OnDataSuccessfullySent()
    {
        DisposeNecesseryObjects();
        Console.WriteLine("DATA WAS DISPOSED");
    }

    public void OnDataUnsuccessfullySent()
    {
        
    }

    private void DisposeNecesseryObjects()
    {
        
    }
}