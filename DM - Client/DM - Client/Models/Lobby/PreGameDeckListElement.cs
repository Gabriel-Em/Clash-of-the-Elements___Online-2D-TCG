using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Client.Models
{
    public class PreGameDeckListElement
    {
        public int ID { get; set; }
        public string DeckName { get; set; }

        public PreGameDeckListElement(int ID_, string DeckName_)
        {
            ID = ID_;
            DeckName = DeckName_;
        }
    }
}