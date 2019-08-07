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
        private static Log.Logger logger;

        public Server()
        {
            logger = new Log.Logger();
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
                logger.Log(e.Message);
            }
        }

        // ACCEPTING CLIENTS //

        private static void AcceptCallBack(IAsyncResult AR)
        {
            try
            {
                Socket socket = _serverSocket.EndAccept(AR);
                Storage.createStorage(socket);
                Console.WriteLine("Client connected on IP [" + IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Address.ToString()) + "] as [Guest]");
                socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceieveCallBack), socket);
                _serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null); // we begin accepting any new incoming clients
            }
            catch (Exception e)
            {
                logger.Log(e.Message);
            }
        }

        // RECEIVING DATA FROM CLIENTS //

        private static void ReceieveCallBack(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;

            try
            {
                // we received data from a client and we parse it
                int received = socket.EndReceive(AR);
                byte[] dataBuf = new byte[received];
                Array.Copy(_buffer, dataBuf, received);
                string message = Encoding.ASCII.GetString(dataBuf);

                // we add the chunks of data we received from our client to its Storage
                Storage.addChunks(message, socket);

                Models.Message obMessage;
                while ((obMessage = Storage.getWholeMessage(socket)) != null)
                {
                    processResponseMessage(ctrl.messageProcessor(obMessage, socket));
                }
                socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceieveCallBack), socket);
            }
            catch (SocketException)
            {
                clientDisconnected(socket);
            }
            catch (Exception ex)
            {
                logger.Log(ex.ToString());
            }
        }

        // ACTIONS TAKEN AFTER RECEIVING DATA FROM CLIENTS //

        private static void processResponseMessage(Models.Response response)
        {
            try
            {
                if (response != null)
                {
                    if (response.Type == 1)
                    {
                        Models.ClientMessage cm;

                        if (response.responseCommandToSender != null)
                        {
                            cm = new Models.ClientMessage(
                                response.responseCommandToSender,
                                response.commandStringArgumentsToSender,
                                response.commandIntArgumentsToSender,
                                response.CardCollection
                                );
                            send(Models.Message.Serialize(Models.Message.FromValue(cm)), response.sender);
                        }

                        if (response.responseCommandToSockets != null)
                        {
                            cm = new Models.ClientMessage(
                                response.responseCommandToSockets,
                                response.commandIntArgumentsToSockets,
                                response.commandStringArgumentsToSockets
                                );
                            foreach (Socket s in response.socketsToNotify)
                            {
                                send(Models.Message.Serialize(Models.Message.FromValue(cm)), s);
                            }
                        }
                    }
                    else
                    {
                        Models.GameMessage gm;

                        if (response.responseCommandToSender != null)
                        {
                            gm = new Models.GameMessage(
                                response.responseCommandToSender,
                                response.commandStringArgumentsToSender,
                                response.commandIntArgumentsToSender);
                            send(Models.Message.Serialize(Models.Message.FromValue(gm)), response.sender);
                        }

                        if (response.responseCommandToSockets != null)
                        {
                            gm = new Models.GameMessage(
                                response.responseCommandToSockets,
                                response.commandStringArgumentsToSockets,
                                response.commandIntArgumentsToSockets
                                );
                            foreach (Socket s in response.socketsToNotify)
                            {
                                send(Models.Message.Serialize(Models.Message.FromValue(gm)), s);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(e.ToString());
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
            catch (SocketException)
            {
                clientDisconnected(socket);
            }
            catch (Exception e)
            {
                logger.Log(e.ToString());
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
                logger.Log(e.ToString());
            }
        }

        // PROCESSING A CLIENTDISCONNECTED EVENT //

        private static void clientDisconnected(Socket clientSocket)
        {
            try
            {
                Console.WriteLine("Client [" + IPAddress.Parse(((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString()) + "] disconnected");
                // if the user that disconnected was logged in
                if (!ctrl.checkIfUserIsGuest(clientSocket))
                {
                    Models.Response response = new Models.Response();

                    // we notify every client to remove the user from their lobby
                    response.socketsToNotify = ctrl.lobbyRoomData.lobbyRoomUsersToSocketList();
                    response.responseCommandToSockets = "REMOVELOBBYROOMUSER";
                    response.commandStringArgumentsToSockets.Add(ctrl.lobbyRoomData.getNickNameBySocket(clientSocket));

                    Models.ClientMessage cm = new Models.ClientMessage(
                        response.responseCommandToSockets,
                        response.commandStringArgumentsToSockets
                        );

                    response.socketsToNotify.Remove(clientSocket);
                    foreach (Socket s in response.socketsToNotify)
                    {
                        send(Models.Message.Serialize(Models.Message.FromValue(cm)), s);
                    }
                    ctrl.removeClient(clientSocket);
                }
                Storage.removeStorage(clientSocket);
                clientSocket.Close();
                clientSocket.Dispose();
            }
            catch (Exception ex)
            {
                logger.Log(ex.ToString());
            }
        }
    }
}