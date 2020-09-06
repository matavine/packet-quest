using UnityEngine;
using System.Collections;

public class ObstacleScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
		renderer.material.color = Color.magenta;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnCollisionEnter(Collision c){
		PlayerScript player = c.gameObject.GetComponent<PlayerScript>();
		if(player){
			player.killMe();
		}
	}
}
