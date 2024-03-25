using System.Net.Sockets;

namespace InjectedLogic;

public interface IDataSender
{
    public void OnNewServerCycle();
    public bool IsReadyToSendData();
    public ReadOnlySpan<byte> GetDataToWrite();
    public void OnDataSuccessfullySent();
    public void OnDataUnsuccessfullySent();
}