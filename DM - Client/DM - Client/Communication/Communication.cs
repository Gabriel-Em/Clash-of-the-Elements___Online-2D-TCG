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
        private DispatcherTimer listenTimer = new DispatcherTimer();
        private DispatcherTimer connectionTimer = new DispatcherTimer();
        private DispatcherTimer sendTimer = new DispatcherTimer();
        private BackgroundWorker receiveBackgroundWorker = new BackgroundWorker();
        private BackgroundWorker connectBackgroundWorker = new BackgroundWorker();
        private BackgroundWorker sendBackgroundWorker = new BackgroundWorker();
        private static Socket _clientSocket;
        private List<Models.ClientMessage> cDataToSend;
        private List<Models.GameMessage> gDataToSend;
        private List<Models.Message> dataReceived;
        private int attempts;
        private int PORT = 100;
        private IPAddress serverAddress = IPAddress.Parse("127.0.0.1");
        private string storage;
        private const string BEGIN_MESSAGE_DELIMITER = "[[_~_begin_~_]]";
        private const string END_MESSAGE_DELIMITER = "[[_!_end_!_]]";
        private object _locker = 0;
        public string Status { get; set; }
        
        // CONSTRUCTOR //

        public Communication()
        {
            Status = "Not connected!";
            storage = string.Empty;
            cDataToSend = new List<Models.ClientMessage>();
            gDataToSend = new List<Models.GameMessage>();
            dataReceived = new List<Models.Message>();

            receiveBackgroundWorker.DoWork += Listen;
            connectBackgroundWorker.DoWork += connectBackgroundWorker_DoWork;
            sendBackgroundWorker.DoWork += sendBackgroundWorker_DoWork;

            listenTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            listenTimer.Tick += listen_Timer_Tick;
            connectionTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            connectionTimer.Tick += connection_Timer_Tick;
            sendTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            sendTimer.Tick += send_Timer_Tick;
        }

        // CONNECTING TO SERVER //

        public void tryConnect()
        {
            Status = "Not connected! | Connecting to server...";
            attempts = 1;

            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            connectionTimer.Start();
        }

        private void connectBackgroundWorker_DoWork(object sender, EventArgs e)
        {
            try
            {
                _clientSocket.Connect(serverAddress, PORT);
                Status = "Connected!";
                connectionTimer.Stop();
            }
            catch (SocketException)
            {
                attempts++;
                if (attempts == 6)
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
            if (attempts < 6)
            {
                connectionTimer.IsEnabled = false;

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

        private void Listen(object sender, EventArgs e)
        {
            try
            {
                byte[] receivedBuf = new byte[10000];
                int received = _clientSocket.Receive(receivedBuf);
                byte[] receivedMessage = new byte[received];
                Array.Copy(receivedBuf, receivedMessage, received);
                string message = Encoding.ASCII.GetString(receivedMessage);
                storage += message;
            }
            catch (Exception ex)
            {
                if (!Directory.Exists(@".\Logs"))
                    Directory.CreateDirectory(@".\Logs");

                StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + "_Crash_Log.txt");
                file.Write(ex.ToString());
                file.Close();

                listenTimer.Stop();
                _clientSocket.Dispose();
                dataReceived.Add(Models.Message.FromValue(new Models.ClientMessage("DISCONNECTED")));
            }

            storageToQueue();
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
                if (endIndex >= 0 && beginIndex >= 0 && endIndex < beginIndex)
                    storage = storage.Substring(beginIndex);
                else
                {
                    message = storage.Substring(beginIndex + BEGIN_MESSAGE_DELIMITER.Length,
                        endIndex - (beginIndex + BEGIN_MESSAGE_DELIMITER.Length));
                    try
                    {
                        Models.Message obMessage = Models.Message.Deserialize(message);
                        lock (_locker)
                            dataReceived.Add(obMessage);
                    }
                    catch (Exception ex)
                    {
                        if (!Directory.Exists(@".\Logs"))
                            Directory.CreateDirectory(@".\Logs");

                        StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + "_Crash_Log.txt");
                        file.Write(ex.ToString());
                        file.Close();
                        sendTimer.Stop();
                    }
                    if (endIndex + END_MESSAGE_DELIMITER.Length == storage.Length)
                        storage = "";
                    else
                        storage = storage.Substring(endIndex + END_MESSAGE_DELIMITER.Length);
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
            if (cDataToSend.Count != 0 || gDataToSend.Count != 0)
            {
                try
                {
                    byte[] data;
                    if (cDataToSend.Count != 0)
                    {
                        data = Encoding.ASCII.GetBytes(BEGIN_MESSAGE_DELIMITER + Models.Message.Serialize(Models.Message.FromValue(cDataToSend[0])) + END_MESSAGE_DELIMITER);
                        cDataToSend.RemoveAt(0);
                        _clientSocket.Send(data);
                    }
                    else
                    {
                        data = Encoding.ASCII.GetBytes(BEGIN_MESSAGE_DELIMITER + Models.Message.Serialize(Models.Message.FromValue(gDataToSend[0])) + END_MESSAGE_DELIMITER);
                        gDataToSend.RemoveAt(0);
                        _clientSocket.Send(data);
                    }
                }
                catch (Exception ex)
                {
                    if (!Directory.Exists(@".\Logs"))
                        Directory.CreateDirectory(@".\Logs");

                    StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + "_Crash_Log.txt");
                    file.Write(ex.ToString());
                    file.Close();
                    sendTimer.Stop();
                }
            }
            else
                sendTimer.Stop();
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