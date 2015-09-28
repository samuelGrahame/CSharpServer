using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace cSharpServer
{
    /// <summary>
    /// This is for MYSQL.
    /// </summary>
    public class Database
    {
        public static Server CurrentServer;
        public const string Str0002 = "Server={0};Port={1};Uid={2};Pwd={3};Allow User Variables=True;Compress=true;";

        public static MySqlConnection GetConnection()
        {                        
            return new MySqlConnection(String.Format(Str0002, CurrentServer.DataSource,
            CurrentServer.DataPort, CurrentServer.DataUser,
            CurrentServer.DataPass)); //neo
        }

        /// <summary>
        /// Write data to the Client.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="stream"></param>
        public static void WriteBuffer(byte[] data, NetworkStream stream)
        {
            int length = data.Length;
            int bytesRead = 1;
            byte[] buffer = new byte[8192];
            stream.Write(BitConverter.GetBytes(length), 0, 4);

            using (MemoryStream ms = new MemoryStream(data))
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
        public static byte[] ReadBuffer(NetworkStream stream)
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

        public static bool sql_Connect(ref MySqlConnection connection)
        {
            if (connection == null)
            {
                connection = GetConnection();
            }
            if (connection.State == ConnectionState.Open)
            {
                return true;
            }
            bool result = false;
            try
            {
                connection.Open();
                result = true;
            }
            catch (Exception)
            {
            }
            return result;
        }

        public static void sql_Disconnect(ref MySqlConnection connection)
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        public static void DisposeMysql(MySqlCommand obj)
        {
            if (obj != null)
            {
                obj.Dispose();
            }
        }

        public static void DisposeMysql(MySqlDataReader obj)
        {
            if (obj != null)
            {
                obj.Close();
            }
        }

        public static void DisposeMysql(MySqlDataReader obj, MySqlCommand obj1)
        {
            DisposeMysql(obj);
            DisposeMysql(obj1);
        }

        public static void DisposeMysql(MySqlCommand obj, MySqlDataReader obj1)
        {
            DisposeMysql(obj1, obj);
        }

        public static DataTable GetDataTable(string value, MySqlConnection connection, string[] args, bool EnforceDatabase = true, bool useParentConnection = false)
        {
            return GetDataTable(value, connection, useParentConnection, EnforceDatabase, useParentConnection, args);
        }

        public static DataTable GetDataTable(string value, MySqlConnection connection, bool EnforceDatabase = true, bool useParentConnection = false, params string[] args)
        {
            DataTable dt = new DataTable();

            if (sql_Connect(ref connection))
            {
                MySqlCommand mcMysqlCommand = new MySqlCommand(value, connection);

                for (int i = 0; i < args.Length; i += 2)
                {
                    if (!String.IsNullOrWhiteSpace(args[i + 1]))
                    {
                        mcMysqlCommand.Parameters.AddWithValue(args[i], args[i + 1]);
                    }
                    else
                    {
                        mcMysqlCommand.Parameters.AddWithValue(args[i], DBNull.Value);
                    }
                }

                MySqlDataReader drMysqlDataReader = null;

                try
                {                    
                    drMysqlDataReader = mcMysqlCommand.ExecuteReader();
                    if (drMysqlDataReader.HasRows)
                    {
                        DataSet ds = new DataSet();
                        dt = new DataTable();
                        ds.Tables.Add(dt);
                        ds.EnforceConstraints = false;

                        dt.Load(drMysqlDataReader);
                    }
                }
                catch (Exception)
                {                    
                }
                finally
                {
                    DisposeMysql(mcMysqlCommand, drMysqlDataReader);

                    if (!useParentConnection)
                    {
                        sql_Disconnect(ref connection);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetDataTable(string value, MySqlConnection connection, bool loadColumns, bool EnforceDatabase = true, bool useParentConnection = false,
        params string[] args)
        {
            DataTable dt = new DataTable();

            if (sql_Connect(ref connection))
            {
                MySqlCommand mcMysqlCommand = new MySqlCommand(value, connection);

                for (int i = 0; i < args.Length; i += 2)
                {
                    if (!String.IsNullOrWhiteSpace(args[i + 1]))
                    {
                        mcMysqlCommand.Parameters.AddWithValue(args[i], args[i + 1]);
                    }
                    else
                    {
                        mcMysqlCommand.Parameters.AddWithValue(args[i], DBNull.Value);
                    }
                }

                MySqlDataReader drMysqlDataReader = null;

                try
                {
                    drMysqlDataReader = mcMysqlCommand.ExecuteReader();
                    if (drMysqlDataReader.HasRows || loadColumns)
                    {
                        DataSet ds = new DataSet();
                        dt = new DataTable();
                        ds.Tables.Add(dt);
                        ds.EnforceConstraints = false;

                        dt.Load(drMysqlDataReader);
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    DisposeMysql(mcMysqlCommand, drMysqlDataReader);

                    if (!useParentConnection)
                    {
                        sql_Disconnect(ref connection);
                    }
                }
            }
            return dt;
        }

        public static bool SetDataTable(string value, MySqlConnection connection, string[] args,bool EnforceDatabase = true, bool useParentConnection = false)
        {
            return SetDataTable(value, connection, EnforceDatabase, useParentConnection, args);
        }

        public static bool SetDataTable(string value, MySqlConnection connection, bool EnforceDatabase = true, bool useParentConnection = false, params string[] args)
        {
            if (sql_Connect(ref connection))
            {
                MySqlCommand mcMysqlCommand = new MySqlCommand(value, connection);

                for (int i = 0; i < args.Length; i += 2)
                {
                    if (!String.IsNullOrWhiteSpace(args[i + 1]))
                    {
                        mcMysqlCommand.Parameters.AddWithValue(args[i], args[i + 1]);
                    }
                    else
                    {
                        mcMysqlCommand.Parameters.AddWithValue(args[i], DBNull.Value);
                    }
                }

                try
                {
                    mcMysqlCommand.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    DisposeMysql(mcMysqlCommand);

                    if (!useParentConnection)
                    {
                        sql_Disconnect(ref connection);
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        ///     This will perform a singlar Query to the Database. Use when you are inserting and need last_insert_id()
        /// </summary>
        /// <param name="value">the query string</param>
        /// <param name="id">where the id will be returned to</param>
        /// <param name="useParentConnection"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool SetDataTable(string value, out long id, MySqlConnection connection, bool EnforceDatabase = true, bool useParentConnection = false,
        params string[] args)
        {
            id = 0;
            if (sql_Connect(ref connection))
            {
                MySqlCommand mcMysqlCommand = new MySqlCommand(value, connection);

                for (int i = 0; i < args.Length; i += 2)
                {
                    if (!String.IsNullOrWhiteSpace(args[i + 1]))
                    {
                        mcMysqlCommand.Parameters.AddWithValue(args[i], args[i + 1]);
                    }
                    else
                    {
                        mcMysqlCommand.Parameters.AddWithValue(args[i], DBNull.Value);
                    }
                }

                try
                {
                    mcMysqlCommand.ExecuteNonQuery();
                    id = mcMysqlCommand.LastInsertedId;
                }
                catch (Exception)
                {                    
                    return false;
                }
                finally
                {
                    DisposeMysql(mcMysqlCommand);

                    if (!useParentConnection)
                    {
                        sql_Disconnect(ref connection);
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Used to do multiple queries in a single connection
        /// </summary>
        /// <param name="value">An array of string to send to the DataBase</param>
        /// <param name="useParentConnection">
        /// Optional: doesnt actually do anything inside the function yet, but may be used to set
        /// the connection to a nondefault Database
        /// </param>
        /// <param name="args">
        /// Optional: args MUST be a multiple of 2 in length. args should be used in the following format
        /// "@paramter", stringValue
        /// </param>
        public static void TransactionSetDataTable(string[] value, MySqlConnection connection, bool EnforceDatabase = true, bool useParentConnection = false, params string[] args)
        {
            if (sql_Connect(ref connection))
            {
                MySqlTransaction trans = connection.BeginTransaction();

                //TODO - IF SOMETHING IS GOING TO GO WRONG, ITS PROBABALLY HERE
                try
                {
                    for (int f = 0; value.Length > f; f++)
                    {
                        MySqlCommand mcMysqlCommand = new MySqlCommand(value[f], connection);

                        for (int i = 0; i < args.Length; i += 2)
                        {
                            if (!String.IsNullOrWhiteSpace(args[i + 1]))
                            {
                                mcMysqlCommand.Parameters.AddWithValue(args[i], args[i + 1]);
                            }
                            else
                            {
                                mcMysqlCommand.Parameters.AddWithValue(args[i], DBNull.Value);
                            }
                        }
                        mcMysqlCommand.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
                catch (Exception)
                {                    
                    //TODO - ROLLBACK MIGHT CAUSE AN EXCEPTION IN ITSELF... MAYBE USE ANOTHER TRY - CATCH

                    if (connection.State == ConnectionState.Open)
                    {
                        trans.Rollback();
                    }
                }
                finally
                {

                    if (!useParentConnection)
                    {
                        sql_Disconnect(ref connection);
                    }
                }
            }
        }
    }
}
