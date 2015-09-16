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
        public SocketAsyncEventArgs saea;
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
        /// Write data to the Client.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="stream"></param>
        public void WriteBuffer(byte[] data, NetworkStream stream)
        {            
            int length = data.Length;
            int bytesRead = 1;
            byte[] buffer = new byte[8192];
            stream.Write(BitConverter.GetBytes(length), 0, 4);

            using(MemoryStream ms = new MemoryStream(data))
            {
                  while (length > 0 && bytesRead > 0)
                    {
                        bytesRead = ms.Read(buffer, 0, Math.Min(length, buffer.Length));
                        stream.Write(buffer, 0, bytesRead);
                        length -= bytesRead;
                    }
            }
            stream.Flush();            
        }

        /// <summary>
        /// Read the data from the client.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public byte[] ReadBuffer(NetworkStream stream)
        {
            BinaryBuffer buff = new BinaryBuffer();

            buff.BeginWrite();

            int read = -1;
            int i = 0;

            while ((read = stream.ReadByte()) != -1)
            {
                buff.Write((byte)read);
                if (i++ == 3)
                    break;
            }

            buff.EndWrite();

            if (buff.ByteBuffer.Length != 4)
                return new byte[] { };

            int Contentlength = BitConverter.ToInt32(buff.ByteBuffer, 0);

            buff = new BinaryBuffer();

            int bytesRead = 1;
            byte[] buffer = new byte[8192];

            using (MemoryStream ms = new MemoryStream())
            {
                while (Contentlength > 0 && bytesRead > 0)
                {
                    bytesRead = stream.Read(buffer, 0, Math.Min(Contentlength, buffer.Length));
                    ms.Write(buffer, 0, bytesRead);
                    Contentlength -= bytesRead;
                }
                stream.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>
        /// This is the MYSQL connection used for Database Class.
        /// </summary>
        public MySql.Data.MySqlClient.MySqlConnection Connection = null;

        /// <summary>
        /// This is for a static MYSQL login using pwCredentials.tbl_users, you can now implement your own server.... with you own login.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Login(byte[] data)
        {            
            // what is the username and password?
            BinaryBuffer buff = new BinaryBuffer(data);
            // what is the username?
            // what is the password?
            buff.BeginRead();
            try
            {
                Username = buff.ReadString(buff.ReadInt());
                Password = buff.ReadString(buff.ReadInt());
                DataTable dt = Database.GetDataTable(@"SELECT * FROM pwCredentials.tbl_users WHERE username=@user && password=@pass;", Connection, false, false, "@user", Username, "@pass", Password);
                if(dt != null && dt.Rows.Count > 0)
                {
                    return true;
                }
            }
            catch (Exception)
            { }
            return false;
        }

        /// <summary>
        /// Compress data, make sure you check the size. the stream might be compressed already.
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="data"></param>
        public static void Compress(ref byte[] raw, out byte[] data)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory,
                CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                data = memory.ToArray();
            }
        }

        public static void Decompress(ref byte[] gzip, out byte[] data)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    data = memory.ToArray();
                }
            }
        }

        /// <summary>
        ///  handle MYSQL datatable data.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="buff"></param>
        public void ResultParseBack(DataTable result, ref BinaryBuffer buff)
        {            
            Stopwatch swa = Stopwatch.StartNew();
            DataTable dt = (DataTable)result;
            buff.Write(dt.Columns.Count);
            TypeCode[] types = new TypeCode[dt.Columns.Count];
            
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                buff.WriteField(dt.Columns[i].ColumnName);
                types[i] = Type.GetTypeCode(dt.Columns[i].DataType);
                buff.Write((byte)types[i]);
            }

            buff.Write(dt.Rows.Count);
            for (int y = 0; y < dt.Rows.Count; y++)
            {
                DataRow dr = dt.Rows[y];
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    // we need to get the typecode, depending on what type code we set the bytes...
                    object value = dr[i];
                    TypeCode type = ((IConvertible)value).GetTypeCode();
                    buff.Write((byte)type);

                    switch (type)
                    {
                        case TypeCode.Byte:
                        case TypeCode.Boolean:
                            buff.Write((byte)value);
                            break;
                        case TypeCode.String:
                            buff.WriteField((string)value);
                            break;
                        case TypeCode.Int16:
                            buff.Write(BitConverter.GetBytes((short)value));
                            break;
                        case TypeCode.Int32:
                            buff.Write((int)value);
                            break;
                        case TypeCode.Int64:
                            buff.Write((long)value);
                            break;
                        case TypeCode.Single:
                            buff.Write((float)value);
                            break;
                        case TypeCode.Double:
                            buff.Write(BitConverter.GetBytes((double)value));
                            break;
                        case TypeCode.Decimal:
                            buff.Write((decimal)value);
                            break;
                        case TypeCode.DBNull:
                            //buff.Write((double)value);
                            break;
                        case TypeCode.DateTime:
                            buff.Write(((DateTime)value).ToBinary());
                            break;
                    }
                }
            }

            swa.Stop();

            Console.WriteLine("Client {1}: Query Executed: {0} ms, byte length: {2}", swa.ElapsedMilliseconds, SessionId, buff.UncommitedLength());
        }

        /// <summary>
        ///  Handle MYSQL Update TRUE.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="buff"></param>
        public void ResultParseBack(object result, ref BinaryBuffer buff)
        {
            if(result == null)
            {
                buff.Write((byte)0);
            }
            else
            {
                if(result is bool)
                {
                    buff.Write(((bool)result) ? (byte)1 : (byte)0);
                }
            }
        }

        /// <summary>
        /// This will be called before a client Interface gets called.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public bool ProessRequest(NetworkStream stream)
        {
            try
            {
                BinaryBuffer buff = new BinaryBuffer(ReadBuffer(stream));
                
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
                    //// get args?
                    //int CountOfArgs = buff.ReadInt();
                    //// name, value;
                    //string[] args = new string[CountOfArgs];

                    //for (int y = 0; y < CountOfArgs;y++ )                    
                    //    args[y] = buff.ReadString(buff.ReadInt());

                    //switch (buff.ReadByte())
                    //{
                    //    case 0: // Execute                            
                    //        RequestId++;
                    //        ResultParseBack(Database.SetDataTable(buff.ReadString(buff.ReadInt()), Connection, true, true, args), ref writeBuff);
                    //        break;
                    //    case 1: // Reader
                    //        RequestId++;
                    //        ResultParseBack(Database.GetDataTable(buff.ReadString(buff.ReadInt()), Connection, true, true, args), ref writeBuff);
                    //        break;
                    //}
                    //Database.sql_Disconnect(ref Connection);
                }

                writeBuff.EndWrite();

                WriteBuffer(writeBuff.ByteBuffer, stream);

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
                    if (CurrentServer.ClientLogin.Login(ReadBuffer((stream = TcpConnection.GetStream())), this))                    
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
