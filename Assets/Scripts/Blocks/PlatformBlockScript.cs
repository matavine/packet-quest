using UnityEngine;
using System;
using System.Collections;

public class PlatformBlockScript : MonoBehaviour {
	public bool pulse;

	[Range(0, float.MaxValue)]
	public float pulseDuration;
	public bool randomPulse;
	public Color colourA;
	public Color colourB;

	public bool reactToAudio;

	private const int RAND_FACTOR = 3;
	private static System.Random random = new System.Random();

	void Start() {
		if (randomPulse) {
			float min = pulseDuration/RAND_FACTOR;
			pulseDuration = (float)random.NextDouble() * (pulseDuration - min) + min;
		}
		if (reactToAudio) {
			AudioPitchChanger.Instance.OnPitchChanged += HandleOnPitchChanged;
		}
	}

	void HandleOnPitchChanged (object sender, PitchChangedEventArgs e) {
		pulseDuration /= e.factor;
	}
	
	// Update is called once per frame
	void Update () {
		if(pulse) {
			Pulse();
		}
	}

	public void Pulse() {
		float t = Mathf.PingPong(Time.time, pulseDuration) / pulseDuration;
		renderer.material.color = Color.Lerp(colourA, colourB, t);
	}
}
