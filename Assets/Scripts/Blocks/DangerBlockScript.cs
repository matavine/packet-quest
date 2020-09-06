using UnityEngine;
using System.Collections;

public class DangerBlockScript : MonoBehaviour {

	public void OnCollisionEnter2D (Collision2D collision) {
		DestroyEntity(collision.gameObject);
	}

	public void OnTriggerEnter2D (Collider2D collider){
		DestroyEntity(collider.gameObject);
	}

	private void DestroyEntity(GameObject entity) {
		PlayerBehaviour player = entity.GetComponent<PlayerBehaviour>();
		if(player){
			player.Kill();
		}
	}
}
