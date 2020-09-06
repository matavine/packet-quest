using UnityEngine;
using System.Collections;

public class DoorBlockScript : MonoBehaviour {
	public int keyId;

	void OnCollisionEnter2D (Collision2D c){
		PlayerBehaviour player = c.gameObject.GetComponent<PlayerBehaviour>();
		if(player){
			if(player.RemoveKey()){
				gameObject.SetActive(false);
			}
		}
	}
}
