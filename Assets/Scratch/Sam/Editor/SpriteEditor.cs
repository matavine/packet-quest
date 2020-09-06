using UnityEngine;
using UnityEditor;

using System.Collections;
using System;

public class SpriteEditor : EditorWindow {
	
	private static SpriteEditor m_instance;
	
	private static GameObject m_spriteObj;
	private static Shader m_shader;
	
	private Texture2D m_image;
	private String m_name = "Sprite";
	private Vector3 m_spritePos = new Vector3(0,0,0);
	private int m_cols = 2;
	private int m_rows = 2;
	private Vector3 m_angle = new Vector3(90,180,0);
	private Vector3 m_scale = new Vector3(0.5f,1,0.5f);
	
	[MenuItem("Iris/Sprite")]
	public static void Init() {
		if (m_instance == null) {
			m_instance = EditorWindow.GetWindow<SpriteEditor>();
		}
		
		m_instance.Show();
		
		m_shader = Shader.Find("Transparent/Diffuse");
		m_spriteObj = (GameObject)Resources.Load("Sprite");
	}
	
	void CreateSprite() {
		GameObject sprite = (GameObject) Instantiate(m_spriteObj, m_spritePos, Quaternion.Euler(m_angle));
		sprite.name = m_name;
		Material material = new Material(sprite.GetComponent<Renderer>().sharedMaterial);
		material.color = new Color(1,1,1,1);
		material.mainTexture = m_image;
		material.shader = m_shader;
		sprite.GetComponent<Renderer>().sharedMaterial = material;
		
		AnimatedTexture animatedTexture = (AnimatedTexture)sprite.GetComponent(typeof(AnimatedTexture));
		animatedTexture._columns = m_cols;
		animatedTexture._rows = m_rows;
	}
	

	public void OnGUI() {
		GUILayout.Label("Sprite Settings", EditorStyles.boldLabel);
		m_name = EditorGUILayout.TextField("Name: ", m_name);
		m_image = (Texture2D) EditorGUILayout.ObjectField("Image", m_image, typeof (Texture2D), false);
		m_cols = EditorGUILayout.IntField("Columns: ", m_cols);
		m_rows = EditorGUILayout.IntField("Rows: ", m_rows);
		m_spritePos.x = EditorGUILayout.FloatField("X: ", m_spritePos.x);
		m_spritePos.y = EditorGUILayout.FloatField("Y: ", m_spritePos.y);
		m_scale.x = EditorGUILayout.FloatField("X: ", m_scale.x);
		m_scale.z = EditorGUILayout.FloatField("Y: ", m_scale.z); // Aligned with y-axis after rotation
		if (GUILayout.Button("Create")) {
			CreateSprite();
		}
	}
}
