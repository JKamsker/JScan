using JScan.Net.Data;
using System;
using System.Net;
using System.Net.Sockets;

namespace JScan.Net.Scan
{
    public class TcpScan : IDisposable
    {
        public EtcPortState TcpState { get; private set; }
        public IPAddress Host { get; private set; }
        public int Port { get; private set; }

        private Action<TcpScan> _cbaPortScanFinished;

        private TcpClient _tcpcli;

        internal TcpScan(IPAddress host, int port, Action<TcpScan> portScanFinishedCallback)
        {
            _cbaPortScanFinished = portScanFinishedCallback;
            TcpState = EtcPortState.Init;
            Host = host;
            Port = port;

            var cllbck = new AsyncCallback(PortScanCompletedCallback);
            _tcpcli = new TcpClient(AddressFamily.InterNetwork);
            _tcpcli.BeginConnect(host, port, cllbck, null);
            TcpState = EtcPortState.Started;
        }

        private void PortScanCompletedCallback(IAsyncResult ar)
        {
            if (_tcpcli.Connected)
            {
                TcpState = EtcPortState.Open;
                _tcpcli.Close();
            }
            else
            {
                //Offline
                TcpState = EtcPortState.Closed;
            }
            _tcpcli = null;

            _cbaPortScanFinished?.Invoke(this);
        }

        ~TcpScan()
        {
            Dispose();
        }

        public void Dispose() => _tcpcli?.Close();
    }
}