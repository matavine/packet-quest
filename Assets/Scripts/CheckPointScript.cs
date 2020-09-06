using UnityEngine;
using System.Collections;

public class CheckPointScript : MonoBehaviour {
	[Range(0, int.MaxValue/2)]
	public int timeLimit;

	private Sprite m_activeSprite;

	void Start () {
		m_activeSprite = Resources.Load<Sprite>("checkpoint_on") as Sprite;
	}

	void OnTriggerEnter2D (Collider2D c){
		PlayerBehaviour player = c.gameObject.GetComponent<PlayerBehaviour>();
		if(player){
			LevelController.Instance.currentCheckPoint = this;
			this.enabled = false; // Only pass a checkpoint once
			gameObject.GetComponent<SpriteRenderer>().sprite = m_activeSprite;
		}
	}
}
