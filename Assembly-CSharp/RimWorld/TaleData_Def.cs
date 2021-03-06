using System;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class TaleData_Def : TaleData
	{
		public Def def;

		private string tmpDefName;

		private Type tmpDefType;

		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.tmpDefName = ((this.def == null) ? null : this.def.defName);
				this.tmpDefType = ((this.def == null) ? null : this.def.GetType());
			}
			Scribe_Values.Look<string>(ref this.tmpDefName, "defName", (string)null, false);
			Scribe_Values.Look<Type>(ref this.tmpDefType, "defType", (Type)null, false);
			if (Scribe.mode == LoadSaveMode.LoadingVars && this.tmpDefName != null)
			{
				this.def = GenDefDatabase.GetDef(this.tmpDefType, this.tmpDefName, true);
			}
		}

		public override IEnumerable<Rule> GetRules(string prefix)
		{
			if (this.def == null)
				yield break;
			yield return (Rule)new Rule_String(prefix + "_label", this.def.label);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public static TaleData_Def GenerateFrom(Def def)
		{
			TaleData_Def taleData_Def = new TaleData_Def();
			taleData_Def.def = def;
			return taleData_Def;
		}
	}
}
