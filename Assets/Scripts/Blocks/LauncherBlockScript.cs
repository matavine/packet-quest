using UnityEngine;
using System.Collections;

public class LauncherBlockScript : MonoBehaviour {
	public Vector2 launchVector = new Vector2(0,10);
	public Vector2 fallOff = new Vector2(0.1f, 0.1f);
	public float enableTime = 0.5f;
	// This scale factor controls how easiliy a player can oppose this force
	// Eg: 10 makes the players opposition to the force 10 times less effective
	public Vector2 counteractScale = new Vector2(50, 1);
	[Range(1, 100)]
	public float maxParticleDistance = 5.0f;
	public Sprite arrowSprite;
	public bool resetPlayerVelocity = false;
	
	private bool m_enabled = true;
	private bool horizontal;

	public void Start() {
		horizontal = (launchVector.y == 0)? true : false;

		if(horizontal) {
			particleSystem.startLifetime = Mathf.Abs((-launchVector.x) / Physics2D.gravity.magnitude);
			if(Mathf.Abs(particleSystem.startLifetime * launchVector.x) > maxParticleDistance) {
				particleSystem.startLifetime = Mathf.Abs(maxParticleDistance / launchVector.x);
			}
		}
		else {
			particleSystem.startLifetime = Mathf.Abs((-launchVector.y) / Physics2D.gravity.magnitude);
			float speed = Mathf.Sqrt(Mathf.Pow(launchVector.x, 2) + Mathf.Pow(launchVector.y, 2));
			float distance = speed * particleSystem.startLifetime;
			if(distance > maxParticleDistance) {
				particleSystem.startLifetime = Mathf.Abs(maxParticleDistance / speed);
			}
		}

		if (arrowSprite == null) {
			Debug.LogWarning("Missing arrow sprite on launcher block.");
			return;
		}

		GameObject arrowObj = new GameObject("Arrow");
		arrowObj.transform.parent = transform;
		arrowObj.transform.position = transform.position + Vector3.back * 0.1f;
		float angle = Vector2.Angle(Vector2.up, launchVector);
		if (launchVector.x < 0) {
			angle *= -1;
		}

		arrowObj.transform.Rotate(Vector3.back, angle);

		SpriteRenderer spriteRenderer = arrowObj.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = arrowSprite;
	}

	void OnCollisionEnter2D (Collision2D c){
		PlayerBehaviour player = c.gameObject.GetComponent<PlayerBehaviour>();
		if(player && m_enabled){
			m_enabled = false;
			if (resetPlayerVelocity) {
				player.ResetVelocity();
			}

			player.JumpsLeft = 0;
			player.DisableJumpNextFrame = true;
			player.AddBoost(new Boost(launchVector,fallOff,counteractScale));
			StartCoroutine(Enable());
		}
	}

	public IEnumerator Enable() {
		yield return new WaitForSeconds(enableTime);
		m_enabled = true;
	}

	public void LateUpdate() {
		ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.particleCount+1];
		int numParticles = particleSystem.GetParticles(particles);

		for(int i=0; i<numParticles; i++) {
			if(horizontal) {
				particles[i].velocity = new Vector3(launchVector.x, 0, 0);
			}
			else {
				float yVelocity = ((particles[i].startLifetime - particles[i].lifetime) * -Physics2D.gravity.magnitude) + launchVector.y;
				particles[i].velocity = new Vector3(launchVector.x, yVelocity, 0);
			}
		}

		particleSystem.SetParticles(particles, numParticles);
	}
}