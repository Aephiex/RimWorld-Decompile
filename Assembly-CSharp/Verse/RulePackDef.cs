using System.Collections.Generic;
using Verse.Grammar;

namespace Verse
{
	public class RulePackDef : Def
	{
		public List<RulePackDef> include;

		private RulePack rulePack;

		private List<Rule> cachedRules;

		public List<Rule> RulesPlusIncludes
		{
			get
			{
				if (this.cachedRules == null)
				{
					this.cachedRules = new List<Rule>();
					if (this.rulePack != null)
					{
						this.cachedRules.AddRange(this.rulePack.Rules);
					}
					if (this.include != null)
					{
						for (int i = 0; i < this.include.Count; i++)
						{
							this.cachedRules.AddRange(this.include[i].RulesPlusIncludes);
						}
					}
				}
				return this.cachedRules;
			}
		}

		public List<Rule> RulesImmediate
		{
			get
			{
				return (this.rulePack == null) ? null : this.rulePack.Rules;
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string err = enumerator.Current;
					yield return err;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (this.include == null)
				yield break;
			int i = 0;
			while (true)
			{
				if (i < this.include.Count)
				{
					if (this.include[i].include != null && this.include[i].include.Contains(this))
						break;
					i++;
					continue;
				}
				yield break;
			}
			yield return "includes other RulePackDef which includes it: " + this.include[i].defName;
			/*Error: Unable to find new state assignment for yield return*/;
			IL_018c:
			/*Error near IL_018d: Unexpected return in MoveNext()*/;
		}

		public static RulePackDef Named(string defName)
		{
			return DefDatabase<RulePackDef>.GetNamed(defName, true);
		}
	}
}
