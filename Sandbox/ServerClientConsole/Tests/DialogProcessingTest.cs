using DialogGraph;

namespace Sandbox;

public class DialogProcessingTest
{
    public void TestDialogProcessing()
    {
        IOCContainer.CreateIOCContainer();
        var container = IOCContainer.Instance;
        
        container.ConfigManager.Value.InitializeConfig();
        
        //container.DialogProcessor.Value.ProcessNewDialog(new NetworkDialogGraphData { DialogName = "TUT_CargoDeck_UnrulyPrisoner"});
        container.DialogProcessor.Value.ProcessNewDialog(new NetworkDialogGraphData { DialogName = "EG_LucianDallisBraccus.lsj"});  // dialog with huge amount of different types for nodes
        
    }

    public void TestLocalizationReading()
    {
        IOCContainer.CreateIOCContainer();
        IOCContainer.Instance.ConfigManager.Value.InitializeConfig();
        
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h00002e6cg3610g4788g8e0eg7615ac443b6e");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h00007224gb454g4b8bgb762g7865d9ee3dbb");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h0001d8b9g13d6g4605g85e9g708fe1e537c8");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h0001ffa1ge978g493dg9974ga0df697ee0c8");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h0002c88eg09a6g4f51g9976g8d7091681e05");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h000310b1gc6f0g4341gbbddgf1f5fc471eae");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h00033b76gbbb5g4048g8477g4fb01a461099");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h00057e69g7968g4876gb9a9g36b71f085fc8");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h00058310gceb3g43eega841g19e30087e3b5");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h000597d6g6d62g4627gb536g68407f808829");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h00060d7bg6a18g45e0gacd5gc8d2b018b7bd");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h0006d286gb15dg4bc6g8c07g70bdc980c571");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h00076287g5eb6g445ag8841g8bf11679a8d6");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h000832a3g6348g424fg8a1eg67646d851acf");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h0008fb2agcaabg4d00ga076gcdd9c86ad667");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h00093067g912cg44b9g8351gf2e2fce0ba1f");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h000995e1gf6dfg4267g9d56g8ab2b8bd9344");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h000a403bg94f2g44b0gada3g079f569ad788");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h000aaaedg26f1g451eg9f80ge451d946b379");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h000ac948ge1bag4369g9907gd9839b65ad86");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h000c0dabg665dg467fgbb15ged8ed4bbb76c");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h000d18f0g409fg40d5ga165g468be552d247");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h000d2853gd90cg4684gafafga1132dd4dcae");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h000d3d39g31f9g41e1ga9a9g1f13e43dbc4b");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h000e5d8cgb17bg4fbdgbdafg8b16870b7010");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h000fbcfcg5857g45bbg8fb8g062ceafeaa4e");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h000fc6fbg8931g484dg90f3gc32ed5de67fd");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h001001b6gc81fg49a1g9e78gc8eed60922cc");
        // IOCContainer.Instance.LocalizationFileParser.Value.TryGetLocalizationStringById("h0010c542g4ab4g46a8gba50g4a4d74cf6199");
        
        
        IOCContainer.Instance.LocalizationFileParser.Value.TryCreateLocalizationStringById("hffffb804gc54fg4409g88d1gd3e0209d616d");
        IOCContainer.Instance.LocalizationFileParser.Value.TryCreateLocalizationStringById("h00002e6cg3610g4788g8e0eg7615ac443b6e");
        IOCContainer.Instance.LocalizationFileParser.Value.TryCreateLocalizationStringById("hc7f5e018gd7b4g4435gbb6cgeb93e734a6b2");
        IOCContainer.Instance.LocalizationFileParser.Value.TryCreateLocalizationStringById("h8adf3a2bg39e7g4d3eg848age44602c6d374");
        IOCContainer.Instance.LocalizationFileParser.Value.TryCreateLocalizationStringById("h613db7b3gdbb0g486bgb23dg262f954ee485");
        IOCContainer.Instance.LocalizationFileParser.Value.TryCreateLocalizationStringById("h3792f2adg928fg4c54g9ca7g49ec0eb72b95");
        IOCContainer.Instance.LocalizationFileParser.Value.TryCreateLocalizationStringById("h1b3f0071g7157g4605g9ae5g31d2325de2e8");
        IOCContainer.Instance.LocalizationFileParser.Value.TryCreateLocalizationStringById("hffffffffffffffffffffffffffffffffffff");
        IOCContainer.Instance.LocalizationFileParser.Value.TryCreateLocalizationStringById("h000000000000000000000000000000000000");
    }
}