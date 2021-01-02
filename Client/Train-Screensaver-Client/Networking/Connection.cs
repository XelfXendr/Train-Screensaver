using System.Net.Sockets;
using System.Threading;

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

        //Open new stream
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

        //Send message to server
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

        //Read message froms server
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

        //Close stream
        public void Close()
        {
            if (!(stream is null))
                stream.Close();

            if (!(client is null))
                client.Close();

            stream = null;
            client = null;
        }

        //Try to reconnect every minute until a connection is established
        public void Reconnect()
        {
            Close();
            while (!Open())
                Thread.Sleep(60000);
        }
    }
}
