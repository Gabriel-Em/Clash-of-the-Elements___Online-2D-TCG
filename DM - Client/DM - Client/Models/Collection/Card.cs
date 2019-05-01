using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Client.Models
{
    [Serializable]
    public class Card
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Element { get; set; }
        public int Cost { get; set; }
        public string Race { get; set; }
        public string Text { get; set; }
        public int Power { get; set; }
        public List<SpecialEffect> SpecialEffects { get; set; }

        public Card()
        {
            ID = -1;
            Name = null;
            Type = null;
            Element = null;
            Cost = -1;
            Race = null;
            Text = null;
            Power = -1;
            SpecialEffects = null;
        }

        public Card(
            int ID_,
            string Name_,
            string Type_,
            string Element_,
            int Cost_,
            string Race_,
            string Text_,
            int Power_,
            List<SpecialEffect> SpecialEffects_)
        {
            ID = ID_;
            Name = Name_;
            Type = Type_;
            Element = Element_;
            Cost = Cost_;
            Race = Race_;
            Text = Text_;
            Power = Power_;
            SpecialEffects = SpecialEffects_;
        }

        public Card(Card card)
        {
            ID = card.ID;
            Name = card.Name;
            Type = card.Type;
            Element = card.Element;
            Cost = card.Cost;
            Race = card.Race;
            Text = card.Text;
            Power = card.Power;
            SpecialEffects = card.SpecialEffects.ToList<SpecialEffect>();
        }
    }
}
