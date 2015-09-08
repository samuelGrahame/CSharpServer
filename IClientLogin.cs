using System;
using System.Collections.Generic;
using System.Linq;

namespace cSharpServer
{
    public interface IClientLogin
    {
        /// <summary>
        /// used to handle the login of the client.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        bool Login(byte[] data, Client client);       
    }
}
