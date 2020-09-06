using UnityEngine;
using System.Collections;

public class FirewallBlockScript : AbsToggleBlock {

	private bool m_active = true;
	private Sprite m_activeSprite;
	private Sprite m_inactiveSprite;

	// Use this for initialization
	void Start () {
		m_activeSprite = Resources.Load<Sprite>("firewall_on") as Sprite;
		m_inactiveSprite = Resources.Load<Sprite>("firewall_off") as Sprite;
	}

	public override void ToggleBlock () {
		if(m_active){
			m_active = false;
			gameObject.GetComponent<SpriteRenderer>().sprite = m_inactiveSprite;
		}
		else {
			m_active = true;
			gameObject.GetComponent<SpriteRenderer>().sprite = m_activeSprite;
		}
	}

	public void OnTriggerEnter2D (Collider2D collider) {
		if (m_active) {
			PlayerBehaviour player = collider.gameObject.GetComponent<PlayerBehaviour>();
			if (player) {
				player.Kill();
			}
		}
	}
}
