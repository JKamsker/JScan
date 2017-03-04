using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using JScan.Net.Data;

namespace JScan.Net
{
    public static class NetMask
    {
        /// <summary>
        /// Calculates all IPAddresses in the given mask information
        /// </summary>
        /// <param name="inputCollection"></param>
        /// <returns></returns>
        public static List<IPAddress> GetIpsInMask(AddressByteCollection[] inputCollection)
        {
            var retVar = new List<IPAddress>();
            foreach (var item in inputCollection)
            {
                byte[] netid = BitConverter.GetBytes(BitConverter.ToUInt32(item.AddressBytes, 0) & BitConverter.ToUInt32(item.IPv4MaskBytes, 0));
                retVar.AddRange(GetIpRange(netid, BitConverter.GetBytes(BitConverter.ToUInt32(netid, 0) ^ BitConverter.ToUInt32(item.IPv4MaskBytes.Select(r => (byte)(byte.MaxValue - r)).ToArray(), 0))));
            }
            return retVar;
        }

        //+		IPv4Mask	{255.255.255.0}	System.Net.IPAddress
        //+		Address	{10.0.0.6}	System.Net.IPAddress

        /// <summary>
        /// Get all IPAdresses which are in the current subnet
        /// </summary>
        /// <returns></returns>
        public static List<IPAddress> GetAllIP()
        {
            var retVar = new List<IPAddress>();

            //Gather all networkinterface information required
            UnicastIPAddressInformation[] interfaceInfos = NetworkInterface.GetAllNetworkInterfaces()
                .Where(m => m.NetworkInterfaceType != NetworkInterfaceType.Loopback && m.OperationalStatus == OperationalStatus.Up)
                .SelectMany(m => m.GetIPProperties().UnicastAddresses)
                .Where(m => m.Address.AddressFamily == AddressFamily.InterNetwork).ToArray();

            //Fill all ip ranges
            foreach (var interfaceInfo in interfaceInfos)
            {
                byte[] mask = interfaceInfo.IPv4Mask.GetAddressBytes();
                byte[] netid = BitConverter.GetBytes(BitConverter.ToUInt32(interfaceInfo.Address.GetAddressBytes(), 0) & BitConverter.ToUInt32(mask, 0));
                retVar.AddRange(GetIpRange(netid, BitConverter.GetBytes(BitConverter.ToUInt32(netid, 0) ^ BitConverter.ToUInt32(mask.Select(r => (byte)(byte.MaxValue - r)).ToArray(), 0))));
            }

            return retVar;
        }

        private static List<IPAddress> GetIpRange(byte[] ip1, byte[] ip2)
        {
            var retVar = new List<IPAddress>();
            UInt32 bMax = BitConverter.ToUInt32(ip2.Reverse().ToArray(), 0) - 1;

            for (UInt32 n = BitConverter.ToUInt32(ip1.Reverse().ToArray(), 0) + 1; n <= bMax; n++)
                retVar.Add(new IPAddress(BitConverter.GetBytes(n).Reverse().ToArray()));

            return retVar;
        }
    }
}