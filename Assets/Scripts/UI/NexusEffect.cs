using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class NexusEffect : MonoBehaviour {
	private const string BEAM_SHADER = "Sprites/Default";

	// Amount of world units outside of the camera bounds
	// from which to spawn the beams.
	private const int SCREEN_OFFSET_BUFFER = 1;

	// Percent boundary of where in the camera view the
	// beams should appear.
	private const float CAMERA_BOUNDS_THRESHOLD = 0.8f;

	// How far back along the z-axis the beams are set to.
	private const float Z_OFFSET = 5f;

	// Weight value for determining the position of the
	// middle point of the trail.
	private const float MIDPOINT_OFFSET_FACTOR = 3.0f;

	// The alpha value of the trail color.
	private const float TRAIL_ALPHA = 0.7f;

	public int numNexusBeams = 1;
	public float minScale = 0.5f;
	public float maxScale = 0.5f;
	public float width = 3f;
	public Color[] beamColors;

	public float minSpeed = 2f;
	public float maxSpeed = 5f;

	public bool reactToAudio;

	private List<NexusBeam> m_activeBeams;
	private List<NexusBeam> m_inactiveBeams;

	// Side from which to spawn the nexus beam.
	private enum CameraSide {
		Up, Right, Down, Left
	};

	// Set of possible directions the beam can go.
	private readonly Dictionary<CameraSide, Vector3> m_moveDirections = new Dictionary<CameraSide, Vector3>() 
	{
		{ CameraSide.Up, Vector3.down },
		{ CameraSide.Right, Vector3.left },
		{ CameraSide.Down, Vector3.up },
		{ CameraSide.Left, Vector3.right }
	};

	private class NexusBeam {
		public GameObject quad;
		public LineRenderer trail;
		public Vector3 moveDirection = Vector3.zero;
		public float speed = 0.0f;
		public float width = 0.0f;

		public NexusBeam(float scale, float width) {
			quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
			quad.SetActive(false);
			quad.renderer.material = new Material(Shader.Find(BEAM_SHADER));
			quad.transform.localScale = new Vector3(scale, scale, 1.0f);
			quad.transform.position = Vector3.zero;
			this.width = width;
			
			trail = quad.AddComponent<LineRenderer>();
			trail.SetVertexCount(3);
			trail.SetWidth(scale, scale);
			trail.material = new Material(Shader.Find(BEAM_SHADER));
		}

		public void SetScale(float scale) {
			quad.transform.localScale = new Vector3(scale, scale, 1.0f);
			trail.SetWidth(scale, scale);
		}

		public void SetColor(Color color) {
			quad.renderer.material.color = color;
			trail.SetColors(new Color(color.r, color.g, color.b, TRAIL_ALPHA), Color.clear);
		}

		public void Update(float deltaTime) {
			quad.transform.position += (moveDirection * speed * deltaTime);

			Vector3 startPos = quad.transform.position + (-moveDirection * quad.transform.localScale.x / 2.0f);
			Vector3 endPos = startPos + (-moveDirection * width);
			Vector3 midPoint = startPos + ((endPos - startPos) / MIDPOINT_OFFSET_FACTOR);

			trail.SetPosition(0, startPos);
			trail.SetPosition(1, midPoint);
			trail.SetPosition(2, endPos);
		}
	}

	// Use this for initialization
	public void Start () {
		m_activeBeams = new List<NexusBeam>();
		m_inactiveBeams = new List<NexusBeam>();

		for (int i = 0; i < numNexusBeams; i++) {
			NexusBeam beam = new NexusBeam(Random.Range(minScale, maxScale), width);
			beam.quad.transform.parent = gameObject.transform;
			m_inactiveBeams.Add(beam);
		}

		if (reactToAudio) {
			AudioPitchChanger.Instance.OnPitchChanged += HandleOnPitchChanged;
		}
	}

	void HandleOnPitchChanged (object sender, PitchChangedEventArgs e) {
		foreach(NexusBeam beam in m_activeBeams) {
			beam.speed *= e.factor;
		}
		minSpeed *= e.factor;
		maxSpeed *= e.factor;
	}
	
	// Update is called once per frame
	public void Update () {
		List<NexusBeam> offScreen = new List<NexusBeam>();

		foreach (NexusBeam beam in m_activeBeams) {
			if (IsOutOfCameraBounds(beam)) {
				beam.quad.SetActive(false);
				offScreen.Add(beam);
				continue;
			}

			beam.Update(Time.deltaTime);
		}

		foreach (NexusBeam beam in offScreen) {
			m_activeBeams.Remove(beam);
			m_inactiveBeams.Add(beam);
		}

		foreach (NexusBeam beam in m_inactiveBeams) {
			CameraSide side = (CameraSide)Random.Range(0, 4);
			beam.moveDirection = m_moveDirections[side];
			if (beamColors.Length > 0) {
				beam.SetColor(beamColors[Random.Range(0, beamColors.Length)]);
			}

			beam.speed = Random.Range(minSpeed, maxSpeed);
			beam.SetScale(Random.Range(minScale, maxScale));
			beam.quad.transform.position = GetRandomStartingPos(side) + (Vector3.forward * Z_OFFSET);
			beam.quad.SetActive(true);

			m_activeBeams.Add(beam);
		}

		m_inactiveBeams.Clear();
		offScreen.Clear();
	}

	private Vector2 GetCameraExtents() {
		return new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize);
	}

	private bool IsOutOfCameraBounds(NexusBeam beam) {
		Vector2 cameraExtents = GetCameraExtents();
		float xLimit = cameraExtents.x + SCREEN_OFFSET_BUFFER + width;
		float yLimit = cameraExtents.y + SCREEN_OFFSET_BUFFER + width;

		Vector3 cameraToBeam = beam.quad.transform.position - camera.transform.position;
		return (Mathf.Abs(cameraToBeam.x) >= xLimit || Mathf.Abs(cameraToBeam.y) >= yLimit);
	}

	private Vector3 GetRandomStartingPos(CameraSide side) {
		Vector2 cameraExtents = GetCameraExtents();
		Vector3 cameraPos = new Vector3(camera.transform.position.x, camera.transform.position.y, 0);
		
		float randXPos = Random.Range(-cameraExtents.x, cameraExtents.x) * CAMERA_BOUNDS_THRESHOLD;
		float randYPos = Random.Range(-cameraExtents.y, cameraExtents.y) * CAMERA_BOUNDS_THRESHOLD;
		
		float horizontalOffset = cameraExtents.x + SCREEN_OFFSET_BUFFER;
		float verticalOffset = cameraExtents.y + SCREEN_OFFSET_BUFFER;

		switch (side) {
			case CameraSide.Up:
				return cameraPos + new Vector3(randXPos, verticalOffset);
			case CameraSide.Right:
				return cameraPos + new Vector3(horizontalOffset, randYPos);
			case CameraSide.Down:
				return cameraPos + new Vector3(randXPos, -verticalOffset);
			case CameraSide.Left:
				return cameraPos + new Vector3(-horizontalOffset, randYPos);
		}

		return Vector3.zero;
	}
}
