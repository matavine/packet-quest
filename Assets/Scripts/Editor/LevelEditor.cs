using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

public class LevelEditor : EditorWindow {
	private static LevelEditor m_instance;

	[SerializeField]
	private GridTool m_gridTool;

	private BlockTool m_blockTool;

	[SerializeField]
	private bool m_showGridSettings = true;

	[SerializeField]
	private bool m_showBlockSettings = true;

	private Vector2 m_scrollPosition;
	
	[MenuItem("Iris/Level Editor %l")]
	public static void Init() {
		if (m_instance == null) {
			m_instance = EditorWindow.GetWindow<LevelEditor>();
		}
		
		m_instance.Show();
	}
	
	public void OnEnable() {
		SceneView.onSceneGUIDelegate += OnSceneGUI;
		EditorApplication.playmodeStateChanged += OnPlaybackModeChanged;

		if (m_gridTool == null) {
			m_gridTool = new GridTool();
		}

		m_blockTool = new BlockTool(m_gridTool, this);
	}
	
	public void OnDestroy() {
		SceneView.onSceneGUIDelegate -= OnSceneGUI;
		EditorApplication.playmodeStateChanged -= OnPlaybackModeChanged;
		m_gridTool.RemoveGrid();
	}
	
	public void OnPlaybackModeChanged() {
		if (EditorApplication.isPlaying) {
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			m_gridTool.RemoveGrid();
		} else {
			SceneView.onSceneGUIDelegate += OnSceneGUI;
		}
	}
	
	public void OnSceneGUI(SceneView sceneView) {
		m_gridTool.OnSceneGUI(sceneView);
		m_blockTool.OnSceneGUI(sceneView);
	}
	
	public void OnGUI() {
		m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);

		m_showGridSettings = EditorGUILayout.Foldout(m_showGridSettings, "Grid");
		if (m_showGridSettings) {
			m_gridTool.OnGUI();
		}

		m_showBlockSettings = EditorGUILayout.Foldout(m_showBlockSettings, "Blocks");
		if (m_showBlockSettings) {
			m_blockTool.OnGUI();
		}

		EditorGUILayout.EndScrollView();
	}
}
