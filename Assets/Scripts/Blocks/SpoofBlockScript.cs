using UnityEngine;
using System.Collections;

public class SpoofBlockScript : MonoBehaviour {
	public GameObject outsideBlock;
	public GameObject insideBlock;
	
	private GameObject m_outsideBlock;
	private GameObject m_insideBlock;

	// Use this for initialization
	void Start () {
		m_outsideBlock = GameObject.Instantiate(outsideBlock) as GameObject;
		m_insideBlock = GameObject.Instantiate(insideBlock) as GameObject;
		m_insideBlock.SetActive(false);
		m_outsideBlock.transform.position = transform.position;
		m_insideBlock.transform.position = transform.position;
	}

	void OnTriggerExit2D (Collider2D c){
		m_insideBlock.SetActive(false);
		m_outsideBlock.SetActive(true);
	}

	void OnTriggerEnter2D (Collider2D c){
		m_outsideBlock.SetActive(false);
		m_insideBlock.SetActive(true);
	}
}
