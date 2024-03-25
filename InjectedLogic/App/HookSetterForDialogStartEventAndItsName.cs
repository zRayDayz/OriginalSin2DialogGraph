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
// This hook fires when a new dialog is opened
public unsafe class HookSetterForDialogStartEventAndItsName
{
    private static IIOProvider ioProvider;
    
    private static IAsmHook? asmHook;
    private static delegate* unmanaged[Cdecl]<IntPtr, void> nativeFunctionPointerForHookForDialogStartEvent;
    
    public HookSetterForDialogStartEventAndItsName(IIOProvider ioProvider)
    {
        HookSetterForDialogStartEventAndItsName.ioProvider = ioProvider;
    }
    
    public void SetHook(Scanner scanner, IntPtr baseAddress)
    {
        PatternScanResult pointerOfTESTFUNCTION = scanner.FindPattern("48 8B 4B 10 48 8D 54 24 50 41 B9 01 00 00 00");   // EoCApp.exe+192C943
        
        nativeFunctionPointerForHookForDialogStartEvent = (delegate* unmanaged[Cdecl]<IntPtr, void>)&HookForDialogStartEventAndItsName;  // C#9 Function pointer
        ioProvider.WriteLine("nativeFunctionPointerForHookForDialogStartEvent: " + ((IntPtr)nativeFunctionPointerForHookForDialogStartEvent).ToString("x"));

        string[] functionsBytesToInject = 
        {
            $"{_use32}",
            $"push {_eax}",
            $"push {_ebx}",
            $"push {_ecx}",     // argument is stored here
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
            
            $"mov {_eax}, {(IntPtr)nativeFunctionPointerForHookForDialogStartEvent}",
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

        var tempAddr = baseAddress + pointerOfTESTFUNCTION.Offset;
        asmHook = ReloadedHooks.Instance.CreateAsmHook(functionsBytesToInject, (long)tempAddr, AsmHookBehaviour.ExecuteFirst).Activate();
    }
    
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HookForDialogStartEventAndItsName(IntPtr pointerToString)
    {
        try
        {
            if (HooksSetter.AreAllHooksSet == false) return;

            bool isLogEnabled = HooksSetter.IsLogEnabled;

            if (isLogEnabled)
            {
                ioProvider.WriteLineSynchronizedWithoutWaiting(Environment.NewLine + "HookForDialogStartEventAndItsName" + 
                                                               Environment.NewLine + pointerToString.ToString("x"));
            }

            NativeStringWrapper dialogName = CommonHelpers.GetNativeString(pointerToString);
            if (dialogName == null)
            {
                if (isLogEnabled) ioProvider.WriteLineSynchronizedWithoutWaiting("The pointer was pointing to an empty string");
                return;
            }
            
            if (isLogEnabled)
            {
                ioProvider.WriteLineSynchronizedWithoutWaiting("Dialog name: " + dialogName);
                HooksSetter.LogSpanBytesSynchronizedWithoutWaiting(dialogName.NativeStringSpan);
            }

            DataToSendSyncKeeper.SetDialogPhase(DialogPhase.Start);
            
            DataToSendSyncKeeper.SetDialogName(dialogName);

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