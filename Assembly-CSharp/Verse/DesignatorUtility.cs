using RimWorld;
using System.Collections.Generic;

namespace Verse
{
	public static class DesignatorUtility
	{
		public static Designator FindAllowedDesignator<T>() where T : Designator
		{
			List<DesignationCategoryDef> allDefsListForReading = DefDatabase<DesignationCategoryDef>.AllDefsListForReading;
			GameRules rules = Current.Game.Rules;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				List<Designator> allResolvedDesignators = allDefsListForReading[i].AllResolvedDesignators;
				for (int j = 0; j < allResolvedDesignators.Count; j++)
				{
					if (rules.DesignatorAllowed(allResolvedDesignators[j]))
					{
						T val = (T)(allResolvedDesignators[j] as T);
						if (val != null)
						{
							return (Designator)(object)val;
						}
					}
				}
			}
			return null;
		}
	}
}
