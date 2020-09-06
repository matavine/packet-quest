using UnityEngine;
using System.Collections;

public class TurretBlockScript : AbsToggleBlock {
	public float fireRate;
	public bool reactToAudio;
	public GameObject missile;

	private int m_missileSpeed = 8;
	private Vector2 m_direction;
	private Vector3 m_offset;
	private bool m_active;
	private bool m_firing {
		get {
			return m_active;
		}
		set {
			if (value) {
				// Have to use string in order to be able to stop
				StartCoroutine("TriggerTurret");
			} else {
				StopCoroutine("TriggerTurret");
			}
			m_active = value;
		}
	}

	// Use this for initialization
	void Start () {
		InitializeTurret();
		m_firing = true;
		if (reactToAudio) {
			AudioPitchChanger.Instance.OnPitchChanged += HandleOnPitchChanged;
		}
	}

	void HandleOnPitchChanged (object sender, PitchChangedEventArgs e)
	{
		fireRate *= e.factor;
	}

	private void InitializeTurret(){
		Vector3 direction = transform.rotation * Vector3.up * -1;
		m_offset = transform.rotation * new Vector3(0, -1f, 0);
		m_direction = new Vector2(direction.x, direction.y);
	}

	public IEnumerator TriggerTurret(){
		while(true){
			GameObject prefabRay = Instantiate(missile, gameObject.transform.position + m_offset, gameObject.transform.rotation) as GameObject;
			prefabRay.GetComponent<Rigidbody2D>().velocity = m_missileSpeed*m_direction;
			yield return new WaitForSeconds(1.0f/fireRate);
		}
	}

	public override void ToggleBlock () {
		m_firing = !m_firing;
	}
}
