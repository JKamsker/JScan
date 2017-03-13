using JScan.Net.Data;
using System;
using System.Net;
using System.Net.Sockets;

namespace JScan.Net.Scan
{
    public class TCPScan : IDisposable
    {
        public ETCPortState TcpState { get; private set; }
        public IPAddress Host { get; private set; }
        public int Port { get; private set; }

        private Action<TCPScan> _cbaPortScanFinished;

        private TcpClient _tcpcli;

        internal TCPScan(IPAddress host, int port, Action<TCPScan> portScanFinishedCallback)
        {
            _cbaPortScanFinished = portScanFinishedCallback;
            TcpState = ETCPortState.init;
            Host = host;
            Port = port;

            AsyncCallback cllbck = new AsyncCallback(PortScanCompletedCallback);
            _tcpcli = new TcpClient(AddressFamily.InterNetwork);
            _tcpcli.BeginConnect(host, port, cllbck, null);
            TcpState = ETCPortState.started;
        }

        private void PortScanCompletedCallback(IAsyncResult ar)
        {
            if (_tcpcli.Connected)
            {
                TcpState = ETCPortState.open;
                _tcpcli.Close();
            }
            else
            {
                //Offline
                TcpState = ETCPortState.closed;
            }
            _tcpcli = null;

            _cbaPortScanFinished?.Invoke(this);
        }

        ~TCPScan()
        {
            Dispose();
        }

        public void Dispose() => _tcpcli?.Close();
    }
}