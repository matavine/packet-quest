using UnityEngine;
using System.Collections;

public class RunnerScript : MonoBehaviour {
	public float acceleration = 5f;
	public float maxSpeed = 10f;
	
	private bool touchedGround = false;
	private bool playing = false;

	void Start() {
		AudioPlayer.Instance.PlayWithTransform("gameaudio-loop", this.transform);
		playing = true;
	}
	
	// Update is called once per frame
	void Update () {
		var currSpeed = GetComponent<Rigidbody2D>().velocity;
		if(touchedGround && currSpeed.magnitude < maxSpeed){
			GetComponent<Rigidbody2D>().AddForce(new Vector2(acceleration,0));
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			if (playing) {
				AudioPlayer.Instance.StopAll();
				playing = false;
			} else {
				AudioPlayer.Instance.PlayWithTransform("gameaudio-loop", this.transform);
				playing = true;
			}
		}
	}

	void OnCollisionEnter2D() {
		touchedGround = true;	
	}
}
