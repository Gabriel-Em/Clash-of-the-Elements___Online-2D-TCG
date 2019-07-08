using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DM___Server.Models;

namespace DM___Server.Controllers
{
    public class ControllerOnGoingGame
    {
        private Database db;
        private Data.OnGoingGamesData onGoingGamesData;
        private Data.LobbyRoomData lobbyRoomData;

        public ControllerOnGoingGame(Database db, Data.OnGoingGamesData onGoingGamesData, Data.LobbyRoomData lobbyRoomData)
        {
            this.db = db;
            this.onGoingGamesData = onGoingGamesData;
            this.lobbyRoomData = lobbyRoomData;
        }

        public Models.Response processReadyToStart(Models.ClientMessage message, Socket sender)
        {
            Models.Response response = new Models.Response(sender);
            Models.Game game;

            game = onGoingGamesData.getGameByID(Int32.Parse(message.stringArguments[0]));

            if (game.isPlayer1(sender))
                game.isP1Ready = true;
            else
                game.isP2Ready = true;

            if (game.isP1Ready && game.isP2Ready)
            {
                response.responseCommandToSockets = "READYTOGO";
                response.socketsToNotify.Add(game.Player1Socket);
                response.socketsToNotify.Add(game.Player2Socket);
            }

            return response;
        }

        public Response processFetchDeck(ClientMessage message, Socket sender)
        {
            Response response = new Response(sender);
            Models.Game game;
            string deck;

            game = onGoingGamesData.getGameByID(Int32.Parse(message.stringArguments[0]));
            deck = db.getDeckByID(Int32.Parse(message.stringArguments[1]));
            if (game.isPlayer1(sender))
                game.loadPlayer1InitialData(deck);
            else
                game.loadPlayer2InitialData(deck);
            response.responseCommandToSender = "DECKSET";

            return response;
        }

        public Response processGetHand(ClientMessage message, Socket sender)
        {
            Response response = new Response(sender);
            Models.Game game;

            game = onGoingGamesData.getGameByID(Int32.Parse(message.stringArguments[0]));
            if (game.isPlayer1(sender))
                response.commandStringArgumentsToSender = game.listPlayer1Hand.Select(x => x.ToString()).ToList<string>();
            else
                response.commandStringArgumentsToSender = game.listPlayer2Hand.Select(x => x.ToString()).ToList<string>();
            response.responseCommandToSender = "HANDRECEIVED";

            return response;
        }

        public Response processGetFirstGameState(ClientMessage message, Socket sender)
        {
            Response response = new Response(sender);

            Models.Game game = onGoingGamesData.getGameByID(Int32.Parse(message.stringArguments[0]));
            if (game.isPlayer1(sender))
                if (game.isPlayer1First)
                    response.responseCommandToSender = "YOURTURN";
                else
                    response.responseCommandToSender = "OPPTURN";
            else
            {
                if (game.isPlayer1First)
                    response.responseCommandToSender = "OPPTURN";
                else
                    response.responseCommandToSender = "YOURTURN";
            }
            return response;
        }

        public Response processPlayAsMana(GameMessage message, Socket sender)
        {
            Response response = new Response(null);

            Models.Game game = onGoingGamesData.getGameByID(message.GameID);

            if (game.isPlayer1(sender))
            {
                game.playAsMana(message.intArguments[0], 1);
                response.socketsToNotify.Add(game.Player2Socket);
            }
            else
            {
                game.playAsMana(message.intArguments[0], 2);
                response.socketsToNotify.Add(game.Player1Socket);
            }

            response.responseCommandToSockets = "PLAYEDASMANA";
            response.commandIntArgumentsToSockets = message.intArguments;
            
            return response;
        }

        public Response processSetPhase(GameMessage message, Socket sender)
        {
            Response response = new Response();

            Models.Game game = onGoingGamesData.getGameByID(message.GameID);

            if (game.isPlayer1(sender))
                response.socketsToNotify.Add(game.Player2Socket);
            else
                response.socketsToNotify.Add(game.Player1Socket);

            response.responseCommandToSockets = "SETPHASE";
            response.commandStringArgumentsToSockets = message.stringArguments;

            return response;
        }

        public Response processEndTurn(GameMessage message, Socket sender)
        {
            Response response = new Response();

            Models.Game game = onGoingGamesData.getGameByID(message.GameID);

            if (game.isPlayer1(sender))
            {
                response.socketsToNotify.Add(game.Player2Socket);
            }
            else
            {
                response.socketsToNotify.Add(game.Player1Socket);
            }
            response.responseCommandToSockets = "YOURTURN";

            return response;
        }

        public Response processStartTurn(GameMessage message, Socket sender)
        {
            Response response = new Response(sender);

            Models.Game game = onGoingGamesData.getGameByID(message.GameID);

            if (game.isPlayer1(sender))
            {
                response.commandIntArgumentsToSender.Add(game.drawCard(1));
            }
            else
            {
                response.commandIntArgumentsToSender.Add(game.drawCard(2));
            }

            response.responseCommandToSender = "ROLLON";
            return response;
        }

        public Response processDrawCard(GameMessage message, Socket sender)
        {
            Response response = new Response(sender);

            Models.Game game = onGoingGamesData.getGameByID(message.GameID);
            
            if (game.isPlayer1(sender))
            {
                response.commandIntArgumentsToSender.Add(game.drawCard(1));
                response.socketsToNotify.Add(game.Player2Socket);
            }
            else
            {
                response.commandIntArgumentsToSender.Add(game.drawCard(2));
                response.socketsToNotify.Add(game.Player1Socket);
            }

            response.responseCommandToSender = "YOURECEIVEDCARD";
            response.responseCommandToSockets = "OPPRECEIVEDCARD";
            return response;
        }

        public Response processSummon(GameMessage message, Socket sender)
        {
            Response response = new Response();

            Models.Game game = onGoingGamesData.getGameByID(message.GameID);

            if (game.isPlayer1(sender))
            {
                game.summon(message.intArguments[0], 1);
                response.socketsToNotify.Add(game.Player2Socket);
            }
            else
            {
                game.summon(message.intArguments[0], 2);
                response.socketsToNotify.Add(game.Player1Socket);
            }

            response.responseCommandToSockets = "SUMMON";
            response.commandIntArgumentsToSockets = message.intArguments;

            return response;
        }

        public Response processAttackSafeguards(GameMessage message, Socket sender)
        {
            Response response = new Response();

            Models.Game game = onGoingGamesData.getGameByID(message.GameID);

            if (game.isPlayer1(sender))
            {
                response.socketsToNotify.Add(game.Player2Socket);
            }
            else
            {
                response.socketsToNotify.Add(game.Player1Socket);
            }

            response.responseCommandToSockets = "ATTACKSAFEGUARDS";
            response.commandIntArgumentsToSockets = message.intArguments;

            return response;
        }

        public Response processBrokenSafeguards(GameMessage message, Socket sender)
        {
            Response response = new Response(sender);
            Models.Game game = onGoingGamesData.getGameByID(message.GameID);
            List<int> cardIDs = new List<int>();

            if (game.isPlayer1(sender))
            {
                foreach (int index in message.intArguments)
                    cardIDs.Add(game.breakSafeguard(index, 1));
                response.socketsToNotify.Add(game.Player2Socket);
            }
            else
            {
                foreach (int index in message.intArguments)
                    cardIDs.Add(game.breakSafeguard(index, 2));
                response.socketsToNotify.Add(game.Player1Socket);
            }

            response.responseCommandToSender = "YOURGUARDSBROKE";
            response.commandIntArgumentsToSender.Add(cardIDs.Count);
            foreach (int index in message.intArguments)
                response.commandIntArgumentsToSender.Add(index);
            foreach (int cardID in cardIDs)
                response.commandIntArgumentsToSender.Add(cardID);

            response.responseCommandToSockets = "YOUBROKEGUARDS";
            response.commandIntArgumentsToSockets = message.intArguments;

            return response;
        }

        public Response processAttackCreature(GameMessage message, Socket sender)
        {
            Response response = new Response(null);
            Models.Game game = onGoingGamesData.getGameByID(message.GameID);

            if (game.isPlayer1(sender))
            {
                response.socketsToNotify.Add(game.Player2Socket);
            }
            else
            {
                response.socketsToNotify.Add(game.Player1Socket);
            }

            response.responseCommandToSockets = "ATTACKCREATURE";
            response.commandIntArgumentsToSockets = message.intArguments;

            return response;
        }

        public Response processDoBattle(GameMessage message, Socket sender)
        {
            Response response = new Response(sender);
            Models.Game game = onGoingGamesData.getGameByID(message.GameID);

            if (game.isPlayer1(sender))
            {
                response.socketsToNotify.Add(game.Player2Socket);
            }
            else
            {
                response.socketsToNotify.Add(game.Player1Socket);
            }

            response.responseCommandToSockets = "BATTLE";
            response.commandIntArgumentsToSockets = message.intArguments;

            return response;
        }

        public Response processSendTo(GameMessage message, Socket sender)
        {
            Response response = new Response();

            Models.Game game = onGoingGamesData.getGameByID(message.GameID);

            if (game.isPlayer1(sender))
            {
                response.socketsToNotify.Add(game.Player2Socket);
            }
            else
            {
                response.socketsToNotify.Add(game.Player1Socket);
            }

            response.responseCommandToSockets = "SENDTO";
            response.commandIntArgumentsToSockets = message.intArguments;
            response.commandStringArgumentsToSockets = message.stringArguments;

            return response;
        }

        public Response processAttackOpponent(GameMessage message, Socket sender)
        {
            Response response = new Response(sender);
            Models.Game game = onGoingGamesData.getGameByID(message.GameID);

            if (game.isPlayer1(sender))
            {
                response.socketsToNotify.Add(game.Player2Socket);
            }
            else
            {
                response.socketsToNotify.Add(game.Player1Socket);
            }

            response.responseCommandToSockets = "ATTACKPLAYER";
            response.commandIntArgumentsToSockets = message.intArguments;

            return response;
        }

        public Response processISurrender(GameMessage message, Socket sender)
        {
            Response response = new Response(sender);
            Models.Game game = onGoingGamesData.getGameByID(message.GameID);
            string victorUsername;
            string defeatedUsername;

            if (game == null)
                return response;

            if (game.isPlayer1(sender))
            {
                victorUsername = lobbyRoomData.getUsernameBySocket(game.Player2Socket);
                defeatedUsername = lobbyRoomData.getUsernameBySocket(game.Player1Socket);
                response.socketsToNotify.Add(game.Player2Socket);
            }
            else
            {
                victorUsername = lobbyRoomData.getUsernameBySocket(game.Player1Socket);
                defeatedUsername = lobbyRoomData.getUsernameBySocket(game.Player2Socket);
                response.socketsToNotify.Add(game.Player1Socket);
            }

            db.updateUserData(victorUsername, defeatedUsername);
            onGoingGamesData.removeGame(game);

            response.responseCommandToSockets = "OPPSURRENDERED";
            response.commandIntArgumentsToSockets = message.intArguments;

            return response;
        }
    }
}
