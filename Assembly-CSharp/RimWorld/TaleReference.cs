using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class TaleReference : IExposable
	{
		private Tale tale;

		private int seed;

		public static TaleReference Taleless
		{
			get
			{
				return new TaleReference(null);
			}
		}

		public TaleReference()
		{
		}

		public TaleReference(Tale tale)
		{
			this.tale = tale;
			this.seed = Rand.Range(0, 2147483647);
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.seed, "seed", 0, false);
			Scribe_References.Look<Tale>(ref this.tale, "tale", false);
		}

		public void ReferenceDestroyed()
		{
			if (this.tale != null)
			{
				this.tale.Notify_ReferenceDestroyed();
				this.tale = null;
			}
		}

		public string GenerateText(TextGenerationPurpose purpose, List<Rule> extraRules)
		{
			return TaleTextGenerator.GenerateTextFromTale(purpose, this.tale, this.seed, extraRules);
		}

		public override string ToString()
		{
			return "TaleReference(tale=" + ((this.tale != null) ? this.tale.ToString() : "null") + ", seed=" + this.seed + ")";
		}
	}
}
