using UnityEngine;
using System.Collections;

public class TeleporterScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
		renderer.material.color = Color.yellow;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnCollisionEnter(Collision c){
		PlayerScript player = c.gameObject.GetComponent<PlayerScript>();
		GameObject location = GameObject.Find("checkpoint2");
		if(player){
			c.gameObject.transform.position = location.transform.position;
			GameObject.Find("PlayerInfo").GetComponent<PlayerInfoScript>().setCurrentCheckpoint(location);
		}
	}
}
