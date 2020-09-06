using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class FallingBehaviour : MonoBehaviour {

	// After the player steps on the platform,
	// how many seconds before it begins to fall?
	[Range(0, float.MaxValue)]
	public float fallTriggerTime = 2f;
	
	public bool killObject = true;

	// Number of seconds after which the block
	// is destroyed once it begins falling.
	[Range(0, float.MaxValue)]
	public float killTime = 5f;

	private bool m_isFalling = false;

	public void Update () {
		if (!m_isFalling)
			return;

		// For objects with RigidBodies, we may want to instead
		// activate the "gravity" field for them.
		// TODO: Identify use cases for this script.
		Vector2 displacement = Physics2D.gravity * Time.deltaTime;
		transform.position += new Vector3(displacement.x, displacement.y);
	}

	public IEnumerator TriggerFall() {
		if (fallTriggerTime > 0)
			yield return new WaitForSeconds(fallTriggerTime);

		m_isFalling = true;

		if (killObject) {
			yield return new WaitForSeconds(killTime);
			Destroy(gameObject);
		}
	}

	public void OnCollisionEnter2D(Collision2D collider) {
		PlayerBehaviour player = collider.gameObject.GetComponent<PlayerBehaviour>();
		if (player) {
			StartCoroutine(TriggerFall());
		}
	}
}
