namespace DialogGraph;

public class DialogProcessor : IDialogProcessor
{
    private IMediator mediator;
    private ILogger logger;
    private IDialogFileProcessor dialogFileProcessor;
    private LocalizationProcessor localizationProcessor;

    private Dictionary<string, DialogAndGraphWrapper> createdDialogGraphs = new Dictionary<string, DialogAndGraphWrapper>(16);
    private Dictionary<string, string> namesAndGuids = new Dictionary<string, string>(128);
    private Dictionary<string, GraphVertex> tempVerticesDictionary = new Dictionary<string, GraphVertex>(32);

    // The dialog "EG_LucianDallisBraccus" contains 520 nodes (vertexes) and 652 edges. The memory size of such a dialog (including an instance of the Dialog class and an instance of the CustomGraph class) is about 306.5 kb.
    // Which approximately (averaged) means that a dialog with 1 node (and several edges) will have a size of about 0.58 kb
    private int currentTotalAmoutOfCachedNodes = 0;
    private const int maxTotalAmountOfCachedNodes = 18000; // ~ 10,5 mb

    record struct DialogAndGraphWrapper(Dialog dialog, CustomGraph dialogGraph);
        
    public DialogProcessor(IMediator mediator, ILogger logger, IDialogFileProcessor dialogFileProcessor, LocalizationProcessor localizationProcessor)
    {
        this.mediator = mediator;
        this.logger = logger;
        this.dialogFileProcessor = dialogFileProcessor;
        this.localizationProcessor = localizationProcessor;
    }

    public void ProcessNewDialog(NetworkDialogGraphData networkDialogGraphData)
    {
        var dialogName = networkDialogGraphData.DialogName;

        if (createdDialogGraphs.TryGetValue(dialogName, out DialogAndGraphWrapper dialogAndGraph))
        {
            UpdateActiveNodeAndSpeakerNames(dialogAndGraph.dialogGraph, networkDialogGraphData);
            mediator.Send(new GraphMessage(dialogAndGraph.dialogGraph, false));
            return;
        }

        Dialog dialog = dialogFileProcessor.CreateNewDialogByName(dialogName);
        
        CheckAndReleaseMemoryIfNeeded(dialog);
        
        PrepareDialogNodesForDialogGraphCreation(dialog);
        CustomGraph dialogGraph = CreateGraphFromDialog(dialog);

        createdDialogGraphs.Add(dialog.Name, new DialogAndGraphWrapper(dialog, dialogGraph));
        
        UpdateActiveNodeAndSpeakerNames(dialogGraph, networkDialogGraphData);
        mediator.Send(new GraphMessage(dialogGraph, true));
    }

    private void CheckAndReleaseMemoryIfNeeded(Dialog dialog)
    {
        var amountOfNodesInNewDialog = dialog.DialogNodes.Count;
        currentTotalAmoutOfCachedNodes += amountOfNodesInNewDialog;
        if (currentTotalAmoutOfCachedNodes >= maxTotalAmountOfCachedNodes)
        {
            currentTotalAmoutOfCachedNodes = amountOfNodesInNewDialog;
            createdDialogGraphs.Clear();
        }
    }
    
    private void UpdateActiveNodeAndSpeakerNames(CustomGraph dialogGraph, NetworkDialogGraphData networkDialogGraphData)
    {
        var currentDialogOptionName = networkDialogGraphData.CurrentDialogOptionName;
        foreach (var vertex in dialogGraph.Vertices)
        {
            if (vertex.Id == currentDialogOptionName) dialogGraph.SetNewActiveNode(vertex);
        }
        
        var localNamesAndGuids = networkDialogGraphData.NamesAndGuids;
        if (localNamesAndGuids != null)
        {
            for (int i = 0; i < localNamesAndGuids.Length; i++)
            {
                var nameAndGuid = localNamesAndGuids[i];
                namesAndGuids.TryAdd(nameAndGuid.guid, nameAndGuid.name);
            }
            
            foreach (var vertex in dialogGraph.Vertices)
            {
                if (vertex.SpeakerName != String.Empty) continue;
                if (namesAndGuids.TryGetValue(vertex.SpeakerId, out string name)) vertex.SpeakerName = name;
            }
        }
    }
    
    private void PrepareDialogNodesForDialogGraphCreation(Dialog dialog)
    {
        foreach (var node in dialog.DialogNodes)
        {
            var speakerIndex = node.SpeakerIndex;
            if (Int32.TryParse(speakerIndex, out int intSpeakerId))
            {
                if (intSpeakerId < 0) node.SpeakerName = "Narrator";
            }

            var type = node.Type;
            switch (type)
            {
                case "TagGreeting": 
                    node.StrictType = DialogNodeType.Greeting;
                    break;
                case "TagQuestion":
                    node.StrictType = DialogNodeType.Question;
                    break;
                case "TagAnswer":
                    node.StrictType = DialogNodeType.Answer;
                    break;
                case "TagJump":
                    node.StrictType = DialogNodeType.Jump;
                    break;
                case "Persuasion":
                    node.StrictType = DialogNodeType.Persuasion;
                    break;
                case "PersuasionResult":
                    node.StrictType = DialogNodeType.PersuasionResult;
                    break;
                case "Pop":
                    node.StrictType = DialogNodeType.Pop;
                    break;
                case "DualDialog":
                    node.StrictType = DialogNodeType.DualDialog;
                    break;
                default:
                    node.StrictType = DialogNodeType.UNRECOGNIZED;
                    break;
            }
        }
    }
    
    private CustomGraph CreateGraphFromDialog(Dialog dialog)
    {
        CustomGraph graph = new CustomGraph(dialog.Id, dialog.Name);

        tempVerticesDictionary.Clear();

        Dictionary<string, GraphDialogFlag> graphDialogFlagsByIds = CreateGraphDialogFlagDictionaryByIds(dialog);

        var dialogNodes = dialog.DialogNodes;
        for (int i = 0; i < dialogNodes.Count; i++)
        {
            DialogNode dialogNode = dialogNodes[i];
            DialogNodeText[] dialogTextArr = dialogNode.DialogNodeTexts;
            
            string text = GetFirstLocalizedOrOriginText(dialogTextArr);
            
            (GraphDialogFlag[] flagsToCheck, GraphDialogFlag[] flagsToSet) = CreateGraphDialogFlags(dialogNode, graphDialogFlagsByIds);
            GraphVertex vertex = new GraphVertex(dialogNode.Id, text, dialogNode.SpeakerId, dialogNode.StrictType, dialogNode.EndNode, flagsToCheck, flagsToSet, dialogNode.FullData, dialogNode.SpeakerName);
            tempVerticesDictionary.Add(dialogNode.Id, vertex);
            graph.AddVertex(vertex);
        }
        
        for (int i = 0; i < dialogNodes.Count; i++)
        {
            DialogNode dialogNode = dialogNodes[i];
            
            DialogNode[] childrenNodes = dialogNode.Children;
            if(childrenNodes.Length == 0) continue;
            GraphVertex currentVertex = tempVerticesDictionary[dialogNode.Id];
                
            foreach (DialogNode childNode in childrenNodes)
            {
                GraphVertex childVertex = tempVerticesDictionary[childNode.Id];
                GraphEdge edge = new GraphEdge(childNode.Id, currentVertex, childVertex);
                graph.AddEdge(edge);
            }
        }

        return graph;
    }

    private string GetFirstLocalizedOrOriginText(DialogNodeText[] dialogTextArr)
    {
        string text;
        if(dialogTextArr.Length == 0) return String.Empty;
        
        DialogNodeText firstDialogText = dialogTextArr[0];
        if (firstDialogText.TextId == null) return CombineAdditionalTextAndMainText(firstDialogText.Text, firstDialogText.AdditionalText);
        
        if (localizationProcessor.ShoudLocalizeText == false) return CombineAdditionalTextAndMainText(firstDialogText.Text, firstDialogText.AdditionalText);

        string? localizedText = localizationProcessor.TryGetLocalizationStringById(firstDialogText.TextId);
        if (localizedText == null)
        {
            logger.LogText($"{Environment.NewLine}========== DIALOG PROCESSOR =========={Environment.NewLine}" +
                           $"Localized text for id: {firstDialogText.TextId} - was not found{Environment.NewLine}" +
                           $"=========={Environment.NewLine}");
            text = firstDialogText.Text;
        }
        else
        {
            text = localizedText;
        }
        
        return CombineAdditionalTextAndMainText(text, firstDialogText.AdditionalText);
        
        string CombineAdditionalTextAndMainText(string mainText, string? additionalText)
        {
            return additionalText != null ? additionalText + mainText : mainText;
        }
    }

    
    private Dictionary<string, GraphDialogFlag> CreateGraphDialogFlagDictionaryByIds(Dialog dialog)
    {
        Dictionary<string, GraphDialogFlag> graphDialogFlagsByIds = new Dictionary<string, GraphDialogFlag>(16);
        var dialogNodes = dialog.DialogNodes;
        for (int i = 0; i < dialogNodes.Count; i++)
        {
            var node = dialogNodes[i];
            
            var flagsToCheck = node.FlagsToCheck;
            for (int j = 0; j < flagsToCheck.Length; j++)
            {
                DialogFlag dialogFlag = flagsToCheck[j];
                if (graphDialogFlagsByIds.ContainsKey(dialogFlag.Id) == false)
                {
                    GraphDialogFlag graphDialogFlag = new GraphDialogFlag(dialogFlag.Id, dialogFlag.FullData);
                    graphDialogFlagsByIds.Add(dialogFlag.Id, graphDialogFlag);
                }
            }
            
            var flagsToSet = node.FlagsToSet;
            for (int j = 0; j < flagsToSet.Length; j++)
            {
                DialogFlag dialogFlag = flagsToSet[j];
                if (graphDialogFlagsByIds.ContainsKey(dialogFlag.Id) == false)
                {
                    GraphDialogFlag graphDialogFlag = new GraphDialogFlag(dialogFlag.Id, dialogFlag.FullData);
                    graphDialogFlagsByIds.Add(dialogFlag.Id, graphDialogFlag);
                }
            }
        }
        return graphDialogFlagsByIds;
    }

    private (GraphDialogFlag[] flagsToCheck, GraphDialogFlag[] flagsToSet) CreateGraphDialogFlags(DialogNode dialogNode, in Dictionary<string, GraphDialogFlag> graphDialogFlagsByIds)
    {
        var flagsToCheck = dialogNode.FlagsToCheck;
        GraphDialogFlag[] graphDialogFlagsToCheck = flagsToCheck.Length == 0 ? Array.Empty<GraphDialogFlag>() : new GraphDialogFlag[flagsToCheck.Length];
        for (int i = 0; i < flagsToCheck.Length; i++)
        {
            DialogFlag dialogFlag = flagsToCheck[i];
            GraphDialogFlag graphDialogFlag = graphDialogFlagsByIds[dialogFlag.Id];
            graphDialogFlagsToCheck[i] = graphDialogFlag;
        }
        
        var flagsToSet = dialogNode.FlagsToSet;
        GraphDialogFlag[] graphDialogFlagsToSet = flagsToSet.Length == 0 ? Array.Empty<GraphDialogFlag>() : new GraphDialogFlag[flagsToSet.Length];
        for (int i = 0; i < flagsToSet.Length; i++)
        {
            DialogFlag dialogFlag = flagsToSet[i];
            GraphDialogFlag graphDialogFlag = graphDialogFlagsByIds[dialogFlag.Id];
            graphDialogFlagsToSet[i] = graphDialogFlag;
        }
        
        return (graphDialogFlagsToCheck, graphDialogFlagsToSet);
    }

}


