using UnityEngine;
using System.Collections;

public class PlaneScript : MonoBehaviour {

	// Use this for initialization
	void Start () {

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