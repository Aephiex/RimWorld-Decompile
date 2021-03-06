using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.Sound
{
	public static class MouseoverSounds
	{
		private struct MouseoverRegionCall
		{
			public bool mouseIsOver;

			public Rect rect;

			public SoundDef sound;

			public bool IsValid
			{
				get
				{
					return this.rect.x >= 0.0;
				}
			}

			public static MouseoverRegionCall Invalid
			{
				get
				{
					MouseoverRegionCall result = default(MouseoverRegionCall);
					result.rect = new Rect(-1000f, -1000f, 0f, 0f);
					return result;
				}
			}

			public bool Matches(MouseoverRegionCall other)
			{
				return this.rect.Equals(other.rect);
			}

			public override string ToString()
			{
				if (!this.IsValid)
				{
					return "(Invalid)";
				}
				return "(rect=" + this.rect + ((!this.mouseIsOver) ? string.Empty : "mouseIsOver") + ")";
			}
		}

		private static List<MouseoverRegionCall> frameCalls = new List<MouseoverRegionCall>();

		private static int lastUsedCallInd = -1;

		private static MouseoverRegionCall lastUsedCall;

		private static int forceSilenceUntilFrame = -1;

		public static void SilenceForNextFrame()
		{
			MouseoverSounds.forceSilenceUntilFrame = Time.frameCount + 1;
		}

		public static void DoRegion(Rect rect)
		{
			MouseoverSounds.DoRegion(rect, SoundDefOf.MouseoverStandard);
		}

		public static void DoRegion(Rect rect, SoundDef sound)
		{
			if (sound != null && Event.current.type == EventType.Repaint)
			{
				MouseoverRegionCall item = default(MouseoverRegionCall);
				item.rect = rect;
				item.sound = sound;
				item.mouseIsOver = Mouse.IsOver(rect);
				MouseoverSounds.frameCalls.Add(item);
			}
		}

		public static void ResolveFrame()
		{
			for (int i = 0; i < MouseoverSounds.frameCalls.Count; i++)
			{
				MouseoverRegionCall mouseoverRegionCall = MouseoverSounds.frameCalls[i];
				if (mouseoverRegionCall.mouseIsOver)
				{
					if (MouseoverSounds.lastUsedCallInd != i && !MouseoverSounds.frameCalls[i].Matches(MouseoverSounds.lastUsedCall) && MouseoverSounds.forceSilenceUntilFrame < Time.frameCount)
					{
						MouseoverRegionCall mouseoverRegionCall2 = MouseoverSounds.frameCalls[i];
						mouseoverRegionCall2.sound.PlayOneShotOnCamera(null);
					}
					MouseoverSounds.lastUsedCallInd = i;
					MouseoverSounds.lastUsedCall = MouseoverSounds.frameCalls[i];
					MouseoverSounds.frameCalls.Clear();
					return;
				}
			}
			MouseoverSounds.lastUsedCall = MouseoverRegionCall.Invalid;
			MouseoverSounds.lastUsedCallInd = -1;
			MouseoverSounds.frameCalls.Clear();
		}
	}
}
