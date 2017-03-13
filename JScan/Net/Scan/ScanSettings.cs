using JScan.Net.Data;
using System;
using System.Collections.Generic;
using System.Net;

namespace JScan.Net.Scan
{
    public class ScanSettings
    {
        internal EScanMode Mode { get; private set; }
        internal EIPScanMode IpMode { get; private set; }
        public IScanStorage Storage { get; set; }
        internal UInt16[] portList { get; private set; }
        internal int PingTimeout;

        internal Action<TCPScan> _progressiveAsyncScanStatusChangedCallback;
        internal Action<Dictionary<IPAddress, Dictionary<int, ETCPortState>>> _completeAsyncScanFinishedCallback;

        public ScanSettings(IScanStorage StorageData = null, UInt16[] scanPorts = null,
            EIPScanMode ipmode = EIPScanMode.AllSubnet, EScanMode mode = EScanMode.Synchronous, int pingtimeout = 10000)
        {
            IpMode = ipmode;
            Mode = mode;
            portList = scanPorts == null ? new UInt16[] { 3000, 3001 } : scanPorts;
            PingTimeout = pingtimeout;

            switch (ipmode)
            {
                case EIPScanMode.List:
                    Storage = StorageData == null ? new ScanStorageListData() : (ScanStorageListData)StorageData;
                    break;

                case EIPScanMode.Subnet:
                    Storage = new ScanStorageMaskData((List<AddressByteCollection>)StorageData);
                    break;

                case EIPScanMode.Range:
                default:
                    break;
            }
        }

        /// <summary>
        /// Can only be used if <see cref="EScanMode"/> is AsyncProgressive
        /// </summary>
        public Action<TCPScan> progressiveAsyncScanStatusChangedCallback
        {
            get
            {
                return _progressiveAsyncScanStatusChangedCallback;
            }
            set
            {
                if (Mode == EScanMode.AsyncProgressive)
                    _progressiveAsyncScanStatusChangedCallback = value;
                else
                    throw new Exception("The Current mode doesn't support Asynch Progressive Status Callbacks");
            }
        }

        /// <summary>
        /// Can only be used if <see cref="EScanMode"/> is AsyncComplete or AsyncProgressive
        /// </summary>
        public Action<Dictionary<IPAddress, Dictionary<int, ETCPortState>>> completeAsyncScanFinishedCallback
        {
            get
            {
                return _completeAsyncScanFinishedCallback;
            }
            set
            {
                if (Mode == EScanMode.AsyncComplete || Mode == EScanMode.AsyncProgressive)
                    _completeAsyncScanFinishedCallback = value;
                else
                    throw new Exception("The Current mode doesn't support Asynch Completition Status Callbacks");
            }
        }
    }
}