using JScan.Net.Data;
using JScan.Net.Scan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JScan_Example
{
    internal class Program
    {
        private static void Main(string[] args)
        {
        }

        private static void TestScanAsyncronously()
        {
            ScanSettings ScS = new ScanSettings(
                 scanPorts: new UInt16[] { 22 },
                 ipmode: IPScanMode.AllSubnet,
                 mode: ScanMode.AsyncProgressive,
                 pingtimeout: (int)TimeSpan.FromSeconds(2).TotalMilliseconds
           );

            ScS.progressiveAsyncScanStatusChangedCallback = new Action<TCPScan>((TCPScan ar) =>
            {
                if (ar.TcpState == TCPortState.open)
                {
                    Console.WriteLine("{0}:{1} {2}", ar.Host, ar.Port, ar.TcpState);
                }
            });
            ScS.completeAsyncScanFinishedCallback = new Action<Dictionary<IPAddress, Dictionary<int, TCPortState>>>(ar =>
            {
                Console.WriteLine("Finished Scan asyncronously");
            });
            ScanWrapper sw = new ScanWrapper(ScS);
            sw.ExecuteScan();
            Console.ReadLine();
        }

        private static void TestScanSyncronously()
        {
            ScanSettings ScS = new ScanSettings(
                   scanPorts: new UInt16[] { 80 },
                   ipmode: IPScanMode.List,
                   mode: ScanMode.Synchronous,
                   pingtimeout: (int)TimeSpan.FromSeconds(2).TotalMilliseconds,
                   StorageData: (IScanStorage)new ScanStorageListData(new[] {
                       IPAddress.Parse("10.0.0.1"),
                       IPAddress.Parse("10.0.0.2"),
                   })
           );

            ScanWrapper sw = new ScanWrapper(ScS);
            sw.ExecuteScan();
            Console.WriteLine("Scan finished syncronously");
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