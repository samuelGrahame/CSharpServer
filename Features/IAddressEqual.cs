using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cSharpServer.Features
{
    public interface IAddressEqual
    {
        bool EqualOrInRange(IAddressEqual address);
    }
}
