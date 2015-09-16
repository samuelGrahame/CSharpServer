using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace cSharpServer.ClientLibrary
{
    public class ClientConnection : IDisposable
    {
        private TcpClient _TcpClient;

        public ClientConnection(string address, int port, string username, string password)
        {
            this.ServerAddress = address;
            this.ServerPort = port;
            this.Username = username;
            this.Password = password;
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string ServerAddress { get; set; }
        public int ServerPort { get; set; }

        public ClientDataRequest RequestAction(ClientDataRequest clientdatarequest)
        {


            return null;
        }
        
        public void Dispose()
        {
        }
        /// <summary>
        /// This will only be used if the server is linked to MYSQL, This will be changed.
        /// </summary>
        /// <param name="Query"></param>
        /// <param name="useParentconnection"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool SetDataTable(string Query, bool useParentconnection, params string[] args)
        {
            bool Result = false;

            if (Connect())
            {
                BinaryBuffer buff = new BinaryBuffer();
                buff.BeginWrite();
                args = args ?? new string[] { };
                buff.Write(1);
                buff.Write(args.Length);

                for (int i = 0; i < args.Length; i++)
                    buff.WriteField(args[i]);

                buff.Write((byte)0);
                buff.WriteField(Query);

                buff.EndWrite();

                try
                {
                    Database.WriteBuffer(buff.ByteBuffer, _TcpClient.GetStream());
                    byte[] data = Database.ReadBuffer(_TcpClient.GetStream());

                    BinaryBuffer buffRead = new BinaryBuffer(data);

                    buffRead.BeginRead();

                    buffRead.ReadInt();

                    Result = buffRead.ReadByte() == (byte)1;

                    buffRead.EndRead();
                }
                catch (Exception)
                {
                }
                if (!useParentconnection)
                    Disconnect();
            }

            return Result;
        }

        /// <summary>
        /// Only If Server is MYSQL.
        /// </summary>
        /// <param name="Query"></param>
        /// <param name="useParentconnection"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public DataTable GetDataTable(string Query, bool useParentconnection, params string[] args)
        {
            DataTable dt = null;
            if (Connect())
            {
                BinaryBuffer buff = new BinaryBuffer();
                buff.BeginWrite();
                args = args ?? new string[] { };
                buff.Write(1);
                buff.Write(args.Length);

                for (int i = 0; i < args.Length; i++)
                    buff.WriteField(args[i]);

                buff.Write((byte)1);
                buff.WriteField(Query);

                buff.EndWrite();

                try
                {
                    Database.WriteBuffer(buff.ByteBuffer, _TcpClient.GetStream());
                    byte[] data = Database.ReadBuffer(_TcpClient.GetStream());
                    dt = BinaryBuffer.ConvertBlobToDataTable(ref data);
                }
                catch (Exception)
                {
                }

                if (!useParentconnection)
                    Disconnect();
            }

            return dt;
        }

        public bool Connected()
        {
            if (_TcpClient != null)
            {
                if (_TcpClient.Connected)
                    return true;
            }
            return false;
        }

        public bool Connect()
        {
            if (_TcpClient != null)
            {
                if (_TcpClient.Connected)
                    return true;
            }
            try
            {
                _TcpClient = new TcpClient();
                _TcpClient.Connect(ServerAddress, ServerPort);
                //_TcpClient.NoDelay = true;
                BinaryBuffer buff = new BinaryBuffer();

                buff.BeginWrite();

                buff.WriteField(Username);
                buff.WriteField(Password);

                buff.EndWrite();

                Database.WriteBuffer(buff.ByteBuffer, _TcpClient.GetStream());
            }
            catch (Exception)
            {
                if (_TcpClient != null)
                {
                    if (_TcpClient.Connected)
                    {
                        _TcpClient.Close();
                    }
                }
            }
            if (_TcpClient != null)
            {
                if (_TcpClient.Connected)
                    return true;
            }

            return false;
        }

        public void Disconnect()
        {
            if (_TcpClient != null)
            {
                if (_TcpClient.Connected)
                    _TcpClient.Close();
            }
        }
    }
}
