using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioPlayer : MonoBehaviour {
	public bool audioEnabled {
		get { return m_audioEnabled; }
		set {
			if (!value) {
				FadeOutAll(2.0f);
			}
			m_audioEnabled = value;
		}
	}
	
	private bool m_audioEnabled = true;
	private const int DEFAULT_INCREMENTS_PER_SECOND = 10;
	private Dictionary<int, GameObject> m_clips = new Dictionary<int, GameObject>();
	private HashSet<int> m_paused = new HashSet<int>(); //subset of m_clips that are paused
	private LRUCache<string, AudioClip> m_cache = new LRUCache<string, AudioClip>();
 
	private static AudioPlayer m_instance;
	private static object m_creationLock = new object();

	protected AudioPlayer () { } //Singletons FTW?

	public static AudioPlayer Instance {
		get {
			lock(m_creationLock) {
				if (m_instance == null) {
					GameObject obj = new GameObject("AudioPlayer");
					m_instance = obj.AddComponent<AudioPlayer>();
				}
				return m_instance;
			}
		}
	}

	private AudioClip GetAudioClip(string name) {
		AudioClip clip = m_cache[name];
		if (!clip) {
			clip = Resources.Load<AudioClip>(name);
			if (clip) {
				m_cache.Add(name, clip);
			}
		}
		return clip;
	}

	private GameObject CreateAudioGameObject(string name, float volume, bool loop) {
		AudioClip clip = GetAudioClip(name);
		if (!clip) {
			Debug.LogWarning("Cannot find clip: " + name);
			return null;
		}
		GameObject obj = new GameObject("Audio: " + name);
		AudioSource source = obj.AddComponent<AudioSource>();
		source.clip = clip;
		m_clips.Add(obj.GetInstanceID(), obj);
		source.loop = loop;
		source.volume = volume;
		source.Play();
		return obj;
	}

	private IEnumerator FadeRoutine(AudioSource source, float finalVolume, float volumeChange, float time, int key = 0, bool removeAfter = false) {
		yield return new WaitForSeconds(time);

		// In case the audio source has been destroyed.
		if (source == null) {
			yield break;
		}

		source.volume += volumeChange;
		if (Mathf.Abs(source.volume - finalVolume) >= Mathf.Abs(volumeChange)) {
			StartCoroutine(FadeRoutine(source, finalVolume, volumeChange, time, key, removeAfter));
		} else if (removeAfter) {
			Stop(key);
		}
	}

	private void Fade(AudioSource source, float finalVolume, float duration, int incrementsPerSecond, int key = 0, bool removeAfter = false) {
		StartCoroutine(FadeRoutine(source, finalVolume, (finalVolume - source.volume)/(duration*incrementsPerSecond), 1.0f/incrementsPerSecond, key, removeAfter));
	}

	public void Fade(int key, float finalVolume, float duration, int incrementsPerSecond = DEFAULT_INCREMENTS_PER_SECOND, bool removeAfter = false) {
		if (!m_clips.ContainsKey(key)) {
			Debug.LogWarning("Clip " + key + " does not exist");
			return;
		}
		AudioSource source = m_clips[key].GetComponent<AudioSource>();
		if (source.volume == finalVolume) {
			Debug.Log("Clip " + key + " already at target volume");
			return;
		}
		Fade(source, finalVolume, duration, incrementsPerSecond, key, removeAfter);
	}

	public void FadeOut(int key, float duration, int incrementsPerSecond = DEFAULT_INCREMENTS_PER_SECOND, bool removeAfter = true) {
		Fade(key, 0.0f, duration, incrementsPerSecond, removeAfter);
	}

	public void FadeOutAll(float duration, int incrementsPerSecond = DEFAULT_INCREMENTS_PER_SECOND, bool removeAfter = true) {
		foreach (int key in m_clips.Keys) {
			FadeOut(key, duration, incrementsPerSecond, removeAfter);
		}
	}


	public int Play(string name, float volume = 1.0f, bool loop = false, float fadeIn = 0.0f, int incrementsPerSecond = DEFAULT_INCREMENTS_PER_SECOND) {
		if (!audioEnabled) {
			return -1;
		}
		GameObject obj = CreateAudioGameObject(name, fadeIn > 0.0f ? 0.0f : volume, loop);
		if (obj != null && fadeIn > 0.0f) {
			Fade(obj.GetComponent<AudioSource>(), volume, fadeIn, incrementsPerSecond);
		}
		return obj ? obj.GetInstanceID() : -1;
	}
	
	public int PlayAtPosition(string name, Vector3 position, float volume = 1.0f, bool loop = false, float fadeIn = 0.0f, int incrementsPerSecond = DEFAULT_INCREMENTS_PER_SECOND) {
		if (!audioEnabled) {
			return -1;
		}
		GameObject obj = CreateAudioGameObject(name, fadeIn > 0.0f ? 0.0f : volume, loop);
		if (obj != null && fadeIn > 0.0f) {
			Fade(obj.GetComponent<AudioSource>(), volume, fadeIn, incrementsPerSecond);
		}
		obj.transform.position = position;
		return obj ? obj.GetInstanceID() : -1;
	}

	// Audio moves with object by supplying GameObject.transform
	public int PlayWithTransform(string name, Transform source, float volume = 1.0f, bool loop = false, float fadeIn = 0.0f, int incrementsPerSecond = DEFAULT_INCREMENTS_PER_SECOND) {
		if (!audioEnabled) {
			return -1;
		}
		GameObject obj = CreateAudioGameObject(name, fadeIn > 0.0f ? 0.0f : volume, loop);
		if (fadeIn > 0.0f) {
			Fade(obj.GetComponent<AudioSource>(), volume, fadeIn, incrementsPerSecond);
		}
		obj.transform.position = source.position;
		obj.transform.parent = source;
		return obj ? obj.GetInstanceID() : -1;
	}

	private IEnumerator ChangePitchRoutine(AudioSource source, float finalPitch, float pitchChange, float time) {
		yield return new WaitForSeconds(time);
		source.pitch += pitchChange;
		if (Mathf.Abs(source.pitch - finalPitch) >= Mathf.Abs(pitchChange)) {
			StartCoroutine(ChangePitchRoutine(source, finalPitch, pitchChange, time));
		}
	}

	public void ChangePitch(int key, float pitch, int increments, float timeBetweenIncrements) {
		if (!m_clips.ContainsKey(key)) {
			Debug.LogWarning("Clip " + key + " does not exist");
			return;
		}
		AudioSource source = m_clips[key].GetComponent<AudioSource>();
		StartCoroutine(ChangePitchRoutine(source, pitch, (pitch - source.pitch)/increments, timeBetweenIncrements));
	}

	public void Pause(int key) {
		if (!m_clips.ContainsKey(key)) {
			Debug.LogWarning("Clip " + key + " does not exist");
			return;
		}
		if (m_paused.Contains(key)) {
			Debug.LogWarning("Clip " + key + " is already paused");
			return;
		}
		m_clips[key].GetComponent<AudioSource>().Pause();
		m_paused.Add(key);
	}

	public void PauseAll() {
		foreach (int key in m_clips.Keys) {
			m_clips[key].GetComponent<AudioSource>().Pause();
			m_paused.Add(key);
		}
	}

	private void UnPause(int key, bool remove){
		if (!m_clips.ContainsKey(key)) {
			Debug.LogWarning("Clip " + key + " does not exist");
			return;
		}
		if (!m_paused.Contains(key)) {
			Debug.LogWarning("Clip " + key + " is not paused");
			return;
		}
		AudioSource source = m_clips[key].GetComponent<AudioSource>();
		source.Play();
		// Can't remove while we're iterating in UnPauseAll
		if (remove) {
			m_paused.Remove(key);
		}
	}

	public void UnPause(int key) {
		UnPause(key, true);
	}

	public void UnPauseAll() {
		Debug.LogWarning ("unpause all");
		foreach (int key in m_paused) {
			Debug.LogWarning ("unpausing " + key);
			UnPause(key, false);
		}
		m_paused.Clear();
	}

	//Once a clip is stopped, it no longer exists.  To play again, recreate it.
	private void Stop(int key, bool remove) {
		if (!m_clips.ContainsKey(key)) {
			Debug.LogWarning("Clip " + key + " does not exist");
			return;
		}
		GameObject obj = m_clips[key];
		obj.GetComponent<AudioSource>().Stop();
		DestroyImmediate(obj);
		if (remove) {
			m_clips.Remove(key);
			m_paused.Remove(key);
		}
	}

	public void Stop(int key) {
		Stop(key, true);
	}

	public void StopAll() {
		foreach (int key in m_clips.Keys) {
			Stop(key, false);
		}
		m_clips.Clear();
		m_paused.Clear();
	}

	public float GetClipLength(int key) {
		if (!m_clips.ContainsKey(key)) {
			Debug.LogWarning("Clip " + key + " does not exist");
			return 0;
		}
		return m_clips[key].GetComponent<AudioSource>().clip.length;
	}

	public float GetClipLength(string name) {
		AudioClip clip = m_cache[name];
		if(clip) {
			return clip.length;
		}
		return 0;
	}

	void Update() {
		Dictionary<int, GameObject> remainingClips = new Dictionary<int, GameObject>();
		//remove all clips that are finished
		foreach (KeyValuePair<int, GameObject> entry in m_clips) {
			if (entry.Value.GetComponent<AudioSource>().timeSamples < entry.Value.GetComponent<AudioSource>().clip.samples) {
				remainingClips.Add(entry.Key, entry.Value);
			} else {
				// Not executed until the after Update is finished
				Destroy(entry.Value);
			}
		}
		m_clips = remainingClips;
	}
}
