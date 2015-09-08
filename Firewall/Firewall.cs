using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cSharpServer.Firewall
{
    public class Firewall
    {
        public FirewallRule Rule = FirewallRule.None;

        public enum FirewallRule
        {
            None = 0,
            OnlyBlackList = 1,
            OnlyWhiteList = 2,
            WhiteAndBlack = 3
        }

        public IpList BlackList = new IpList();
        public IpList WhiteList = new IpList();

        public bool ClientAllowed(IpAddress address)
        {
            if (Rule == FirewallRule.None)
                return true;
            
            if(Rule == FirewallRule.OnlyBlackList || 
                Rule == FirewallRule.WhiteAndBlack)
            {
                if (BlackList.InList(address))
                    return false;
            }
            if (Rule == FirewallRule.OnlyWhiteList ||
                Rule == FirewallRule.WhiteAndBlack)
            {
                if (!BlackList.InList(address))
                    return false;
            }

        }
    }
}
