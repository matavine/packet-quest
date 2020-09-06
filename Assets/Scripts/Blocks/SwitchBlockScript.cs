using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwitchBlockScript : MonoBehaviour {

	public List<GameObject> blockList = new List<GameObject>();

	private bool m_active = false;
	private Sprite m_activeSprite;
	private Sprite m_inactiveSprite;

	// Use this for initialization
	void Start () {
		m_activeSprite = Resources.Load<Sprite>("switch_on") as Sprite;
		m_inactiveSprite = Resources.Load<Sprite>("switch_off") as Sprite;
	}

	void OnTriggerEnter2D (Collider2D c){
		PlayerBehaviour player = c.gameObject.GetComponent<PlayerBehaviour>();
		if(player){
			foreach(GameObject affectedBlock in blockList){
				AbsToggleBlock toggleBlock = affectedBlock.GetComponent<AbsToggleBlock>();
				if(toggleBlock){
					toggleBlock.ToggleBlock();
				}
			}
			if(m_active){
				m_active = false;
				gameObject.GetComponent<SpriteRenderer>().sprite = m_inactiveSprite;
			} else {
				m_active = true;
				gameObject.GetComponent<SpriteRenderer>().sprite = m_activeSprite;
				AudioPlayer.Instance.PlayAtPosition("circuit", this.transform.position);
			}

			BoxCollider2D box = collider2D as BoxCollider2D;
			CircuitParams circuit = new CircuitParams();
			CircuitScript.SendCircuitSignal(null, box, circuit);
		}
	}
}
