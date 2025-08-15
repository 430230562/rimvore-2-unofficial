using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class ProposalPawnData : IExposable
    {
        public int CurrentProposalCooldown = -1;
        public int LastProposalTick = -1;

        public void ExposeData()
        {
            Scribe_Values.Look(ref CurrentProposalCooldown, "CurrentProposalCooldown");
            Scribe_Values.Look(ref LastProposalTick, "LastProposalTick");
        }
    }
}
