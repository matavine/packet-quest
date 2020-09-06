using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CreditsScript : MonoBehaviour {
	private const float MENU_TIMEOUT_SECONDS = 0.15f;
	
	public GUISkin menuSkin;
	public int menuWidth;
	public int menuHeight;
	
	public float letterPause = 0.05f;
	public AudioClip sound;
	
	string message;
	
	private UIMenu m_startMenu;
	
	public void Start () {
		// Mono seems to have forgetten how default arguments work and 
		// won't let me omit any of these because it's a bitch
		m_startMenu = new UIMenu(new string[] {
			"Back to Menu",
		}, RenderMenu, OnButtonClicked);
		
		guiText.pixelOffset = new Vector2(Screen.width/2, Screen.height/2);
		message = guiText.text;
		guiText.text = "";
		StartCoroutine(TypeText ());
	}
	
	private void OnButtonClicked(UIMenuItem menuItem) {
		if (menuItem.name == "Back to Menu") {
			Application.LoadLevel("StartMenu");
		}
	}
	
	IEnumerator TypeText () {
		int playClipAfter = 0;
		System.Random generator = new System.Random();
		foreach (char letter in message.ToCharArray()) {
			if (letter == '@') {
				guiText.text += '.'; // Used to ensure we don't pause for Dr. Michievous
			} else {
				guiText.text += letter;
			}
			if (letter == '\n') {
				AudioPlayer.Instance.Play("typewriter-line-break-1");
				playClipAfter++;
			}
			if (sound && playClipAfter-- == 0) {
				audio.PlayOneShot (sound);
				playClipAfter = generator.Next(3); // ensure greater than 0
			}
			if (letter == '.' || letter == '!' || letter == ':') {
				yield return new WaitForSeconds(2*letterPause);
			}
			yield return new WaitForSeconds (letterPause);
		}
		UIManager.Instance.AddMenu(m_startMenu);
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
