using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class BlockTool {
	private const string BLOCK_PREFABS_DIRECTORY = "/Prefabs/Blocks";
	private const string BLOCK_CURSOR_NAME = "Block Cursor";
	private const int GRID_HIT_MAX_DISTANCE = 100;
	private const int INPUT_TIMEOUT_MILLISECONDS = 150;

	private GridTool m_gridTool;
	private LevelEditor m_levelEditor;

	private enum GridMode { View, Edit };
	private GridMode m_mode = GridMode.View;

	private GameObject m_blockCursor;
	private bool m_isMouseDragged = false;

	private List<GameObject> m_blockPrefabs;
	private string[] m_blockPrefabNames;
	private int m_selectedBlock;
	private Texture2D m_blockPreview;
	private DateTime m_lastInputTime = DateTime.UtcNow;

	public BlockTool(GridTool gridTool, LevelEditor levelEditor) {
		m_gridTool = gridTool;
		m_levelEditor = levelEditor;

		LoadBlockData();
	}

	private void LoadBlockData() {
		m_selectedBlock = 0;
		m_blockPrefabs = GetBlockPrefabs();
		m_blockPrefabNames = GetBlockPrefabNames(m_blockPrefabs);
	}

	private List<GameObject> GetBlockPrefabs() {
		string blocksDir = Application.dataPath + BLOCK_PREFABS_DIRECTORY;
		int pathOffset = blocksDir.Length;

		string[] blockPrefabFiles = Directory.GetFiles(blocksDir, "*.prefab", SearchOption.AllDirectories);

		List<GameObject> blockPrefabs = new List<GameObject>();
		foreach (string blockPrefab in blockPrefabFiles) {
			string relativePath = blockPrefab.Substring(pathOffset);
			relativePath = "Assets" + BLOCK_PREFABS_DIRECTORY + relativePath;
			blockPrefabs.Add(AssetDatabase.LoadAssetAtPath(relativePath, typeof(GameObject)) as GameObject);
		}

		return blockPrefabs;
	}

	private string[] GetBlockPrefabNames(List<GameObject> blockPrefabs) {
		string[] blockPrefabNames = new string[blockPrefabs.Count];
		for (int i = 0; i < blockPrefabs.Count; i++) {
			blockPrefabNames[i] = blockPrefabs[i].name;
		}

		return blockPrefabNames;
	}

	public void OnSceneGUI(SceneView sceneView) {
		if (!m_blockCursor) {
			m_blockCursor = GameObject.Find(BLOCK_CURSOR_NAME);

			if (!m_blockCursor) {
				m_blockCursor = GameObject.CreatePrimitive(PrimitiveType.Quad);
				m_blockCursor.name = BLOCK_CURSOR_NAME;
				m_blockCursor.transform.parent = m_gridTool.GridObject.transform;
				m_blockCursor.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
				m_blockCursor.GetComponent<MeshRenderer>().enabled = (m_mode == GridMode.Edit);
				GameObject.DestroyImmediate(m_blockCursor.GetComponent<MeshCollider>());
			}
		}

		CheckModeToggle(Event.current);

		if (m_mode == GridMode.Edit) {
			UpdateEditMode();
		}
	}

	private void CheckModeToggle(Event currentEvent) {
		DateTime currentTime = DateTime.UtcNow;
		TimeSpan timeSinceLastInput = currentTime - m_lastInputTime;
		if ((currentEvent.keyCode == KeyCode.Tab || currentEvent.character == '\t') &&
		    timeSinceLastInput.TotalMilliseconds >= INPUT_TIMEOUT_MILLISECONDS) {
			m_lastInputTime = currentTime;
			m_mode = (m_mode == GridMode.View) ? GridMode.Edit : GridMode.View;
			UpdateGridCursor();

			SceneView.RepaintAll();
			m_levelEditor.Repaint();
			currentEvent.Use();
		}
	}

	private void UpdateEditMode() {
		Event currentEvent = Event.current;
		
		// Enable capturing of left mouse button up events.
		if (currentEvent.type == EventType.layout) {
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
		}
		
		if (currentEvent.type == EventType.mouseDown && currentEvent.button == 0) {
			m_isMouseDragged = true;
		} else if (currentEvent.type == EventType.mouseUp && currentEvent.button == 0) {
			m_isMouseDragged = false;
		}
		
		Vector3 screenPos = new Vector3(currentEvent.mousePosition.x, -currentEvent.mousePosition.y + Camera.current.pixelHeight);
		Ray ray = Camera.current.ScreenPointToRay(screenPos);

		RaycastHit gridHit;
		BoxCollider gridCollider = m_gridTool.GridCollider;
		if (gridCollider.Raycast(ray, out gridHit, GRID_HIT_MAX_DISTANCE)) {
			Vector3 mouseGridIndex = m_gridTool.GetGridIndex(gridHit.point);
			float cubeCenterOffset = m_gridTool.CellSize / 2.0f;
			Vector3 blockPosition = new Vector3(mouseGridIndex.x + cubeCenterOffset, mouseGridIndex.y + cubeCenterOffset, -cubeCenterOffset);
			
			// Render a shadow of a block where the mouse cursor is.
			if (m_blockPreview) {
				m_blockCursor.SetActive(true);
				m_blockCursor.transform.position = blockPosition;
				m_blockCursor.transform.localScale = new Vector3(m_gridTool.CellSize, m_gridTool.CellSize, m_gridTool.CellSize);
				m_blockCursor.renderer.material = new Material(m_blockCursor.renderer.sharedMaterial);
				m_blockCursor.renderer.sharedMaterial.mainTexture = m_blockPreview;
			}
			
			if (m_isMouseDragged) {
				bool defaultRaycastSetting = Physics2D.raycastsHitTriggers;
				Physics2D.raycastsHitTriggers = true; // Must be enabled to hit trigger box colliders.

				Collider2D[] colliders = Physics2D.OverlapPointAll(blockPosition);
				Physics2D.raycastsHitTriggers = defaultRaycastSetting;
				if (currentEvent.shift) {
					foreach (Collider2D collider in colliders) {
						Undo.DestroyObjectImmediate(collider.gameObject);
					}
				} else if (colliders.Length == 0) {
					CreateBlock(blockPosition);
				}
			}
		}
	}
	
	private void CreateBlock(Vector3 position) {
		GameObject block = PrefabUtility.InstantiatePrefab(m_blockPrefabs[m_selectedBlock]) as GameObject;
		float scale = m_gridTool.CellSize;
		block.transform.position = position;
		block.transform.localScale = new Vector3(scale, scale, scale);
		Undo.RegisterCreatedObjectUndo(block, "Created " + m_blockPrefabNames[m_selectedBlock]);
	}

	private class BlockComparer : IComparer<GameObject> {
		int IComparer<GameObject>.Compare(GameObject a, GameObject b) {
			if (a.transform.position.y > b.transform.position.y) {
				return -1;
			} else if (a.transform.position.y < b.transform.position.y) {
				return 1;
			}

			return (int)Mathf.Sign(a.transform.position.x - b.transform.position.x);
		}
	}

	private void SetBlockCollidersEnabled(GameObject block, bool enabled) {
		BoxCollider2D[] blockColliders = block.GetComponents<BoxCollider2D>();
		
		foreach (BoxCollider2D blockCollider in blockColliders) {
			if (!blockCollider.isTrigger) {
				Undo.RecordObject(blockCollider, "Toggle BoxCollider2D.");
				blockCollider.enabled = enabled;
			}
		}
	}
	
	private void CreatePlatform(List<GameObject> platformBlocks) {
		if (platformBlocks.Count <= 1)
			return;

		GameObject platform = new GameObject("Platform");
		Undo.RegisterCreatedObjectUndo (platform, "Created platform.");

		Vector3 leftBlockPos = platformBlocks[0].transform.position;
		Vector3 rightBlockPos = platformBlocks[platformBlocks.Count - 1].transform.position;

		platform.transform.position = leftBlockPos + ((rightBlockPos - leftBlockPos) / 2);
		float platformWidth = rightBlockPos.x - leftBlockPos.x + m_gridTool.CellSize;
		float platformHeight = leftBlockPos.y - rightBlockPos.y + m_gridTool.CellSize;

		BoxCollider2D collider = Undo.AddComponent<BoxCollider2D>(platform);
		collider.size = new Vector2(platformWidth, platformHeight);

		foreach (GameObject block in platformBlocks) {
			Undo.SetTransformParent(block.transform, platform.transform, "Parenting to platform.");
			SetBlockCollidersEnabled(block, false);
		}
	}

	private class Platform {
		public int leftIndex;
		public int rightIndex;
		public List<GameObject> blocks;

		public Platform(List<GameObject> pBlocks, int left) {
			blocks = new List<GameObject>(pBlocks);
			leftIndex = left;
			rightIndex = left + pBlocks.Count - 1;
		}
	}

	private class PlatformGroup : IEnumerable<Platform> {
		private List<Platform> m_platforms;
		public int yIndex;

		public PlatformGroup(int y) {
			yIndex = y;
			m_platforms = new List<Platform>();
		}

		public void Add(Platform platform) {
			m_platforms.Add(platform);
		}

		public IEnumerator<Platform> GetEnumerator() {
			return m_platforms.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

	private void MergeBlockColliders(GameObject[] blocks) {
		if (blocks.Length == 0)
			return;

		Array.Sort(blocks, new BlockComparer());
		Transform firstBlock = blocks[0].transform;
		Vector3 startGridIndex = m_gridTool.GetGridIndex(firstBlock.position);
		int currYIndex = (int)startGridIndex.y;
		int startXIndex = (int)startGridIndex.x;
		int nextXIndex = startXIndex;

		List<PlatformGroup> platformGroups = new List<PlatformGroup>();
		PlatformGroup platformGroup = new PlatformGroup(currYIndex);
		List<GameObject> platformBlocks = new List<GameObject>();

		foreach (GameObject block in blocks) {
			Vector3 blockGridIndex = m_gridTool.GetGridIndex(block.transform.position);
			if (currYIndex != blockGridIndex.y && platformBlocks.Count > 0) {
				platformGroup.Add(new Platform(platformBlocks, startXIndex));
				platformGroups.Add(platformGroup);

				currYIndex = (int)blockGridIndex.y;
				startXIndex = (int)blockGridIndex.x;
				nextXIndex = startXIndex;

				platformGroup = new PlatformGroup(currYIndex);
				platformBlocks.Clear();
			}

			if (nextXIndex == blockGridIndex.x) {
				nextXIndex++;
			} else {
				platformGroup.Add(new Platform(platformBlocks, startXIndex));
				platformBlocks.Clear();

				startXIndex = (int)blockGridIndex.x;
				nextXIndex = startXIndex + 1;
			}

			platformBlocks.Add(block);
		}

		if (platformBlocks.Count > 0) {
			platformGroup.Add(new Platform(platformBlocks, startXIndex));
			platformGroups.Add(platformGroup);
		}

		HashSet<Platform> mergedPlatforms = new HashSet<Platform>();

		for (int row = 0; row < platformGroups.Count; row++) {
			PlatformGroup pGroup = platformGroups[row];
			foreach (Platform platform in pGroup) {
				if (mergedPlatforms.Contains(platform)) {
					continue;
				}

				int nextYIndex = pGroup.yIndex - 1;
				List<GameObject> pBlocks = new List<GameObject>(platform.blocks);
				mergedPlatforms.Add(platform);

				for (int i = row + 1; i < platformGroups.Count; i++, nextYIndex--) {
					PlatformGroup tmpGroup = platformGroups[i];
					if (tmpGroup.yIndex != nextYIndex) {
						break;
					}

					foreach (Platform tmpPlatform in tmpGroup) {
						if (tmpPlatform.leftIndex > platform.leftIndex ||
						    (tmpPlatform.leftIndex == platform.leftIndex &&
						 	tmpPlatform.rightIndex != platform.rightIndex)) {
							break;
						} else if (tmpPlatform.leftIndex < platform.leftIndex || 
						           mergedPlatforms.Contains(tmpPlatform)) {
							continue;
						}
						
						pBlocks.AddRange(tmpPlatform.blocks);
						mergedPlatforms.Add(tmpPlatform);
						break;
					}
				}

				CreatePlatform(pBlocks);
			}
		}
	}

	private void BreakPlatforms(GameObject[] platforms) {
		if (platforms.Length == 0)
			return;

		foreach (GameObject platform in platforms) {
			if (platform.transform.childCount == 0)
				continue;

			LinkedList<Transform> blocksToUnparent = new LinkedList<Transform>();
			foreach (Transform blockTransform in platform.transform) {
				blocksToUnparent.AddLast(blockTransform);
				SetBlockCollidersEnabled(blockTransform.gameObject, true);
			}

			foreach (Transform blockTransform in blocksToUnparent) {
				Undo.SetTransformParent(blockTransform, null, "Unparenting block from platform.");
			}

			Undo.DestroyObjectImmediate(platform);
		}
	}

	private void UpdateGridCursor() {
		MeshRenderer cursorRenderer = m_blockCursor.GetComponent<MeshRenderer>();
		cursorRenderer.enabled = (m_mode == GridMode.Edit);
		SceneView.RepaintAll();
	}

	public void OnGUI() {
//		CheckModeToggle(Event.current);

		if (m_blockPrefabs.Count == 0) {
			EditorGUILayout.HelpBox("No block prefabs have been found. Please make sure they are placed " +
			                        "inside the following directory: Assets/" + BLOCK_PREFABS_DIRECTORY, MessageType.Info);
			return;
		}

		GUILayout.Label("Modes", EditorStyles.boldLabel);
		m_mode = (GridMode)GUILayout.SelectionGrid((int)m_mode, Enum.GetNames(typeof(GridMode)), 2);
		if (GUI.changed) {
			UpdateGridCursor();
		}

		GUILayout.Label("Blocks", EditorStyles.boldLabel);

		GUI.enabled = Selection.gameObjects.Length > 1;
		if (GUILayout.Button("Create Platforms")) {
			MergeBlockColliders(Selection.gameObjects);
			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
		}

		GUI.enabled = Selection.gameObjects.Length > 0;
		if (GUILayout.Button("Break Platforms")) {
			BreakPlatforms(Selection.gameObjects);
			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
		}
		GUI.enabled = true;

		EditorGUILayout.BeginHorizontal();

		m_selectedBlock = EditorGUILayout.Popup(m_selectedBlock, m_blockPrefabNames);
		bool refreshBlockList = GUILayout.Button("Refresh");

		EditorGUILayout.EndHorizontal();

		m_blockPreview = AssetPreview.GetAssetPreview(m_blockPrefabs[m_selectedBlock]);
		if (GUI.changed) {
			SceneView.RepaintAll();
		}

		GUI.enabled = false;
		EditorGUILayout.ObjectField(m_blockPreview, typeof(Texture2D), false, GUILayout.Width(100), GUILayout.Height(100));
		GUI.enabled = true;

		if (refreshBlockList) {
			LoadBlockData();
		}
	}
}
