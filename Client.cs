using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;

namespace cSharpServer
{
    /// <summary>
    /// This handles everything a user needs.
    /// </summary>
    public class Client : IDisposable
    {
        public TcpClient TcpConnection;
        public Client(TcpClient _TcpConnection, Server _CurrentServer)
        {
            this.CurrentServer = _CurrentServer;
            this.TcpConnection = _TcpConnection;
        }        
        public long SessionId;
        public string Username;
        public string Password;
        public Server CurrentServer;

        /// <summary>
        /// Store custom data for the user.
        /// </summary>
        public List<ClientArgument> Arguments = new List<ClientArgument>();

        public void GetArgument(string Name, out string Value)
        {
            for(int i = 0, y = Arguments.Count; i < y; i++)
                if(string.Compare(Name, Arguments[i].Name) == 0)
                {
                    Value = Arguments[i].GetString();
                    return;
                }           
            Value = string.Empty;
        }
        public void GetArgument(string Name, out int Value)
        {
            for (int i = 0, y = Arguments.Count; i < y; i++)
                if (string.Compare(Name, Arguments[i].Name) == 0)
                {
                    Value = Arguments[i].GetInt();
                    return;
                }
            Value = 0;
        }
        
        /// <summary>
        /// This is the MYSQL connection used for Database Class.
        /// </summary>
        public MySql.Data.MySqlClient.MySqlConnection Connection = null;
        
        /// <summary>
        /// This will be called before a client Interface gets called.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public bool ProessRequest(NetworkStream stream)
        {
            try
            {
                BinaryBuffer buff = new BinaryBuffer(Database.ReadBuffer(stream));
                
                buff.BeginRead();

                int count = buff.ReadInt();

                BinaryBuffer writeBuff = new BinaryBuffer();

                writeBuff.BeginWrite();

                for (int i = 0; i < count;i++)
                {
                    IClientRequest Request = CurrentServer.GetRequest(buff.ReadByte());
                    // if a IClientRequest to close. we need to handle by not reading and writing back!
                    if (!this.TcpConnection.Connected)
                        return false;

                    byte[] data = buff.ReadByteArray(buff.ReadInt());
                    if(Request != null)
                    {
                        data = Request.Process(new BinaryBuffer(data), this);
                        writeBuff.Write(data.Length);
                        writeBuff.Write(data);
                    }
                    else
                    {
                        writeBuff.Write(0); // length;
                    }
                }

                writeBuff.EndWrite();

                Database.WriteBuffer(writeBuff.ByteBuffer, stream);

                return true;
            }
            catch (Exception)
            { }
            return false;
        }

        /// <summary>
        /// Connection time.
        /// </summary>
        public Stopwatch ClientStopwatch = Stopwatch.StartNew();

        /// <summary>
        /// Launch the process/login of the client.
        /// </summary>
        public void Launch()
        {
            string localEndPoint = TcpConnection.Client.LocalEndPoint.ToString();
            try
            {
                NetworkStream stream;
                
                if (TcpConnection.Connected)
                {
                    if (CurrentServer.ClientLogin.Login(Database.ReadBuffer((stream = TcpConnection.GetStream())), this))                    
                        while (TcpConnection.Connected)                        
                            if(!ProessRequest(stream))                            
                                break;                                                
                    TcpConnection.Close();
                }
            }
            catch(Exception e)
            {
                // log!
                Console.WriteLine(e.ToString());
            }
            finally
            {
                ClientStopwatch.Stop();
                Console.WriteLine("Client {1}: {0} has disconnected, Connected for: {2} ms", localEndPoint, SessionId, ClientStopwatch.ElapsedMilliseconds);
            }            
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
