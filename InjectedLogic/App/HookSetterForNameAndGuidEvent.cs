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
// This hook fires every couple of seconds when the game processes another bunch of names + guids.
public unsafe class HookSetterForNameAndGuidEvent
{
    private static IIOProvider ioProvider;
    
    private static IAsmHook? asmHook;
    private static delegate* unmanaged[Cdecl]<IntPtr, void> nativeFunctionPointerForHookForNameAndGuidEvent;
    private static NativeStringWrapper tempNativeStringWrapperToCheckIfHasMetBefore;
    private static byte[] tempStrBytes;
    
    public HookSetterForNameAndGuidEvent(IIOProvider ioProvider)
    {
        HookSetterForNameAndGuidEvent.ioProvider = ioProvider;
    }
    
    public void SetHook(Scanner scanner, IntPtr baseAddress)
    {
        PatternScanResult pointerOfTargetFunctionBody = scanner.FindPattern("8B 4F 18 FF C3 FF C5 3B D9 0F 82 71 FF FF FF 44 01 BE DC 03 00 00 4C 8B 7C 24 20 4C 8B 74 24 50 48 8B 6C 24 48 48 8B 5C 24 40 48 83 C4 28 5F 5E C3");   // EoCApp.exe+1D7D060

        nativeFunctionPointerForHookForNameAndGuidEvent = (delegate* unmanaged[Cdecl]<IntPtr, void>)&HookForNameAndGuidEvent;
        ioProvider.WriteLine("nativeFunctionPointerForHookForNameAndGuidEvent: " + ((IntPtr)nativeFunctionPointerForHookForNameAndGuidEvent).ToString("x"));
        
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
            
            $"mov {_ecx}, {_edx}",     // argument
            $"mov {_eax}, {(IntPtr)nativeFunctionPointerForHookForNameAndGuidEvent}",
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

        tempNativeStringWrapperToCheckIfHasMetBefore = new NativeStringWrapper();
        NativeStringWrapper.UnsafeActions.SetIsDisposed(tempNativeStringWrapperToCheckIfHasMetBefore, false);
        tempStrBytes = new byte[128];
    }
    
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HookForNameAndGuidEvent(IntPtr pointer)
    {
        try
        {
            if (HooksSetter.AreAllHooksSet == false) return;

            bool isLogEnabled = HooksSetter.IsLogEnabled;
            bool isLogForNameAndGuidEventTrashEnabled = HooksSetter.IsLogForNameAndGuidEventTrashEnabled;

            if (isLogEnabled)
            {
                ioProvider.WriteLineSynchronizedWithoutWaiting(Environment.NewLine + "HookForNameAndGuidEvent" + 
                                                               Environment.NewLine + pointer.ToString("x"));
            }

            int strLength = CommonHelpers.GetNativeStringLength(pointer);
            
            #region String validity checks
            if (strLength == 0)
            {
                if (isLogEnabled && isLogForNameAndGuidEventTrashEnabled) ioProvider.WriteLineSynchronizedWithoutWaiting("The pointer was pointing to an empty string");
                return;
            }

            if (strLength < 37)    // GUID usually has 36 symbols
            {
                if (isLogEnabled && isLogForNameAndGuidEventTrashEnabled) ioProvider.WriteLineSynchronizedWithoutWaiting("The pointer did not point to a name and GUID, so it was skipped");
                return;
            }

            byte* strBytesPointer = (byte*)pointer;

            string nullStr = "NULL";
            int nullStrLength = nullStr.Length;
            int counterForNullStrComparison;
            for (counterForNullStrComparison = 0; counterForNullStrComparison < nullStrLength; counterForNullStrComparison++)
            {
                byte currentByte = strBytesPointer[counterForNullStrComparison];
                if (nullStr[counterForNullStrComparison] == currentByte) continue;
                break;
            }
            if (counterForNullStrComparison == nullStrLength)
            {
                if (isLogEnabled && isLogForNameAndGuidEventTrashEnabled) ioProvider.WriteLineSynchronizedWithoutWaiting("The pointer did not point to a name and GUID, so it was skipped");
                return;
            }
            
            bool isHyphenFound = false;
            bool isUnderscoreFound = false;
            int lastIndexOfHyphen = -1;
            int lastIndexOfUnderscore = -1;
            for (int i = 0; i < strLength; i++)
            {
                var currentChar = (char)strBytesPointer[i];
                if (isHyphenFound && isUnderscoreFound) break;
                if (currentChar == '-')
                {
                    isHyphenFound = true;
                    lastIndexOfHyphen = i;
                }

                if (currentChar == '_')
                {
                    isUnderscoreFound = true;
                    lastIndexOfUnderscore = i;
                }
            }
            
            // Example of a correct GUID: S_TUT_CargoHold_UnrulyPrisoner_d7a61dc6-a249-4e97-9914-53ea24e320ae
            if (isHyphenFound == false || isUnderscoreFound == false || lastIndexOfHyphen < lastIndexOfUnderscore)
            {
                if (isLogEnabled && isLogForNameAndGuidEventTrashEnabled) ioProvider.WriteLineSynchronizedWithoutWaiting("The pointer did not point to a name and GUID, so it was skipped");
                return;
            }
            
            Array.Clear(tempStrBytes);  // it's not important, just for convenience
            CommonHelpers.EnsureArrayLength(ref tempStrBytes, strLength);
            CommonHelpers.CopyBytePointerDataToArray(strBytesPointer, tempStrBytes, strLength);
            
            NativeStringWrapper.UnsafeActions.SetNewArrayAndLength(tempNativeStringWrapperToCheckIfHasMetBefore, tempStrBytes, strLength);
            if (DataToSendSyncKeeper.IsNewNameAndGuid(new HolderForNativeStringWrapperWithoutNeedForDisposal(tempNativeStringWrapperToCheckIfHasMetBefore)) == false)
            {
                if (isLogEnabled && isLogForNameAndGuidEventTrashEnabled) ioProvider.WriteLineSynchronizedWithoutWaiting("This name and Guid are not new and already cached, so they are skipped");
                return;
            }
            #endregion

            var strBytes = new byte[strLength];
            Array.Copy(tempStrBytes, strBytes, strLength);
            NativeStringWrapper nameAndGUID = new NativeStringWrapper().RestoreDisposedObject(strBytes, strLength, false);
            
            if (isLogEnabled)
            {
                ioProvider.WriteLineSynchronizedWithoutWaiting("Name and GUID: " + nameAndGUID);
                HooksSetter.LogSpanBytesSynchronizedWithoutWaiting(nameAndGUID.NativeStringSpan);
            }
            
            DataToSendSyncKeeper.AddNameAndGuidToListForSending(new HolderForNativeStringWrapperWithoutNeedForDisposal(nameAndGUID));
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