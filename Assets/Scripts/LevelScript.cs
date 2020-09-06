using UnityEngine;

public class LevelScript : MonoBehaviour {
	public string backgroundAudio;
	public float fadeIn = 0.0f;

	void Start () {
		LevelController.Instance.Begin(backgroundAudio, fadeIn);
	}
}