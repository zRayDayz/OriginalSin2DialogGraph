using System.Text;
using System.Text.Json;

namespace DialogGraph;

public class DataReceiver
{
    private IDialogProcessor dialogProcessor;
    private IExceptionHandler exceptionHandler;
    private readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

    public DataReceiver(Progress<byte[]> progress, IDialogProcessor dialogProcessor, IExceptionHandler exceptionHandler)
    {
        progress.ProgressChanged += ParseReceivedData;
        this.dialogProcessor = dialogProcessor;
        this.exceptionHandler = exceptionHandler;
    }
    
    public void ParseReceivedData(object sender, byte[] bytes)
    {
        byte[] data = null;
        try
        {
            data = bytes ?? throw new ArgumentException("A byte array was expected as an argument, 'null' received");

            var networkDialogGraphData = JsonSerializer.Deserialize<NetworkDialogGraphData>(data, jsonSerializerOptions);
            
            var namesAndGuidsCombined = networkDialogGraphData.namesAndGuidsCombined;
            if (namesAndGuidsCombined != null)
            {
                NameAndGuid[] namesAndGuids = new NameAndGuid[namesAndGuidsCombined.Length];
                for (int i = 0; i < namesAndGuidsCombined.Length; i++)
                {
                    var nameAndGuid = namesAndGuidsCombined[i];
                    var lastIndexOfUnderscore = nameAndGuid.LastIndexOf('_');
                    string name = nameAndGuid.Substring(0, lastIndexOfUnderscore);
                    string guid = nameAndGuid.Substring(lastIndexOfUnderscore + 1);
                    namesAndGuids[i] = new NameAndGuid(name, guid);
                }

                networkDialogGraphData.NamesAndGuids = namesAndGuids;
            }
            
            dialogProcessor.ProcessNewDialog(networkDialogGraphData);
        }
        catch (Exception e)
        {
            string errorText = "Received network data in UTF8:" + Environment.NewLine +
                               Encoding.UTF8.GetString(data) + Environment.NewLine + Environment.NewLine +
                               e;
            
            exceptionHandler.HandleExceptionBlockingWay(errorText, this);
        }
    }
}

