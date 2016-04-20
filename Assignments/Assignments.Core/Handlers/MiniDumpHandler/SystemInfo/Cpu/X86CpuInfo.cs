﻿using Assignments.Core.WinApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Assignments.Core.Handlers.MiniDumpHandler.SystemInfo.Cpu
{
    public class X86CpuInfo
    {
        private DbgHelp.CPU_INFORMATION _cpuInfo;
        private uint[] _vendorIdRaw;
        private string _vendorId;

        internal unsafe X86CpuInfo(DbgHelp.CPU_INFORMATION cpuInfo)
        {
            _cpuInfo = cpuInfo;

            _vendorIdRaw = new uint[3];
            _vendorIdRaw[0] = cpuInfo.VendorId[0];
            _vendorIdRaw[1] = cpuInfo.VendorId[1];
            _vendorIdRaw[2] = cpuInfo.VendorId[2];

            char[] vendorId = new char[12];

            GCHandle handle = GCHandle.Alloc(vendorId, GCHandleType.Pinned);

            try
            {
                ASCIIEncoding.ASCII.GetChars(cpuInfo.VendorId, 12, (char*)handle.AddrOfPinnedObject(), 12);
                _vendorId = new String(vendorId);
            }
            finally
            {
                handle.Free();
            }
        }

        public uint[] VendorIdRaw { get { return _vendorIdRaw; } }
        public string VendorId { get { return this._vendorId; } }
        public uint VersionInformation { get { return _cpuInfo.VersionInformation; } }
        public uint FeatureInformation { get { return _cpuInfo.FeatureInformation; } }
        public uint AMDExtendedCpuFeatures { get { return _cpuInfo.AMDExtendedCpuFeatures; } }
    }
}
