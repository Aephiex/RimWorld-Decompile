using UnityEngine;

namespace Verse.Noise
{
	public class AbsLatitudeCurve : ModuleBase
	{
		public SimpleCurve curve;

		public float planetRadius;

		public AbsLatitudeCurve()
			: base(0)
		{
		}

		public AbsLatitudeCurve(SimpleCurve curve, float planetRadius)
			: base(0)
		{
			this.curve = curve;
			this.planetRadius = planetRadius;
		}

		public override double GetValue(double x, double y, double z)
		{
			float f = (float)(Mathf.Asin((float)(y / (double)this.planetRadius)) * 57.295780181884766);
			return (double)this.curve.Evaluate(Mathf.Abs(f));
		}
	}
}
