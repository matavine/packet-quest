using System;
using UnityEngine;

//TODO: Move this to another folder in the project
public class Boost {
	public Vector2 BoostVector {set { m_boost = value; } get { return m_boost; }}

	private Vector2 m_boost, m_fallOff, dir;
	// This scale factor controls how easiliy a player can oppose this force
	private Vector2 m_counteractScale;

	private bool m_started = false;
	
	public Boost(Vector2 boost, Vector2 falloff, Vector2 counteract) {
		m_counteractScale = counteract;
		m_boost = boost;
		m_fallOff = falloff;
		dir = new Vector2(Mathf.Sign(m_boost.x), Mathf.Sign(m_boost.y));
	}

	//TODO: Refactor
	private Vector2 Delta(Vector2 delta) {
		Vector2 remaining = delta;
		float absCur, absDelta;

		if(HaveOppositeSigns(m_boost.x, delta.x) && m_boost.x != 0f) {
			absCur = Math.Abs(m_boost.x); 
			absDelta = Mathf.Abs(delta.x);

			if(absCur > absDelta) {
				m_boost.x += delta.x;
				remaining.x = 0;
			}
			else {
				m_boost.x = 0;
				remaining.x = Mathf.Sign(delta.x)*( absDelta - absCur);
			}
		}

		if(HaveOppositeSigns(m_boost.y, delta.y) && m_boost.y != 0f) {
			absCur = Math.Abs(m_boost.y);
			absDelta = Mathf.Abs(delta.y);	
				
			if(absCur > absDelta) {
				m_boost.y += delta.y;
				remaining.y = 0;
			}
			else {
				m_boost.y = 0;
				remaining.y = Mathf.Sign(delta.y)*( absDelta - absCur);
			}
		}

		return remaining;
	}

	public bool HasStarted() {
		return m_started;
	}

	public void Start() {
		m_started = true;
	}

	public Vector2 ApplyPlayerDelta(Vector2 delta) {
		Vector2 scaled = new Vector2(delta.x / m_counteractScale.x, delta.y / m_counteractScale.y);
		Vector2 result = Delta(scaled);
		return new Vector2(m_counteractScale.x * result.x, m_counteractScale.y * result.y);
	}
	
	public Vector2 Tick(float time) {
		Vector2 tickDelta = new Vector2(m_fallOff.x*dir.x, m_fallOff.y*dir.y)*time;
		return Delta (-tickDelta);
	}

	private bool HaveOppositeSigns(float x, float y) {
		return (x*y < 0);
	}
	
	public bool CheckIfFinished() {
		return m_boost.Equals(Vector2.zero);
	}
}
