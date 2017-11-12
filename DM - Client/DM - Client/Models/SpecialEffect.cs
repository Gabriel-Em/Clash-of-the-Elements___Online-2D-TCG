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
        public List<string> Arguments { get; set; }
        public string TargetFrom { get; set; }
        public string TargetTo { get; set; }

        public SpecialEffect(string effect, List<string> arguments, string targetFrom, string targetTo)
        {
            Effect = effect;
            Arguments = arguments;
            TargetFrom = targetFrom;
            TargetTo = TargetTo;
        }

        public SpecialEffect(string effect, string targetFrom, string targetTo)
        {
            Effect = effect;
            Arguments = null;
            TargetFrom = targetFrom;
            TargetTo = TargetTo;
        }

        public SpecialEffect(string effect, List<string> arguments, string targetFrom)
        {
            Effect = effect;
            Arguments = arguments;
            TargetFrom = targetFrom;
            TargetTo = null;
        }

        public SpecialEffect(string effect, List<string> arguments)
        {
            Effect = effect;
            Arguments = arguments;
            TargetFrom = null;
            TargetTo = null;
        }

        public SpecialEffect(string effect)
        {
            Effect = effect;
            Arguments = null;
            TargetFrom = null;
            TargetTo = null;
        }

        public SpecialEffect()
        {
            Effect = null;
            Arguments = null;
            TargetFrom = null;
            TargetTo = null;
        }
    }
}
