using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {
	bool canJump;
	bool canMove;
	bool gameDone;

	PlayerInfoScript playerInfo;
	
	// Use this for initialization
	void Start () {
		MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
		renderer.material.color = Color.blue;
		canMove = true;
		canJump = true;
		gameDone = false;
		
		GameObject playerInfoObject = GameObject.Find("PlayerInfo");
		if(!playerInfoObject){
			playerInfoObject = new GameObject("PlayerInfo");
		}
		playerInfo = playerInfoObject.GetComponent<PlayerInfoScript>();
		if(!playerInfo){
			playerInfo = playerInfoObject.AddComponent<PlayerInfoScript>();
		}
		
		GameObject firstCheckpoint = GameObject.Find ("checkpoint1");
		playerInfo.setCurrentCheckpoint(firstCheckpoint);
	}
	
	// Update is called once per frame
	void Update () {
		if(canMove){
			float x = Input.GetAxis("Horizontal");
			if(x < 0){
				float m = -0.1f;
				gameObject.transform.position += new Vector3(m,0,0);
			}
			else if(x > 0){
				float m = 0.1f;
				gameObject.transform.position += new Vector3(m,0,0);
			}
			if(Input.GetKeyDown(KeyCode.Space) && canJump){
				canJump = false;
				gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up*8, ForceMode.Impulse);
			}
		}
	}
	
	void OnGUI(){
		if(gameDone){
			GUI.Box(new Rect(520, 300, 200, 20), "GAME OVER!");
		}
		else{
			string output = "Lives: " + playerInfo.getLives();
			GUI.Box(new Rect(10, 10, 200, 20), output);
		}
	}
	
	void OnCollisionEnter(Collision c){
		canJump = true;
	}
	
	public void killMe(){
		if(playerInfo.getLives() == 0){
			playerInfo.gameOver();
			gameDone = true;
		}
		else{
			Debug.Log("I'm dead");
			canMove = false;
			playerInfo.decrementLives();
			canMove = true;
			gameObject.transform.position = playerInfo.getCurrentCheckpoint().transform.position;
		}
	}		
}
