using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cSharpServer.Firewall
{
    public class IpList
    {
        List<IAddressEqual> IpAddress = new List<IAddressEqual>();    
    
        public void Add(params IAddressEqual[] address)
        {
            IpAddress.AddRange(address);
        }

        public void Remove(params int[] index)
        {
            for (int i = 0, y = index.Length; i < y; i++)
                IpAddress[index[i]] = null;
            IpAddress.RemoveAll(null);
        }

        public bool IsEmpty()
        {
            return (IpAddress == null || IpAddress.Count == 0);
        }

        public bool InList(IpAddress Ip)
        {
            if (IsEmpty())
                return false;

            for(int i = 0, y = IpAddress.Count;i < y; i++)
            {
                if (IpAddress[i].EqualOrInRange(Ip))
                    return true;
            }
            return false;
        }

    }
}
