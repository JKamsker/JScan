using JScan.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace JScan.Net.Scan
{
    public class PingScan
    {
        public bool PingCompleted { get; private set; }

        private readonly int _pingTimeout;
        private readonly PingOptions _pingOptions = null;

        private Dictionary<uint, bool> DScanStatus = new Dictionary<uint, bool>(); // Allows double scan (lower failure rate)
        private ConcurrentQueue<IPAddress> AddressQueue = new ConcurrentQueue<IPAddress>();

        /// <summary>
        /// Null means that the process has been finished
        /// </summary>
        private Action<IPAddress> _scanSuccessCallback;

        private int _iRunningPingScanners = 0;

        public PingScan(Action<IPAddress> scanSuccessCallback, int timeout = 10000)
        {
            _pingOptions = new PingOptions(128, true);
            _scanSuccessCallback = scanSuccessCallback;
            _pingTimeout = timeout;
        }

        /// <summary>
        /// Scans the given IP Address list asyncronously
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="uMlimiter">Prevents memory leakage</param>
        public void InitScan(IPAddress[] addresses, int uMlimiter = 1000)
        {
            int uClimiter = 0;
            //Init Ping scan

            foreach (var address in addresses)
            {
                DScanStatus[address.ToUint()] = false;
                if (uClimiter >= uMlimiter)
                    AddressQueue.Enqueue(address);
                else
                {
                    uClimiter++;
                    _iRunningPingScanners++;
                    var pingSender = new Ping();
                    pingSender.PingCompleted += PingCompletedCallback;
                    pingSender.SendAsync(address, _pingTimeout, Encoding.ASCII.GetBytes("a"), _pingOptions);
                }
            }
        }

        /// <summary>
        /// Is called when the ping succeeded or timed out
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PingCompletedCallback(object sender, PingCompletedEventArgs e)
        {
            IPAddress nextScanAddress = null;
            uint uiAddress = e.Reply.Address.ToUint();

            if (uiAddress != 0) //Happens some times that the ip is 0
            {
                if (e.Reply.Status == IPStatus.Success)
                {
                    //Enqueue PortScan
                    _scanSuccessCallback?.Invoke(e.Reply.Address);
                }
                else if (!DScanStatus[e.Reply.Address.ToUint()])
                {
                    DScanStatus[e.Reply.Address.ToUint()] = true;
                    nextScanAddress = e.Reply.Address;
                }
            }

            while (nextScanAddress == null && AddressQueue.Count != 0)
                AddressQueue.TryDequeue(out nextScanAddress);

            if (nextScanAddress != null)
                ((Ping)sender).SendAsync(nextScanAddress, _pingTimeout, Encoding.ASCII.GetBytes("a"), _pingOptions);
            else
            {
                Interlocked.Decrement(ref _iRunningPingScanners);
                PingCompleted = (_iRunningPingScanners == 0 && AddressQueue.Count == 0);
                if (PingCompleted)
                {
                    _scanSuccessCallback?.Invoke(null);
                }
            }
        }
    }
}