using UnityEngine;
using System.Collections;

public class PlatformScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
		renderer.material.color = Color.green;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
