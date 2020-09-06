using UnityEngine;
using System.Collections;

public class FixedRateCameraScript : MonoBehaviour {
	public float speed = 1;
	public float lenience = 2;
	private GameObject m_player;
	private PlayerBehaviour m_playerBehaviour;

	void Start () {
		m_player = GameObject.FindGameObjectWithTag("Player");
		m_playerBehaviour = m_player.GetComponent<PlayerBehaviour>();
	}

	void Update () {
		if (m_player.transform.position.x < this.transform.position.x - this.GetComponent<Camera>().orthographicSize * this.GetComponent<Camera>().aspect - lenience) {
			m_playerBehaviour.Kill(false); // don't show kill animation cause we wouldn't see it anyway
			this.transform.Translate(m_player.transform.position.x - this.transform.position.x,
			                         m_player.transform.position.y - this.transform.position.y,
			                         0);
		} else {
			this.transform.Translate(Time.deltaTime * speed, m_player.transform.position.y - this.transform.position.y, 0);
		}
	}
}
