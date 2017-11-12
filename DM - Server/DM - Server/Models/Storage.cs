using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DM___Server.Models
{
    public class Storage
    {
        private List<Models.StorageForSocket> storage;
        private string BEGIN_MESSAGE_DELIMITER;
        private string END_MESSAGE_DELIMITER;

        public Storage(string begin_message_delimiter, string end_message_delimiter)
        {
            storage = new List<StorageForSocket>();
            BEGIN_MESSAGE_DELIMITER = begin_message_delimiter;
            END_MESSAGE_DELIMITER = end_message_delimiter;
        }

        public void createStorage(Socket socket)
        {
            storage.Add(new StorageForSocket(socket));
        }

        public void removeStorage(Socket socket)
        {
            int index;

            index = getIndexOfSocket(socket);
            if(index!=-1)
                storage.RemoveAt(index);
        }

        public void addChunks(string chunks, Socket socket)
        {
            int index;
            index = getIndexOfSocket(socket);
            if(index != -1)
                storage[index].addChunks(chunks);
        }

        private int getIndexOfSocket(Socket socket)
        {
            for (int i = 0; i < storage.Count; i++)
                if (storage[i].Socket == socket)
                    return i;
            return -1;
        }

        public Models.Message getWholeMessage(Socket socket)
        {
            int index;
            Models.Message obMessage = null;
            index = getIndexOfSocket(socket);

            if(index != -1)
            {
                int beginIndex = storage[index].Storage.IndexOf(BEGIN_MESSAGE_DELIMITER);
                int endIndex = storage[index].Storage.IndexOf(END_MESSAGE_DELIMITER);
                string message;

                if (endIndex >= 0 && beginIndex >= 0 && endIndex < beginIndex)
                {
                    storage[index].Storage = storage[index].Storage.Substring(beginIndex);
                    beginIndex = storage[index].Storage.IndexOf(BEGIN_MESSAGE_DELIMITER);
                    endIndex = storage[index].Storage.IndexOf(END_MESSAGE_DELIMITER);
                }

                if (endIndex >= 0 && beginIndex >= 0)
                {
                    message = storage[index].Storage.Substring(beginIndex + BEGIN_MESSAGE_DELIMITER.Length,
                        endIndex - (beginIndex + BEGIN_MESSAGE_DELIMITER.Length));
                    try
                    {
                        obMessage = Models.Message.Deserialize(message);
                    }
                    catch (Exception ex)
                    {
                        obMessage = null;
                        if (!Directory.Exists(@".\Logs"))
                            Directory.CreateDirectory(@".\Logs");

                        StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                        file.Write(ex.ToString());
                        file.Close();
                    }
                    if (endIndex + END_MESSAGE_DELIMITER.Length == storage[index].Storage.Length)
                        storage[index].Storage = "";
                    else
                        storage[index].Storage = storage[index].Storage.Substring(endIndex + END_MESSAGE_DELIMITER.Length);
                }
            }
            return obMessage;
        }
    }
}
