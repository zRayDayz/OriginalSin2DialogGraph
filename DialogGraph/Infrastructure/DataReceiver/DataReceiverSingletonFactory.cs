namespace DialogGraph;

public class DataReceiverFactory
{
    private IDialogProcessor dialogProcessor;
    private IExceptionHandler exceptionHandler;
    public DataReceiverFactory(IDialogProcessor dialogProcessor, IExceptionHandler exceptionHandler)
    {
        this.dialogProcessor = dialogProcessor;
        this.exceptionHandler = exceptionHandler;
    }
    public DataReceiver Create(Progress<byte[]> progress)
    {
        return new DataReceiver(progress, dialogProcessor, exceptionHandler);
    }
}