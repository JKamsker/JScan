using JScan.Net.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace JScan.Net.Scan
{
    public class ScanWrapper
    {
        private Action<TCPScan> TCPScanChanged;
        private Action<IPAddress> pingScanSuccessCb;

        private List<TCPScan> ActiveTCPScan = new List<TCPScan>();
        private ScanSettings _settings;
        private PingScan _ps;

        public Dictionary<IPAddress, Dictionary<int, ETCPortState>> Results { get { return _resultDict; } }
        private Dictionary<IPAddress, Dictionary<int, ETCPortState>> _resultDict;

        public ScanWrapper(ScanSettings settings)
        {
            TCPScanChanged = new Action<TCPScan>(TCPScanChangedCallback);
            pingScanSuccessCb = new Action<IPAddress>(pingStatusSuccessCallback);
            _settings = settings;
            _resultDict = new Dictionary<IPAddress, Dictionary<int, ETCPortState>>();
        }

        /// <summary>
        /// Synchronous:
        ///     All result data can be optained via the Results property afterwards
        ///
        /// </summary>
        public void ExecuteScan()
        {
            List<IPAddress> ipAddresses = null;

            switch (_settings.IpMode)
            {
                case EIPScanMode.AllSubnet:
                    ipAddresses = NetMask.GetAllIP();
                    break;

                case EIPScanMode.Subnet:
                    ipAddresses = NetMask.GetIpsInMask(((ScanStorageMaskData)_settings.Storage).MaskData.ToArray());
                    break;

                case EIPScanMode.Range:
                    throw new NotImplementedException();
                case EIPScanMode.List:
                    ipAddresses = ((ScanStorageListData)_settings.Storage).IPAddresses;
                    break;

                default:
                    break;
            }

            _ps = new PingScan(pingScanSuccessCb, _settings.PingTimeout);

            _ps.InitScan(ipAddresses.ToArray());

            if (_settings.Mode == EScanMode.Synchronous)
            {
                while (ActiveTCPScan.Count > 0 || !_ps.PingCompleted)
                {
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// Decides what happens when a ping succeeded
        /// </summary>
        /// <param name="ac"></param>
        private void pingStatusSuccessCallback(IPAddress ac)
        {
            //Check if it's a ping-finished call
            if (ac == null)
            {
                if ((_settings.Mode == EScanMode.AsyncProgressive || _settings.Mode == EScanMode.AsyncComplete) && ActiveTCPScan.Count == 0 && _ps.PingCompleted)
                    _settings.completeAsyncScanFinishedCallback?.Invoke(_resultDict);
                return;
            }
            _resultDict[ac] = new Dictionary<int, ETCPortState>();
            foreach (var port in _settings.portList)
            {
                _resultDict[ac][port] = ETCPortState.init;
                lock (ActiveTCPScan) ActiveTCPScan.Add(new TCPScan(ac, port, TCPScanChanged));
            }
        }

        /// <summary>
        /// Decides what happens when a port scan finishes (success/failure)
        /// </summary>
        /// <param name="res"></param>
        private void TCPScanChangedCallback(TCPScan res)
        {
            _resultDict[res.Host][res.Port] = res.TcpState;

            lock (ActiveTCPScan) ActiveTCPScan.Remove(res);

            if (_settings.Mode == EScanMode.AsyncProgressive)
                _settings.progressiveAsyncScanStatusChangedCallback?.Invoke(res);

            if ((_settings.Mode == EScanMode.AsyncProgressive || _settings.Mode == EScanMode.AsyncComplete) && ActiveTCPScan.Count == 0 && _ps.PingCompleted)
                _settings.completeAsyncScanFinishedCallback?.Invoke(_resultDict);
        }
    }
}