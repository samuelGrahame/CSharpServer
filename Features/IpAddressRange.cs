using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cSharpServer.Features
{
    public class IpAddressRange : IAddressEqual
    {
        public IpAddress BeginAddress;
        public IpAddress EndAddress;

        public IpAddressRange(IpAddress a, IpAddress b)
        {
            BeginAddress = a;
            EndAddress = b;
        }

        public bool EqualOrInRange(IAddressEqual address)
        {
            var ip = address as IpAddress;
            return (address != null && ip.Value >= BeginAddress.Value && ip.Value <= EndAddress.Value);
        }
    }
}
