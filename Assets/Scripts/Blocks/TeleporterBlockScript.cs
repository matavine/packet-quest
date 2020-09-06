using UnityEngine;
using System.Collections;

public class TeleporterBlockScript : MonoBehaviour {
	public GameObject destination;

	private AnimationClip m_teleportInAnim;
	private AnimationClip m_teleportOutAnim;
	private static bool m_teleportingPlayer = false;

	public void Start() {
		m_teleportInAnim = Resources.Load<AnimationClip>("PlayerTeleportIn");
		m_teleportOutAnim = Resources.Load<AnimationClip>("PlayerTeleportOut");
	}

	void OnCollisionEnter2D (Collision2D c){
		PlayerBehaviour player = c.gameObject.GetComponent<PlayerBehaviour>();
		if(player){
			StartCoroutine(TeleportPlayer(player));
		}
	}

	private IEnumerator TeleportPlayer(PlayerBehaviour player) {
		if (m_teleportingPlayer) {
			yield break;
		}

		m_teleportingPlayer = true;

		GameObject playerShell = new GameObject("PlayerShell");
		Animation anim = playerShell.AddComponent<Animation>();
		anim.AddClip(m_teleportOutAnim, m_teleportOutAnim.name);
		anim.AddClip(m_teleportInAnim, m_teleportInAnim.name);

		Transform playerTrans = player.transform;
		playerShell.transform.position = playerTrans.position;
		playerTrans.parent = playerShell.transform;
		player.enabled = false;
		player.collider2D.enabled = false;

		AudioPlayer.Instance.PlayWithTransform("teleport", player.transform);

		anim.Play(m_teleportOutAnim.name, PlayMode.StopAll);
		yield return new WaitForSeconds(m_teleportOutAnim.length);
		player.renderer.enabled = false;

		Vector3 startPlayerPos = playerShell.transform.position;
		Vector3 toDest = destination.transform.position - startPlayerPos;
		float moveStartTime = Time.time;

		while (true) {
			float distFactor = Mathf.Lerp(0f, 1f, Time.time - moveStartTime);
			playerShell.transform.position = startPlayerPos + (distFactor * toDest);
			if (distFactor == 1f) {
				playerShell.transform.position = destination.transform.position;
				break;
			}

			yield return null;
		}

		player.renderer.enabled = true;
		anim.Play(m_teleportInAnim.name, PlayMode.StopAll);
		yield return new WaitForSeconds(m_teleportInAnim.length);

		playerTrans.parent = null;
		playerTrans.localScale = new Vector3(Mathf.Sign(playerTrans.localScale.x), 1f, 1f);
		player.ResetVelocity();
		player.enabled = true;
		player.collider2D.enabled = true;
		m_teleportingPlayer = false;

		Destroy(playerShell);
	}
}
