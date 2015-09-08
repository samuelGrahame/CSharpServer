using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cSharpServer.Firewall
{
    public class IpAddress : IAddressEqual
    {
        public int Value;
        public bool EqualOrInRange(IAddressEqual address)
        {
            var ip = address as IpAddress;
            return ip != null && ip.Value == this.Value;
        }
    }
}
