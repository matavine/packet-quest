using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class GridTool {
	private const string GRID_OBJECT_NAME = "Grid";
	
	private const int MIN_GRID_WIDTH = 1;
	private const int MIN_GRID_HEIGHT = 1;
	
	private const int MAX_GRID_WIDTH = 128;
	private const int MAX_GRID_HEIGHT = 128;
	
	private const int MIN_CELL_SIZE = 1;
	private const int MAX_CELL_SIZE = 32;

	[SerializeField]
	private int m_gridWidth = 32;
	
	[SerializeField]
	private int m_gridHeight = 32;
	
	[SerializeField]
	private bool m_displayGrid = true;
	
	[SerializeField]
	private int m_cellSize = 1;
	public int CellSize {
		get { return m_cellSize; }
	}

	private GameObject m_gridObject;
	public GameObject GridObject {
		get { return m_gridObject; }
	}

	private BoxCollider m_gridCollider;
	public BoxCollider GridCollider {
		get { return m_gridCollider; }
	}
	
	private string m_groupName;

	public GridTool() {
	}

	public void RemoveGrid() {
		if (m_gridObject) {
			GameObject.DestroyImmediate(m_gridObject);
		}
	}

	public int GetColumnIndex(Vector3 position) {
		return Mathf.FloorToInt(position.x / m_cellSize) * m_cellSize;
	}
	
	public int GetRowIndex(Vector3 position) {
		return Mathf.FloorToInt(position.y / m_cellSize) * m_cellSize;
	}
	
	public Vector3 GetGridIndex(Vector3 position) {
		return new Vector3(GetColumnIndex(position), GetRowIndex(position));
	}

	public void OnSceneGUI(SceneView sceneView) {
		if (!m_gridObject || !m_gridCollider) {
			m_gridObject = GameObject.Find(GRID_OBJECT_NAME);
			if (!m_gridObject) {
				m_gridObject = new GameObject(GRID_OBJECT_NAME);
				m_gridObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
			}
			
			m_gridCollider = m_gridObject.GetComponent<BoxCollider>();
			if (!m_gridCollider) {
				m_gridCollider = m_gridObject.AddComponent<BoxCollider>();
			}
		}
		
		UpdateGridColliderDimensions();
		
		if (m_displayGrid) {
			RenderGrid();
		}
	}

	private void RenderGrid() {
		Vector3 cameraPos = Camera.current.transform.position;
		int gridColIndex = Mathf.FloorToInt((cameraPos.x - (m_gridWidth / 2 * m_cellSize)) / m_cellSize);
		int gridRowIndex = Mathf.FloorToInt((cameraPos.y + (m_gridHeight / 2 * m_cellSize)) / m_cellSize);
		
		// Calculate grid limits.
		int top = gridRowIndex * m_cellSize;
		int bottom = (gridRowIndex - m_gridHeight) * m_cellSize;
		int left = gridColIndex * m_cellSize;
		int right = (gridColIndex + m_gridWidth) * m_cellSize;
		
		int colPosition = left;
		int rowPosition = top;
		
		// Render the columns.
		for (int i = 0; i <= m_gridWidth; i++, colPosition += m_cellSize) {
			Handles.DrawLine(new Vector3(colPosition, top, 0f),
			                 new Vector3(colPosition, bottom, 0f));
		}
		
		// Render the rows.
		for (int i = 0; i <= m_gridHeight; i++, rowPosition -= m_cellSize) {
			Handles.DrawLine(new Vector3(left, rowPosition, 0f),
			                 new Vector3(right, rowPosition, 0f));
		}
	}
	
	public void UpdateGridColliderDimensions() {
		Vector3 cameraPos = Camera.current.transform.position;
		Vector3 gridIndex = GetGridIndex(cameraPos);
		m_gridCollider.transform.position = new Vector3(gridIndex.x, gridIndex.y, 0.5f);
		m_gridCollider.size = new Vector3(m_gridWidth * m_cellSize, m_gridHeight * m_cellSize, 1);
	}

	public void OnGUI() {
		GUILayout.Label("Settings", EditorStyles.boldLabel);
		bool guiChanged = false;

		m_displayGrid = EditorGUILayout.Toggle("Show Grid?", m_displayGrid);
		guiChanged = guiChanged || GUI.changed;

		m_gridWidth = EditorGUILayout.IntSlider("Grid Width", m_gridWidth, MIN_GRID_WIDTH, MAX_GRID_WIDTH);
		guiChanged = guiChanged || GUI.changed;

		m_gridHeight = EditorGUILayout.IntSlider("Grid Height", m_gridHeight, MIN_GRID_HEIGHT, MAX_GRID_HEIGHT);
		guiChanged = guiChanged || GUI.changed;

		m_cellSize = EditorGUILayout.IntSlider("Cell Size", m_cellSize, MIN_CELL_SIZE, MAX_CELL_SIZE);
		guiChanged = guiChanged || GUI.changed;

		if (guiChanged) {
			SceneView.RepaintAll();
		}

		GUILayout.Label("Controls", EditorStyles.boldLabel);
		if (GUILayout.Button("Snap To Grid")) {
			Undo.RecordObjects(Selection.transforms, "Snapping selected objects to grid.");
			foreach (Transform transform in Selection.transforms) {
				Vector3 gridIndex = GetGridIndex(transform.position);
				transform.position = gridIndex + 
					(new Vector3(m_cellSize, m_cellSize, m_cellSize) / 2.0f);
			}
		}

		m_groupName = EditorGUILayout.TextField("Name: ", m_groupName);
		GUI.enabled = !string.IsNullOrEmpty(m_groupName);
		if (GUILayout.Button("Group Selected Objects")) {
			GameObject groupObject = new GameObject(m_groupName);
			Undo.RecordObjects(Selection.transforms, "Grouping Selected Objects");
			foreach (Transform transform in Selection.transforms) {
				transform.parent = groupObject.transform;
			}
		}

		GUI.enabled = true;
	}
}
