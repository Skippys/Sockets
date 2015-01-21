using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NetworkData;

//
// Eric 21/1/2015;
//

namespace SocketServer
{
    class SocketServer
    {

        public const string key = "<EOF>";
        public static List<Client> _clients;

        public static void StartListening()
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 6969);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);
                Console.WriteLine("Waiting for connections...");

                while (true)
                {
                    _clients.Add(new Client(listener.Accept()));
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        public static void HandleClient(object Client)
        {
            Client c = (Client)Client;
            Socket handler = c.s;
            string data = null;
            byte[] bytes = new byte[1024];

            while (true)
            {
                int bytesRec = 0;

                try
                {
                    bytesRec = handler.Receive(bytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }

                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                while (true)
                {
                    if (data.IndexOf(key) > -1)
                    {
                        string tempstring = data.Substring(0, data.IndexOf(key));
                        DataManager(new Packet(tempstring));
                        byte[] msg = Encoding.ASCII.GetBytes(tempstring + key);

                        string t2 = tempstring + key;

                        BroadcastToClients(t2);
                        data = data.Substring(data.IndexOf(key) + key.Length, (data.Length - data.IndexOf(key) - key.Length));
                    }
                    else
                    {
                        break;
                    }
                }
            }

            c.active = false;
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            Thread.CurrentThread.Abort();
        }

        public static void DataManager(Packet p)
        {
            switch(p.packetType) {
                case PacketType.Test:
                    Console.WriteLine("Text Recieved: {0}", p.data[0]);
                    break;
                default:
                    break;
            }
        }

        public static void BroadcastToClients(string msg)
        {
            for (int i = 0; i < _clients.Count; i++)
            {
                if (_clients[i].active)
                {
                    try
                    {
                        byte[] temp = Encoding.ASCII.GetBytes(msg);
                        _clients[i].s.Send(temp);
                    }
                    catch
                    {
                        Console.WriteLine("could not send message");
                    }
                }
                else
                {
                    _clients.RemoveAt(i);
                    Console.WriteLine("removing client");
                }
            }
        }

        static void Main(string[] args)
        {
            _clients = new List<Client>();
            StartListening();
            return;
        }
    }

    class Client
    {
        public Socket s;
        public bool active = true;
        public Thread t;

        public Client(Socket socket)
        {
            Console.WriteLine("New Client Connected");
            this.s = socket;
            this.active = true;

            this.t = new Thread(new ParameterizedThreadStart(SocketServer.HandleClient));
            t.Start(this);
        }
    }
}
