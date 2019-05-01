using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Client.Models
{
    [Serializable]
    public class SpecialEffect
    {
        public string Effect { get; set; }
        public string Trigger { get; set; }
        public string Condition { get; set; }
        public List<int> Arguments { get; set; }
        public string TargetFrom { get; set; }
        public string TargetTo { get; set; }

        public SpecialEffect(string effect, string trigger, string condition, List<int> arguments, string targetFrom, string targetTo)
        {
            Effect = effect;
            Trigger = trigger;
            Condition = condition;
            Arguments = arguments;
            TargetFrom = targetFrom;
            TargetTo = targetTo;
        }
    }
}
