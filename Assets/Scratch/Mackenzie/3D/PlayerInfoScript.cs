using UnityEngine;
using System.Collections;

public class PlayerInfoScript : MonoBehaviour {
	int lives;
	GameObject currCheckpoint;
	
	void Awake () {
		DontDestroyOnLoad(gameObject);
	}

	// Use this for initialization
	void Start () {
		lives = 3;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public int getLives(){
		return lives;
	}
	public void decrementLives(){
		lives--;
	}
	public void setLives(int x){
		lives = x;
	}
	public GameObject getCurrentCheckpoint(){
		return currCheckpoint;
	}
	public void setCurrentCheckpoint(GameObject x){
		currCheckpoint = x;
	}
	public void gameOver(){
		GameObject[] allObjects = GameObject.FindGameObjectsWithTag("platform");
		foreach(GameObject thisObject in allObjects){
   			if(thisObject.activeInHierarchy){
      			thisObject.GetComponent<Renderer>().enabled = false;
			}
		}
		GameObject player = GameObject.FindGameObjectWithTag("player");
		player.GetComponent<Renderer>().enabled = false;
		
	}
}
