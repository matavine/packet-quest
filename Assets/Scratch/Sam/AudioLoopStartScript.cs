using UnityEngine;
using System.Collections;

public class AudioLoopStartScript : MonoBehaviour {
	public AudioClip loopClip;
	
	void Update() {
		// Once the initial clip has finished, loop loopClip indefinitely
		if (!GetComponent<AudioSource>().isPlaying) {
			GetComponent<AudioSource>().clip = loopClip;
			GetComponent<AudioSource>().Play();
		}
	}
	
}
