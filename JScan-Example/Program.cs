using JScan.Net.Data;
using JScan.Net.Scan;
using System;
using System.Collections.Generic;
using System.Net;

namespace JScan_Example
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TestScanAsyncronously();
            Console.ReadLine();
        }

        /// <summary>
        /// Run an async scan - don't block the current progress
        /// </summary>
        private static void TestScanAsyncronously()
        {
            //Initialize a new Settings object
            ScanSettings ScS = new ScanSettings(
                 scanPorts: new UInt16[] { 22 },    //Define which ports should be searched for
                 ipmode: EIPScanMode.AllSubnet,      //Scan all ips from my subnet
                 mode: EScanMode.AsyncProgressive,   //Don't block the current progress and call the callback: on finish, on ip dedection
                 pingtimeout: (int)TimeSpan.FromSeconds(2).TotalMilliseconds //Timeout for pings
           );

            //Action which is called when an open port/online ip is found
            ScS.progressiveAsyncScanStatusChangedCallback = new Action<TCPScan>((TCPScan ar) =>
            {
                if (ar.TcpState == ETCPortState.open)
                {
                    Console.WriteLine("{0}:{1} {2}", ar.Host, ar.Port, ar.TcpState);
                }
            });

            //This one is called when the scan has been finished
            ScS.completeAsyncScanFinishedCallback = new Action<Dictionary<IPAddress, Dictionary<int, ETCPortState>>>(ar =>
            {
                Console.WriteLine("Finished Scan asyncronously");
            });

            //Initialize the actual scanwrapper with the settings object
            ScanWrapper sw = new ScanWrapper(ScS);
            //Run the scan
            sw.ExecuteScan();
        }

        /// <summary>
        /// Run an  scan - blocks the current progress until completition
        /// </summary>
        private static void TestScanSyncronously()
        {
            //Initialize a new Settings object
            ScanSettings ScS = new ScanSettings(
                   scanPorts: new UInt16[] { 80 },  //Define which ports should be searched for
                   ipmode: EIPScanMode.List,         //Scan for a list of ip's
                   mode: EScanMode.Synchronous,      //Run syncronously
                   pingtimeout: (int)TimeSpan.FromSeconds(2).TotalMilliseconds, //Timeout for pings
                   StorageData: (IScanStorage)new ScanStorageListData(new[] {
                       IPAddress.Parse("10.0.0.1"), //IP 1 to scan for
                       IPAddress.Parse("10.0.0.2"), //IP 2 to scan for
                   })
           );

            //Initialize the actual scanwrapper with the settings object
            ScanWrapper sw = new ScanWrapper(ScS);
            //Run the scan
            sw.ExecuteScan();
            Console.WriteLine("Scan finished syncronously");

            //In (ScanWrapper)sw.Results are the results (IP:PORT)
            var res = sw.Results;

            foreach (var cIP in res.Keys)
            {
                var cres = res[cIP];
                Console.WriteLine("{0}:", cIP);
                foreach (var port in cres.Keys)
                {
                    Console.WriteLine("\t{0}:{1}", port, cres[port].ToString());
                }
            }
            Console.ReadLine();
        }
    }
}