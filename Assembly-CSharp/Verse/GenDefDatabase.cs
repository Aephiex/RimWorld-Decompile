using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class GenDefDatabase
	{
		public static Def GetDef(Type defType, string defName, bool errorOnFail = true)
		{
			return (Def)GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), defType, "GetNamed", defName, errorOnFail);
		}

		public static Def GetDefSilentFail(Type type, string targetDefName)
		{
			if (type == typeof(SoundDef))
			{
				return SoundDef.Named(targetDefName);
			}
			return (Def)GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), type, "GetNamedSilentFail", targetDefName);
		}

		public static IEnumerable<Type> AllDefTypesWithDatabases()
		{
			foreach (Type item in typeof(Def).AllSubclasses())
			{
				if (!item.IsAbstract && item != typeof(Def))
				{
					bool foundNonAbstractAncestor = false;
					Type parent = item.BaseType;
					while (parent != null && parent != typeof(Def))
					{
						if (parent.IsAbstract)
						{
							parent = parent.BaseType;
							continue;
						}
						foundNonAbstractAncestor = true;
						break;
					}
					if (!foundNonAbstractAncestor)
					{
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_0166:
			/*Error near IL_0167: Unexpected return in MoveNext()*/;
		}

		public static IEnumerable<T> DefsToGoInDatabase<T>(ModContentPack mod)
		{
			return mod.AllDefs.OfType<T>();
		}
	}
}
