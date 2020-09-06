using UnityEngine;
using System.Collections;

public class LauncherScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
		renderer.material.color = Color.red;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnCollisionEnter(Collision c){
		Vector3 launch = new Vector3(10, 10, 0);
		PlayerScript player = c.gameObject.GetComponent<PlayerScript>();
		if(player){
			c.gameObject.GetComponent<Rigidbody>().AddForce(launch, ForceMode.Impulse);
		}
	}
}
