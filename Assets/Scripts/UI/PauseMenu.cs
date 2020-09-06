using UnityEngine;
using System;
using System.Collections;

public class PauseMenu : MonoBehaviour {
	private const int INPUT_TIMEOUT_MILLISECONDS = 150;
	private const float SCREEN_USAGE_PERCENTAGE = 0.80f;

	public GUISkin menuSkin;
	public Color menuBackground;

	public int menuWidth = 300;
	public int menuHeight = 300;

	private Texture2D m_background;
	private bool m_inputTimedOut = false;
	private bool m_show = false;

	private float m_timeScale;
	private DateTime m_lastInputTime;

	private UIMenu m_pauseMenu;
	private UIMenu m_quitMenu;

	public void Start() {
		m_background = GUIHelper.GetColourTexture(menuBackground);
		m_lastInputTime = DateTime.Now;

		m_pauseMenu = new UIMenu(new string[] {
			"Resume",
			"Restart Checkpoint",
			"Exit to Main Menu"
		}, RenderPauseMenu, OnButtonClicked);

		m_quitMenu = new UIMenu(new string[] {
			"Yes",
			"No",
		}, RenderQuitMenu, OnButtonClicked);
	}

	private void OnButtonClicked(UIMenuItem menuItem) {
		if (menuItem.name == "Resume") {
			TogglePauseMenu();
		} else if (menuItem.name == "Restart Checkpoint") {
			TogglePauseMenu();
			GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
			PlayerBehaviour player = playerObj.GetComponent<PlayerBehaviour>();
			player.Kill();
		} else if (menuItem.name == "Exit to Main Menu") {
			UIManager.Instance.RemoveMenu(m_pauseMenu);
			UIManager.Instance.AddMenu(m_quitMenu, true, 1);
		} else if (menuItem.name == "Yes") {
			TogglePauseMenu();
			Application.LoadLevel("StartMenu");
		} else if (menuItem.name == "No") {
			UIManager.Instance.RemoveMenu(m_quitMenu);
			UIManager.Instance.AddMenu(m_pauseMenu);
		}
	}

	private void RenderBackground() {
		Rect bgBounds = new Rect(0, 0, Screen.width, Screen.height);
		GUI.DrawTexture(bgBounds, m_background);
	}

	private void RenderQuitMenu(UIMenuItem[] menu) {
		RenderBackground();

		GUI.skin = menuSkin;

		GUILayout.BeginArea(new Rect((Screen.width - menuWidth) / 2,
		                             (Screen.height - menuHeight) / 2,
		                             menuWidth,
		                             menuHeight));
		GUILayout.BeginVertical();

		GUILayout.TextArea("Are you sure you want to return to the main menu?");

		foreach (UIMenuItem item in menu) {
			GUI.SetNextControlName(item.guid);
			if (GUILayout.Button(new GUIContent(item.name, item.guid))) {
				OnButtonClicked(item);
			}
		}
		
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	private void RenderPauseMenu(UIMenuItem[] menu) {
		if (!m_show) {
			return;
		}

		RenderBackground();

		GUI.skin = menuSkin;
		GUILayout.BeginArea(new Rect((Screen.width - menuWidth) / 2,
		                             (Screen.height - menuHeight) / 2,
		                             menuWidth,
		                             menuHeight));
		GUILayout.BeginVertical();
		
		foreach (UIMenuItem item in menu) {
			GUI.SetNextControlName(item.guid);
			if (GUILayout.Button(new GUIContent(item.name, item.guid))) {
				OnButtonClicked(item);
			}
		}
		
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	private void TogglePauseMenu() {
		m_lastInputTime = DateTime.Now;
		m_inputTimedOut = true;
		m_show = !m_show;

		if (m_show) {
			UIManager.Instance.AddMenu(m_pauseMenu, true, 0);
		} else {
			UIManager.Instance.RemoveMenu(m_pauseMenu);
			UIManager.Instance.RemoveMenu(m_quitMenu);
		}
		
		if (m_show) {
			m_timeScale = Time.timeScale;
			Time.timeScale = 0f;
		} else {
			Time.timeScale = m_timeScale;
		}
	}

	public void LateUpdate() {
		if (m_inputTimedOut) {
			m_inputTimedOut = UIManager.IsInputTimedOut(m_lastInputTime, INPUT_TIMEOUT_MILLISECONDS);
		}

		if (Input.GetButtonDown("Start") && !m_inputTimedOut) {
			TogglePauseMenu();
		}
	}
}
