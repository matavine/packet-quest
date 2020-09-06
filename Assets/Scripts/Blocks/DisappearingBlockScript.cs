using UnityEngine;
using System.Collections;

public class DisappearingBlockScript : MonoBehaviour {

	[Range(0, float.MaxValue)]
	public float fadeOutTime;
	[Range(0, float.MaxValue)]
	public float fadeInTime;
	[Range(0, float.MaxValue)]
	public float disabledTime;
	[Range(0, 1)]
	public float transparency;
	public bool allowRigidbodies = false;

	private bool fadingOut = false;
	private bool fadingIn = false;

	private Color color;
	private Color transparent;

	private float timerStart;

	private bool playerInBlock = false;

	public void Start () {
		color = GetComponent<Renderer>().material.color;
		transparent = new Color(color.r, color.g, color.b, transparency);
	}
	
	// Update is called once per frame
	public void Update () {
		if(fadingOut) {
			Fade(color, transparent, fadeOutTime);
		}
		else if(fadingIn) {
			Fade(transparent, color, fadeInTime);
		}
	}

	public void Fade(Color colorA, Color colorB, float time) {
		float t = (Time.time - timerStart) / time;
		GetComponent<Renderer>().material.color = Color.Lerp(colorA, colorB, t);
	}

	private IEnumerator FadeOutTimer() {
		if (fadingIn || fadingOut) {
			yield break;
		}
		//Fade Out
		timerStart = Time.time;
		fadingOut = true;
		yield return new WaitForSeconds(fadeOutTime);
		fadingOut = false;
		gameObject.GetComponent<Collider2D>().isTrigger = true;

		//Disabled
		yield return new WaitForSeconds(disabledTime);

		StartCoroutine(FadeInTimer());
	}

	private IEnumerator FadeInTimer() {
		if (fadingIn || fadingOut) {
			yield break;
		}
		timerStart = Time.time;
		fadingIn = true;
		yield return new WaitForSeconds(fadeInTime);
		fadingIn = false;
		while (playerInBlock) {
			yield return new WaitForFixedUpdate();
		}
		gameObject.GetComponent<Collider2D>().isTrigger = false;
	}

	public void OnTriggerEnter2D(Collider2D collider) {
		if (collider.gameObject.GetComponent<PlayerBehaviour>() != null) {
			playerInBlock = true;
		}
	}

	public void OnTriggerStay2D(Collider2D collider) {
		if (collider.gameObject.GetComponent<PlayerBehaviour>() != null) {
			playerInBlock = true;
		}
	}

	public void OnTriggerExit2D(Collider2D collider) {
		if (playerInBlock && collider.gameObject.GetComponent<PlayerBehaviour>() != null) {
			playerInBlock = false;
		}
	}

	public void OnCollisionEnter2D(Collision2D collider) {
		PlayerBehaviour player = collider.gameObject.GetComponent<PlayerBehaviour>();
		Rigidbody2D objRigidbody = collider.gameObject.GetComponent<Rigidbody2D>();
		if ((player || (objRigidbody && allowRigidbodies)) && !fadingOut) {
			StartCoroutine(FadeOutTimer());
		}
	}
}
