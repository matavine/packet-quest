using UnityEngine;
using System.Collections;

public class MessageScript : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D collider) {
		PlayerBehaviour player = collider.gameObject.GetComponent<PlayerBehaviour>();
		if(player){
			Destroy(this.gameObject);
			LevelController.Instance.messagesCollected++;
		}
	}
}
