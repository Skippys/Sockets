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

namespace SocketClient
{
    class SocketClient
    {

        public const string key = "<EOF>";
        public static string name = "default";

        public static void HandleSend(object socket)
        {
            Socket s = (Socket)socket;

            while (true)
            {
                string temp = Console.ReadLine();
                byte[] msg = Encoding.ASCII.GetBytes(name + ": " + temp + key);

                try
                {
                    s.Send(msg);
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
                
            }

            s.Shutdown(SocketShutdown.Both);
            s.Close();
            Thread.CurrentThread.Abort();
            

        }

        public static void HandleReceive(object socket)
        {
            Socket s = (Socket)socket;
            byte[] bytes = new byte[1024];
            string data = null;

            while (true)
            {
                int bytesRec = 0;

                try
                {
                    bytesRec = s.Receive(bytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }

                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                //Console.WriteLine("Recieved {0} bytes", bytesRec);

                while (true)
                {
                    if (data.IndexOf(key) > -1)
                    {
                        string tempstring = data.Substring(0,data.IndexOf(key));
                        DataManager(new Packet(tempstring));
                        data = data.Substring(data.IndexOf(key) + key.Length, (data.Length - data.IndexOf(key) - key.Length));
                    }
                    else 
                    {
                        break;
                    }
                }
            }
            s.Shutdown(SocketShutdown.Both); 
            s.Close();
            Thread.CurrentThread.Abort();
        }

        public static void DataManager(Packet p)
        {
            switch (p.packetType) {
                case PacketType.Test:
                    Console.WriteLine(p.data[0]);
                    break;
                default:
                    break;
            }
        }

        public static void StartClient()
        {
            try
            {
                Console.WriteLine("Enter Ip Address");
                string tempIP = Console.ReadLine();

                Console.WriteLine("Please Enter your name");
                name = Console.ReadLine();

                IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];


               

                IPEndPoint remoteEp = new IPEndPoint(IPAddress.Parse(tempIP), 6969);

                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    sender.Connect(remoteEp);

                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                    Thread t = new Thread(new ParameterizedThreadStart(HandleSend));
                    t.Start(sender);

                    Thread t2 = new Thread(new ParameterizedThreadStart(HandleReceive));
                    t2.Start(sender);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        static void Main(string[] args)
        {
            StartClient();
            Console.Write("\nPress ENTER to continue");
            Console.ReadLine();
            return;
        }
    }
}
