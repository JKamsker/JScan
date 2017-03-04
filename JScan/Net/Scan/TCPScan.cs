using System;
using System.Net;
using System.Net.Sockets;

namespace JScan.Net.Scan
{
    /// <summary>
    ///
    /// </summary>
    public enum TCPortState
    {
        init,
        started,
        open,
        closed
    }

    public class TCPScan
    {
        public TCPortState TcpState { get; private set; }
        public IPAddress Host { get; private set; }
        public int Port { get; private set; }

        private Action<TCPScan> _cbaPortScanFinished;

        private TcpClient _tcpcli;

        internal TCPScan(IPAddress host, int port, Action<TCPScan> portScanFinishedCallback)
        {
            _cbaPortScanFinished = portScanFinishedCallback;
            TcpState = TCPortState.init;
            Host = host;
            Port = port;

            AsyncCallback cllbck = new AsyncCallback(PortScanCompletedCallback);
            _tcpcli = new TcpClient(AddressFamily.InterNetwork);
            _tcpcli.BeginConnect(host, port, cllbck, null);
            TcpState = TCPortState.started;
        }

        private void PortScanCompletedCallback(IAsyncResult ar)
        {
            if (_tcpcli.Connected)
            {
                TcpState = TCPortState.open;
                _tcpcli.Close();
            }
            else
            {
                //Offline
                TcpState = TCPortState.closed;
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