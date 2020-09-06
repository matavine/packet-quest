using UnityEngine;
using System.Collections;

public class MovingPlatformScript : MonoBehaviour {
	public Vector2 speed;
	public bool relativeLimits = true;
	public bool reactToAudio = false;
	
	[HideInInspector] /* Set by MovingPlatformEditor */
	public Rect limits;

	void Start () {
		if(relativeLimits) {
			// get absolute positions
			limits.xMin += transform.position.x;
			limits.xMax += transform.position.x;
			limits.yMin += transform.position.y;
			limits.yMax += transform.position.y;
		}
		if (reactToAudio) {
			AudioPitchChanger.Instance.OnPitchChanged += HandleOnPitchChanged;
		}
	}

	void HandleOnPitchChanged (object sender, PitchChangedEventArgs e)
	{
		speed *= e.factor;
	}
	
	void Update () {
		if(transform.position.x == limits.xMin || transform.position.x == limits.xMax){
			speed.x *= -1;
		}
		if(transform.position.y == limits.yMin || transform.position.y == limits.yMax){
			speed.y *= -1;
		}

		float x = transform.position.x + speed.x * Time.deltaTime;
		float y = transform.position.y + speed.y * Time.deltaTime;

		if(x > limits.xMax) x = limits.xMax;
		if(x < limits.xMin) x = limits.xMin;
		if(y > limits.yMax) y = limits.yMax;
		if(y < limits.yMin) y = limits.yMin;

		transform.position = new Vector3(x,y,transform.position.z);
	}

	private void CheckPlayerCollision(Collision2D c) {
		PlayerBehaviour player = c.gameObject.GetComponent<PlayerBehaviour>();
		if(player){
			BoxCollider2D playerCollider = player.collider2D as BoxCollider2D;
			BoxCollider2D platformCollider = this.collider2D as BoxCollider2D;
			
			// Check if the play hit the platform from the top
			if (player.transform.position.y - playerCollider.size.y/2 >= this.transform.position.y + platformCollider.size.y/2) {
				c.gameObject.transform.parent = transform;
			}
		}
	}
	
	void OnCollisionEnter2D (Collision2D c){
		CheckPlayerCollision(c);
	}

	void OnCollisionStay2D(Collision2D c) {
		CheckPlayerCollision(c);
	}
}
