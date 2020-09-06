using UnityEngine;
using System.Collections;

public class FlippableBehaviour : MonoBehaviour {

	public float randomTorqueMax = 20f;
	public float randomTorqueMin = -20f;

	private float m_originalGravity;

	public void Start() {
		PlayerBehaviour p = GetComponent<PlayerBehaviour>();

		if(p) {
			m_originalGravity = p.gravityScale;
		} else {
			m_originalGravity = rigidbody2D.gravityScale;
		}
	}

	public void Flip(int direction) {
		PlayerBehaviour p = GetComponent<PlayerBehaviour>();

		if(p) {
			p.gravityScale = FlipGravity(direction);
		} else if (rigidbody2D) {
			rigidbody2D.gravityScale = FlipGravity(direction);
			rigidbody2D.AddTorque(Random.Range(randomTorqueMin,randomTorqueMax));
		}
	}

	private float FlipGravity(int direction) {
		if(direction == 1) {
			return m_originalGravity;
		} else {
			return -1.0f*m_originalGravity;
		}
	}
}
