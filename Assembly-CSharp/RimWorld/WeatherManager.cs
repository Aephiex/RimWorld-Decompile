using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public sealed class WeatherManager : IExposable
	{
		public Map map;

		public WeatherEventHandler eventHandler = new WeatherEventHandler();

		public WeatherDef curWeather = WeatherDefOf.Clear;

		public WeatherDef lastWeather = WeatherDefOf.Clear;

		public int curWeatherAge;

		private List<Sustainer> ambienceSustainers = new List<Sustainer>();

		public TemperatureMemory growthSeasonMemory;

		public const float TransitionTicks = 4000f;

		public float TransitionLerpFactor
		{
			get
			{
				float num = (float)((float)this.curWeatherAge / 4000.0);
				if (num > 1.0)
				{
					num = 1f;
				}
				return num;
			}
		}

		public float RainRate
		{
			get
			{
				return Mathf.Lerp(this.lastWeather.rainRate, this.curWeather.rainRate, this.TransitionLerpFactor);
			}
		}

		public float SnowRate
		{
			get
			{
				return Mathf.Lerp(this.lastWeather.snowRate, this.curWeather.snowRate, this.TransitionLerpFactor);
			}
		}

		public float CurWindSpeedFactor
		{
			get
			{
				return Mathf.Lerp(this.lastWeather.windSpeedFactor, this.curWeather.windSpeedFactor, this.TransitionLerpFactor);
			}
		}

		public float CurMoveSpeedMultiplier
		{
			get
			{
				return Mathf.Lerp(this.lastWeather.moveSpeedMultiplier, this.curWeather.moveSpeedMultiplier, this.TransitionLerpFactor);
			}
		}

		public float CurWeatherAccuracyMultiplier
		{
			get
			{
				return Mathf.Lerp(this.lastWeather.accuracyMultiplier, this.curWeather.accuracyMultiplier, this.TransitionLerpFactor);
			}
		}

		public WeatherDef CurPerceivedWeather
		{
			get
			{
				if (this.curWeather == null)
				{
					return this.lastWeather;
				}
				if (this.lastWeather == null)
				{
					return this.curWeather;
				}
				float num = 0f;
				num = (float)((!(this.curWeather.perceivePriority > this.lastWeather.perceivePriority)) ? ((!(this.lastWeather.perceivePriority > this.curWeather.perceivePriority)) ? 0.5 : 0.81999999284744263) : 0.18000000715255737);
				if (this.TransitionLerpFactor < num)
				{
					return this.lastWeather;
				}
				return this.curWeather;
			}
		}

		public WeatherManager(Map map)
		{
			this.map = map;
			this.growthSeasonMemory = new TemperatureMemory(map);
		}

		public void ExposeData()
		{
			Scribe_Defs.Look<WeatherDef>(ref this.curWeather, "curWeather");
			Scribe_Defs.Look<WeatherDef>(ref this.lastWeather, "lastWeather");
			Scribe_Values.Look<int>(ref this.curWeatherAge, "curWeatherAge", 0, true);
			Scribe_Deep.Look<TemperatureMemory>(ref this.growthSeasonMemory, "growthSeasonMemory", new object[1]
			{
				this.map
			});
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.ambienceSustainers.Clear();
			}
		}

		public void TransitionTo(WeatherDef newWeather)
		{
			this.lastWeather = this.curWeather;
			this.curWeather = newWeather;
			this.curWeatherAge = 0;
		}

		public void DoWeatherGUI(Rect rect)
		{
			WeatherDef curPerceivedWeather = this.CurPerceivedWeather;
			Text.Anchor = TextAnchor.MiddleRight;
			Rect rect2 = new Rect(rect);
			rect2.width -= 15f;
			Text.Font = GameFont.Small;
			Widgets.Label(rect2, curPerceivedWeather.LabelCap);
			if (!curPerceivedWeather.description.NullOrEmpty())
			{
				TooltipHandler.TipRegion(rect, curPerceivedWeather.description);
			}
			Text.Anchor = TextAnchor.UpperLeft;
		}

		public void WeatherManagerTick()
		{
			this.eventHandler.WeatherEventHandlerTick();
			this.curWeatherAge++;
			this.curWeather.Worker.WeatherTick(this.map, this.TransitionLerpFactor);
			this.lastWeather.Worker.WeatherTick(this.map, (float)(1.0 - this.TransitionLerpFactor));
			this.growthSeasonMemory.GrowthSeasonMemoryTick();
			for (int i = 0; i < this.curWeather.ambientSounds.Count; i++)
			{
				bool flag = false;
				int num = this.ambienceSustainers.Count - 1;
				while (num >= 0)
				{
					if (this.ambienceSustainers[num].def != this.curWeather.ambientSounds[i])
					{
						num--;
						continue;
					}
					flag = true;
					break;
				}
				if (!flag && this.VolumeOfAmbientSound(this.curWeather.ambientSounds[i]) > 9.9999997473787516E-05)
				{
					SoundInfo info = SoundInfo.OnCamera(MaintenanceType.None);
					Sustainer sustainer = this.curWeather.ambientSounds[i].TrySpawnSustainer(info);
					if (sustainer != null)
					{
						this.ambienceSustainers.Add(sustainer);
					}
				}
			}
		}

		public void WeatherManagerUpdate()
		{
			this.SetAmbienceSustainersVolume();
		}

		public void EndAllSustainers()
		{
			for (int i = 0; i < this.ambienceSustainers.Count; i++)
			{
				this.ambienceSustainers[i].End();
			}
			this.ambienceSustainers.Clear();
		}

		private void SetAmbienceSustainersVolume()
		{
			for (int num = this.ambienceSustainers.Count - 1; num >= 0; num--)
			{
				float num2 = this.VolumeOfAmbientSound(this.ambienceSustainers[num].def);
				if (num2 > 9.9999997473787516E-05)
				{
					this.ambienceSustainers[num].externalParams["LerpFactor"] = num2;
				}
				else
				{
					this.ambienceSustainers[num].End();
					this.ambienceSustainers.RemoveAt(num);
				}
			}
		}

		private float VolumeOfAmbientSound(SoundDef soundDef)
		{
			if (this.map != Find.VisibleMap)
			{
				return 0f;
			}
			for (int i = 0; i < Find.WindowStack.Count; i++)
			{
				if (Find.WindowStack[i].silenceAmbientSound)
				{
					return 0f;
				}
			}
			float num = 0f;
			for (int j = 0; j < this.lastWeather.ambientSounds.Count; j++)
			{
				if (this.lastWeather.ambientSounds[j] == soundDef)
				{
					num = (float)(num + (1.0 - this.TransitionLerpFactor));
				}
			}
			for (int k = 0; k < this.curWeather.ambientSounds.Count; k++)
			{
				if (this.curWeather.ambientSounds[k] == soundDef)
				{
					num += this.TransitionLerpFactor;
				}
			}
			return num;
		}

		public void DrawAllWeather()
		{
			this.eventHandler.WeatherEventsDraw();
			this.lastWeather.Worker.DrawWeather(this.map);
			this.curWeather.Worker.DrawWeather(this.map);
		}
	}
}
