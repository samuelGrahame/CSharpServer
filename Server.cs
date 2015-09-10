using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using cSharpServer.Features;

namespace cSharpServer
{
    public class Server
    {
        /// <summary>
        /// This is the MYSQL Server Address.
        /// </summary>
        public string DataSource = "localhost";
        /// <summary>
        /// This is the MYSQL Server Port.
        /// </summary>
        public string DataPort = "";
        /// <summary>
        /// This is the MYSQL Default User.
        /// </summary>
        public string DataUser = "";
        /// <summary>
        /// This is the MYSQL Default Password.
        /// </summary>
        public string DataPass = "";
        /// <summary>
        /// This is the MYSQL Default Database.
        /// </summary>
        public string DataSchema = "";

        /// <summary>
        /// Hosted Server Port for the current server.
        /// </summary>
        public int ServerPort = 1888;
        /// <summary>
        /// Server Listener, make sure you dont set to NULL!
        /// </summary>
        public TcpListener ServerListener;
        /// <summary>
        /// List of Clients, when using this Make sure you use LOCK() it is used in other threads.
        /// </summary>
        public List<Client> ServerClients = new List<Client>();

        /// <summary>
        /// This is used for Client Requests.
        /// </summary>
        public List<IClientRequest> ClientRequestTypes = new List<IClientRequest>();
        /// <summary>
        /// This is used for a client login.
        /// </summary>
        public IClientLogin ClientLogin;

        /// <summary>
        /// If this is true then the server is currently running.
        /// </summary>
        public bool ServerRunning = false;
        /// <summary>
        /// Max Set Connection Id.
        /// </summary>
        public long ServerMaxSessionId = 0L;

        /// <summary>
        /// This will stop people from connection. Without having permission to admin to add ipaddress.
        /// </summary>
        public Firewall ServerFirewall = new Firewall();

        /// <summary>
        /// Start the server. use handle commands to keep the server alive. (Program)
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            Task.Run(new Action(WaitForConnections));
            
            return true;
        }

        /// <summary>
        /// Stop the server, this can be used to force close.
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            ServerRunning = false;            
            ServerListener.Stop();
            return true;
        }

        /// <summary>
        /// Used to join a new Client with a Closed Client.
        /// </summary>
        /// <param name="client"></param>
        public void JoinClient(Client client)
        {
            if (!ServerFirewall.ClientAllowed(new IpAddress(client.TcpConnection.Client.LocalEndPoint.ToString())))
                return;

            bool Added = false;
            lock (ServerClients)
            {
                for (int i = ServerClients.Count - 1; i >= 0; i--)
                {
                    if (ServerClients[i] == null || (ServerClients[i].TcpConnection == null || !ServerClients[i].TcpConnection.Connected))
                    {
                        if (Added)
                        {
                            ServerClients.RemoveAt(i);
                        }
                        else
                        {
                            ServerClients[i] = client;
                            Added = true;
                        }                                                
                    }
                }
                client.SessionId = ServerMaxSessionId++;
                Console.WriteLine("Client {1}: {0} has connected...", client.TcpConnection.Client.LocalEndPoint.ToString(), client.SessionId);

                if(!Added)
                {                    
                    ServerClients.Add(client);
                }                
            }
        }

        /// <summary>
        /// Get the client Request by the Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IClientRequest GetRequest(byte id)
        {
            for(int i = 0, y = ClientRequestTypes.Count ;i < y; i ++)
            {
                if (ClientRequestTypes[i].IdType == id)
                    return ClientRequestTypes[i];
            }
            return null;
        }

        /// <summary>
        /// Wait for a new client to connect to the server.
        /// </summary>
        public async void WaitForConnections()
        {            
            ServerRunning = true;

            ServerListener = new TcpListener(IPAddress.Any, ServerPort);
            ServerListener.Start();

            Console.WriteLine("Listening on {0}...", ServerListener.LocalEndpoint.ToString());
            Console.WriteLine("Waiting for connections...");

            while(ServerRunning)
            {
                try
                {
                    Client IncommingClient = new Client(await ServerListener.AcceptTcpClientAsync(), this);
                    if (IncommingClient.TcpConnection.Connected)
                    {
                        JoinClient(IncommingClient);
                        Task.Run(new Action(IncommingClient.Launch));
                    }
                }
                catch (Exception)
                {                    
                }                
            }
        }

        /// <summary>
        /// Show all the connected Clients.
        /// </summary>
        private void ListConnections()
        {
            Console.Clear();

            lock(ServerClients)
            {
                for(int i = 0; i < ServerClients.Count; i ++)
                {
                    if(ServerClients[i] != null && ServerClients[i].TcpConnection != null && ServerClients[i].TcpConnection.Connected)
                    {
                        Console.WriteLine("Client {1}: {0}.", ServerClients[i].TcpConnection.Client.LocalEndPoint.ToString(),  ServerClients[i].SessionId);
                    }
                }
            }
        }
        
        /// <summary>
        /// Kill a client by Session ID.
        /// </summary>
        /// <param name="command"></param>
        private void Kill(string command)
        {
            string[] data = command.Split(' ');
            if (data.Length > 1)
            {
                for (int i = 1; i < data.Length; i++)
                {
                    lock (Database.CurrentServer.ServerClients)
                    {
                        for (int y = 0; y < Database.CurrentServer.ServerClients.Count; y++)
                        {
                            if (Database.CurrentServer.ServerClients[y] == null)
                            {
                                Database.CurrentServer.ServerClients.RemoveAt(y);
                                break;
                            }
                            long index;
                            if (long.TryParse(data[i], out index) && Database.CurrentServer.ServerClients[y].SessionId == index)
                            {
                                Database.CurrentServer.ServerClients[y].TcpConnection.Close();
                                Database.CurrentServer.ServerClients.RemoveAt(y);
                                break;
                            }
                        }
                    }
                }
            }
            Database.CurrentServer.Stop();
        }

        /// <summary>
        /// Handle string command.
        /// </summary>
        public void HandleCommands()
        {
            string command;

            while ((command = Console.ReadLine().ToLower()) != "close")
            {
                if (command.StartsWith("stop"))
                {
                    Database.CurrentServer.Stop();
                }
                else if (command.StartsWith("restart"))
                {
                    Database.CurrentServer.Stop();
                    Console.Clear();
                    Database.CurrentServer.Start();
                }
                else if (command.StartsWith("start"))
                {
                    Database.CurrentServer.Stop();
                }
                else if (command.StartsWith("kill"))
                {
                    this.Kill(command);
                }
                else if (command.StartsWith("connections"))
                {
                    this.ListConnections();
                }
            }
        }
    }
}
