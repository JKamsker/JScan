using JScan.Net.Data;
using System;
using System.Collections.Generic;
using System.Net;

namespace JScan.Net.Scan
{
    public class ScanSettings
    {
        internal EScanMode Mode { get; private set; }
        internal EipScanMode IpMode { get; private set; }
        public IScanStorage Storage { get; set; }
        internal ushort[] PortList { get; private set; }
        internal int PingTimeout;

        private Action<TcpScan> _progressiveAsyncScanStatusChangedCallback;
        private Action<Dictionary<IPAddress, Dictionary<int, EtcPortState>>> _completeAsyncScanFinishedCallback;

        public ScanSettings(IScanStorage storageData = null, UInt16[] scanPorts = null,
            EipScanMode ipmode = EipScanMode.AllSubnet, EScanMode mode = EScanMode.Synchronous, int pingtimeout = 10000)
        {
            IpMode = ipmode;
            Mode = mode;
            PortList = scanPorts ?? (new ushort[] { 3000, 3001 });
            PingTimeout = pingtimeout;

            switch (ipmode)
            {
                case EipScanMode.List:
                    Storage = storageData == null ? new ScanStorageListData() : (ScanStorageListData)storageData;
                    break;

                case EipScanMode.Subnet:
                    Storage = new ScanStorageMaskData((List<AddressByteCollection>)storageData);
                    break;

                case EipScanMode.Range:
                    throw new NotImplementedException();
                default:
                    break;
            }
        }

        /// <summary>
        /// Can only be used if <see cref="EScanMode"/> is AsyncProgressive
        /// </summary>
        public Action<TcpScan> ProgressiveAsyncScanStatusChangedCallback
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
        public Action<Dictionary<IPAddress, Dictionary<int, EtcPortState>>> CompleteAsyncScanFinishedCallback
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