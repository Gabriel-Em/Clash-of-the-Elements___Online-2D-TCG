using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DM___Server.Data
{
    public class OnGoingGamesData
    {
        private List<Models.Game> OnGoingGames;

        public OnGoingGamesData()
        {
            OnGoingGames = new List<Models.Game>();
        }

        public void createNewGame(int GameID, Socket Player1, Socket Player2)
        {
            OnGoingGames.Add(new Models.Game(GameID, Player1, Player2));
        }

        public Models.Game getGameByID(int GameID)
        {
            foreach(Models.Game game in OnGoingGames)
            {
                if (game.ID == GameID)
                {
                    return game;
                }
            }

            return null;
        }

        public void removeClient(Socket clientSocket)
        {
            for (int i = 0; i < OnGoingGames.Count; i++)
                if (OnGoingGames[i].Player1Socket == clientSocket || OnGoingGames[i].Player2Socket == clientSocket)
                {
                    OnGoingGames.RemoveAt(i);
                    break;
                }
        }

        public void removeGame(Models.Game game)
        {
            OnGoingGames.Remove(game);
        }
    }
}
