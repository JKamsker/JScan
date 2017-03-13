using JScan.Net.Data;
using System;
using System.Collections.Generic;
using System.Net;

namespace JScan.Net.Scan
{
    public enum ScanMode
    {
        AsyncComplete,
        AsyncProgressive,
        Synchronous
    }

    public enum IPScanMode
    {
        AllSubnet,
        Subnet,
        Range,
        List
    }

    public class ScanSettings
    {
        public ScanSettings(IScanStorage StorageData = null, UInt16[] scanPorts = null, IPScanMode ipmode = IPScanMode.AllSubnet, ScanMode mode = ScanMode.Synchronous, int pingtimeout = 10000)
        {
            IpMode = ipmode;
            Mode = mode;
            portList = scanPorts == null ? new UInt16[] { 3000, 3001 } : scanPorts;
            PingTimeout = pingtimeout;

            switch (ipmode)
            {
                case IPScanMode.List:
                    Storage = StorageData == null ? new ScanStorageListData() : (ScanStorageListData)StorageData;
                    break;

                case IPScanMode.Subnet:
                    Storage = new ScanStorageMaskData((List<AddressByteCollection>)StorageData);
                    break;

                case IPScanMode.Range:
                default:
                    break;
            }
        }

        internal ScanMode Mode { get; private set; }
        internal IPScanMode IpMode { get; private set; }
        public IScanStorage Storage { get; set; }
        internal UInt16[] portList { get; private set; }
        internal int PingTimeout;

        internal Action<TCPScan> _progressiveAsyncScanStatusChangedCallback;
        internal Action<Dictionary<IPAddress, Dictionary<int, TCPortState>>> _completeAsyncScanFinishedCallback;

        /// <summary>
        /// Can only be used if <see cref="ScanMode"/> is AsyncProgressive
        /// </summary>
        public Action<TCPScan> progressiveAsyncScanStatusChangedCallback
        {
            get
            {
                return _progressiveAsyncScanStatusChangedCallback;
            }
            set
            {
                if (Mode == ScanMode.AsyncProgressive)
                    _progressiveAsyncScanStatusChangedCallback = value;
                else
                    throw new Exception("The Current mode doesn't support Asynch Progressive Status Callbacks");
            }
        }

        /// <summary>
        /// Can only be used if <see cref="ScanMode"/> is AsyncComplete or AsyncProgressive
        /// </summary>
        public Action<Dictionary<IPAddress, Dictionary<int, TCPortState>>> completeAsyncScanFinishedCallback
        {
            get
            {
                return _completeAsyncScanFinishedCallback;
            }
            set
            {
                if (Mode == ScanMode.AsyncComplete || Mode == ScanMode.AsyncProgressive)
                    _completeAsyncScanFinishedCallback = value;
                else
                    throw new Exception("The Current mode doesn't support Asynch Completition Status Callbacks");
            }
        }
    }
}