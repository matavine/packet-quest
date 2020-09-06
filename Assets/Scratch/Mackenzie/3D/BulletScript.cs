using UnityEngine;
using System.Collections;

public class BulletScript : MonoBehaviour {
	float duration = 0.5f;
	private float m_elapsedTime;

	// Use this for initialization
	void Start () {
		GetComponent<Renderer>().material.color = Color.red;
	}
	
	// Update is called once per frame
	void Update () {
		m_elapsedTime += Time.deltaTime;
		if (m_elapsedTime >= duration){
			Destroy(gameObject);
		}
	}
	
	void OnCollisionEnter(Collision c){
		PlayerScript player = c.gameObject.GetComponent<PlayerScript>();
		if(player){
			player.killMe();
		}
	}
}
