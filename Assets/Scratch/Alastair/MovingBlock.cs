using UnityEngine;
using System.Collections;

public class MovingBlock : MonoBehaviour, MovablePlatform {
	public int xSpeed;
	public int ySpeed;
	public int leftLimit;
	public int rightLimit;
	public int upLimit;
	public int downLimit;
	
	void Start () {
    
	}
  
	void FixedUpdate () {
		Vector2 nextSpeed = getNextSpeed ();
		xSpeed = (int)nextSpeed.x;
		ySpeed = (int)nextSpeed.y;
		rigidbody2D.velocity = nextSpeed;
	}

	public Vector2 getNextSpeed () {
    
		Vector2 speed = new Vector2 (xSpeed, ySpeed);

		// Player also uses this to determine its next relative speed
		// This is needed because ordering to calls of FixedUpdate is not enforced.
		if (transform.position.x < leftLimit || transform.position.x > rightLimit) {
			speed.x = xSpeed * -1;
		}
		if (transform.position.y < downLimit || transform.position.y > upLimit) {
			speed.y = ySpeed * -1;
		}

		return speed;
	}
  
	void OnCollisionEnter2D (Collision2D c) {
//		PlayerBehaviour player = c.gameObject.GetComponent<VelocityPlayerControl> ();
//
//		// Only collide if the platform hits the players feet.
//		// Will find a cleaner way to make this check...
//		if (player && c.collider == c.transform.FindChild ("PlayerFeet").collider2D) {
//			player.setPlatform (this);
//		}
	}
}
  