using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIMenu : IEnumerable {
	private UIMenuItem[] m_items;
	public UIMenuItem[] Items {
		get { return m_items; }
	}

	private Dictionary<UIMenuItem, int> m_itemToIndex;

	private Action<UIMenuItem> m_onMenuItemClicked;
	private Action<UIMenuItem[]> m_renderCallback;

	private int m_selectedIndex = 0;

	public UIMenu(string[] items, 
	              Action<UIMenuItem[]> renderCallback,
	              Action<UIMenuItem> onMenuItemClicked = null) {

		m_items = new UIMenuItem[items.Length];
		m_itemToIndex = new Dictionary<UIMenuItem, int>();
		for (int i = 0; i < items.Length; i++) {
			m_items[i] = new UIMenuItem(items[i], this);
			m_itemToIndex.Add(m_items[i], i);
		}

		for (int i = 0; i < m_items.Length; i++) {
			UIMenuItem previousItem = m_items[GetWrappedIndex(i, -1, m_items.Length)];
			UIMenuItem nextItem = m_items[GetWrappedIndex(i, 1, m_items.Length)];

			UIMenuItem currItem = m_items[i];
			currItem.SetNeighbour(UIMenuItem.Direction.Up, previousItem);
			currItem.SetNeighbour(UIMenuItem.Direction.Down, nextItem);
		}

		m_onMenuItemClicked = onMenuItemClicked;
		m_renderCallback = renderCallback;
	}

	public UIMenu(Action<UIMenuItem[]> renderCallback,
	              Action<UIMenuItem> onMenuItemClicked = null) {

		m_onMenuItemClicked = onMenuItemClicked;
		m_renderCallback = renderCallback;

		m_items = new UIMenuItem[1];
		m_items[0] = new UIMenuItem("empty", this);
		m_itemToIndex = new Dictionary<UIMenuItem, int>();
		m_itemToIndex.Add(m_items[0], 0);
	}

	private int GetWrappedIndex(int index, int offset, int max) {
		int nextIndex = index + offset;
		return (nextIndex < 0) ? max - 1 : nextIndex % max;
	}

	public IEnumerator GetEnumerator() {
		return m_items.GetEnumerator();
	}

	public void MoveSelection(int offset) {
		m_selectedIndex = GetWrappedIndex(m_selectedIndex, offset, m_items.Length);
	}

	public void SetSelection(UIMenuItem item) {
		if (item == null) {
			return;
		}

		int index;
		if (m_itemToIndex.TryGetValue(item, out index)) {
			SetSelection(index);
		}
	}

	public void SetSelection(int index) {
		m_selectedIndex = index;
	}

	public UIMenuItem GetSelectedItem() {
		return m_items[m_selectedIndex];
	}

	public void OnClicked() {
		if (m_onMenuItemClicked != null) {
			m_onMenuItemClicked(m_items[m_selectedIndex]);
		}
	}

	public void Render() {
		m_renderCallback(m_items);
	}
}
