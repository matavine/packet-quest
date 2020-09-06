using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DisguiseItemScript : MonoBehaviour {
	public float duration = 4;
	public float respawnTime = 6;

	public IEnumerator TriggerRespawn(){
		gameObject.GetComponent<SpriteRenderer>().enabled = false;
		gameObject.GetComponent<BoxCollider2D>().enabled = false;
		yield return new WaitForSeconds(respawnTime);
		gameObject.GetComponent<SpriteRenderer>().enabled = true;
		gameObject.GetComponent<BoxCollider2D>().enabled = true;
	}

	void OnCollisionEnter2D (Collision2D c){
		PlayerBehaviour player = c.gameObject.GetComponent<PlayerBehaviour>();
		if(player){
			player.ActivateDisguise(duration);
			StartCoroutine("TriggerRespawn");
		}
	}
}
