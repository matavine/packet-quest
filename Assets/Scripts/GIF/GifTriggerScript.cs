using UnityEngine;
using System.Collections;

public class GifTriggerScript : MonoBehaviour {
	public GameObject gifPrefab;
	[Range(0, 60)]
	public float triggerDisabledTime;

	private GifController m_gifController;

	// Use this for initialization
	void Start () {
		m_gifController = GifController.Instance;
	}

	void OnCollisionEnter2D (Collision2D collision) {
		PlayerBehaviour player = collision.gameObject.GetComponent<PlayerBehaviour>();
		if(player) {
			m_gifController.gifs.Enqueue(gifPrefab);
			gameObject.GetComponent<Collider2D>().enabled = false;
			StartCoroutine(TriggerCoolDown());
		}
	}

	public IEnumerator TriggerCoolDown() {
		gameObject.GetComponent<Collider2D>().enabled = false;
		yield return new WaitForSeconds(triggerDisabledTime);
		gameObject.GetComponent<Collider2D>().enabled = true;
	}
}