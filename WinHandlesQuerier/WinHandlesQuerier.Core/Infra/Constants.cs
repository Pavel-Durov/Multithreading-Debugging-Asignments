﻿using System;

namespace WinHandlesQuerier.Core.Infra
{
    public class Constants
    {
        public const uint INVALID_PID = 0;
        public const int MAX_ATTACH_TO_PPROCESS_TIMEOUT = 999999;
        private readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
    }
}
