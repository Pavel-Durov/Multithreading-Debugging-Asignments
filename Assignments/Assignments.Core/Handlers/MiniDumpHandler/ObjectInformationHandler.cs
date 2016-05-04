﻿using Assignments.Core.WinApi;
using System;
using System.Collections.Generic;
using Assignments.Core.Model.MiniDump;



namespace Assignments.Core.Handlers.MiniDump
{
    public class ObjectInformationHandler
    {
        public static unsafe DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION DealWithHandleInfo(DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION pObjectInfo, MiniDumpHandle handle, uint address, IntPtr baseOfView)
        {
            Action<MiniDumpHandle, uint> action = null;
            if(_actions.TryGetValue(pObjectInfo.InfoType, out action))
            {
                action(handle, address);

                if (pObjectInfo.NextInfoRva == 0)
                {
                    return default(DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION);
                }
                else
                {
                    pObjectInfo = StreamHandler.ReadStruct<DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION>((uint)baseOfView + pObjectInfo.NextInfoRva);
                }
            }
          
            return pObjectInfo;
        }

        static ObjectInformationHandler()
        {
            _actions = new Dictionary<DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION_TYPE, Action<MiniDumpHandle, uint>>();

            _actions[DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION_TYPE.MiniHandleObjectInformationNone] = (handle, address) => { handle.Type = MiniDumpHandleType.NONE; };

            _actions[DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION_TYPE.MiniThreadInformation1] = SetMiniThreadInformation1; 

            _actions[DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION_TYPE.MiniMutantInformation1] = SetMiniMutantInformation1;

            _actions[DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION_TYPE.MiniMutantInformation2] = SetMiniMutantInformation2;

            _actions[DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION_TYPE.MiniProcessInformation1] = SetMiniProcessInformation1;

            _actions[DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION_TYPE.MiniProcessInformation2] = SetMiniProcessInformation2;

            _actions[DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION_TYPE.MiniEventInformation1] = (handle, address) => { handle.Type = MiniDumpHandleType.EVENT; };

            _actions[DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION_TYPE.MiniSectionInformation1] = (handle, address) => { handle.Type = MiniDumpHandleType.SECTION; };

            _actions[DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION_TYPE.MiniHandleObjectInformationTypeMax] = (handle, address) => { handle.Type = MiniDumpHandleType.TYPE_MAX; };

        }

        static Dictionary<DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION_TYPE, Action<MiniDumpHandle, uint>> _actions;
        Action<MiniDumpHandle, uint> HandleInfoTypeAction;

        #region Actions

        private static unsafe void SetMiniProcessInformation2(MiniDumpHandle handle, uint address)
        {
            handle.Type = MiniDumpHandleType.PROCESS2;
            PROCESS_ADDITIONAL_INFO_2* pInfo = (PROCESS_ADDITIONAL_INFO_2*)(((char*)address) + sizeof(DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION));

            handle.OwnerProcessId = pInfo->ProcessId;
            handle.OwnerThreadId = 0;
        }

        private static void SetMiniProcessInformation1(MiniDumpHandle handle, uint address)
        {
            handle.Type = MiniDumpHandleType.PROCESS1;
        }

        private static unsafe void SetMiniMutantInformation2(MiniDumpHandle handle, uint address)
        {
            handle.Type = MiniDumpHandleType.MUTEX2;

            MUTEX_ADDITIONAL_INFO_2* mutexInfo2 = (MUTEX_ADDITIONAL_INFO_2*)(((char*)address) + sizeof(DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION));

            handle.OwnerProcessId = mutexInfo2->OwnerProcessId;
            handle.OwnerThreadId = mutexInfo2->OwnerThreadId;
        }

        private static unsafe void SetMiniMutantInformation1(MiniDumpHandle handle, uint address)
        {
            handle.Type = MiniDumpHandleType.MUTEX1;

            MUTEX_ADDITIONAL_INFO_1* mutexInfo1 = (MUTEX_ADDITIONAL_INFO_1*)(((char*)address) + sizeof(DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION));

            handle.MutexUnknown = new MutexUnknownFields()
            {
                Field1 = mutexInfo1->Unknown1,
                Field2 = mutexInfo1->Unknown2
            };
        }

        private static unsafe void SetMiniThreadInformation1(MiniDumpHandle handle, uint address)
        {
            handle.Type = MiniDumpHandleType.THREAD;

            THREAD_ADDITIONAL_INFO* threadInfo = (THREAD_ADDITIONAL_INFO*)(((char*)address) + sizeof(DbgHelp.MINIDUMP_HANDLE_OBJECT_INFORMATION));

            handle.OwnerProcessId = threadInfo->ProcessId;
            handle.OwnerThreadId = threadInfo->ThreadId;
        }

        #endregion

    }
}
