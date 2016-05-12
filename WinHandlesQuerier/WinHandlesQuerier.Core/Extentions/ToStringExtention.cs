﻿using WinHandlesQuerier.Core.Handlers;
using WinHandlesQuerier.Core.Model.Unified;
using WinHandlesQuerier.Core.Model.Unified.Thread;
using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinHandlesQuerier.Core.Extentions
{
    public static class ToStringExtention
    {
        public static string AsString(this UnifiedThread thread)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendWithNewLine($"OSThreadId : {thread.OSThreadId}");
            sb.AppendWithNewLine($"IsManagedThreadManaged: {thread.IsManagedThread }");
            sb.AppendWithNewLine($"Detail: {thread.Detail}");
            sb.AppendWithNewLine($"EngineThreadId: {thread.EngineThreadId}");

            return sb.ToString();
        }

        #region Blocking objects

        public static string AsString(this List<UnifiedBlockingObject> blockingObjects, char prefix)
        {
            StringBuilder sb = new StringBuilder();


            var itemPrefix = $"{prefix}{prefix}";

            for (int i = 0; i < blockingObjects.Count; i++)
            {
                sb.AppendWithNewLine($"{Environment.NewLine}{prefix}blockingObject: {i}");
                var loopObj = blockingObjects[i];
                sb.Append(loopObj.AsString(itemPrefix));
            }

            return sb.ToString();
        }

        public static string AsString(this UnifiedBlockingObject blockingObject, string prefix)
        {
            StringBuilder sb = new StringBuilder();
            if(blockingObject.Handle != 0)
            {
                sb.AppendWithNewLine(String.Format("{0}Hadle : 0x{1:x}", prefix, blockingObject.Handle));
            }
            sb.AppendWithNewLine($"{prefix}Source : {blockingObject.Origin}");
            sb.AppendWithNewLine($"{prefix}ManagedObjectAddress : {blockingObject.ManagedObjectAddress}");
            if (!String.IsNullOrEmpty(blockingObject.KernelObjectName))
            {
                sb.AppendWithNewLine($"{prefix}KernelObjectName: {blockingObject.KernelObjectName}");
            }
            
            sb.AppendWithNewLine($"{prefix}KernelObjectTypeName: {blockingObject.KernelObjectTypeName}");
            sb.AppendWithNewLine($"{prefix}WaitReason: {blockingObject.WaitReason}");

            //TODO: Complete the info
            return sb.ToString();
        }
        #endregion


        #region StackTrace

        public static String AsString(this List<UnifiedStackFrame> list, char prefix)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var frame in list)
            {
                sb.AppendWithNewLine(frame.AsString(prefix));
            }

            return sb.ToString();
        }

        public static String AsString(this UnifiedStackFrame frame, char prefix)
        {
            StringBuilder result = new StringBuilder();

            if (frame.Type == UnifiedStackFrameType.Special)
            {
                result.Append(String.Format("{0}{1,-10}", prefix, "Special"));
                return result.ToString();
            }
            if (String.IsNullOrEmpty(frame.SourceFileName))
            {
                result.Append(String.Format("{0}{1,-10} {2,-20:x16} {3}!{4}+0x{5:x}",
                    prefix, frame.Type, frame.InstructionPointer,
                    frame.Module, frame.Method, frame.OffsetInMethod));
            }
            else
            {
                result.Append(String.Format("{0}{1,-10} {2,-20:x16} {3}!{4} [{5}:{6},{7}]",
                    prefix, frame.Type, frame.InstructionPointer,
                    frame.Module, frame.Method, frame.SourceFileName,
                    frame.SourceLineNumber, frame.SourceColumnNumber));
            }

            if (frame.Type == UnifiedStackFrameType.Native)
            {
               
                if(frame.Handles != null && frame.Handles.Count > 0)
                {
                    result.Append($"{prefix}{frame.Handles.AsString(prefix)}");
                }
                else
                { 
                    result.Append($"{prefix}{frame.NativeParams.AsString(prefix)}");
                }

            }

            return result.ToString();
        }

        #endregion

        public static String AsString(this List<uint> parms, char prefix)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            sb.Append(prefix);

            for (int i = 0; i < parms.Count; i++)
            {
                var msg = String.Format("[{0}]= 0x{1:x} ", i, parms[i]);
                sb.Append(msg);
            }

            return sb.ToString();
        }

        public static String AsString(this List<UnifiedHandle> handles, char prefix)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            
            int index = 0;
            foreach (var item in handles)
            {
                sb.Append(prefix);
                var msg = String.Format("{0}: HandleId = 0x{1:x}, HandleType = {2}, HandleObjectName = {3}", index, item.Id, item.Type, item.ObjectName);
                sb.AppendWithNewLine(msg);
                index++;
            }

            return sb.ToString();
        }

        public static String AsString(this List<byte[]> parms, char prefix)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            sb.Append(prefix);
            
            for (int i = 0; i < parms.Count; i++)
            {
                int byteValue = BitConverter.ToInt32(parms[i], 0);

                var msg = String.Format("[{0}]= 0x{1:x} ", i, byteValue);
                sb.Append(msg);
            }

            return sb.ToString();
        }

        private static void PrintBytesAsHex(ConsoleColor color, List<byte[]> parms)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < parms.Count; i++)
            {
                int byteValue = BitConverter.ToInt32(parms[i], 0);

                var msg = String.Format("p{0}= 0x{1:x}", i, byteValue);
                sb.Append(msg);
            }

            Console.WriteLine(sb.ToString());
        }

    }
}