using System.Xml;

namespace Verse
{
	public sealed class ThingCountClass
	{
		public ThingDef thingDef;

		public int count;

		public string Summary
		{
			get
			{
				return this.count + "x " + ((this.thingDef == null) ? "null" : this.thingDef.label);
			}
		}

		public ThingCountClass()
		{
		}

		public ThingCountClass(ThingDef thingDef, int count)
		{
			this.thingDef = thingDef;
			this.count = count;
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			if (xmlRoot.ChildNodes.Count != 1)
			{
				Log.Error("Misconfigured ThingCount: " + xmlRoot.OuterXml);
			}
			else
			{
				DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thingDef", xmlRoot.Name);
				this.count = (int)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(int));
			}
		}

		public override string ToString()
		{
			return "(" + this.count + "x " + ((this.thingDef == null) ? "null" : this.thingDef.defName) + ")";
		}

		public override int GetHashCode()
		{
			return this.thingDef.shortHash + this.count << 16;
		}

		public static implicit operator ThingCountClass(ThingCount t)
		{
			return new ThingCountClass(t.ThingDef, t.Count);
		}
	}
}
