using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DM___Client.Controllers
{
    public abstract class Controller
    {
        protected Communication com;
        protected List<bool> loadedData { get; set; }
        public bool Listening { get; set; }

        public Controller()
        {
            Listening = false;
        }

        public void tryConnect()
        {
            com.tryConnect();
        }

        public string getStatus()
        {
            return com.Status;
        }

        public void beginListen()
        {
            Listening = true;
            com.startListening();
        }

        public void stopListen()
        {
            Listening = false;
            com.stopListening();
        }

        public bool hasReceivedResponse()
        {
            return com.thereIsData();
        }

        public List<Models.Message> getReceivedResponse()
        {
            return com.getReceivedResponse();
        }

        public void send(Models.ClientMessage message)
        {
            com.send(message);
        }

        public void send(Models.GameMessage message)
        {
            com.send(message);
        }

        public virtual void loadPageData() { }

        public List<bool> getLoadedData()
        {
            return loadedData;
        }

        public void messageProcessor(List<Models.Message> messageList)
        {
            foreach (Models.Message message in messageList)
            {
                try
                {
                    if (message.Type == "ClientMessage")
                    {
                        Models.ClientMessage cm = message.Value.ToObject<Models.ClientMessage>();
                        commandProcessor(cm);
                    }
                    else if (message.Type == "GameMessage")
                    {
                        Models.GameMessage gm = message.Value.ToObject<Models.GameMessage>();
                        gameCommandProcessor(gm);
                    }
                }
                catch (Exception ex)
                {
                    if (!Directory.Exists(@".\Logs"))
                        Directory.CreateDirectory(@".\Logs");

                    StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                    file.Write(ex.ToString());
                    file.Close();
                }
            }
        }

        public virtual void commandProcessor(Models.ClientMessage message) { }
        public virtual void gameCommandProcessor(Models.GameMessage message) { }
    }
}