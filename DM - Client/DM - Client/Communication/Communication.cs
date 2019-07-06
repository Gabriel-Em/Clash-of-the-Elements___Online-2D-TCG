using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DM___Client
{
    public class Communication
    {
        // timers that keep assigning work to background workers when they finish their work 
        private DispatcherTimer listenTimer = new DispatcherTimer();
        private DispatcherTimer connectionTimer = new DispatcherTimer();
        private DispatcherTimer sendTimer = new DispatcherTimer();

        // background workers responsible with communicating with the server
        private BackgroundWorker receiveBackgroundWorker = new BackgroundWorker();
        private BackgroundWorker connectBackgroundWorker = new BackgroundWorker();
        private BackgroundWorker sendBackgroundWorker = new BackgroundWorker();

        // our Client's Socket
        private static Socket _clientSocket;

        // Array of messages that need to be delivered to the server
        private List<Models.ClientMessage> cDataToSend;
        private List<Models.GameMessage> gDataToSend;

        // Array of messages that came from the server
        private List<Models.Message> dataReceived;

        // Number of attemtps we're trying to connect to the server
        private int attempts;

        // Server's Port and IP Address
        private int PORT = 100;
        private IPAddress serverAddress = IPAddress.Parse("78.96.147.160");

        // the raw bytes we received from the server (might be an incomplete message, so we keep storing bytes from the server until we have a complete message
        private string storage;

        // delimiters that delimit a message within an array of bytes received from the server
        private const string BEGIN_MESSAGE_DELIMITER = "[[_~_begin_~_]]";
        private const string END_MESSAGE_DELIMITER = "[[_!_end_!_]]";

        // a mutex
        private object _locker = 0;

        // the status of the current connection to server
        public string Status { get; set; }

        // a logger object
        private Log.Logger logger;

        // some macros
        private const int MESSAGE_TYPE_CLIENT = 0;
        private const int MESSAGE_TYPE_GAME = 1;

        // CONSTRUCTOR //

        public Communication()
        {
            // initialize the strings
            Status = "Not connected!";
            storage = string.Empty;

            // initialize the lists
            cDataToSend = new List<Models.ClientMessage>();
            gDataToSend = new List<Models.GameMessage>();
            dataReceived = new List<Models.Message>();

            // asign methods to background workers
            receiveBackgroundWorker.DoWork += Listen;
            connectBackgroundWorker.DoWork += connectBackgroundWorker_DoWork;
            sendBackgroundWorker.DoWork += sendBackgroundWorker_DoWork;

            // initialize timers
            listenTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            listenTimer.Tick += listen_Timer_Tick;
            connectionTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            connectionTimer.Tick += connection_Timer_Tick;
            sendTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            sendTimer.Tick += send_Timer_Tick;

            // initialize the logger object

            logger = new Log.Logger();
        }

        // CONNECTING TO SERVER //

        public void tryConnect()        // try to connect to the server
        {
            // initial status while trying to connect
            Status = "Not connected! | Connecting to server...";

            // set the index of attempts to 1
            attempts = 1;                                           

            // create the Client Socket
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // start the timer that will try to connect to the server
            connectionTimer.Start();                                
        }

        private void connectBackgroundWorker_DoWork(object sender, EventArgs e)
        {
            try
            {
                // try to connect to the server
                _clientSocket.Connect(serverAddress, PORT);
                Status = "Connected!";

                // if connection was successful stop trying to connect
                connectionTimer.Stop();
            }
            catch (SocketException)             
            {
                // if we failed to connect

                attempts++;
                if (attempts > 5)
                {
                    Status = "Server unreachable! | Could not connect to server after 5 attempts, please retry... ";
                    connectionTimer.Stop();
                }
                else
                    Status = "Not connected! | Attempt " + attempts + " to connect to server...";
            }
        }

        private void connection_Timer_Tick(object sender, EventArgs e)
        {
            if (attempts <= 5)
            {
                connectionTimer.IsEnabled = false;

                // wait for the background worker to finish whatever they're doing and
                if (!connectBackgroundWorker.IsBusy)                        
                    connectBackgroundWorker.RunWorkerAsync();

                connectionTimer.IsEnabled = true;
            }
            else
                connectionTimer.Stop();
        }

        // RECEIVING MESSAGES FROM SERVER //

        public void startListening()
        {
            listenTimer.Start();
        }

        public void stopListening()
        {
            listenTimer.Stop();
        }

        // the receiveBackgroundWorker uses this method to receive data from the server
        private void Listen(object sender, EventArgs e)
        {
            try
            {
                // try to receive data and store it in a buffer
                byte[] receivedBuf = new byte[23000];
                int received = _clientSocket.Receive(receivedBuf);

                // truncate the data
                byte[] receivedMessage = new byte[received];
                Array.Copy(receivedBuf, receivedMessage, received);

                // byte array to string
                string message = Encoding.ASCII.GetString(receivedMessage);

                // store data in storage, parse it, and move received messages to their respective queues
                storage += message;
                storageToQueue();
            }
            catch (Exception ex)
            {
                logger.Log(ex.ToString());

                listenTimer.Stop();
                _clientSocket.Dispose();
                dataReceived.Add(Models.Message.FromValue(new Models.ClientMessage("DISCONNECTED")));
            }
        }

        private void listen_Timer_Tick(object sender, EventArgs e)
        {
            listenTimer.IsEnabled = false;

            if (!receiveBackgroundWorker.IsBusy)
                receiveBackgroundWorker.RunWorkerAsync();

            listenTimer.IsEnabled = true;
        }

        private void storageToQueue()
        {
            int beginIndex = storage.IndexOf(BEGIN_MESSAGE_DELIMITER);
            int endIndex = storage.IndexOf(END_MESSAGE_DELIMITER);
            string message;

            while (beginIndex >= 0 && endIndex >= 0)
            {
                // we remove corrupted messages from storage
                if (endIndex >= 0 && beginIndex >= 0 && endIndex < beginIndex)
                    storage = storage.Substring(beginIndex);
                else
                {
                    // we have found a complete message and we convert it to a message object
                    message = storage.Substring(beginIndex + BEGIN_MESSAGE_DELIMITER.Length,
                        endIndex - (beginIndex + BEGIN_MESSAGE_DELIMITER.Length));
                    try
                    {
                        Models.Message obMessage = Models.Message.Deserialize(message);
                        lock (_locker)
                        {
                            dataReceived.Add(obMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(ex.ToString());
                        sendTimer.Stop();
                    }
                    finally
                    {
                        // we clean the storage of the END_MESSAGE_DELIMITER after we parsed the message
                        if (endIndex + END_MESSAGE_DELIMITER.Length == storage.Length)
                            storage = "";
                        else
                            storage = storage.Substring(endIndex + END_MESSAGE_DELIMITER.Length);
                    }
                }
                beginIndex = storage.IndexOf(BEGIN_MESSAGE_DELIMITER);
                endIndex = storage.IndexOf(END_MESSAGE_DELIMITER);
            }
        }

        // SENDING MESSAGES TO SERVER //

        public void send(Models.ClientMessage message)
        {
            cDataToSend.Add(message);
            sendTimer.Start();
        }

        public void send(Models.GameMessage message)
        {
            gDataToSend.Add(message);
            sendTimer.Start();
        }

        private void sendBackgroundWorker_DoWork(object sender, EventArgs e)
        {
            // if we have data to send in either queue
            if (cDataToSend.Count != 0 || gDataToSend.Count != 0)
            {
                try
                {
                    if (cDataToSend.Count != 0)
                    {
                        _clientSocket.Send(encodeSendMessage(MESSAGE_TYPE_CLIENT));
                    }
                    else
                    {
                        _clientSocket.Send(encodeSendMessage(MESSAGE_TYPE_GAME));
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(ex.ToString());
                    sendTimer.Stop();
                }
            }
            else
                sendTimer.Stop();
        }

        private byte[] encodeSendMessage(int messageType)
        {
            Models.Message message;

            switch (messageType)
            {
                case MESSAGE_TYPE_CLIENT:
                    message = Models.Message.FromValue(cDataToSend[0]);
                    cDataToSend.RemoveAt(0);
                    break;
                case MESSAGE_TYPE_GAME:
                    message = Models.Message.FromValue(gDataToSend[0]);
                    gDataToSend.RemoveAt(0);
                    break;
                default:
                    return null;
            }

            return Encoding.ASCII.GetBytes(
                BEGIN_MESSAGE_DELIMITER +
                Models.Message.Serialize(message) +
                END_MESSAGE_DELIMITER);
        }

        private void send_Timer_Tick(object sender, EventArgs e)
        {
            sendTimer.IsEnabled = false;

            if (!sendBackgroundWorker.IsBusy)
                sendBackgroundWorker.RunWorkerAsync();

            sendTimer.IsEnabled = true;
        }

        // DELIVERING SERVER MESSAGES TO THE ACTIVE GUI //

        public bool thereIsData()
        {
            if (dataReceived.Count != 0)
                return true;
            return false;
        }

        public List<Models.Message> getReceivedResponse()
        {
            if (dataReceived.Count != 0)
            {
                List<Models.Message> response;

                lock (_locker)
                {
                    response = dataReceived.ToList<Models.Message>();
                    dataReceived.Clear();
                }

                return response;
            }
            else
                return null;
        }
    }
}