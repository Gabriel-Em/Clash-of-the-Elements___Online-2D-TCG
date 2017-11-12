using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DM___Server.Models
{
    public class StorageForSocket
    {
        public string Storage { get; set; }
        public Socket Socket { get; set; }

        public StorageForSocket(Socket socket)
        {
            Socket = socket;
            Storage = "";
        }

        public void addChunks(string chunks)
        {
            Storage += chunks;
        }
    }
}
