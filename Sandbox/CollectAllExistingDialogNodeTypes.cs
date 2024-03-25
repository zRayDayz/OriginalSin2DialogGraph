using DialogGraph;

namespace Sandbox;

record struct DialogTypesInfo(List<string> dialogNames);

public class CollectAllExistingDialogNodeTypes
{
    private Dictionary<string, DialogTypesInfo> dialogTypeInfos = new Dictionary<string, DialogTypesInfo>(32);
    public void Start()
    {
        TestIOCContainer.CreateIOCContainer();
        //TestIOCContainer.Instance.InjectedLogicConfigManager.InitializeConfig();
        TestIOCContainer.Instance.DialogGraphConfigManager.Value.InitializeConfig();
        
        var modFolderPath = TestIOCContainer.Instance.DialogGraphConfigManager.Value.Config.DialogFolderPath;
        
        string[] allFiles = Directory.GetFiles(modFolderPath, "*.*", SearchOption.AllDirectories);
    
        // var TEST_PARSER = new TEST_PARSER();
        // foreach (var filePath in allFiles)
        // {
        //     // Спарсить файл и забрать лист всех типов в нем
        //     using var stream = new StreamReader(filePath);
        //     var typesInDialog = TEST_PARSER.ParseDialogFile(filePath, stream);
        //
        //     for (int i = 0; i < typesInDialog.Length; i++)
        //     {
        //         var type = typesInDialog[i];
        //         
        //         if (dialogTypeInfos.TryGetValue(type, out DialogTypesInfo info))
        //         {
        //             info.dialogNames.Add(filePath);
        //         }
        //         else
        //         {
        //             var dialogNames = new List<string>(128);
        //             dialogNames.Add(filePath);
        //             DialogTypesInfo newInfo = new DialogTypesInfo(dialogNames);
        //             dialogTypeInfos.Add(type, newInfo);
        //             Console.WriteLine(type);
        //         }
        //     }
        //     
        //     
        //     
        // }
        //
        // Console.WriteLine("END");
    
    }
}

//// Should be placed in DialogGraph project, other
// public class TEST_PARSER
// {
//     private JPathWrapper jPathForUpperDialogNode = new JPathWrapper("$..dialog");
//     private JPathWrapper jPathForDialogNodes = new JPathWrapper("$..node[*]");
//     
//     public string[] ParseDialogFile(string dialogName, StreamReader streamReader)
//     {
//         try
//         {
//             using var jsonTextReader = new JsonTextReader(streamReader);
//             var dialogFullJson =  (JObject)JToken.ReadFrom(jsonTextReader);
//         
//             var tempUpperDialogNode = jPathForUpperDialogNode.SelectFirstToken(dialogFullJson.Root);
//             var dialogNodesJson = jPathForDialogNodes.SelectTokens(tempUpperDialogNode);
//
//             HashSet<string> uniqueTypes = new HashSet<string>(16);
//             foreach (var dialogNodeJson in dialogNodesJson)
//             {
//                 var type = dialogNodeJson["constructor"]["value"].ToString();
//                 uniqueTypes.Add(type);
//             }
//
//             string[] resultArr = new string[uniqueTypes.Count];
//             uniqueTypes.CopyTo(resultArr);
//             return resultArr;
//         }
//         catch (Exception e)
//         {
//             Console.WriteLine("+++++++++++ ERROR on file: " + dialogName);
//         }
//
//         return Array.Empty<string>();
//     }
// }