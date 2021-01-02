using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Train_Screensaver_Client.Networking
{
    class Connection
    {
        private string server;
        private ushort port;

        TcpClient client;
        NetworkStream stream;

        public Connection(string server, ushort port)
        {
            this.server = server;
            this.port = port;
        }

        public bool Open()
        {
            try
            {
                client = new TcpClient(server, port);
                stream = client.GetStream();
            }
            catch
            {
                Close();
                return false;
            }

            return true;
        }

        public bool Send(byte[] data)
        {
            try
            {
                data = (byte[])data.Clone();
                stream.Write(data);
            }
            catch
            {
                return false;
            }

            return true;
            
        }

        public bool Read(out byte[] data)
        {
            data = new byte[3];
            try
            {
                stream.Read(data);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void Close()
        {
            if (!(stream is null))
                stream.Close();

            if (!(client is null))
                client.Close();

            stream = null;
            client = null;
        }
    }
}
