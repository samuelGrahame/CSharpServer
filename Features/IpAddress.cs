using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cSharpServer.Features
{
    public class IpAddress : IAddressEqual
    {
        public IpAddress(int value)
        {
            this.Value = value;
        }
        public IpAddress(string value)
        {
            string[] array = value.Split('.');
            this.Value = BitConverter.ToInt32(new byte[] {
                byte.Parse(array[1]), 
                byte.Parse(array[2]),
                byte.Parse(array[3]),
                byte.Parse(array[4])}, 0);
        }
        public int Value;
        public bool EqualOrInRange(IAddressEqual address)
        {
            var ip = address as IpAddress;
            return ip != null && ip.Value == this.Value;
        }
    }
}
