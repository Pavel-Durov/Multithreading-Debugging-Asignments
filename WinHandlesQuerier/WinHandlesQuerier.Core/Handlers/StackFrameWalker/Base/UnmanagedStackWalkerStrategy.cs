﻿using WinHandlesQuerier.Core.Infra;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interop;
using System;
using System.Linq;
using System.Collections.Generic;
using WinHandlesQuerier.Core.Exceptions;
using WinHandlesQuerier.Core.Handlers;
using WinHandlesQuerier.Core.Model.Unified;
using WinHandlesQuerier.Core.msos;

namespace WinHandlesQuerier.Core.Handlers.UnmanagedStackFrame.Strategies.Base
{
    internal abstract class UnmanagedStackWalkerStrategy
    {
        #region Constants

        public const string WAIT_FOR_SINGLE_OBJECTS_FUNCTION_NAME = "WaitForSingleObject";
        public const string WAIT_FOR_MULTIPLE_OBJECTS_FUNCTION_NAME = "WaitForMultipleObjects";
        public const string ENTER_CRITICAL_SECTION_FUNCTION_NAME = "EnterCriticalSection";

        protected const int ENTER_CRITICAL_SECTION_FUNCTION_PARAM_COUNT = 1;
        protected const int WAIT_FOR_SINGLE_OBJECT_PARAM_COUNT = 2;
        protected const int WAIT_FOR_MULTIPLE_OBJECTS_PARAM_COUNT = 4;

        #endregion

        private List<byte[]> ReadFromMemmory(ulong startAddress, uint count, ClrRuntime runtime)
        {
            List<byte[]> result = new List<byte[]>();
            int sum = 0;
            //TODO: Check if dfor can be inserted into the REadMemmory result (seems to be..)
            for (int i = 0; i < count; i++)
            {
                byte[] readedBytes = new byte[IntPtr.Size];
                if (runtime.ReadMemory(startAddress, readedBytes, IntPtr.Size, out sum))
                {
                    result.Add(readedBytes);
                }
                else
                {
                    throw new AccessingNonReadableMemmory(string.Format("Accessing Unreadable memorry at {0}", startAddress));
                }
                //Advancing the pointer by 4 (32-bit system)
                startAddress += (ulong)IntPtr.Size;
            }
            return result;
        }


        internal List<UnifiedStackFrame> ConvertToUnified(IEnumerable<DEBUG_STACK_FRAME> stackFrames,
            ClrRuntime runtime, IDebugClient debugClient, ThreadInfo info, uint pid = Constants.INVALID_PID)
        {
            bool waitFound = false;
            var reversed = stackFrames.Reverse();
            List<UnifiedStackFrame> stackTrace = new List<UnifiedStackFrame>();

            foreach (var frame in reversed)
            {
                var unified_frame = new UnifiedStackFrame(frame, (IDebugSymbols2)debugClient);
                unified_frame.ThreadContext = info.ContextStruct;

                if (!waitFound)
                {
                    waitFound = Inpsect(unified_frame, runtime, pid);
                }

                stackTrace.Add(unified_frame);
            }

            return stackTrace;
        }

        internal bool CheckForWinApiCalls(UnifiedStackFrame frame, string key)
        {
            bool result = frame != null
                && !String.IsNullOrEmpty(frame.Method)
                && frame.Method != null && frame.Method.Contains(key);

            return result;
        }

        internal bool GetCriticalSectionBlockingObject(UnifiedStackFrame frame, ClrRuntime runtime, out UnifiedBlockingObject blockingObject)
        {
            bool result = false;

            if (frame.Handles != null)
            {
                blockingObject = GetCriticalSectionBlockingObject(frame, runtime);
                result = blockingObject != null;
            }
            else
            {
                blockingObject = null;
            }

            return result;
        }




        protected bool Inpsect(UnifiedStackFrame frame, ClrRuntime runtime, uint pid)
        {
            bool waitCallFound = false;

            if (CheckForWinApiCalls(frame, WAIT_FOR_SINGLE_OBJECTS_FUNCTION_NAME))
            {
                DealWithSingle(frame, runtime, pid);
            }
            else if (waitCallFound = CheckForWinApiCalls(frame, WAIT_FOR_MULTIPLE_OBJECTS_FUNCTION_NAME))
            {
                DealWithMultiple(frame, runtime, pid);
            }
            else if(CheckForWinApiCalls(frame, ENTER_CRITICAL_SECTION_FUNCTION_NAME))
            {
               DealWithCriticalSection(frame, runtime, pid);
            }

            return waitCallFound;
        }

        protected UnifiedHandle GenerateUnifiedHandle(ulong handleUint, uint pid)
        {
            UnifiedHandle result;

            if (pid != Constants.INVALID_PID)
            {
                var typeName = NtQueryHandler.GetHandleType((IntPtr)handleUint, pid);
                var handleName = NtQueryHandler.GetHandleObjectName((IntPtr)handleUint, pid);

                result = new UnifiedHandle(handleUint, typeName, handleName);
            }
            else
            {
                result = new UnifiedHandle(handleUint);
            }
            return result;
        }

        protected void EnrichUnifiedStackFrame(UnifiedStackFrame frame, ulong handle, uint pid)
        {
            UnifiedHandle unifiedHandle = GenerateUnifiedHandle(handle, pid);
            if (unifiedHandle != null)
            {
                frame.Handles = new List<UnifiedHandle>();
                frame.Handles.Add(unifiedHandle);
            }
        }

        protected void EnrichUnifiedStackFrame(UnifiedStackFrame frame, ClrRuntime runtime, uint pid, ulong waitCount, ulong hPtr)
        {
            if (waitCount > Kernel32.Const.MAXIMUM_WAIT_OBJECTS)
            {
                throw new InvalidOperationException($"waitCount exceeded MAXIMUM_WAIT_OBJECTS :{ waitCount }");
            }

            var handles = ReadFromMemmory(hPtr, (uint)waitCount, runtime);

            frame.Handles = new List<UnifiedHandle>();
            foreach (var handle in handles)
            {
                ulong handleUint = Convert(handle);

                UnifiedHandle unifiedHandle = GenerateUnifiedHandle(handleUint, pid);

                if (unifiedHandle != null)
                {
                    frame.Handles.Add(unifiedHandle);
                }
            }
        }

        protected ulong Convert(byte[] bits)
        {
            return BitConverter.ToUInt64(bits, 0);
        }



        #region Abstract Methods

        protected abstract void DealWithSingle(UnifiedStackFrame frame, ClrRuntime runtime, uint pid);

        protected abstract void DealWithMultiple(UnifiedStackFrame frame, ClrRuntime runtime, uint pid);
        protected abstract void DealWithCriticalSection(UnifiedStackFrame frame, ClrRuntime runtime, uint pid);

        protected abstract UnifiedBlockingObject GetCriticalSectionBlockingObject(UnifiedStackFrame frame, ClrRuntime runtime);

        #endregion

    }
}
