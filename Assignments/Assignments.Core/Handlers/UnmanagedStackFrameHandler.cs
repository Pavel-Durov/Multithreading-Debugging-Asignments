﻿using System;
using System.Collections.Generic;
using System.Text;
using Assignments.Core.msos;
using Microsoft.Diagnostics.Runtime;
using Assignments.Core.Exceptions;
using Assignments.Core.Model.Unified;

namespace Assignments.Core.Handlers
{
    public class UnmanagedStackFrameHandler
    {
        public const string SINGLE_WAIT_FUNCTION_NAME = "WaitForSingleObject";
        public const string MULTI_WAIT_FUNCTION_NAME = "WaitForMultipleObjects";

        public static void SetParams(UnifiedStackFrame frame, ClrRuntime runtime)
        {
            List<byte[]> result = new List<byte[]>();
            if (CheckForWinApiCalls(frame, SINGLE_WAIT_FUNCTION_NAME))
            {
                result = GetSingleStackFrameParams(frame, runtime);
            }
            else if (CheckForWinApiCalls(frame, MULTI_WAIT_FUNCTION_NAME))
            {
                result = GetMultipleStackFrameParams(frame, runtime);
            }

            frame.NativeParams = result;
        }

        public static bool CheckForWinApiCalls(UnifiedStackFrame c, string key)
        {
            bool result = c != null
                && !String.IsNullOrEmpty(c.Method)
                && c.Method != null && c.Method.Contains(key);

            return result;
        }



        public static List<byte[]> GetMultipleStackFrameParams(UnifiedStackFrame frame, ClrRuntime runtime)
        {
            List<byte[]> result = new List<byte[]>();
            var nativeParams = GetNativeParams(frame, runtime, 4);

            if (nativeParams != null && nativeParams.Count > 0)
            {

                var HandlesCunt = BitConverter.ToUInt32(nativeParams[0], 0);
                var HandleAddress = BitConverter.ToUInt32(nativeParams[1], 0);
                var WaitallFlag = BitConverter.ToUInt32(nativeParams[2], 0);
                var Timeout = BitConverter.ToUInt32(nativeParams[3], 0);

                result = ReadFromMemmory(HandleAddress, HandlesCunt, runtime);
            }
            return result;
        }


        public static List<byte[]> GetSingleStackFrameParams(UnifiedStackFrame frame, ClrRuntime runtime)
        {

            var nativeParams = GetNativeParams(frame, runtime, 2);

            if (nativeParams != null && nativeParams.Count > 0)
            {
                //Handle Address 
                var HandleAddress = BitConverter.ToUInt32(nativeParams[0], 0);
                //Timeout Param
                var Timeout = BitConverter.ToUInt32(nativeParams[1], 0);
            }

            return nativeParams;
        }


        /// <summary>
        /// Iterates paramCount times and reads the value from memmory using runtime.ReadMemory function
        /// </summary>
        /// <param name="stackFrame"></param>
        /// <param name="runtime"></param>
        /// <param name="paramCount">Number of params of the passed stackFrame</param>
        /// <returns></returns>
        public static List<byte[]> GetNativeParams(UnifiedStackFrame stackFrame, ClrRuntime runtime, int paramCount)
        {
            List<byte[]> result = new List<byte[]>();

            var offset = stackFrame.FrameOffset; //Base Pointer - % EBP
            byte[] paramBuffer;
            int bytesRead = 0;
            offset += 4;

            for (int i = 0; i < paramCount; i++)
            {
                paramBuffer = new byte[4];
                offset += 4;
                if (runtime.ReadMemory(offset, paramBuffer, 4, out bytesRead))
                {
                    result.Add(paramBuffer);
                }
            }

            return result;
        }


        /// <summary>
        /// Reads 'count' times from the address using runtime.ReadMemory function 
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="handlesCunt"></param>
        /// <param name="runtime"></param>
        /// <returns>Array with memmory fetched parameters</returns>
        public static List<byte[]> ReadFromMemmory(uint startAddress, uint count, ClrRuntime runtime)
        {
            List<byte[]> result = new List<byte[]>();
            int sum = 0;
            //TODO: Check if dfor can be inserted into the REadMemmory result (seems to be..)
            for (int i = 0; i < count; i++)
            {
                byte[] readedBytes = new byte[4];
                if (runtime.ReadMemory(startAddress, readedBytes, 4, out sum))
                {
                    result.Add(readedBytes);
                }
                else
                {
                    throw new AccessingNonReadableMemmory(string.Format("Accessing Unreadable memorry at {0}", startAddress));
                }
                //Advancing the pointer by 4 (32-bit system)
                startAddress += 4;
            }
            return result;
        }

    }
}
