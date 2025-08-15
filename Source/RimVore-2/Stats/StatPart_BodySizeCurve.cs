using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
	public class StatPart_BodySizeCurve : StatPart
	{
		private SimpleCurve curve;
		private bool ActiveFor(Pawn pawn)
		{
			return true;
		}
		public override void TransformValue(StatRequest req, ref float val)
		{
			Pawn pawn = req.Thing as Pawn;
			if(pawn == null)
            {
				return;
            }

			val *= curve.Evaluate(pawn.BodySize);
		}

		public override string ExplanationPart(StatRequest req)
		{
			Pawn pawn = req.Thing as Pawn;
			if(pawn == null)
			{
				return null;
			}

			return $"{"RV2_StatsReport_FactorForBodySize".Translate(pawn.BodySize)}: ({pawn.BodySize}) -> x {curve.Evaluate(pawn.BodySize).ToStringByStyle(ToStringStyle.PercentZero)}";
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach(string error in base.ConfigErrors())
            {
				yield return error;
            }

            if(curve == null)
                yield return $"Required field \"{nameof(curve)}\" is not set";
			yield break;
		}

	}
}
