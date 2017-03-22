using JScan.Net.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace JScan.Net.Scan
{
    public class ScanWrapper
    {
        private Action<TcpScan> TCPScanChanged;
        private Action<IPAddress> pingScanSuccessCb;

        private List<TcpScan> ActiveTCPScan = new List<TcpScan>();
        private ScanSettings _settings;
        private PingScan _ps;

        public Dictionary<IPAddress, Dictionary<int, EtcPortState>> Results { get; private set; }

        public ScanWrapper(ScanSettings settings)
        {
            TCPScanChanged = TcpScanChangedCallback;
            pingScanSuccessCb = PingStatusSuccessCallback;
            _settings = settings;
            Results = new Dictionary<IPAddress, Dictionary<int, EtcPortState>>();
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
                case EipScanMode.AllSubnet:
                    ipAddresses = NetMask.GetAllIp();
                    break;

                case EipScanMode.Subnet:
                    ipAddresses = NetMask.GetIpsInMask(((ScanStorageMaskData)_settings.Storage).MaskData.ToArray());
                    break;

                case EipScanMode.Range:
                    throw new NotImplementedException();
                case EipScanMode.List:
                    ipAddresses = ((ScanStorageListData)_settings.Storage).IpAddresses;
                    break;

                default:
                    break;
            }
            if (ipAddresses == null)
            {
                throw new Exception("No ip Addresses in Stack");
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
        private void PingStatusSuccessCallback(IPAddress ac)
        {
            //Check if it's a ping-finished call
            if (ac == null)
            {
                if ((_settings.Mode == EScanMode.AsyncProgressive || _settings.Mode == EScanMode.AsyncComplete) && ActiveTCPScan.Count == 0 && _ps.PingCompleted)
                    _settings.CompleteAsyncScanFinishedCallback?.Invoke(Results);
                return;
            }
            Results[ac] = new Dictionary<int, EtcPortState>();
            foreach (var port in _settings.PortList)
            {
                Results[ac][port] = EtcPortState.Init;
                lock (ActiveTCPScan) ActiveTCPScan.Add(new TcpScan(ac, port, TCPScanChanged));
            }
        }

        /// <summary>
        /// Decides what happens when a port scan finishes (success/failure)
        /// </summary>
        /// <param name="res"></param>
        private void TcpScanChangedCallback(TcpScan res)
        {
            Results[res.Host][res.Port] = res.TcpState;

            lock (ActiveTCPScan) ActiveTCPScan.Remove(res);

            if (_settings.Mode == EScanMode.AsyncProgressive)
                _settings.ProgressiveAsyncScanStatusChangedCallback?.Invoke(res);

            if ((_settings.Mode == EScanMode.AsyncProgressive || _settings.Mode == EScanMode.AsyncComplete) && ActiveTCPScan.Count == 0 && _ps.PingCompleted)
                _settings.CompleteAsyncScanFinishedCallback?.Invoke(Results);
        }
    }
}