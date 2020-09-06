using UnityEngine;
using System.Collections;

public class TurretScript : MonoBehaviour {
	float speed = 20;
	float attackRate = 1;
	float timer=0;
	
	// Use this for initialization
	void Start () {
		renderer.material.color = Color.grey;
	}
	
	// Update is called once per frame
	void Update () {
		timer += Time.deltaTime;	
		if(timer > attackRate){
			GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Cube);
			bullet.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
			bullet.AddComponent<Rigidbody>();
			bullet.AddComponent<BulletScript>();
			bullet.transform.position = transform.position;
			bullet.transform.rotation = transform.rotation;
			bullet.rigidbody.velocity = transform.TransformDirection(Vector3.down*speed);
			
			timer = 0;
		}
	}
}
