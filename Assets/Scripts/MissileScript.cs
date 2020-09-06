using UnityEngine;
using System.Collections;

public class MissileScript : MonoBehaviour {
	void OnCollisionEnter2D (Collision2D c){
		PlayerBehaviour player = c.gameObject.GetComponent<PlayerBehaviour>();
		if(player){
			player.Kill();
		}
		Destroy(gameObject);
	}
}
