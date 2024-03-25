using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DialogGraph;

public class DialogJsonParser
{
    private JPathWrapper jPathForUpperDialogNode = new JPathWrapper("$..dialog");
    private JPathWrapper jPathForSpeakerList = new JPathWrapper("$.speakerlist[*].*[*]");
    private JPathWrapper jPathForDialogNodes = new JPathWrapper("$..node[*]");
    private JPathWrapper jPathForTagTextElements = new JPathWrapper("$..[?(@.handle && @.value)]");
    
    private Dictionary<string, DialogNode> dictionaryForDialogsByIds = new Dictionary<string, DialogNode>(16);
    private List<DialogNodeText> tempDialogNodeTextsList = new List<DialogNodeText>();
    private List<DialogFlag> tempFlagsToCheckList = new List<DialogFlag>();
    private List<DialogFlag> tempFlagsToSetList = new List<DialogFlag>();
    
    private const string jumpNodeType = "Jump";
    private const string popNodeType = "Pop";
    private const string persuasionNodeType = "Persuasion";
    private const string persuasionResultNodeType = "PersuasionResult";
    
    public Dialog ParseDialogFile(string dialogName, StreamReader streamReader)
    {
        try
        {
            dictionaryForDialogsByIds.Clear();

            using var jsonTextReader = new JsonTextReader(streamReader);
            var dialogFullJson = (JObject)JToken.ReadFrom(jsonTextReader);
            
            var tempUpperDialogNode = jPathForUpperDialogNode.SelectFirstToken(dialogFullJson.Root);
            string dialogId = tempUpperDialogNode["UUID"]["value"].ToString();
            
            IEnumerable<JToken> speakerListJson = jPathForSpeakerList.SelectTokens(tempUpperDialogNode);
            Dictionary<string, DialogSpeaker> dialogSpeakersByIndexes = CreateDialogSpeakersDictionary(speakerListJson);
            
            var dialogNodesJson = jPathForDialogNodes.SelectTokens(tempUpperDialogNode);

            List<DialogNode> dialogNodes = CreateDialogNodesList(dialogNodesJson, dictionaryForDialogsByIds);

            foreach (DialogNode dialogNode in dialogNodes)
            {
                var tempChildrenIds = dialogNode.TempChildrenIds;

                DialogNode[] childerNodes;
                if (tempChildrenIds.Length == 0)
                {
                    childerNodes = Array.Empty<DialogNode>();
                }
                else
                {
                    childerNodes = new DialogNode[tempChildrenIds.Length];
                    for (int i = 0; i < tempChildrenIds.Length; i++)
                    {
                        var childrenId = tempChildrenIds[i];
                        dictionaryForDialogsByIds.TryGetValue(childrenId, out DialogNode childrenNode);
                        childerNodes[i] = childrenNode;
                    }
                }

                dialogNode.Children = childerNodes;
            }

            SetDialogNodesSpeakerIds(dialogNodes, dialogSpeakersByIndexes);

            var dialog = new Dialog(dialogName, dialogId, dialogNodes);

            return dialog;
        }
        catch (Exception e)
        {
            string errorText;
            if (e.InnerException != null) errorText = $"Dialog name: '{dialogName}'" + Environment.NewLine;
            else errorText = $"Error during parsing of dialog file. Dialog name: '{dialogName}'" + Environment.NewLine;
            throw new JsonReaderException(errorText, e);
        }
    }
    
    // An array (or List) can be used (as a sort of jump table) instead of Dictionary, but there is no guarantee that the index will always be safe
    private Dictionary<string, DialogSpeaker> CreateDialogSpeakersDictionary(IEnumerable<JToken> speakerListJson)
    {
        Dictionary<string, DialogSpeaker> dialogSpeakers = new Dictionary<string, DialogSpeaker>(4);
        foreach (var speakerJson in speakerListJson)
        {
            var index = speakerJson["index"]["value"].ToString();
            var id = speakerJson["list"]["value"].ToString();
            var dialogSpeaker = new DialogSpeaker(id, index);
            dialogSpeakers.Add(index, dialogSpeaker);
        }

        return dialogSpeakers;
    }
    
    private void SetDialogNodesSpeakerIds(List<DialogNode> dialogNodes, Dictionary<string, DialogSpeaker> dialogSpeakers)
    {
        for (int i = 0; i < dialogNodes.Count; i++)
        {
            var node = dialogNodes[i];
            var speakerIndex = node.SpeakerIndex;
            if (speakerIndex == String.Empty) continue;
            if (dialogSpeakers.TryGetValue(speakerIndex, out DialogSpeaker speaker)) node.SpeakerId = speaker.Id;
        }
    }

    private List<DialogNode> CreateDialogNodesList(IEnumerable<JToken> dialogNodesJson, in Dictionary<string, DialogNode> dictionaryForDialogsByIds)
    {
        List<DialogNode> dialogNodes = new List<DialogNode>(16);
        int loopCounter = 0;
        foreach (var dialogNodeJson in dialogNodesJson)
        {
            loopCounter++;
            try
            {
                tempDialogNodeTextsList.Clear();
                tempFlagsToCheckList.Clear();
                tempFlagsToSetList.Clear();
                
                var type = dialogNodeJson["constructor"]["value"].ToString();
                var id = dialogNodeJson["UUID"]["value"].ToString();
                if (type == jumpNodeType)
                {
                    DialogNodeText[] jumpTexts = { new DialogNodeText("jump node") };
                    var jumpToId = dialogNodeJson["jumptarget"]["value"].ToString();
                    string[] jumpTo = { jumpToId };

                    var jumpDialog = new DialogNode(type, id, jumpTexts, jumpTo, dialogNodeJson.ToString());
                    dictionaryForDialogsByIds.Add(id, jumpDialog);

                    dialogNodes.Add(jumpDialog);
                    continue;
                }

                if (type == popNodeType)
                {
                    var popLevel = dialogNodeJson["PopLevel"]["value"].ToString();
                    DialogNodeText[] texts = { new DialogNodeText("jump to dialog layer №" + popLevel) };
                    var jumpDialog = new DialogNode(type, id, texts, Array.Empty<string>(), dialogNodeJson.ToString());
                    dictionaryForDialogsByIds.Add(id, jumpDialog);

                    dialogNodes.Add(jumpDialog);
                    continue;
                }

                var tempEndNode = dialogNodeJson["endnode"]?["value"]?.ToString();
                string endNode = tempEndNode ?? "0";
                var tempSpeakerIndex = dialogNodeJson["speaker"]?["value"]?.ToString();
                string speakerIndex = tempSpeakerIndex ?? String.Empty;

                #region Text
                if (type == persuasionResultNodeType)
                {
                    var persuasionResult = dialogNodeJson["Success"]["value"].ToString();
                    var persuasionResultText = persuasionResult == "1"
                        ? "successful persuasion result"
                        : "unsuccessful persuasion result";
                    var dialogNodeText = new DialogNodeText(null, persuasionResultText);
                    tempDialogNodeTextsList.Add(dialogNodeText);
                }
                else
                {
                    ParseText(dialogNodeJson, tempDialogNodeTextsList);
                }
                if (type == persuasionNodeType)
                {
                    var statName = dialogNodeJson["StatName"]["value"].ToString();
                    for (int i = 0; i < tempDialogNodeTextsList.Count; i++)
                    {
                        DialogNodeText newText = tempDialogNodeTextsList[i];
                        newText.AdditionalText = "Stat: " + statName + Environment.NewLine;
                        tempDialogNodeTextsList[i] = newText;
                    }
                }
                DialogNodeText[] dialogNodeTexts = tempDialogNodeTextsList.ToArray();
                #endregion
                
                string[] childrenIds = ParseChildren(dialogNodeJson);

                #region Flags
                ParseFlagsToCheck(dialogNodeJson, tempFlagsToCheckList);
                DialogFlag[] resultFlagsToCheck = tempFlagsToCheckList.ToArray();
                
                ParseFlagsToSet(dialogNodeJson, tempFlagsToSetList);
                DialogFlag[] resultFlagsToSet = tempFlagsToSetList.ToArray();
                #endregion
                
                var dialog = new DialogNode(type, id, endNode, speakerIndex, dialogNodeTexts, resultFlagsToCheck, resultFlagsToSet, childrenIds, dialogNodeJson.ToString());
                dictionaryForDialogsByIds.Add(id, dialog);
                dialogNodes.Add(dialog);
            }
            catch (Exception e)
            {
                var errorText = $"Error during parsing of dialog file. Dialog node №{loopCounter - 1}" + Environment.NewLine;
                throw new JsonReaderException(errorText, e);
            }

        }

        return dialogNodes;
    }

    void ParseText(JToken dialogNodeJson, in List<DialogNodeText> result)
    {
        var taggedTexts = dialogNodeJson["TaggedTexts"];
        if (taggedTexts == null) return;
        
        //  taggedTexts.SelectTokens("$..[?(@.handle && @.value)].['handle','value']") - will return an array of all "handles" and "values" 
        var tagTextElementsJson = jPathForTagTextElements.SelectTokens(taggedTexts);
        foreach (var tagTextElementJson in tagTextElementsJson)
        {
            var textId = tagTextElementJson["handle"].ToString();
            var text = tagTextElementJson["value"].ToString();
            var dialogNodeText = new DialogNodeText(textId, text);
            result.Add(dialogNodeText);
        }
    }
    
    string[] ParseChildren(JToken dialogNodeJson)
    {
        string[] childrenIds = Array.Empty<string>();
        var tempNodeForChilder = dialogNodeJson["children"]?[0];
        if (tempNodeForChilder == null) return childrenIds;
        
        if (tempNodeForChilder.HasValues)
        {
            var childerNodesJson = (JArray)tempNodeForChilder["child"];
            var amountOfChilder = childerNodesJson.Count;
            childrenIds = new string[amountOfChilder];
            for (int i = 0; i < amountOfChilder; i++)
            {
                childrenIds[i] = childerNodesJson[i]["UUID"]["value"].ToString();
            }
        }

        return childrenIds;
    }
    
    void ParseFlagsToCheck(JToken dialogNodeJson, in List<DialogFlag> tempFlagsToCheckCache)
    {
        var checkFlagsJson = dialogNodeJson["checkflags"]?[0];
        if (checkFlagsJson == null) return;
        
        if (checkFlagsJson.HasValues)
        {
            GetFlagsFromJson(checkFlagsJson, tempFlagsToCheckCache);
        }
    }

    void ParseFlagsToSet(JToken dialogNodeJson, in List<DialogFlag> tempFlagsToSetCache)
    {
        var setFlagsJson = dialogNodeJson["setflags"]?[0];
        if (setFlagsJson == null) return;
        
        if (setFlagsJson.HasValues)
        {
            GetFlagsFromJson(setFlagsJson, tempFlagsToSetCache);
        }
    }

    private void GetFlagsFromJson(JToken rootOfFlagsJson, in List<DialogFlag> result)
    {
        var flagGroupJson = (JArray)rootOfFlagsJson["flaggroup"];
        var amountOfFlagGroups = flagGroupJson.Count;

        for (int i = 0; i < amountOfFlagGroups; i++)
        {
            var flagJson = (JArray)flagGroupJson[i]["flag"];
            var amountOfFlags = flagJson.Count;
            for (int j = 0; j < amountOfFlags; j++)
            {
                var innerFlagJson = flagJson[j];
                var flagId = innerFlagJson["name"]["value"].ToString();
                var fullData = innerFlagJson.ToString();

                var dialogFlag = new DialogFlag(flagId, fullData);
                result.Add(dialogFlag);
            }

        }
    }
}

