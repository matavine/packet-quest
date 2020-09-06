using UnityEngine;
using System;

public abstract class AbsToggleBlock : MonoBehaviour {
	public bool reactToCircuit = false;
	public abstract void ToggleBlock();

	public void SignalCircuit(CircuitParams circuit) {
		if (!reactToCircuit || circuit.PowerLine.Contains(gameObject)) {
			return;
		}
		ToggleBlock();
		CircuitScript.SendCircuitSignal(gameObject, (BoxCollider2D)GetComponent<Collider2D>(), circuit);
	}
}

