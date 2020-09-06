using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class EndLevelScript : MonoBehaviour {
	private const float FADE_TO_BLACK_SECONDS = 1.0f;
	private const float DISPLAY_TEXT_DELAY_SECONDS = 0.5f;

	private Texture2D m_background;
	private float m_backgroundOpacity = 0.0f;
	private float m_fadeStartTime;
	private bool m_displayText = false;
	private GUISkin m_uiSkin;

	private enum EndState {
		FADE_TO_BLACK,
		DISPLAY_MESSAGE,
		NONE
	};

	private static string[] m_messageFragments = new string[] {
		"TOP SECRET! AUTHORIZED VIEWING ONLY!\n",
		"Utilizing our latest neurotoxin, Cylon Halitosis, we will begin ",
		"to disrupt the global economy. Using the miniature robotic penguins, ",
		"we will spread the toxin. The second pincer of this attack: subliminal ",
		"messages planted in images of cats on the internet will react with the ",
		"neurotoxin. This will trigger a trance for all the users of the internet ",
		"compelling them to give all their Dogecoins to an anonymous wallet. With ",
		"the newest and hottest cryptocurrency monopolized, we will destabilize the ",
		"worlds economy and compel to U.N. to name ",
		"Dr. Mischievous the Supreme King of the World!"
	};

	private static EndState m_state = EndState.NONE;
	private UIMenu m_endMenu;

	private string m_nextLevel;
	private bool m_instanceActivated = false;
	private string unlockedMessage = "";
	public bool[] unlockedFragments = new bool[10];

	public void Begin(string nextLevel) {
		if (m_state != EndState.NONE) {
			return;
		}

		Regex regex = new Regex(@"\S");
		for(int i=0;i<m_messageFragments.Length;i++){
			if (unlockedFragments[i]) {
				unlockedMessage += m_messageFragments[i];
			} else {
				unlockedMessage += regex.Replace(m_messageFragments[i], "#");
			}
		}

		UIManager.Instance.ClearMenus();

		AudioPlayer.Instance.audioEnabled = false;

		m_instanceActivated = true;
		m_nextLevel = nextLevel;

		m_endMenu = new UIMenu(new string[] {
			"Next Level",
			"Quit"
		}, RenderMenu, OnButtonClicked);

		m_endMenu.Items[0].SetNeighbour(UIMenuItem.Direction.Right, m_endMenu.Items[1]);
		m_endMenu.Items[0].SetNeighbour(UIMenuItem.Direction.Left, m_endMenu.Items[1]);
		m_endMenu.Items[0].SetNeighbour(UIMenuItem.Direction.Up, null);
		m_endMenu.Items[0].SetNeighbour(UIMenuItem.Direction.Down, null);

		m_endMenu.Items[1].SetNeighbour(UIMenuItem.Direction.Right, m_endMenu.Items[0]);
		m_endMenu.Items[1].SetNeighbour(UIMenuItem.Direction.Left, m_endMenu.Items[0]);
		m_endMenu.Items[1].SetNeighbour(UIMenuItem.Direction.Up, null);
		m_endMenu.Items[1].SetNeighbour(UIMenuItem.Direction.Down, null);

		UIManager.Instance.AddMenu(m_endMenu);

		m_uiSkin = Resources.Load("UI/UISkin") as GUISkin;

		m_state = EndState.FADE_TO_BLACK;
		m_background = new Texture2D(1, 1);
		m_fadeStartTime = Time.time;

		// Disable player behaviour.
		GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
		PlayerBehaviour player = playerObj.GetComponent<PlayerBehaviour>();
		player.AnimControl.SetFloat("Speed", 0f);
		player.enabled = false;
	}

	public void Update () {
		if (!m_instanceActivated) {
			return;
		}

		switch (m_state) {
			case EndState.FADE_TO_BLACK:
				m_backgroundOpacity = Mathf.Lerp(0f, 1f, (Time.time - m_fadeStartTime) / FADE_TO_BLACK_SECONDS);
				if (m_backgroundOpacity == 1f) {
					m_state = EndState.DISPLAY_MESSAGE;
					StartCoroutine(DisplayTextDelay());
				}
				break;
			case EndState.DISPLAY_MESSAGE:
				break;
			default:
				break;
		}
	}

	private IEnumerator DisplayTextDelay() {
		yield return new WaitForSeconds(DISPLAY_TEXT_DELAY_SECONDS);
		m_displayText = true;
		UIManager.Instance.SetActiveItem(m_endMenu.Items[0]);
	}

	private void OnButtonClicked(UIMenuItem item) {
		m_state = EndState.NONE;
		if (item.name == "Quit") {
			Application.LoadLevel("StartMenu");
		} else if (item.name == "Next Level") {
			Application.LoadLevel(m_nextLevel);
		}
	}

	private void RenderMenu(UIMenuItem[] menu) {
		Color color = new Color(0, 0, 0, m_backgroundOpacity);
		m_background.SetPixel(0, 0, color);
		m_background.Apply();

		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), m_background);

		if (!m_displayText) {
			return;
		}

		GUI.skin = m_uiSkin;
		GUIStyle headingStyle = GUI.skin.GetStyle("Heading");
		
		string heading = "LEVEL COMPLETE!";
		Vector2 headingSize = headingStyle.CalcSize(new GUIContent(heading));
		Rect headingRect = new Rect((Screen.width - headingSize.x) / 2,
		                            (Screen.height / 6),
		                            headingSize.x,
		                            headingSize.y);
		
		GUI.Label(headingRect, heading, headingStyle);
		
		float textWidth = (Screen.width * 0.7f);
		float textHeight = (Screen.height * 0.66f);
		
		GUILayout.BeginArea(new Rect((Screen.width - textWidth) / 2,
		                             (Screen.height - textHeight) / 1.25f,
		                             textWidth,
		                             textHeight));
		GUILayout.BeginVertical();
		
		string text = "MEMO: Dr. Mischievous Plan to Rule the World # 87\n\n" + unlockedMessage;
		
		GUI.Label(new Rect(0, 0, textWidth, textHeight), text);
		
		GUILayout.EndVertical();
		GUILayout.EndArea();
		
		GUILayout.BeginArea(new Rect((Screen.width - 500) / 2,
		                             (Screen.height - 100),
		                             500,
		                             100));
		GUILayout.BeginHorizontal();

		foreach (UIMenuItem item in menu) {
			GUI.SetNextControlName(item.guid);
			if (GUILayout.Button(new GUIContent(item.name, item.guid), GUILayout.Width(250))) {
				OnButtonClicked(item);
			}
		}

		GUILayout.EndHorizontal();
		GUILayout.EndArea();

		GUI.skin = null;
	}
}
