using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CircuitParams {
	private HashSet<GameObject> m_powerLine = new HashSet<GameObject>();
	public HashSet<GameObject> PowerLine {
		get { return m_powerLine; }
	}
}

[RequireComponent(typeof(BoxCollider2D))]
public class CircuitScript : MonoBehaviour {
	public Color onColor = Color.green;
	public Color offColor = Color.gray;
	public bool circuitEnabled = false;
	public static float propogationDelay = 0.01f;

	public void Start() {
		GetComponent<Renderer>().material.color = circuitEnabled ? onColor : offColor;
	}

	public void SignalCircuit(CircuitParams circuit) {
		if (circuit.PowerLine.Contains(gameObject)) {
			return;
		}
		StartCoroutine(SendSignalRoutine(circuit));
	}

	public IEnumerator SendSignalRoutine(CircuitParams circuit) {
		yield return new WaitForSeconds(propogationDelay);

		SendCircuitSignal(gameObject, (BoxCollider2D)GetComponent<Collider2D>(), circuit);
		
		circuitEnabled = !circuitEnabled;
		GetComponent<Renderer>().material.color = circuitEnabled ? onColor : offColor;
	}

	public static void SendCircuitSignal(GameObject source, BoxCollider2D sourceCollider, CircuitParams circuit) {
		Vector2 pos = new Vector2(sourceCollider.transform.position.x, sourceCollider.transform.position.y);
		Vector2 up = pos + new Vector2(0, sourceCollider.size.y);
		Vector2 right = pos + new Vector2(sourceCollider.size.x, 0);
		Vector2 down = pos - new Vector2(0, sourceCollider.size.y);
		Vector2 left = pos - new Vector2(sourceCollider.size.x, 0);

		bool hitTriggers = Physics2D.queriesHitTriggers;
		Physics2D.queriesHitTriggers = true;
		List<Collider2D> colliders = new List<Collider2D>();
		colliders.AddRange(Physics2D.OverlapPointAll(up));
		colliders.AddRange(Physics2D.OverlapPointAll(right));
		colliders.AddRange(Physics2D.OverlapPointAll(down));
		colliders.AddRange(Physics2D.OverlapPointAll(left));
		Physics2D.queriesHitTriggers = hitTriggers;

		if (source) {
			circuit.PowerLine.Add(source);
		}
		
		foreach (Collider2D collider in colliders) {
			if (collider) {
				collider.SendMessage("SignalCircuit", circuit, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
