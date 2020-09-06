using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EndLevelScript))]
public class LevelChangerBlockScript : MonoBehaviour {
	public string transitionToLevel;
	public string errorMessage = "\"You haven't collected all the messages yet!\" - Sir Vigilant";
	private bool m_showGui = false;
	private Font m_textFont;
	private Texture2D m_background;
	private int m_textWidth = 420;
	private int m_textHeight = 60;
	private int m_textPadding = 10;

	private EndLevelScript m_endLevelScript;

	void OnCollisionEnter2D (Collision2D c){
		PlayerBehaviour player = c.gameObject.GetComponent<PlayerBehaviour>();
		if(player){
			if(!LevelController.Instance.allMessagesCollected) {
				m_showGui = true;
			} else {
				m_endLevelScript.Begin(transitionToLevel);
			}
		}
	}

	void Start() {
		m_textFont = Resources.Load("Fonts/Fixedsys") as Font;
		m_background = new Texture2D(1, 1);
		m_background.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.8f));
		m_background.Apply();

		m_endLevelScript = GetComponent<EndLevelScript>();
	}

	void Update() {
		if (m_showGui && Input.anyKeyDown) {
			m_showGui = false;
		}
	}

	void OnGUI() {
		if (m_showGui) {
			GUI.skin.label.font = m_textFont;
			GUI.skin.label.normal.textColor = Color.white;
			GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
			centeredStyle.alignment = TextAnchor.MiddleCenter;
			centeredStyle.fontSize = 18;
			int left = (Screen.width - m_textWidth)/2;
			int top = (Screen.height - m_textHeight)/2;
			Rect backRect = new Rect(left - m_textPadding, top - m_textPadding, m_textWidth + m_textPadding, m_textHeight + m_textPadding);
			GUI.DrawTexture(backRect, m_background);
			Rect textRect = new Rect(left, top, m_textWidth, m_textHeight);
			GUI.Label(textRect, errorMessage);
			GUI.skin = null;
		}
	}
}
