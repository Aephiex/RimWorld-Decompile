using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace Verse
{
	public class ModContentPack
	{
		private DirectoryInfo rootDirInt;

		public int loadOrder;

		private string nameInt;

		private ModContentHolder<AudioClip> audioClips;

		private ModContentHolder<Texture2D> textures;

		private ModContentHolder<string> strings;

		public ModAssemblyHandler assemblies;

		private List<PatchOperation> patches;

		private List<DefPackage> defPackages = new List<DefPackage>();

		public static readonly string CoreModIdentifier = "Core";

		public string RootDir
		{
			get
			{
				return this.rootDirInt.FullName;
			}
		}

		public string Identifier
		{
			get
			{
				return this.rootDirInt.Name;
			}
		}

		public string Name
		{
			get
			{
				return this.nameInt;
			}
		}

		public int OverwritePriority
		{
			get
			{
				return (!this.IsCoreMod) ? 1 : 0;
			}
		}

		public bool IsCoreMod
		{
			get
			{
				return this.rootDirInt.Name == ModContentPack.CoreModIdentifier;
			}
		}

		public IEnumerable<Def> AllDefs
		{
			get
			{
				return this.defPackages.SelectMany((DefPackage x) => x.defs);
			}
		}

		public bool LoadedAnyAssembly
		{
			get
			{
				return this.assemblies.loadedAssemblies.Count > 0;
			}
		}

		public IEnumerable<PatchOperation> Patches
		{
			get
			{
				if (this.patches == null)
				{
					this.LoadPatches();
				}
				return this.patches;
			}
		}

		public ModContentPack(DirectoryInfo directory, int loadOrder, string name)
		{
			this.rootDirInt = directory;
			this.loadOrder = loadOrder;
			this.nameInt = name;
			this.audioClips = new ModContentHolder<AudioClip>(this);
			this.textures = new ModContentHolder<Texture2D>(this);
			this.strings = new ModContentHolder<string>(this);
			this.assemblies = new ModAssemblyHandler(this);
		}

		public void ClearDestroy()
		{
			this.audioClips.ClearDestroy();
			this.textures.ClearDestroy();
		}

		public ModContentHolder<T> GetContentHolder<T>() where T : class
		{
			if (typeof(T) == typeof(Texture2D))
			{
				return (ModContentHolder<T>)this.textures;
			}
			if (typeof(T) == typeof(AudioClip))
			{
				return (ModContentHolder<T>)this.audioClips;
			}
			if (typeof(T) == typeof(string))
			{
				return (ModContentHolder<T>)this.strings;
			}
			Log.Error("Mod lacks manager for asset type " + this.strings);
			return null;
		}

		public void ReloadContent()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				this.audioClips.ReloadAll();
				this.textures.ReloadAll();
				this.strings.ReloadAll();
			});
			this.assemblies.ReloadAll();
		}

		public void LoadDefs(IEnumerable<PatchOperation> patches)
		{
			DeepProfiler.Start("Loading all defs");
			List<LoadableXmlAsset> list = DirectXmlLoader.XmlAssetsInModFolder(this, "Defs/").ToList();
			foreach (LoadableXmlAsset item in list)
			{
				foreach (PatchOperation patch in patches)
				{
					patch.Apply(item.xmlDoc);
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == null || list[i].xmlDoc == null || list[i].xmlDoc.DocumentElement == null)
				{
					Log.Error(string.Format("{0}: unknown parse failure", list[i].fullFolderPath + "/" + list[i].name));
				}
				else if (list[i].xmlDoc.DocumentElement.Name != "Defs")
				{
					Log.Error(string.Format("{0}: root element named {1}; should be named Defs", list[i].fullFolderPath + "/" + list[i].name, list[i].xmlDoc.DocumentElement.Name));
				}
				XmlInheritance.TryRegisterAllFrom(list[i], this);
			}
			XmlInheritance.Resolve();
			for (int j = 0; j < list.Count; j++)
			{
				string relFolder = GenFilePaths.FolderPathRelativeToDefsFolder(list[j].fullFolderPath, this);
				DefPackage defPackage = new DefPackage(list[j].name, relFolder);
				foreach (Def item2 in DirectXmlLoader.AllDefsFromAsset(list[j]))
				{
					defPackage.defs.Add(item2);
				}
				this.defPackages.Add(defPackage);
			}
			DeepProfiler.End();
		}

		public IEnumerable<DefPackage> GetDefPackagesInFolder(string relFolder)
		{
			string path = Path.Combine(Path.Combine(this.RootDir, "Defs/"), relFolder);
			if (!Directory.Exists(path))
			{
				return Enumerable.Empty<DefPackage>();
			}
			string fullPath = Path.GetFullPath(path);
			return from x in this.defPackages
			where x.GetFullFolderPath(this).StartsWith(fullPath)
			select x;
		}

		public void AddDefPackage(DefPackage defPackage)
		{
			this.defPackages.Add(defPackage);
		}

		private void LoadPatches()
		{
			DeepProfiler.Start("Loading all patches");
			this.patches = new List<PatchOperation>();
			List<LoadableXmlAsset> list = DirectXmlLoader.XmlAssetsInModFolder(this, "Patches/").ToList();
			for (int i = 0; i < list.Count; i++)
			{
				XmlElement documentElement = list[i].xmlDoc.DocumentElement;
				if (documentElement.Name != "Patch")
				{
					Log.Error(string.Format("Unexpected document element in patch XML; got {0}, expected 'Patch'", documentElement.Name));
				}
				else
				{
					for (int j = 0; j < documentElement.ChildNodes.Count; j++)
					{
						XmlNode xmlNode = documentElement.ChildNodes[j];
						if (xmlNode.NodeType == XmlNodeType.Element)
						{
							if (xmlNode.Name != "Operation")
							{
								Log.Error(string.Format("Unexpected element in patch XML; got {0}, expected 'Operation'", documentElement.ChildNodes[j].Name));
							}
							else
							{
								PatchOperation patchOperation = DirectXmlToObject.ObjectFromXml<PatchOperation>(xmlNode, false);
								patchOperation.sourceFile = list[i].FullFilePath;
								this.patches.Add(patchOperation);
							}
						}
					}
				}
			}
			DeepProfiler.End();
		}

		public void ClearPatchesCache()
		{
			this.patches = null;
		}

		public override string ToString()
		{
			return this.Identifier;
		}
	}
}
