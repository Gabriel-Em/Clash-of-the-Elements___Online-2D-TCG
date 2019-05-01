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
        private Log.Logger logger;

        public Storage(string begin_message_delimiter, string end_message_delimiter)
        {
            // initialize the logger object
            logger = new Log.Logger();

            // initialize the list of storages
            storage = new List<StorageForSocket>();

            // initialize message delimiters
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

            index = getIndexOfSocketStorage(socket);
            if (index != -1)
                storage.RemoveAt(index);
        }

        // adds new chunks of data to the storage of specified socket
        public void addChunks(string chunks, Socket socket)
        {
            int index;
            index = getIndexOfSocketStorage(socket);
            if(index != -1)
                storage[index].addChunks(chunks);
        }

        private int getIndexOfSocketStorage(Socket socket)
        {
            for (int i = 0; i < storage.Count; i++)
                if (storage[i].Socket == socket)
                    return i;
            return -1;
        }

        // scans the storage of specified socket and tries to retrieve a complete received message
        public Models.Message getWholeMessage(Socket socket)
        {
            int index;
            Models.Message obMessage = null;
            index = getIndexOfSocketStorage(socket);

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
                        logger.Log(ex.Message);
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
