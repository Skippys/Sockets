using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

//
// Eric 21/1/2015;
//

namespace NetworkData
{
    [Serializable]
    public class Packet
    {

        public List<string> data;
        public PacketType packetType;

        public Packet()
        {
            this.data = new List<string>();
            this.packetType = PacketType.Test;
        }

        public Packet(string s)
        {
            this.data = new List<string>();
            this.packetType = PacketType.Test;
            data.Add(s);
        }
    }

    public enum PacketType
    {
        Test
    }
}
