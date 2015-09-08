using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using buffer = AppLibrary.Buffer;
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
