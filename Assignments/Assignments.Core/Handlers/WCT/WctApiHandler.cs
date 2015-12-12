﻿using System;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Runtime;
using Assignments.Core.Model;
using Assignments.Core.Handlers.WCT;

namespace Assignments.Core.Handlers.WCT
{
    /// <summary>
    /// //https://msdn.microsoft.com/en-us/library/cc308564.aspx
    /// about: https://msdn.microsoft.com/en-us/library/windows/desktop/ms681622(v=vs.85).aspx
    /// use: https://msdn.microsoft.com/en-us/library/windows/desktop/ms681418(v=vs.85).aspx
    /// </summary>
    public class WctApiHandler
    {
        //Consts doc:
        //http://winappdbg.sourceforge.net/doc/v1.4/reference/winappdbg.win32.advapi32-module.html
        const int WCT_MAX_NODE_COUNT = 16;
        const uint WCTP_GETINFO_ALL_FLAGS = 7;

       
        internal ThreadWaitInfo CollectWaitInformation(ClrThread thread)
        {
            ThreadWaitInfo result = null;

            //OpenThreadChainFlags.WCT_ASYNC_OPEN_FLAG
            var g_WctIntPtr = OpenThreadWaitChainSession(0, 0);

            uint threadID = thread.OSThreadId;

            WAITCHAIN_NODE_INFO[] NodeInfoArray = new WAITCHAIN_NODE_INFO[WCT_MAX_NODE_COUNT];


            int isCycle = 0;
            int count = WCT_MAX_NODE_COUNT;

            // Make a synchronous WCT call to retrieve the wait chain.
            bool waitChainResult = GetThreadWaitChain(g_WctIntPtr,
                                    IntPtr.Zero,
                                    WCTP_GETINFO_ALL_FLAGS,
                                    threadID, ref count, NodeInfoArray, out isCycle);

            if (waitChainResult)
            {
                result = new ThreadWaitInfo(thread);
                for (int i = 0; i < count; i++)
                {
                    result.AddInfo(NodeInfoArray[i]);
                }
            }
            else
            {
                var lastErrorCode = GetLastError();
                //TODO : Ifdentify code error and responce accordingly
            }

            //Finaly ...
            CloseThreadWaitChainSession(g_WctIntPtr);
            return result;
        }

        #region External Advapi32 calls

        /// <summary>
        ///  Original Doc: https://msdn.microsoft.com/en-us/library/windows/desktop/ms679360(v=vs.85).aspx
        ///  System errr codes: https://msdn.microsoft.com/en-us/library/windows/desktop/ms681381(v=vs.85).aspx
        /// </summary>
        /// <returns>The return value is the calling thread's last-error code.</returns>
        [DllImport("Kernel32.dll")]
        public static extern UInt32 GetLastError();


        /// <summary>
        /// Original doc: https://msdn.microsoft.com/en-us/library/windows/desktop/ms679282(v=vs.85).aspx
        /// </summary>
        /// <param name="WctIntPtr">A IntPtr to the WCT session created by the OpenThreadWaitChainSession function.</param>
        [DllImport("Advapi32.dll")]
        public static extern void CloseThreadWaitChainSession(IntPtr WctIntPtr);



        /// <summary>
        /// Original Doc : https://msdn.microsoft.com/en-us/library/windows/desktop/ms680543(v=vs.85).aspx
        /// </summary>
        /// <param name="Flags">The session type. This parameter can be one of the following values. (OpenThreadChainFlags)</param>
        /// <param name="callback">If the session is asynchronous, this parameter can be a pointer to a WaitChainCallback callback function.
        /// </param>
        /// <returns>If the function succeeds, the return value is a IntPtr to the newly created session. If the function fails, the return value is NULL.To get extended error information, call GetLastError.
        //</returns>
        [DllImport("Advapi32.dll")]
        public static extern IntPtr OpenThreadWaitChainSession(UInt32 Flags, UInt32 callback);


        /// <summary>
        /// Original doc: https://msdn.microsoft.com/en-us/library/windows/desktop/ms680564(v=vs.85).aspx
        /// </summary>
        /// <param name="CallStateCallback">The address of the CoGetCallState function.</param>
        /// <param name="ActivationStateCallback">The address of the CoGetActivationState function.</param>
        [DllImport("Advapi32.dll")]
        public static extern void RegisterWaitChainCOMCallback(UInt32 CallStateCallback, UInt32 ActivationStateCallback);


        /// <summary>
        /// Original Doc : https://msdn.microsoft.com/en-us/library/windows/desktop/ms679364(v=vs.85).aspx
        /// </summary>
        /// <param name="WctIntPtr"></param>
        /// <param name="Context"></param>
        /// <param name="flags"></param>
        /// <param name="ThreadId"></param>
        /// <param name="NodeCount"></param>
        /// <param name="NodeInfoArray"></param>
        /// <param name="IsCycle"></param>
        /// <returns></returns>
        [DllImport("Advapi32.dll")]
        public static extern bool GetThreadWaitChain(
            IntPtr WctIntPtr,
            IntPtr Context,
            UInt32 Flags,
            uint ThreadId,
            ref int NodeCount,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)]
            [In, Out]
            WAITCHAIN_NODE_INFO[] NodeInfoArray,
            out int IsCycle
        );


        /// <summary>
        /// Original Doc: https://msdn.microsoft.com/en-us/library/windows/desktop/ms681421(v=vs.85).aspx
        /// </summary>
        /// <param name="WctIntPtr">A IntPtr to the WCT session created by the OpenThreadWaitChainSession function.</param>
        /// <param name="Context">A optional pointer to an application-defined context structure specified by the GetThreadWaitChain function.</param>
        /// <param name="CallbackStatus">The callback status. This parameter can be one of the following values, or one of the other system </param>
        /// <param name="NodeCount">The number of nodes retrieved, up to WCT_MAX_NODE_COUNT. If the array cannot contain all the nodes of the wait chain, the function fails, CallbackStatus is ERROR_MORE_DATA, and this parameter receives the number of array elements required to contain all the nodes.</param>
        /// <param name="NodeInfoArray">An array of WAITCHAIN_NODE_INFO structures that receives the wait chain.</param>
        /// <param name="IsCycle">If the function detects a deadlock, this variable is set to TRUE; otherwise, it is set to FALSE.</param>
        [DllImport("Advapi32.dll")]
        public static extern void WaitChainCallback(
           IntPtr WctIntPtr,
           UInt32 Context,
           UInt32 CallbackStatus,
           UInt32 NodeCount,
           UInt32 NodeInfoArray,
           UInt32 IsCycle
        );

        #endregion
    }
}
