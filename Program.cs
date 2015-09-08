using System;
using System.Collections.Generic;
using System.Linq;

namespace cSharpServer
{
    class Program
    {        

        /// <summary>
        /// Used for a manual server.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {            
            Database.CurrentServer = new Server();
            Database.CurrentServer.Start();

            Database.CurrentServer.HandleCommands();
        }        
    }
}
