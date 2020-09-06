using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {
	private GameObject m_player;
	// Use this for initialization
	void Start () {
		m_player = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void LateUpdate () {
		this.transform.position = new Vector3(m_player.transform.position.x, m_player.transform.position.y, this.transform.position.z);
	}
}
