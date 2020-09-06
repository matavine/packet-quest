using UnityEngine;
using System.Collections;

public class VirusScript : MonoBehaviour {
	public float speed;
	public float xMin;
	public float xMax;
	public bool reactToAudio;

	// Use this for initialization
	void Start () {
		// get absolute positions
		xMin += transform.position.x;
		xMax += transform.position.x;

		if (reactToAudio) {
			AudioPitchChanger.Instance.OnPitchChanged += HandleOnPitchChanged;
		}
	}

	void HandleOnPitchChanged (object sender, PitchChangedEventArgs e)
	{
		speed *= e.factor;
	}

	void Update () {
		if(transform.position.x == xMin || transform.position.x == xMax){
			speed *= -1;
		}
		
		float x = transform.position.x + speed * Time.deltaTime;
		
		if(x > xMax) x = xMax;
		if(x < xMin) x = xMin;

		transform.position = new Vector3(x,transform.position.y,transform.position.z);
		transform.localScale = new Vector3(-Mathf.Sign(speed), transform.localScale.y, transform.localScale.z);
	}

	void OnCollisionEnter2D (Collision2D c){
		PlayerBehaviour player = c.gameObject.GetComponent<PlayerBehaviour>();
		if(player){
			player.Kill();
		}
	}
}
