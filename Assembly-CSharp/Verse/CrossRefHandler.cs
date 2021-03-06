using System;
using System.Collections.Generic;

namespace Verse
{
	public class CrossRefHandler
	{
		private LoadedObjectDirectory loadedObjectDirectory = new LoadedObjectDirectory();

		public LoadIDsWantedBank loadIDs = new LoadIDsWantedBank();

		private List<IExposable> crossReferencingExposables = new List<IExposable>();

		public void RegisterForCrossRefResolve(IExposable s)
		{
			if (Scribe.mode != LoadSaveMode.LoadingVars)
			{
				Log.Error("Registered " + s + " for cross ref resolve, but current mode is " + Scribe.mode);
			}
			else if (s != null)
			{
				if (DebugViewSettings.logMapLoad)
				{
					LogSimple.Message("RegisterForCrossRefResolve " + ((s == null) ? "null" : s.GetType().ToString()));
				}
				this.crossReferencingExposables.Add(s);
			}
		}

		public void ResolveAllCrossReferences()
		{
			Scribe.mode = LoadSaveMode.ResolvingCrossRefs;
			if (DebugViewSettings.logMapLoad)
			{
				LogSimple.Message("==================Register the saveables all so we can find them later");
			}
			foreach (IExposable crossReferencingExposable in this.crossReferencingExposables)
			{
				ILoadReferenceable loadReferenceable = crossReferencingExposable as ILoadReferenceable;
				if (loadReferenceable != null)
				{
					if (DebugViewSettings.logMapLoad)
					{
						LogSimple.Message("RegisterLoaded " + loadReferenceable.GetType());
					}
					this.loadedObjectDirectory.RegisterLoaded(loadReferenceable);
				}
			}
			if (DebugViewSettings.logMapLoad)
			{
				LogSimple.Message("==================Fill all cross-references to the saveables");
			}
			foreach (IExposable crossReferencingExposable2 in this.crossReferencingExposables)
			{
				if (DebugViewSettings.logMapLoad)
				{
					LogSimple.Message("ResolvingCrossRefs ExposeData " + crossReferencingExposable2.GetType());
				}
				try
				{
					Scribe.loader.curParent = crossReferencingExposable2;
					Scribe.loader.curPathRelToParent = null;
					crossReferencingExposable2.ExposeData();
				}
				catch (Exception arg)
				{
					Log.Error("Could not resolve cross refs: " + arg);
				}
			}
			Scribe.loader.curParent = null;
			Scribe.loader.curPathRelToParent = null;
			Scribe.mode = LoadSaveMode.Inactive;
			this.Clear(true);
		}

		public T TakeResolvedRef<T>(string pathRelToParent, IExposable parent) where T : ILoadReferenceable
		{
			string loadID = this.loadIDs.Take<T>(pathRelToParent, parent);
			return this.loadedObjectDirectory.ObjectWithLoadID<T>(loadID);
		}

		public T TakeResolvedRef<T>(string toAppendToPathRelToParent) where T : ILoadReferenceable
		{
			string text = Scribe.loader.curPathRelToParent;
			if (!toAppendToPathRelToParent.NullOrEmpty())
			{
				text = text + '/' + toAppendToPathRelToParent;
			}
			return this.TakeResolvedRef<T>(text, Scribe.loader.curParent);
		}

		public List<T> TakeResolvedRefList<T>(string pathRelToParent, IExposable parent)
		{
			List<string> list = this.loadIDs.TakeList(pathRelToParent, parent);
			List<T> list2 = new List<T>();
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					list2.Add(this.loadedObjectDirectory.ObjectWithLoadID<T>(list[i]));
				}
			}
			return list2;
		}

		public List<T> TakeResolvedRefList<T>(string toAppendToPathRelToParent)
		{
			string text = Scribe.loader.curPathRelToParent;
			if (!toAppendToPathRelToParent.NullOrEmpty())
			{
				text = text + '/' + toAppendToPathRelToParent;
			}
			return this.TakeResolvedRefList<T>(text, Scribe.loader.curParent);
		}

		public void Clear(bool errorIfNotEmpty)
		{
			if (errorIfNotEmpty)
			{
				this.loadIDs.ConfirmClear();
			}
			else
			{
				this.loadIDs.Clear();
			}
			this.crossReferencingExposables.Clear();
			this.loadedObjectDirectory.Clear();
		}
	}
}
