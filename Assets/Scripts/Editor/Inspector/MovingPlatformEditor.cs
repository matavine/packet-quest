using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MovingPlatformScript))] 
public class MovingPlatformEditor : Editor {
	public override void OnInspectorGUI() {
		MovingPlatformScript script = (MovingPlatformScript) this.target;
		DrawDefaultInspector();
		script.limits.xMin = EditorGUILayout.FloatField("Left Limit",script.limits.xMin);
		script.limits.xMax = EditorGUILayout.FloatField("Right Limit",script.limits.xMax);
		script.limits.yMax = EditorGUILayout.FloatField("Upper Limit",script.limits.yMax);
		script.limits.yMin = EditorGUILayout.FloatField("Lower Limit",script.limits.yMin);
	}
}