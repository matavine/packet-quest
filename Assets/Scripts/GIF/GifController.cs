using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GifController : MonoBehaviour {
	public static GifController Instance {
		get {
			if(m_object == null) {
				m_object = new GameObject("GifController");
				m_instance = m_object.AddComponent<GifController>();
				m_instance.Init();
			}
			return m_instance;
		}
	}

	public Queue<GameObject> gifs;

	private static GifController m_instance;
	private static GameObject m_object;
	private GameObject m_currentGif;
	private Animator m_currentAnimator;
	private bool m_gifPlaying = false;
	private bool m_firstStateCheck = true; //its terrible I know
	private Camera m_camera;

	// Use this for initialization
	void Init () {
		gifs = new Queue<GameObject>();
		m_camera = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
		if(m_gifPlaying) {
			//Gif has stopped
			if(m_currentAnimator.GetCurrentAnimatorStateInfo(0).IsName("Stop") && !m_firstStateCheck) {
				m_currentAnimator.SetBool("playGif", false);
				Destroy(m_currentGif);
				m_gifPlaying = false;
			}
			if(m_firstStateCheck) {
				m_firstStateCheck = false;
			}
		}
		else {
			if(gifs.Count > 0) {
				m_currentGif = (GameObject)  Instantiate(gifs.Dequeue(), new Vector3(0, 2, 10), new Quaternion());
				m_currentAnimator = m_currentGif.GetComponent<Animator>();
				m_gifPlaying = true;
				m_firstStateCheck = true;
				//Set the gif into the play state
				m_currentAnimator.SetBool("playGif", true);
			}
		}
	}

	void LateUpdate() {
		if(m_gifPlaying) {
			Vector3 bottomLeft = m_camera.ViewportToWorldPoint(new Vector3(0, 0, 10));
			bottomLeft.y += m_currentGif.GetComponent<Renderer>().bounds.size.y;
			m_currentGif.transform.position = bottomLeft;
		}
	}
}
