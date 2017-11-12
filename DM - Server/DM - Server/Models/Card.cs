using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Server.Models
{
    [Serializable]
    public class Card
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Civilization { get; set; }
        public int Cost { get; set; }
        public string Race { get; set; }
        public string Text { get; set; }
        public int Power { get; set; }
        public int ManaNumber { get; set; }
        public string Set { get; set; }
        public List<SpecialEffect> SpecialEffects { get; set; }

        public Card(int ID_, string Name_, string Type_, string Civilization_, int Cost_, string Race_, string Text_, int Power_, int ManaNumber_, string Set_, List<SpecialEffect> SpecialEffects_)
        {
            ID = ID_;
            Name = Name_;
            Type = Type_;
            Civilization = Civilization_;
            Cost = Cost_;
            Race = Race_;
            Text = Text_;
            Power = Power_;
            ManaNumber = ManaNumber_;
            Set = Set_;
            SpecialEffects = SpecialEffects_;
        }

        public Card(int ID_, string Name_, string Type_, string Civilization_, int Cost_, string Text_, int ManaNumber_, string Set_, List<SpecialEffect> SpecialEffects_)
        {
            ID = ID_;
            Name = Name_;
            Type = Type_;
            Civilization = Civilization_;
            Cost = Cost_;
            Race = null;
            Text = Text_;
            Power = -1;
            ManaNumber = ManaNumber_;
            Set = Set_;
            SpecialEffects = SpecialEffects_;
        }

        public Card(int ID_, string Name_, string Type_, string Civilization_, int Cost_, string Race_, int Power_, int ManaNumber_, string Set_)
        {
            ID = ID_;
            Name = Name_;
            Type = Type_;
            Civilization = Civilization_;
            Cost = Cost_;
            Race = Race_;
            Text = null;
            Power = Power_;
            ManaNumber = ManaNumber_;
            Set = Set_;
            SpecialEffects = null;
        }

        public Card(Card card)
        {
            ID = card.ID;
            Name = card.Name;
            Type = card.Type;
            Civilization = card.Civilization;
            Cost = card.Cost;
            Race = card.Race;
            Text = card.Text;
            Power = card.Power;
            ManaNumber = card.ManaNumber;
            Set = card.Set;
            SpecialEffects = card.SpecialEffects.ToList<Models.SpecialEffect>();
        }
    }
}