using UnityEngine;
using System.Collections;

public class LaserBlockScript : AbsToggleBlock {
	public enum Direction {Up, Down, Left, Right};
	
	public float laserHeight = 1;
	public Direction dir;
	public float onTime = 1;
	public float offTime = 1;
	public float warningTime = 1;
	public bool reactToAudio;
	public Sprite laserSprite;

	private bool m_laserActive;
	private Sprite m_warningSprite;
	private GameObject m_laserBeam;

	private bool m_active;
	private bool m_firing {
		get {
			return m_active;
		}
		set {
			if (value) {
				// Have to use string in order to be able to stop
				StartCoroutine("TriggerLaser");
			} else {
				DisableLaser();
				StopCoroutine("TriggerLaser");
			}
			m_active = value;
		}
	}
	
	// Use this for initialization
	void Start () {
		m_laserActive = false;
		
		SetUpLaserParams();
		
		if (reactToAudio) {
			AudioPitchChanger.Instance.OnPitchChanged += HandleOnPitchChanged;
		}

		m_firing = true;
	}

	void HandleOnPitchChanged (object sender, PitchChangedEventArgs e) {
		onTime /= e.factor;
		offTime /= e.factor;
		warningTime /= e.factor;
	}
	
	private void SetUpLaserParams() {
		float blockOffset = 0.5f;
		m_warningSprite = Resources.Load<Sprite>("warning_beam") as Sprite;
		m_laserBeam = new GameObject();
		m_laserBeam.transform.parent = gameObject.transform;
		m_laserBeam.AddComponent<SpriteRenderer>();
		
		switch(dir){
		case Direction.Down:
			m_laserBeam.transform.position = transform.position + new Vector3(0, -laserHeight/2-blockOffset, 0);
			m_laserBeam.transform.localScale = new Vector3(1, laserHeight, 1);
			break;
		case Direction.Up:
			m_laserBeam.transform.position = transform.position + new Vector3(0, laserHeight/2+blockOffset, 0);
			m_laserBeam.transform.localScale = new Vector3(1, laserHeight, 1);
			m_laserBeam.transform.Rotate(new Vector3(0, 0, 180));
			break;
		case Direction.Left:
			m_laserBeam.transform.position = transform.position + new Vector3(-laserHeight/2-blockOffset, 0, 0);
			m_laserBeam.transform.localScale = new Vector3(1, laserHeight, 1);
			m_laserBeam.transform.Rotate(new Vector3(0, 0, 270));
			break;
		case Direction.Right:
			m_laserBeam.transform.position = transform.position + new Vector3(laserHeight/2+blockOffset, 0, 0);
			m_laserBeam.transform.localScale = new Vector3(1, laserHeight, 1);
			m_laserBeam.transform.Rotate(new Vector3(0, 0, 90));
			break;
		}
		m_laserBeam.AddComponent<BoxCollider2D>().size= new Vector2(0.8f,1);
		m_laserBeam.AddComponent<DangerBlockScript>();
	}

	private void DisableLaser() {
		m_laserBeam.SetActive(false);
		m_laserActive = false;
	}

	public IEnumerator TriggerLaser(){
		while(true){
			if(m_laserActive){
				DisableLaser();
				yield return new WaitForSeconds(offTime);
			} else {
				m_laserBeam.SetActive(true);
				//Warn beam displayed for 1 second, no danger to player
				SpriteRenderer sprite = m_laserBeam.GetComponent<SpriteRenderer>();
				BoxCollider2D box = m_laserBeam.GetComponent<BoxCollider2D>();
				box.isTrigger = true;
				sprite.sprite = m_warningSprite;
				box.enabled=false;
				yield return new WaitForSeconds(warningTime);
				//Laser beam shown, danger to player
				sprite.sprite = laserSprite;
				box.enabled=true;
				m_laserActive = true;
				int key = AudioPlayer.Instance.PlayWithTransform("laser", this.transform);
				yield return new WaitForSeconds(onTime);
				if (key != -1) {
					AudioPlayer.Instance.Stop(key);
				}
			}
		}
	}
	
	public override void ToggleBlock () {
		m_firing = !m_firing;
	}
}