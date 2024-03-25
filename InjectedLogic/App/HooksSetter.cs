using System.Diagnostics;
using System.Text;
using Reloaded.Memory.Sigscan;

namespace InjectedLogic;

// For an x64 application, the Microsoft x64 calling convention is used. So the "Cdecl" used for all pointer functions is irrelevant
// The hooks calling order: 1) HookSetterForDialogStartEventAndItsName 2) HookSetterForCurrentDialogOption 3) HookSetterForNextDialogState
public unsafe class HooksSetter
{
    private static SyncConfig syncConfig;
    private static IIOProvider ioProvider;
    
    private IntPtr baseAddress;
    private int exeSize;
    public static bool AreAllHooksSet { get; private set; }

    public static bool IsLogEnabled
    {
        get => syncConfig.IsHooksLogEnabled;
    }

    public static bool IsLogForNameAndGuidEventTrashEnabled
    {
        get => syncConfig.IsHooksTrashLogEnabled;
    }

    // These are static variables to ensure that they are never collected by the GC
    private static HookSetterForDialogStartEventAndItsName hookSetterForDialogStartEventAndItsName;
    private static HookSetterForCurrentDialogOption hookSetterForCurrentDialogOption;
    private static HookSetterForNextDialogState hookSetterForNextDialogState;
    private static HookSetterForNameAndGuidEvent hookSetterForNameAndGuidEvent;

    private static StringBuilder stringBuilder = new StringBuilder(128);

    public HooksSetter(ConfigManager configManager, IIOProvider ioProvider)
    {
        syncConfig = configManager.SyncConfig;
        HooksSetter.ioProvider = ioProvider;
    }
    
    public void SetHooks()
    {
        Process thisProcess = Process.GetCurrentProcess();
        baseAddress = thisProcess.MainModule.BaseAddress;
        exeSize = thisProcess.MainModule.ModuleMemorySize;
        
        using Scanner scanner = new Scanner((byte*)baseAddress, exeSize);

        hookSetterForDialogStartEventAndItsName = new HookSetterForDialogStartEventAndItsName(ioProvider);
        hookSetterForDialogStartEventAndItsName.SetHook(scanner, baseAddress);

        hookSetterForCurrentDialogOption = new HookSetterForCurrentDialogOption(ioProvider);
        hookSetterForCurrentDialogOption.SetHook(scanner, baseAddress);

        hookSetterForNextDialogState = new HookSetterForNextDialogState(ioProvider);
        hookSetterForNextDialogState.SetHook(scanner, baseAddress);

        hookSetterForNameAndGuidEvent = new HookSetterForNameAndGuidEvent(ioProvider);
        hookSetterForNameAndGuidEvent.SetHook(scanner, baseAddress);
        
        AreAllHooksSet = true;
    }

    public static void LogSpanBytesSynchronizedWithoutWaiting(ReadOnlySpan<byte> span)
    {
        stringBuilder.Clear();
        int i;
        for (i = 0; i < span.Length - 1; i++)
        {
            stringBuilder.Append(span[i].ToString("x"));
            stringBuilder.Append(", ");
        }
        stringBuilder.Append(span[i].ToString("x"));
        ioProvider.WriteLineSynchronizedWithoutWaiting(stringBuilder.ToString());
    }

}
