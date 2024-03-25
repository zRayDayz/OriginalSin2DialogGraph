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
// This hook fires at the beginning of a dialog, and then every time a user switches a dialog (i.e. clicks on a dialog option)
public unsafe class HookSetterForCurrentDialogOption
{
    private static IIOProvider ioProvider;
    
    private static IAsmHook? asmHook;
    private static delegate* unmanaged[Cdecl]<IntPtr, void> nativeFunctionPointerForHookForCurrentDialogOption;

    public HookSetterForCurrentDialogOption(IIOProvider ioProvider)
    {
        HookSetterForCurrentDialogOption.ioProvider = ioProvider;
    }
    
    public void SetHook(Scanner scanner, IntPtr baseAddress)
    {
        PatternScanResult pointerOfTargetFunctionBody = scanner.FindPattern("48 89 6C 24 48 81 F9 66 FD FF FF 8B E9 0F 44 E8 74 0F 85 ED 78 5D");   // EoCApp.exe+1AAC292
        
        nativeFunctionPointerForHookForCurrentDialogOption = (delegate* unmanaged[Cdecl]<IntPtr, void>)&HookForCurrentDialogOption;
        ioProvider.WriteLine("nativeFunctionPointerForHookForCurrentDialogOption: " + ((IntPtr)nativeFunctionPointerForHookForCurrentDialogOption).ToString("x"));
        
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

            // "rsp+0x60" - it is the stack frame of a different function that was called by 1 preceding function in the call stack, we can get the pointer to the current dialog option GUID from it
            // above we back up 14 registers, it is 112 bytes
            // then we subtract the pointer to the stack, which is an additional 32 bytes
            // now we need to calculate the new offset to our target dialog option GUID using all the offsets above, so it will be 112 + 32 + 96 (0x60) = 240 (0xF0)
            $"mov {_ecx}, [{_esp}+0xF0]", 
            $"mov {_eax}, {(IntPtr)nativeFunctionPointerForHookForCurrentDialogOption}",
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
    private static void HookForCurrentDialogOption(IntPtr pointer)
    {
        try
        {
            if (HooksSetter.AreAllHooksSet == false) return;

            bool isLogEnabled = HooksSetter.IsLogEnabled;
            var localLastDialogState = DataToSendSyncKeeper.GetDialogPhase();
            
            // This method is called only once during the current stage of the dialog, but just in case we prevent it from being called again until a user switches the dialog (i.e. clicks on a dialog option)
            if (localLastDialogState == DialogPhase.CurrentDialogOptionFired) return; 
            
            if (isLogEnabled)
            {
                ioProvider.WriteLineSynchronizedWithoutWaiting(Environment.NewLine + "HookForCurrentDialogOption" + 
                                                               Environment.NewLine + pointer.ToString("x"));
            }

            if ((long)pointer < 0x7f0000000000) return; // additional optional check that the argument is not a pointer
            
            NativeStringWrapper dialogOptionGuid = CommonHelpers.GetNativeString(pointer);
            if (dialogOptionGuid == null)
            {
                if (isLogEnabled) ioProvider.WriteLineSynchronizedWithoutWaiting("The pointer was pointing to an empty string");
                return;
            }

            if (isLogEnabled)
            {
                ioProvider.WriteLineSynchronizedWithoutWaiting("Current dialog option guid: " + dialogOptionGuid);
                HooksSetter.LogSpanBytesSynchronizedWithoutWaiting(dialogOptionGuid.NativeStringSpan);
            }
            
            DataToSendSyncKeeper.SetDialogPhase(DialogPhase.CurrentDialogOptionFired);

            DataToSendSyncKeeper.SetCurrentDialogOptionGuid(dialogOptionGuid);
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