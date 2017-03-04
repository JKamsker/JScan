using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace JScan.Net.Data
{
    public class AddressByteCollection
    {
        public byte[] AddressBytes { get; set; }
        public byte[] IPv4MaskBytes { get; set; }

        public AddressByteCollection()
        {
        }

        public AddressByteCollection(byte[] addressBytes = null, byte[] ipv4MaskBytes = null)
        {
            //Clone bytes, don't set reference
            AddressBytes = addressBytes?.ToArray();
            IPv4MaskBytes = ipv4MaskBytes?.ToArray();
        }

        public AddressByteCollection(IPAddress addressBytes = null, IPAddress ipv4MaskBytes = null)
        {
            AddressBytes = addressBytes?.GetAddressBytes();
            IPv4MaskBytes = ipv4MaskBytes?.GetAddressBytes();
        }

        public AddressByteCollection(UnicastIPAddressInformation input)
        {
            AddressBytes = input.Address.GetAddressBytes();
            IPv4MaskBytes = input.IPv4Mask.GetAddressBytes();
        }
    }
}