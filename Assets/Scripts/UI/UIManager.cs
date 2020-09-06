using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {
	private const int SELECTION_TIMEOUT_MILLISECONDS = 150;

	private static UIManager m_instance;
	public static UIManager Instance {
		get {
			if (m_instance == null) {
				GameObject uiManagerObj = new GameObject("UIManager");
				m_instance = uiManagerObj.AddComponent<UIManager>();
			}

			return m_instance;
		}
	}

	private bool m_selectionTimedOut = false;
	private DateTime m_lastInputTime = DateTime.Now;
	private string m_lastTooltip;

	private List<UIMenu> m_menus = new List<UIMenu>();
	private Dictionary<string, UIMenuItem> m_guid = new Dictionary<string, UIMenuItem>();
	private UIMenuItem m_activeItem;

	private Queue<UIMenu> m_addMenuQueue = new Queue<UIMenu>();
	private Queue<UIMenu> m_removeMenuQueue = new Queue<UIMenu>();

	public void AddMenu(UIMenu menu, bool setActive = true, int selectedIndex = -1) {
		// Menus may be added during OnGUI when the m_menus 
		// array is iterated over. Add the menu at a later time.
		m_addMenuQueue.Enqueue(menu);

		if (setActive) {
			if (selectedIndex >= 0) menu.SetSelection(selectedIndex);
			m_activeItem = menu.GetSelectedItem();
		}
	}

	public void RemoveMenu(UIMenu menu) {
		// Menus may be removed during OnGUI when the m_menus 
		// array is iterated over. Remove the menu at a later time.
		m_removeMenuQueue.Enqueue(menu);

		if (m_activeItem != null && m_activeItem.menu == menu) {
			UIMenu activeMenu = m_menus.Find(x => x != menu);
			m_activeItem = (activeMenu != null) ? activeMenu.GetSelectedItem() : null;
		}
	}

	public void ClearMenus() {
		foreach (UIMenu menu in m_menus) {
			m_removeMenuQueue.Enqueue(menu);
		}

		m_activeItem = null;
	}

	public void SetActiveItem(UIMenuItem item) {
		if (item == null) {
			return;
		}

		m_activeItem = item;
	}

	public static bool IsInputTimedOut(DateTime lastInputTime, int timeout) {
		DateTime currTime = DateTime.Now;
		return (currTime - lastInputTime).TotalMilliseconds < timeout;
	}

	public void Update () {
		UpdateMenusState();

		if (m_activeItem == null) {
			return;
		}

		if (m_selectionTimedOut) {
			m_selectionTimedOut = IsInputTimedOut(m_lastInputTime, SELECTION_TIMEOUT_MILLISECONDS);
		}

		float vertical = Input.GetAxisRaw("Vertical");
		float horizontal = Input.GetAxisRaw("Horizontal");

		if ((vertical != 0 || horizontal != 0) && !m_selectionTimedOut) {
			UIMenuItem.Direction dir;
			if (Mathf.Abs(vertical) > Mathf.Abs(horizontal)) {
				dir = (Mathf.Sign(vertical) < 0) ? UIMenuItem.Direction.Down : UIMenuItem.Direction.Up;
			} else {
				dir = (Mathf.Sign(horizontal) < 0) ? UIMenuItem.Direction.Left : UIMenuItem.Direction.Right;
			}

			UIMenuItem next = m_activeItem.MoveSelection(dir);
			next.menu.SetSelection(next);
			m_activeItem = next;

			m_selectionTimedOut = true;
			m_lastInputTime = DateTime.Now;
		}

		if (Input.GetButtonDown("Select")) {
			m_activeItem.menu.OnClicked();
		}
	}

	private void UpdateMenusState() {
		if (m_removeMenuQueue.Count > 0) {
			foreach (UIMenu menu in m_removeMenuQueue) {
				m_menus.Remove(menu);
				
				foreach (UIMenuItem item in menu) {
					m_guid.Remove(item.guid);
				}
			}
			
			m_removeMenuQueue.Clear();
		}
		
		if (m_addMenuQueue.Count > 0) {
			foreach (UIMenu menu in m_addMenuQueue) {
				m_menus.Add(menu);
				
				foreach (UIMenuItem item in menu) {
					m_guid[item.guid] = item;
				}
			}
			
			m_addMenuQueue.Clear();
		}
	}

	public void OnGUI() {
		foreach (UIMenu menu in m_menus) {
			menu.Render();
			GUI.skin = null;
		}

		if (m_activeItem == null ||
		    m_addMenuQueue.Count > 0 ||
		    m_removeMenuQueue.Count > 0) {
			return;
		}

		if (GUI.tooltip != "" && GUI.tooltip != m_lastTooltip) {
			UIMenuItem selectedItem;
			if (m_guid.TryGetValue(GUI.tooltip, out selectedItem)) {
				m_activeItem = selectedItem;
				m_activeItem.menu.SetSelection(m_activeItem);
				m_lastTooltip = GUI.tooltip;
			}
		}
		
		GUI.FocusControl(m_activeItem.guid);
	}
}
