using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AudioPitchChanger : MonoBehaviour {
	private float m_speed;
	private float m_pitchSpeedRatio = 2;
	private int m_increments = 30;
	private float m_incrementTime = 0.01f;
	private float m_switchTime;
	private int m_audioClipId;
	private static AudioPitchChanger m_instance;

	private enum PitchState {
		DEFAULT,
		INCREASED
	}

	private PitchState m_pitchState = PitchState.DEFAULT;

	public static AudioPitchChanger Instance {
		get {
			if (!m_instance) {
				GameObject obj = new GameObject("AudioPitchChanger");
				m_instance = obj.AddComponent<AudioPitchChanger>();
			}
			return m_instance;
		}
	}

	public event PitchChangedEvent OnPitchChanged;
	
	public delegate void PitchChangedEvent(object sender, PitchChangedEventArgs e);
	
	private IEnumerator IncreasePitch() {
		yield return new WaitForSeconds(m_switchTime);
		AudioPlayer.Instance.ChangePitch(m_audioClipId, m_speed, m_increments, m_incrementTime);
		OnPitchChanged(this, new PitchChangedEventArgs(m_pitchSpeedRatio*m_speed));
		m_pitchState = PitchState.INCREASED;
		StartCoroutine(ResetPitch());
	}
	
	private IEnumerator ResetPitch() {
		float inc_time = m_increments * m_incrementTime;
		yield return new WaitForSeconds(m_switchTime/m_speed - inc_time);
		AudioPlayer.Instance.ChangePitch(m_audioClipId, 1.0f, m_increments, m_incrementTime);
		yield return new WaitForSeconds(inc_time);
		OnPitchChanged(this, new PitchChangedEventArgs(1.0f/(m_pitchSpeedRatio*m_speed)));
		m_pitchState = PitchState.DEFAULT;
		StartCoroutine(IncreasePitch());
	}

	public void Begin(string clipName, float speed, float pitchSpeedRatio, int increments, float incrementTime, float switchTime, bool loop, float fade) {
		if (m_audioClipId != 0) {
			Debug.Log("Audio playback already started");
			return;
		}
		m_speed = speed;
		m_pitchSpeedRatio = pitchSpeedRatio;
		m_increments = increments;
		m_incrementTime = incrementTime;
		m_audioClipId = AudioPlayer.Instance.Play(clipName, fadeIn: fade, loop: loop);
		m_switchTime = switchTime;
		StartCoroutine(IncreasePitch());
	}

	public void Stop(float fadeOut = 0.0f) {
		if (m_audioClipId == 0) {
			Debug.Log("No audio to stop");
			return;
		}
		StopAllCoroutines();
		if (fadeOut > 0.0f) {
			AudioPlayer.Instance.FadeOut(m_audioClipId, fadeOut);
		} else {
			AudioPlayer.Instance.Stop(m_audioClipId);
		}
		if (m_pitchState == PitchState.INCREASED) {
			// Be a good citizen and leave things the way they were when we got here
			OnPitchChanged(this, new PitchChangedEventArgs(1.0f/(m_pitchSpeedRatio*m_speed)));
		}
		m_audioClipId = 0;
	}
}

public class PitchChangedEventArgs : EventArgs {
	public float factor;
	public PitchChangedEventArgs(float factor){
		this.factor = factor;
	}
}
