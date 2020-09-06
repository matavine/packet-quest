using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIMenuItem {
	public string name;
	public string guid;
	public UIMenu menu;

	public enum Direction {
		Up, Down, Left, Right
	};

	private Dictionary<Direction, UIMenuItem> m_dirMapping;
	public UIMenuItem GetNeighbour(Direction dir) {
		return m_dirMapping[dir];
	}

	public void SetNeighbour(Direction dir, UIMenuItem neighbour) {
		m_dirMapping[dir] = neighbour;
	}

	public UIMenuItem (string name, UIMenu menu) {
		this.name = name;
		this.menu = menu;

		m_dirMapping = new Dictionary<Direction, UIMenuItem>() {
			{ Direction.Up, null },
			{ Direction.Down, null },
			{ Direction.Left, null },
			{ Direction.Right, null }
		};

		guid = GetHashCode().ToString();
	}

	public UIMenuItem MoveSelection(Direction dir) {
		UIMenuItem next = m_dirMapping[dir];
		if (next == null) {
			return this;
		}

		return next;
	}
}
