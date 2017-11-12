using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace DM___Server
{
    public class Server
    {
        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static Models.Storage Storage;
        private static Controller ctrl;
        private static byte[] _buffer = new byte[10000];
        private const string BEGIN_MESSAGE_DELIMITER = "[[_~_begin_~_]]";
        private const string END_MESSAGE_DELIMITER = "[[_!_end_!_]]";
        private const int PORT = 100;
        
        public Server()
        {
            ctrl = new Controller();
            Storage = new Models.Storage(BEGIN_MESSAGE_DELIMITER, END_MESSAGE_DELIMITER);
            SetupServer();
        }

        // SETTING UP THE SERVER //

        private static void SetupServer()
        {
            try
            {
                Console.WriteLine("Setting up server...\n");
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
                Console.WriteLine("Done\nListening...");
                _serverSocket.Listen(5);
                _serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
            }
            catch (Exception e)
            {
                if (!Directory.Exists(@".\Logs"))
                    Directory.CreateDirectory(@".\Logs");

                StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                file.Write(e.ToString());
                file.Close();
            }
        }

        // ACCEPTING CLIENTS //

        private static void AcceptCallBack(IAsyncResult AR)
        {
            try
            {
                Socket socket = _serverSocket.EndAccept(AR);
                Storage.createStorage(socket);
                Console.WriteLine("Client connected on IP [" + IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Address.ToString()) + "] as [" + ctrl.getUsernameOfClientSocket(socket) + "]");
                socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceieveCallBack), socket);
                _serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null); // we begin accepting any new incoming clients
            }
            catch (Exception e)
            {
                if (!Directory.Exists(@".\Logs"))
                    Directory.CreateDirectory(@".\Logs");

                StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                file.Write(e.ToString());
                file.Close();
            }
        }

        // RECEIVING DATA FROM CLIENTS //

        private static void ReceieveCallBack(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;

            try
            {
                int received = socket.EndReceive(AR);
                byte[] dataBuf = new byte[received];
                Array.Copy(_buffer, dataBuf, received);
                string message = Encoding.ASCII.GetString(dataBuf);

                Storage.addChunks(message, socket);

                Models.Message obMessage;
                while ((obMessage = Storage.getWholeMessage(socket)) != null)
                    processResponseMessage(ctrl.messageProcessor(obMessage, socket), socket);

                socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceieveCallBack), socket);
            }
            catch (Exception e)
            {
                if (e.Message == "An existing connection was forcibly closed by the remote host")
                {
                    clientDisconnected(socket);
                }
                else
                {
                    if (!Directory.Exists(@".\Logs"))
                        Directory.CreateDirectory(@".\Logs");

                    StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                    file.Write(e.ToString());
                    file.Close();
                }
            }
        }

        // ACTIONS TAKEN AFTER RECEIVING DATA FROM CLIENTS //

        private static void processResponseMessage(Models.Response response, Socket socket)
        {
            try
            {
                if (response != null)
                {
                    Console.WriteLine("Received command [" + response.AssignedCommand + "] from client [" + ctrl.getUsernameOfClientSocket(socket) + ":" + IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Address.ToString()) + "]");

                    switch (response.messageType)
                    {
                        case 0:
                            if (response.cMessageToSender.Command != "NotRequired")
                                send(Models.Message.Serialize(Models.Message.FromValue(response.cMessageToSender)), socket);
                            if (response.cMessageToSockets.Command != "NotRequired")
                            {
                                foreach (Socket s in response.cSockets)
                                    send(Models.Message.Serialize(Models.Message.FromValue(response.cMessageToSockets)), s);
                            }
                            break;
                        case 1:
                            break;
                        case 2:
                            foreach (Models.Card Card in response.CardCollection)
                                send(Models.Message.Serialize(Models.Message.FromValue(new Models.ClientMessage("ADDNEWCARDTOCOLLECTION", Card))), socket);
                            send(Models.Message.Serialize(Models.Message.FromValue(new Models.ClientMessage("COLLECTIONSENT"))),socket);
                            break;
                        case 3:
                                foreach (Socket s in response.cSockets)
                                    send(Models.Message.Serialize(Models.Message.FromValue(new Models.ClientMessage("ADDGAMEROOM", response.GameRooms[0]))), s);
                            break;
                        case 4: 
                            foreach(List<string> room in response.GameRooms)
                                send(Models.Message.Serialize(Models.Message.FromValue(new Models.ClientMessage("ADDGAMEROOM", room))), socket);
                            send(Models.Message.Serialize(Models.Message.FromValue(new Models.ClientMessage("ROOMSSENT"))), socket);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                if (!Directory.Exists(@".\Logs"))
                    Directory.CreateDirectory(@".\Logs");

                StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                file.Write(e.ToString());
                file.Close();
            }
        }

        // SENDING DATA TO CLIENTS //

        public static void send(string message, Socket socket)
        {
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(BEGIN_MESSAGE_DELIMITER + message + END_MESSAGE_DELIMITER);
                socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallBack), socket);
            }
            catch (Exception e)
            {
                if (e.Message != "An existing connection was forcibly closed by the remote host")
                {
                    if (!Directory.Exists(@".\Logs"))
                        Directory.CreateDirectory(@".\Logs");

                    StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                    file.Write(e.ToString());
                    file.Close();
                }
            }
        }

        private static void SendCallBack(IAsyncResult AR)
        {
            try
            {
                Socket socket = (Socket)AR.AsyncState;
                socket.EndSend(AR);
            }
            catch (Exception e)
            {
                if (!Directory.Exists(@".\Logs"))
                    Directory.CreateDirectory(@".\Logs");

                StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                file.Write(e.ToString());
                file.Close();
            }
        }

        // PROCESSING A CLIENTDISCONNECTED EVENT //

        private static void clientDisconnected(Socket socket)
        {
            string data;
            data = ctrl.getUsernameOfClientSocket(socket);
            Console.WriteLine("Client [" + ctrl.getUsernameOfClientSocket(socket) + ":" + IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Address.ToString()) + "] disconnected");
            if (data != "<Guest>" && ctrl.checkUserLoggedIn(data))
            {
                data = ctrl.getNickNameOfClientSocket(socket);
                List<string> commandArguments = new List<string>();
                commandArguments.Add(data);
                List<Socket> sockets = ctrl.lobbyRoomUsersToSocketList();
                sockets.Remove(socket);
                foreach (Socket s in sockets)
                    send(Models.Message.Serialize(Models.Message.FromValue(new Models.ClientMessage("REMOVELOBBYROOMUSER", commandArguments))), s);
            }

            ctrl.removeClient(socket);
            Storage.removeStorage(socket);
            socket.Close();
            socket.Dispose();
        }
    }
}