using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Reloaded.Hooks;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using static InjectedLogic.CommonHelpers.ASMHelpers;

namespace InjectedLogic;

// For additional shared comments see the HooksSetter class
// This hook fires at the beginning of a dialog and also when the user switches a dialog (i.e. clicks on a dialog option)
public unsafe class HookSetterForNextDialogState
{
    private static IIOProvider ioProvider;
    
    private static IAsmHook? asmHook;
    private static delegate* unmanaged[Cdecl]<void> nativeFunctionPointerForHookForNextDialogState;
    
    public HookSetterForNextDialogState(IIOProvider ioProvider)
    {
        HookSetterForNextDialogState.ioProvider = ioProvider;
    }
    
    public void SetHook(Scanner scanner, IntPtr baseAddress)
    {
        PatternScanResult pointerOfTargetFunctionBody = scanner.FindPattern("48 8B 5C 24 50 40 0F B6 C7 48 8B 74 24 58 4C 8B 7C 24 60 48 83 C4 30 41 5E 5F 5D C3 40 0F B6 C7 EB E7 40 0F B6 C7 48 83 C4 30 41 5E 5F 5D C3");   // EoCApp.exe+15D0BB6 
        
        nativeFunctionPointerForHookForNextDialogState = (delegate* unmanaged[Cdecl]<void>)&HookForNextDialogState;
        ioProvider.WriteLine("nativeFunctionPointerForHookForNextDialogState: " + ((IntPtr)nativeFunctionPointerForHookForNextDialogState).ToString("x"));
        
        string[] functionsBytesToInject = 
        {
            $"{_use32}",
            $"push {_eax}",
            $"push {_ebx}",
            $"push {_ecx}",
            $"push {_edx}",
            $"push {_esi}",
            $"push {_edi}",
            $"push {_r8}",
            $"push {_r9}",
            $"push {_r10}",
            $"push {_r11}",
            $"push {_r12}",
            $"push {_r13}",
            $"push {_r14}",
            $"push {_r15}",
            $"sub {_esp}, 32",
            
            $"mov {_eax}, {(IntPtr)nativeFunctionPointerForHookForNextDialogState}",
            $"call {_eax}",
    
            $"add {_esp}, 32",
            $"pop {_r15}",
            $"pop {_r14}",
            $"pop {_r13}",
            $"pop {_r12}",
            $"pop {_r11}",
            $"pop {_r10}",
            $"pop {_r9}",
            $"pop {_r8}",
            $"pop {_edi}",
            $"pop {_esi}",
            $"pop {_edx}",
            $"pop {_ecx}",
            $"pop {_ebx}",
            $"pop {_eax}",
        };

        var tempAddr = baseAddress + pointerOfTargetFunctionBody.Offset;
        asmHook = ReloadedHooks.Instance.CreateAsmHook(functionsBytesToInject, (long)tempAddr, AsmHookBehaviour.ExecuteFirst).Activate();
    }
    
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HookForNextDialogState()
    {
        try
        {
            if (HooksSetter.AreAllHooksSet == false) return;

            bool isLogEnabled = HooksSetter.IsLogEnabled;
            
            if (isLogEnabled) ioProvider.WriteLineSynchronizedWithoutWaiting(Environment.NewLine + "HookForNextDialogState");
            DataToSendSyncKeeper.SetDialogPhase(DialogPhase.NextDialogStateFired);
        }
        catch (Exception e)
        {
            ioProvider.WriteErrorLine(Environment.NewLine + 
                                 "================ INJECTED LOGIC ERROR ================" + 
                                 Environment.NewLine + 
                                 e + 
                                 Environment.NewLine + 
                                 "======================================================");
        }
    }
}