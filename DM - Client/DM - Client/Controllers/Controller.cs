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
        protected Communication com;                        // server communication class
        protected List<bool> loadedDataChecklist { get; set; }       // checklist for which data was loaded (that's being displayed by the LoadingGUI page)
        public bool Listening { get; set; }                 // 
        private Log.Logger logger;

        public Controller()
        {
            logger = new Log.Logger();
            Listening = false;
        }

        public void tryConnect()        // try to connect to the server
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

        public List<bool> getLoadedDataChecklist()
        {
            return loadedDataChecklist;
        }

        public void messageProcessor(List<Models.Message> messageList)
        {
            foreach (Models.Message message in messageList)
            {
                try
                {
                    if (message.Type == "ClientMessage")
                    {
                        Models.ClientMessage cm = message.Value.ToObject<Models.ClientMessage>(); // parse Message to ClientMessage
                        clientCommandProcessor(cm);
                    }
                    else if (message.Type == "GameMessage")
                    {
                        Models.GameMessage gm = message.Value.ToObject<Models.GameMessage>(); // parse Message to GameMessage
                        gameCommandProcessor(gm);
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(ex.ToString());
                }
            }
        }

        public virtual void clientCommandProcessor(Models.ClientMessage message) { }
        public virtual void gameCommandProcessor(Models.GameMessage message) { }
    }
}