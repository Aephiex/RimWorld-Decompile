using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MusicManagerEntry
	{
		private AudioSource audioSource;

		private const string SourceGameObjectName = "MusicAudioSourceDummy";

		private float CurVolume
		{
			get
			{
				return Prefs.VolumeMusic * SongDefOf.EntrySong.volume;
			}
		}

		public float CurSanitizedVolume
		{
			get
			{
				return AudioSourceUtility.GetSanitizedVolume(this.CurVolume, "MusicManagerEntry");
			}
		}

		public void MusicManagerEntryUpdate()
		{
			if ((Object)this.audioSource == (Object)null || !this.audioSource.isPlaying)
			{
				this.StartPlaying();
			}
			this.audioSource.volume = this.CurSanitizedVolume;
		}

		private void StartPlaying()
		{
			if ((Object)this.audioSource != (Object)null && !this.audioSource.isPlaying)
			{
				this.audioSource.Play();
			}
			else if ((Object)GameObject.Find("MusicAudioSourceDummy") != (Object)null)
			{
				Log.Error("MusicManagerEntry did StartPlaying but there is already a music source GameObject.");
			}
			else
			{
				GameObject gameObject = new GameObject("MusicAudioSourceDummy");
				gameObject.transform.parent = Camera.main.transform;
				this.audioSource = gameObject.AddComponent<AudioSource>();
				this.audioSource.bypassEffects = true;
				this.audioSource.bypassListenerEffects = true;
				this.audioSource.bypassReverbZones = true;
				this.audioSource.priority = 0;
				this.audioSource.clip = SongDefOf.EntrySong.clip;
				this.audioSource.volume = this.CurSanitizedVolume;
				this.audioSource.loop = true;
				this.audioSource.spatialBlend = 0f;
				this.audioSource.Play();
			}
		}
	}
}
