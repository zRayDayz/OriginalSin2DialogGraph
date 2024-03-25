namespace InjectedLogic;

public interface IIOProvider
{
    public string? ReadLine();

    public void WriteErrorLine(string text);

    public void WriteLine(string text = "");

    public void WriteLineSynchronizedWithoutWaiting(string text = "");

    public void WriteTextSynchronizedWithoutWaiting(string text = "");
}