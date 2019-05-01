using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Client.Models
{
    public class CardCollection
    {
        public List<Card> Cards { get; set; }

        public CardCollection()
        {
            Cards = new List<Card>();
        }

        public CardCollection(List<Card> cards_)
        {
            Cards = cards_;
        }

        public void AddCard(Card card)
        {
            Cards.Add(card);
        }

        public CardCollection Clone()
        {
            return new CardCollection(Cards.ToList<Card>());
        }

        public Models.Card getCardById(int id)
        {
            return Cards[id - 1];
        }

        public void setCards(List<Card> cards)
        {
            Cards.Clear();
            Cards = cards.ToList<Card>();
        }
    }
}
