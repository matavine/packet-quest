using UnityEngine;
using System.Collections;

public class LevelController : MonoBehaviour {
	public static LevelController Instance {
		get {
			if (m_obj == null) {
				m_obj = new GameObject("LevelController");
				m_instance = m_obj.AddComponent<LevelController>();
				m_instance.Init();
			}
			return m_instance;
		}
	}

	public uint timeLimit;
	public uint messagesCollected = 0;
	public uint numMessages;
	public uint keysCollected = 0;
	public bool allMessagesCollected {
		get {
			return messagesCollected == numMessages;
		}
	}

	public CheckPointScript currentCheckPoint {
		get {
			return m_currCheckPoint;
		}
		set {
			m_currCheckPoint = value;
			m_checkPointTime = (uint)value.timeLimit;
			timeLimit = (uint)Mathf.Max(value.timeLimit, timeLimit);
		}
	}

	private static LevelController m_instance;
	private static GameObject m_obj;

	private int m_audioKey;
	private string backgroundAudio;
	private Font m_textFont;
	private Texture2D m_msgIcon;
	private Texture2D m_keyIcon;
//	private string timeLabel = "";
	private bool levelHasKeys = false;
//	private bool levelHasTimeLimit = false;

	private CheckPointScript m_currCheckPoint;
	private uint m_checkPointTime;

	private UIMenu m_hud;

	public void Begin (string backgroundAudio, float fadeIn) {
		this.backgroundAudio = backgroundAudio;
		if (backgroundAudio != null && backgroundAudio != "") {
			m_audioKey = AudioPlayer.Instance.Play(backgroundAudio, loop: true, fadeIn: fadeIn);
		}
	}

	IEnumerator UpdateTime() {
		while(true) {
//			timeLabel = string.Format("Time: {0}:{1:D2}", timeLimit/60, timeLimit%60);
			yield return new WaitForSeconds(1);
			timeLimit -= 1;
			if (timeLimit <= 0) {
				GameObject player = GameObject.FindGameObjectWithTag("Player");
				PlayerBehaviour playerBehaviour = player.GetComponent<PlayerBehaviour>();
				playerBehaviour.Kill();
				timeLimit = m_checkPointTime;
				// TODO: Something with music maybe?
			}
		}
	}

	void Init() {
		numMessages = (uint)GameObject.FindGameObjectsWithTag("Message").Length;
		levelHasKeys = GameObject.FindGameObjectsWithTag("Key").Length > 0;
		m_textFont = Resources.Load("Fonts/Fixedsys") as Font;
		m_msgIcon = Resources.Load("msg_hud_icon") as Texture2D;
		m_keyIcon = Resources.Load("key_hud_icon") as Texture2D;

		// Get the initial spawn point.
		GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
		if (spawnPoint == null) {
			Debug.LogError("No initial spawn point set.");
		} else {
			m_instance.currentCheckPoint = spawnPoint.GetComponent<CheckPointScript>();
			if (m_instance.currentCheckPoint == null) {
				Debug.LogError("No checkpoint script found on spawn point.");
			} else if (m_instance.currentCheckPoint.timeLimit > 0) {
				StartCoroutine(UpdateTime());
//				levelHasTimeLimit = true;
			}
		}

//		INFO_BOX_HEIGHT += (levelHasKeys ? 14 : 0) + (timeLabel != "" ? 14 : 0);
		m_hud = new UIMenu(RenderHUD);
		UIManager.Instance.AddMenu(m_hud);
	}

	private void RenderHUD(UIMenuItem[] items) {
		GUI.skin.label.font = m_textFont;
		GUI.skin.label.fontSize = 20;
		GUI.skin.label.normal.textColor = Color.black;
		GUI.skin.label.alignment = TextAnchor.MiddleLeft;
		
		Vector2 uiDimensions = new Vector2(Screen.width * 0.95f,
		                                   Screen.height * 0.95f);
		
		Vector2 topLeft = (new Vector2(Screen.width, Screen.height) - uiDimensions) / 2f;
		
		int width = 164;
		int height = 46;
		
		Rect msgRect = new Rect(topLeft.x, topLeft.y, width, height);
		GUI.DrawTexture(msgRect, m_msgIcon);
		Rect numMsgsRect = new Rect(msgRect.xMin + 77,
		                            msgRect.yMin + 10,
		                            width / 2,
		                            height / 2);
		GUI.Label(numMsgsRect, messagesCollected + " / " + numMessages);
		
		if (levelHasKeys) {
			Rect keyRect = new Rect(msgRect.xMin, msgRect.yMax + 5, width, height);
			GUI.DrawTexture(keyRect, m_keyIcon);
			Rect numKeysRect = new Rect(keyRect.xMin + 100,
			                            keyRect.yMin + 10,
			                            width / 2,
			                            height / 2);
			GUI.Label(numKeysRect, keysCollected.ToString());
		}
		
		GUI.skin = null;
	}

	public void PauseBackgroundAudio() {
		AudioPlayer.Instance.Pause(m_audioKey);
	}

	public void StopBackgroundAudio(float fadeOut = 0.0f) {
		if (fadeOut > 0.0) {
			AudioPlayer.Instance.FadeOut(m_audioKey, fadeOut);
		} else {
			AudioPlayer.Instance.Stop(m_audioKey);
		}
	}

	public void RestartAudio(float fadeOut = 0.0f, float fadeIn = 0.0f) {
		StopBackgroundAudio(fadeOut);
		Begin(backgroundAudio, fadeIn);
	}

//	void OnGUI() {
//		GUI.skin.label.font = m_textFont;
//		GUI.skin.label.fontSize = 20;
//		GUI.skin.label.normal.textColor = Color.black;
//
//		Vector2 uiDimensions = new Vector2(Screen.width * 0.9f,
//		                                   Screen.height * 0.9f);
//
//		Vector2 topLeft = new Vector2(Screen.width, Screen.height) - uiDimensions;
//
//		int width = 164;
//		int height = 46;
//
//		Rect msgRect = new Rect(topLeft.x, topLeft.y, width, height);
//		GUI.DrawTexture(msgRect, m_msgIcon);
//		Rect numMsgsRect = new Rect(msgRect.xMin + 77,
//		                            msgRect.yMin + 10,
//		                            width / 2,
//		                            height / 2);
//		GUI.Label(numMsgsRect, messagesCollected + " / " + numMessages);
//
//		if (levelHasKeys) {
//			Rect keyRect = new Rect(msgRect.xMin, msgRect.yMax + 5, width, height);
//			GUI.DrawTexture(keyRect, m_keyIcon);
//			Rect numKeysRect = new Rect(keyRect.xMin + 100,
//			                            keyRect.yMin + 10,
//			                            width / 2,
//			                            height / 2);
//			GUI.Label(numKeysRect, keysCollected.ToString());
//		}
//
//		GUI.skin = null;
//	}
}

