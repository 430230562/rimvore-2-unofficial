using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class Extension_ValidMentalStateForVore : DefModExtension
    {
        public bool forPrey = false;
        public bool forPredator = false;
        public bool forFeeder = false;

        public bool IsValid(VoreRole role)
        {
            switch(role)
            {
                case VoreRole.Predator:
                    return forPredator;
                case VoreRole.Prey:
                    return forPrey;
                case VoreRole.Feeder:
                    return forFeeder;
                default:
                    throw new ArgumentException("Unknown VoreRole");
            }
        }
    }
}
