using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class StartMenu : MonoBehaviour {
	private const float MENU_TIMEOUT_SECONDS = 0.15f;

	public GUISkin menuSkin;
	public int menuWidth;
	public int menuHeight;

	private UIMenu m_startMenu;
	private UIMenu m_selectLevelMenu;

	public void Start () {
		// Mono seems to have forgetten how default arguments work and 
		// won't let me omit any of these because it's a bitch
		AudioPlayer.Instance.Play("Hexadecimal", 1.0f, true, 2.0f, 10);
		m_startMenu = new UIMenu(new string[] {
			"New Game",
			"Continue",
			"Quit"
		}, RenderMenu, OnButtonClicked);

		m_selectLevelMenu = new UIMenu(new string[] {
			"Introduction",
			"Wub-Wub",
			"Teh Interwebz",
			"Never Gonna Let You Down",
			"Once More, With Feeling",
			"Back"
		}, RenderSelectLevelMenu, OnButtonClicked);

		UIManager.Instance.AddMenu(m_startMenu);
	}

	private void OnButtonClicked(UIMenuItem menuItem) {
		if (menuItem.name == "New Game") {
			Application.LoadLevel("story");
		} else if (menuItem.name == "Continue") {
			UIManager.Instance.RemoveMenu(m_startMenu);
			UIManager.Instance.AddMenu(m_selectLevelMenu, true, 0);
		} else if (menuItem.name == "Quit") {
			Application.Quit();
		} else if (menuItem.name == "Back") {
			UIManager.Instance.RemoveMenu(m_selectLevelMenu);
			UIManager.Instance.AddMenu(m_startMenu);
		} else if (menuItem.name == "Introduction") {
			Application.LoadLevel("scene1");
		} else if (menuItem.name == "Wub-Wub") {
			Application.LoadLevel("Audio-1");
		} else if (menuItem.name == "Teh Interwebz") {
			Application.LoadLevel("Internet-01");
		} else if (menuItem.name == "Never Gonna Let You Down") {
			Application.LoadLevel("gravity-scene1");
		} else if (menuItem.name == "Once More, With Feeling") {
			Application.LoadLevel("Corruption_01");
		}
	}

	private void RenderSelectLevelMenu(UIMenuItem[] menu) {
		GUI.skin = menuSkin;
		GUILayout.BeginArea(new Rect((Screen.width - menuWidth - 100) / 2,
		                             (Screen.height - menuHeight - 100) / 1.25f,
		                             menuWidth + 100,
		                             menuHeight + 200));
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

	private void RenderMenu(UIMenuItem[] menu) {
		GUI.skin = menuSkin;
		GUILayout.BeginArea(new Rect((Screen.width - menuWidth) / 2,
		                             (Screen.height - menuHeight) / 1.25f,
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
}
