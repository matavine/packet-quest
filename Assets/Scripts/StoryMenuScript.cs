using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class StoryMenuScript : MonoBehaviour {
	private const float MENU_TIMEOUT_SECONDS = 0.15f;
	
	public GUISkin menuSkin;
	public int menuWidth;
	public int menuHeight;

	public float letterPause = 0.05f;
	public AudioClip sound;

	private UIMenu m_startGameButton;
	private UIMenu m_skipTextButton;
	private UIMenu m_nextButton;

	private int m_textSection;

	private string[] m_textSections = new string[] {
		"Dr@ Mischievous, a highly intelligent and overly elaborate evil genius,\n" +
		"has constructed a devious plan for total world domination. Sir Vigilant,\n" +
		"a crusader of justice and long time nemesis of Dr@ Mischievous, is alerted\n" +
		"of his dastardly plan and quickly ventures forth towards Dr@ Mischievous'\n" +
		"lair to find the plan and stop Dr@ Mischievous.",

		"Upon breaching the archive room of Dr@ Mischievous' lair, Sir Vigilant\n" +
		"located the encrypted file containing the evil plan by logging into the\n" +
		"central computer. However, as Sir Vigilant attempted to download the file,\n" +
		"a security alarm was tripped causing the computer to fragment the file and\n" +
		"scatter it across the internet. Frustrated with his ineptitude, Sir Vigilant\n" +
		"inserts a USB drive into the computer with \"Byte,\" his sophisticated and\n" +
		"powerful Artificial Intelligence, onboard.",

		"Byte is tasked with traversing through Dr@ Mischievous' network and the\n" +
		"internet to find and collect all pieces of the evil plan. He must endure\n" +
		"the perils of firewalls, viruses, data corruption, and more to achieve his\n" +
		"goal. Dr@ Mischievous is soon to act, so Sir Vigilant and Byte must move\n" +
		"swiftly -- before it's too late!"
	};
	
	public void Start () {
		m_startGameButton = new UIMenu(new string[] {
			"Start Game",
		}, RenderMenu, OnButtonClicked);
		m_skipTextButton = new UIMenu(new string[] {
			"Skip",
		}, RenderMenu, OnButtonClicked);
		m_nextButton = new UIMenu(new string[] {
			"Next",
		}, RenderMenu, OnButtonClicked);

		UIManager.Instance.AddMenu(m_skipTextButton);

		GetComponent<GUIText>().pixelOffset = new Vector2(Screen.width/2, Screen.height/2);
		StartCoroutine("TypeText");
	}

	private void OnButtonClicked(UIMenuItem menuItem) {
		if (menuItem.name == "Start Game") {
			Application.LoadLevel("scene1");
		} else if (menuItem.name == "Skip") {
			StopCoroutine("TypeText");
			GetComponent<GUIText>().text = m_textSections[m_textSection].Replace('@', '.');
			UIManager.Instance.RemoveMenu(m_skipTextButton);
			if (m_textSection == (m_textSections.Length - 1)) {
				UIManager.Instance.AddMenu(m_startGameButton);
			} else {
				UIManager.Instance.AddMenu(m_nextButton);
			}
		} else if (menuItem.name == "Next") {
			m_textSection++;
			StartCoroutine("TypeText");
			UIManager.Instance.RemoveMenu(m_nextButton);
			UIManager.Instance.AddMenu(m_skipTextButton);
		}
	}

	IEnumerator TypeText () {
		GetComponent<GUIText>().text = "";
		int playClipAfter = 0;
		System.Random generator = new System.Random();
		foreach (char letter in m_textSections[m_textSection]) {
			if (letter == '@') {
				GetComponent<GUIText>().text += '.'; // Used to ensure we don't pause for Dr. Michievous
			} else {
				GetComponent<GUIText>().text += letter;
			}
			if (letter == '\n') {
				AudioPlayer.Instance.Play("typewriter-line-break-1");
				playClipAfter++;
			}
			if (sound && playClipAfter-- == 0) {
				GetComponent<AudioSource>().PlayOneShot (sound);
				playClipAfter = generator.Next(3); // ensure greater than 0
			}
			if (letter == '.' || letter == '!' || letter == ':') {
				yield return new WaitForSeconds(2*letterPause);
			}
			yield return new WaitForSeconds (letterPause);
		}
		m_textSection++;
		UIManager.Instance.RemoveMenu(m_skipTextButton);
		if (m_textSection >= m_textSections.Length) {
			UIManager.Instance.AddMenu(m_startGameButton);
		} else {
			UIManager.Instance.AddMenu(m_nextButton);
		}
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
