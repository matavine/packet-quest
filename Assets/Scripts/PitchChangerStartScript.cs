using UnityEngine;

public class PitchChangerStartScript : MonoBehaviour {
	public string clipName;
	public float speed;
	public float pitchSpeedRatio = 2.0f;
	public int increments = 30;
	public float incrementTime = 0.01f;
	public bool loopAudio;
	public float switchTime;
	public float fadeIn = 2.0f;

	void Start () {
		AudioPitchChanger.Instance.Begin(clipName, speed, pitchSpeedRatio, increments, incrementTime, switchTime, loopAudio, fadeIn);
	}
}
