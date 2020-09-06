using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class GravityField : MonoBehaviour {
	
	public enum Mode {
		MANUAL, // Toggle must be called manually
		TOGGLE, // Player entering and leaving will toggle state
		TOGGLEON, // Player can only Toggle on
		TOGGLEOFF // Player can only Toggle off
	}

	public Mode mode;
	public bool startTurnedOn;
	public float toggleOffDelay = 0f;
	public float toggleOnDelay = 0f;
	public float particleAcceleration;
	private Collider2D[] m_flippedColliders;

	private bool m_isOn = true;
	private bool m_isTurningOn = false;
	private ParticleSystem m_childParticleSystem;
	
	public void Start () {
		m_isOn = startTurnedOn;
		particleAcceleration = -Physics2D.gravity.y;
	}

	public void positionParticle() {
		BoxCollider2D box = (BoxCollider2D) collider2D;
		Transform child = transform.FindChild("GravityParticle");
		float duration = Mathf.Sqrt(2f*box.size.y/particleAcceleration);
		Vector2 position = new Vector2(transform.position.x, transform.position.y) + box.center;
		m_childParticleSystem = child.particleSystem;

		position.y -= box.size.y/2f;
		child.position = new Vector3(position.x, position.y, child.transform.position.z);
		m_childParticleSystem.startLifetime = duration;
		child.localScale = new Vector3(box.size.x,1,1);

		if(startTurnedOn) Play();
	}

	public void Update() {
		if(m_childParticleSystem == null) positionParticle();

		ParticleSystem.Particle[] particles = new ParticleSystem.Particle[m_childParticleSystem.particleCount+1];
		int num = m_childParticleSystem.GetParticles(particles);
		Vector3 acceleration = new Vector3(particleAcceleration,0,0);

		for(int i = 0; i < num; i++) {
			particles[i].velocity += acceleration*Time.deltaTime;
		}

	}

	public void OnTriggerEnter2D(Collider2D collider) {
		if(IsPlayer(collider)) {
			if((mode == Mode.TOGGLE || mode == Mode.TOGGLEON) && !m_isTurningOn) {
				StartCoroutine(Toggle(toggleOnDelay));
			}
		}

		if(m_isOn) HandleEnteringGameObject(collider);
	}

	public void OnTriggerExit2D(Collider2D collider) {
		if(IsPlayer(collider)) {
			if((mode == Mode.TOGGLE || mode == Mode.TOGGLEOFF) && m_isTurningOn) {
				StartCoroutine(Toggle(toggleOffDelay));
			}
		}

		HandleExitingGameObject(collider);
	}

	private void HandleEnteringGameObject(Collider2D collider) {
		TryFlipGameObject(collider, -1);
	}

	private void HandleExitingGameObject(Collider2D collider) {
		TryFlipGameObject(collider, 1);
	}

	private void SetGravity(int direction) {
		BoxCollider2D box = (BoxCollider2D) collider2D;
		Vector2 position = new Vector2(transform.position.x, transform.position.y);
		Vector2 right_top = position + box.center + box.size/2f;
		Vector2 bottom_left = position + box.center - box.size/2f;
		Collider2D[] colliders = Physics2D.OverlapAreaAll(right_top, bottom_left);

		foreach(Collider2D c in colliders) {
			TryFlipGameObject(c, direction);
		}
	}

	public IEnumerator Toggle(float delay) {
		m_isTurningOn = !m_isTurningOn;

		if(m_isTurningOn) Play();
		else Stop();

		if (delay > 0.0f)
			yield return new WaitForSeconds(delay);

		if((m_isOn = m_isTurningOn)) {
			SetGravity(-1);
		} else {
			SetGravity(1);
		}
	}

	private void TryFlipGameObject(Collider2D collider, int direction) {
		FlippableBehaviour f;
		if((f=GetFlippableBehaviour(collider))) {
			f.Flip(direction);
		}
	}

	private FlippableBehaviour GetFlippableBehaviour(Collider2D collider) {
		return (FlippableBehaviour)collider.GetComponent<FlippableBehaviour>();
	}

	private bool IsPlayer(Collider2D collider) {
		return collider.gameObject.GetComponent<PlayerBehaviour>();
	}

	private void Play() {
		m_childParticleSystem.Play();
	}

	private void Stop() {
		m_childParticleSystem.Stop();
	}

	private void Clear() {
		m_childParticleSystem.Clear();
	}
}
