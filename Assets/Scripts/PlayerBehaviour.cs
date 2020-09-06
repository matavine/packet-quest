using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ParticleSystem))]
public class PlayerBehaviour : MonoBehaviour {
	
	private const float GROUND_RAY_OFFSET = 0.01f;
	private const float HORIZONTAL_RAY_OFFSET = 0.01f;
	private const float VERTICAL_RAY_OFFSET = 0.01f;
	private const float COLLISION_SKIN_WIDTH = 0.0f;

	private const int ANTI_GRAVITY_METER_BORDER = 2; // pixel width
	private const int ANTI_GRAVITY_METER_OFFSET = 10; // pixels
	private const int ANTI_GRAVITY_METER_HEIGHT = 10; // pixels
	private const int ANTI_GRAVITY_ICON_SIZE = 24; // pixels
	private readonly Color ANTI_GRAVITY_ACTIVATED_COLOR = new Color(0.808f, 0.6f, 0.188f, 1.0f);
	private readonly Color ANTI_GRAVITY_RECHARGING_COLOR = new Color(0f, 0.627f, 1.0f, 1.0f);

	// Top player speed
	public float playerSpeed = 5f;
	
	// Player y-velocity after pressing jump key
	public float jumpSpeed = 7f;
	
	// How quickly the player accelerates to top speed
	public float groundAcceleration = 25f;
	
	// How quickly the player acclerates against thier direction of travel
	public float groundDeceleration = 40f;
	
	// How quicky the player decelerates when no key is pressed
	public float groundDrag = 40f;
	
	// Same as ground paramters but active when player is airborne	
	public float airAcceleration = 1f;
	public float airDeceleration = 4f;
	public float airDrag = 2f;
	
	// Number of jumps player can make (eg: double, triple jump)
	public int jumpsAllowed = 2;

	public float gravityScale = 1.0f;	
	public float antiGravity = 1;
	public float antiGravityDuration = 0.5f;
	public float antiGravityRechargeRate = 0.1f;

	// Invert left and right directions
	public bool invertControls = false;
	
	// Is the player airborne or grounded
	private bool m_isGrounded = false;
	
	// External velocities applied to the player 
	private List<Boost> m_activeBoosts, m_finishedBoosts;
	
	// Remaining number of jumps
	private int m_jumpsLeft;
	public int JumpsLeft {
		get { return m_jumpsLeft; }
		set { m_jumpsLeft = value; }
	}

	public bool DisableJumpNextFrame {
		get; set;
	}
	
	// Control inversion state
	private float m_controlDirection;
	
	// The last computed player speed (ignoring external forces)
	private Vector2 m_lastSpeed;
	
	private float m_antiGravityCharge = 0;

	private Animator m_animator;
	public Animator AnimControl {
		get { return m_animator; }
	}
	
	private int m_keyRing;

	private Collision2D m_playerCollision;
	private HashSet<Collider2D> m_lastTriggersHit;
	private HashSet<Collider2D> m_lastCollidersHit;

	private HashSet<Collider2D> m_currTriggersHit;
	private HashSet<Collider2D> m_currCollidersHit;

	private Texture2D m_antiGravityIconOn;
	private Texture2D m_antiGravityIconOff;
	private bool m_antiGravityActivated;

	public void Start () {
		m_jumpsLeft = jumpsAllowed;
		m_animator = GetComponent<Animator>();
		m_keyRing = 0;
		m_lastSpeed = Vector2.zero;
		m_activeBoosts = new List<Boost>();
		m_finishedBoosts = new List<Boost>();
		m_controlDirection = invertControls ? -1 : 1;
		m_antiGravityCharge = antiGravityDuration;

		#region DANGER: Very dirty code awaits you here. Run and don't ever look back.
		Type collisionType = typeof(Collision2D);
		FieldInfo colliderFieldInfo = collisionType.GetField("m_Collider", BindingFlags.NonPublic | BindingFlags.Instance);

		m_playerCollision = new Collision2D();
		colliderFieldInfo.SetValue(m_playerCollision, GetComponent<Collider2D>().GetInstanceID());
		#endregion

		m_lastTriggersHit = new HashSet<Collider2D>();
		m_lastCollidersHit = new HashSet<Collider2D>();

		m_currTriggersHit = new HashSet<Collider2D>();
		m_currCollidersHit = new HashSet<Collider2D>();

		m_antiGravityIconOn = Resources.Load("UI/anti_gravity_icon_on") as Texture2D;
		m_antiGravityIconOff = Resources.Load("UI/anti_gravity_icon_off") as Texture2D;

		// Ensure that the particles render in front of the player.
		GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingLayerName = "Foreground";
	
		// Spawn the player.
		StartCoroutine(SpawnAnimation());
	}

	public void Update() {
		Vector2 velocity = m_lastSpeed;
		Vector2 deltaVelocity = Vector2.zero;
		Vector2 worldVelocity = Vector2.zero;
		Vector2 acceleration = new Vector2(groundAcceleration, 0);
		Vector2 deceleration = new Vector2(groundDeceleration, Physics2D.gravity.y*gravityScale);
		Vector2 boosts = Vector2.zero;
		Vector2 displacement = Vector2.zero;
		float drag = groundDrag;
		float inputXDir = Input.GetAxisRaw ("Horizontal") * m_controlDirection;

		// Resolve anything colliding with the player.
		ResolveCollisions(m_currTriggersHit, m_currCollidersHit);

		m_isGrounded = IsGrounded();
		
		// Set accelerations depending on ground state
		if (!m_isGrounded) {
			acceleration.x = airAcceleration;
			deceleration.x = airDeceleration;
			drag = airDrag;

			// Unparent player if they are not grounded
			if(transform.parent != null) transform.parent = null;
		}

		// Compute delta velocities given player inputs
		deltaVelocity.x = ResolvePlayerXVelocity(inputXDir, drag, acceleration.x, deceleration.x);
		deltaVelocity.y = ResolvePlayerYVelocity(deceleration.y);

		// Compute external forces
		ResolveBoosts(ref deltaVelocity, ref boosts);

		// Compute next velocity ignoring external forces
		velocity = m_lastSpeed + deltaVelocity;

		// Prevent player from exceeding top speed
		velocity = ApplyVelocityConstraints(velocity, inputXDir);

		// The true player velocity after summing external forces
		worldVelocity = velocity + boosts;

		// Compute displacement, account for external forces
		displacement = (worldVelocity) * Time.deltaTime;

		// Move player and update sprite/animations
		UpdatePlayer(velocity, displacement);

		// Reset velocity if collisions detected 
		velocity = ApplyPhysicalConstraints(velocity, worldVelocity);

		// Save state
		m_lastSpeed = velocity;

		DisableJumpNextFrame = false;

		SendCollisionEvents(m_currTriggersHit, m_currCollidersHit);

		m_lastTriggersHit.Clear();
		m_lastCollidersHit.Clear();

		foreach(Collider2D triggerHit in m_currTriggersHit) {
			m_lastTriggersHit.Add(triggerHit);
		}

		foreach(Collider2D colliderHit in m_currCollidersHit) {
			m_lastCollidersHit.Add(colliderHit);
		}

		m_currTriggersHit.Clear();
		m_currCollidersHit.Clear();
	}

	public void OnGUI() {
		// Display anti-gravity charge meter.
		if (gravityScale < 0) {
			BoxCollider2D box = GetBoxCollider();
			Vector3 topLeftCorner = box.transform.position - new Vector3(box.size.x, box.size.y) / 2;
			Vector3 topRightCorner = topLeftCorner + new Vector3(box.size.x, 0);
			Vector3 meterLeftPos = Camera.allCameras[0].WorldToScreenPoint(topLeftCorner);
			Vector3 meterRightPos = Camera.allCameras[0].WorldToScreenPoint(topRightCorner);

			Texture2D borderTexture = new Texture2D(1, 1);
			borderTexture.SetPixel(0, 0, Color.white);
			borderTexture.Apply();

			Texture2D meterTexture = new Texture2D(1, 1);
			if (m_antiGravityActivated) {
				meterTexture.SetPixel(0, 0, ANTI_GRAVITY_ACTIVATED_COLOR);
			} else {
				meterTexture.SetPixel(0, 0, ANTI_GRAVITY_RECHARGING_COLOR);
			}
			meterTexture.Apply();

			Rect meterRect = new Rect(meterLeftPos.x, 
			                          meterLeftPos.y - ANTI_GRAVITY_METER_OFFSET - ANTI_GRAVITY_METER_HEIGHT, 
			                          (meterRightPos.x - meterLeftPos.x) * (m_antiGravityCharge / antiGravityDuration), 
			                          ANTI_GRAVITY_METER_HEIGHT);

			Rect borderRect = new Rect(meterRect.xMin - ANTI_GRAVITY_METER_BORDER,
			                           meterRect.yMin - ANTI_GRAVITY_METER_BORDER,
			                           (meterRightPos.x - meterLeftPos.x) + (ANTI_GRAVITY_METER_BORDER * 2),
			                           meterRect.height + (ANTI_GRAVITY_METER_BORDER * 2));

			Rect iconBounds = new Rect(meterRect.xMin - 30,
			                           meterRect.yMin - 7,
			                           ANTI_GRAVITY_ICON_SIZE, 
			                           ANTI_GRAVITY_ICON_SIZE);

			if (m_antiGravityActivated) {
				GUI.DrawTexture(iconBounds, m_antiGravityIconOn);
			} else {
				GUI.DrawTexture(iconBounds, m_antiGravityIconOff);
			}

			GUI.DrawTexture(borderRect, borderTexture);
			GUI.DrawTexture(meterRect, meterTexture);
		}
	}
	
	private float ResolvePlayerXVelocity(float inputXDir, float drag, float acceleration, float deceleration) {
		float deltaXVelocity = 0f;
		
		if (inputXDir == 0) {
			// No input, player decelerates according to drag
			deltaXVelocity = drag * Time.deltaTime * - Mathf.Sign (m_lastSpeed.x);
		} else {
			if (inputXDir * m_lastSpeed.x >= 0 && m_lastSpeed.x < playerSpeed) {
				// Accelerating in direction of travel
				deltaXVelocity = acceleration * Time.deltaTime * inputXDir;
			} else {
				// Accelerating opposite to direction of travel
				deltaXVelocity = deceleration * Time.deltaTime * inputXDir;
			}
		}

		return deltaXVelocity;
	}

	private float ResolvePlayerYVelocity(float deceleration) {
		float deltaYVelocity = 0f;

		if(m_isGrounded) {
			if (!DisableJumpNextFrame) {
				m_jumpsLeft = jumpsAllowed;
			}
			deltaYVelocity = m_lastSpeed.y * -1;
		} else {
			if (m_lastSpeed.y < 0 && m_jumpsLeft > 0) {
				m_jumpsLeft = 1;
			}

			deltaYVelocity = deceleration * Time.deltaTime;
		}
		
		if (Input.GetButtonDown("Jump") && m_jumpsLeft > 0) {
			if (m_jumpsLeft == 2) {
				deltaYVelocity += jumpSpeed; // TODO: restrict how quickly a double jump can be executed
				m_jumpsLeft--;
			} else if (m_jumpsLeft == 1 && m_lastSpeed.y <= 3f) {
				if (m_lastSpeed.y < 0) {
					m_lastSpeed.y = 0;
				}

				deltaYVelocity = 0f;
				deltaYVelocity += (jumpSpeed * 0.75f);
				m_jumpsLeft--;
			}

			m_isGrounded = false;
		}

		if(Input.GetAxisRaw("Vertical") < 0 && gravityScale < 0 && 
		   (m_antiGravityCharge - Time.deltaTime) > 0f) {
			deltaYVelocity -= antiGravity * Time.deltaTime;
			m_antiGravityCharge -= Time.deltaTime;
			m_antiGravityActivated = true;
		} else {
			m_antiGravityActivated = false;
		}

		if (m_antiGravityCharge < antiGravityDuration) {
			m_antiGravityCharge += Mathf.Min(antiGravityRechargeRate * Time.deltaTime, antiGravityDuration);
		}

		return deltaYVelocity;
	}

	private void ResolveBoosts(ref Vector2 deltaVelocity, ref Vector2 boosts) {

		foreach (Boost b in m_activeBoosts) {
			
			if(!b.HasStarted()) {
				b.Start();
				m_lastSpeed = b.ApplyPlayerDelta(m_lastSpeed);
			}
			
			// Boosts wear off according to a falloff rate
			b.Tick(Time.deltaTime);
			if(b.CheckIfFinished()) m_finishedBoosts.Add(b);
			else {
				// They may also be counteracted by the players input
				deltaVelocity = b.ApplyPlayerDelta(deltaVelocity);
				boosts += b.BoostVector;
			}
		}
		
		foreach(Boost b in m_finishedBoosts) {
			m_activeBoosts.Remove(b);
			//Debug.Log("Removing finished boost. Active size  "  + m_active_boosts.Count);
		}
		
		m_finishedBoosts.Clear();
	}

	private Vector2 ApplyVelocityConstraints(Vector2 velocity, float inputXDir) {
		// Ensure velocity falls between bounds (excluding boosts)
		if (Mathf.Abs (velocity.x) > playerSpeed) {
			velocity.x = playerSpeed * inputXDir;
		// Prevent issues when speed approaches zero
		} else if (inputXDir == 0 && velocity.x * m_lastSpeed.x <= 0) {
			velocity.x = 0;
		}

		return velocity;
	}

	private Vector2 ApplyPhysicalConstraints(Vector2 velocity, Vector2 worldVelocity) {
		// Prevent sticking to blocks above players head
		if(CheckAbove() && worldVelocity.y > 0) {
			velocity.y = 0;
			ResetBoostY(1);
		}

		// Remove any boost applied in the negative Y direction
		if(m_isGrounded) {
			ResetBoostY(-1);
		}
		
		// Prevent player from slipping around blocks after collision
		if(CheckRight() && worldVelocity.x > 0 ) {
			velocity.x = 0;
			ResetBoostX(1);
		} else if (CheckLeft() && worldVelocity.x < 0) {
			velocity.x = 0;
			ResetBoostX(-1);
		}

		return velocity;
	}

	private void UpdatePlayer(Vector2 velocity, Vector2 displacement) {
		// Update animator
		float speed = Mathf.Abs (velocity.x);
		m_animator.SetFloat("Speed", speed);
		m_animator.SetBool("Grounded", m_isGrounded);
		
		// Flip the sprite on the x-axis to match the movement
		// direction.
		if (speed > 0) {
			transform.localScale = new Vector3 (Mathf.Sign (velocity.x), 
			                                    transform.localScale.y, 
			                                    transform.localScale.z);
		}

		// Set new position
		MoveBy(displacement);
	}

	private void ResetBoostX(int dir) {
		foreach(Boost b in m_activeBoosts) {
			b.BoostVector = new Vector2(ResetVelocity(b.BoostVector.x, dir), b.BoostVector.y);
		}
	}

	private void ResetBoostY(int dir) {
		foreach(Boost b in m_activeBoosts) {
			b.BoostVector = new Vector2(b.BoostVector.x, ResetVelocity(b.BoostVector.y, dir));
		}
	}

	private float ResetVelocity(float value, int dir) {
		if(value > 0 && dir > 0 ||
		   value < 0 && dir < 0) {
			return 0;
		} else {
			return value;
		}
	}

	private void MoveBy(Vector2 displacement) {
		BoxCollider2D playerCollider = GetBoxCollider();
		Vector2 moveDir = displacement.normalized;
		float minSampleDistance = (playerCollider.size * 0.33f).magnitude;
		float distanceLeft = displacement.magnitude;

		// Need to hit triggers to send OnTrigger events.
		bool raycastsHitTriggers = Physics2D.queriesHitTriggers;
		Physics2D.queriesHitTriggers = true;

		if (distanceLeft == 0f) {
			ResolveCollisions(m_currTriggersHit, m_currCollidersHit);
		} else {
			while (distanceLeft > 0f) {
				float moveDist = Mathf.Min(minSampleDistance, distanceLeft);
				distanceLeft -= moveDist;
				transform.position += (new Vector3(moveDir.x, moveDir.y) * moveDist);
				
				ResolveCollisions(m_currTriggersHit, m_currCollidersHit);
			}
		}

		// Make sure we trigger events for blocks that touch the player.
		Vector2 rayStart, rayEnd = Vector2.zero;
		GetGroundRay(out rayStart, out rayEnd);

		List<RaycastHit2D> hits = new List<RaycastHit2D>();
		hits.AddRange(Physics2D.LinecastAll(rayStart, rayEnd));

		GetHeadRay(out rayStart, out rayEnd);
		hits.AddRange(Physics2D.LinecastAll(rayStart, rayEnd));

		GetLeftSideRay(out rayStart, out rayEnd);
		hits.AddRange(Physics2D.LinecastAll(rayStart, rayEnd));

		GetRightSideRay(out rayStart, out rayEnd);
		hits.AddRange(Physics2D.LinecastAll(rayStart, rayEnd));

		foreach (RaycastHit2D hit in hits) {
			if (hit.collider == playerCollider)
				continue;

			if (hit.collider.isTrigger) {
				if (!m_currTriggersHit.Contains(hit.collider)) {
					m_currTriggersHit.Add(hit.collider);
				}
			} else if (!m_currCollidersHit.Contains(hit.collider)) {
				m_currCollidersHit.Add(hit.collider);
			}
		}

		Physics2D.queriesHitTriggers = raycastsHitTriggers;
//		SendCollisionEvents(m_currTriggersHit, m_currCollidersHit);
	}

	private void SendCollisionEvents(HashSet<Collider2D> triggersHit, HashSet<Collider2D> collidersHit) {
		BoxCollider2D playerCollider = GetBoxCollider();
		HashSet<Collider2D> collidersConsumed = new HashSet<Collider2D>();

		foreach (Collider2D blockCollider in m_lastTriggersHit) {
			if (blockCollider == null)
				continue;

			if (triggersHit.Contains(blockCollider)) {
				blockCollider.SendMessage("OnTriggerStay2D", playerCollider, SendMessageOptions.DontRequireReceiver);
			} else {
				blockCollider.SendMessage("OnTriggerExit2D", playerCollider, SendMessageOptions.DontRequireReceiver);
			}
			
			collidersConsumed.Add(blockCollider);
		}
		
		foreach (Collider2D blockCollider in m_lastCollidersHit) {
			if (blockCollider == null)
				continue;

			if (collidersHit.Contains(blockCollider)) {
				blockCollider.SendMessage("OnCollisionStay2D", m_playerCollision, SendMessageOptions.DontRequireReceiver);
			} else {
				blockCollider.SendMessage("OnCollisionExit2D", m_playerCollision, SendMessageOptions.DontRequireReceiver);
			}
			
			collidersConsumed.Add(blockCollider);
		}
		
		foreach (Collider2D blockCollider in triggersHit) {
			if (collidersConsumed.Contains(blockCollider)) {
				continue;
			}
			
			blockCollider.SendMessage("OnTriggerEnter2D", playerCollider, SendMessageOptions.DontRequireReceiver);
		}
		
		foreach (Collider2D blockCollider in collidersHit) {
			if (collidersConsumed.Contains(blockCollider)) {
				continue;
			}
			
			blockCollider.SendMessage("OnCollisionEnter2D", m_playerCollision, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void SetPlayerEnabled(bool enabled) {
		// Disable player components for playing
		// animations.
		GetComponent<Renderer>().enabled = enabled;
		GetComponent<Collider2D>().enabled = enabled;
		this.enabled = enabled;
	}

	public void ResetVelocity() {
		// Clear boosters.
		m_activeBoosts.Clear();
		m_finishedBoosts.Clear();
		m_lastSpeed = Vector2.zero;
	}
	
	public void Kill(bool showKillAnimation = true) {
		// Detach from any platform the player may be
		// parented to.
		if (transform.parent != null) {
			transform.parent = null;
		}

		// Reset anti-gravity charge.
		m_antiGravityCharge = antiGravityDuration;
		gravityScale = 1.0f;

		ResetVelocity();

		if (showKillAnimation) {
			StartCoroutine(KillAnimation());
		} else {
			StartCoroutine(SpawnAnimation());
		}
	}
	
	private IEnumerator KillAnimation() {
		// Keeps the particles consistent.
		transform.localScale = new Vector3(1, 1, 1);

		SetPlayerEnabled(false);

		AudioPlayer.Instance.PlayWithTransform("Death", this.transform, volume: 0.5f);

		GetComponent<ParticleSystem>().Play();
		yield return new WaitForSeconds(GetComponent<ParticleSystem>().duration);
		GetComponent<ParticleSystem>().Stop();
		GetComponent<ParticleSystem>().Clear();

		StartCoroutine(SpawnAnimation());
	}

	private IEnumerator SpawnAnimation() {
		CheckPointScript spawnPoint = LevelController.Instance.currentCheckPoint;
		transform.position = spawnPoint.gameObject.transform.position;
		transform.localScale = new Vector3(1, 1, 1);

		AudioPlayer.Instance.PlayWithTransform("Life", this.transform, volume: 0.5f);

		SetPlayerEnabled(false);

		spawnPoint.GetComponent<ParticleSystem>().Play();
		yield return new WaitForSeconds(GetComponent<ParticleSystem>().duration);
		spawnPoint.GetComponent<ParticleSystem>().Stop();
		spawnPoint.GetComponent<ParticleSystem>().Clear();

		SetPlayerEnabled(true);

		m_lastSpeed = Vector2.zero;
	}

	private class ColliderComparer : IComparer<Collider2D> {
		private Vector3 m_playerPos;

		public ColliderComparer(Vector3 playerPos) {
			m_playerPos = playerPos;
		}

		int IComparer<Collider2D>.Compare(Collider2D a, Collider2D b) {
			float distA = (a.transform.position - m_playerPos).sqrMagnitude;
			float distB = (b.transform.position - m_playerPos).sqrMagnitude;

			return (int)Mathf.Sign(distA - distB);
		}
	}

	private void ResolveCollisions(HashSet<Collider2D> triggersHit, HashSet<Collider2D> collidersHit) {
		BoxCollider2D playerCollider = GetComponent<Collider2D>() as BoxCollider2D;
		float radius = Mathf.Max(playerCollider.size.x, playerCollider.size.y);
		Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y),
		                                                    radius);

		if (colliders.Length > 1)
			Array.Sort(colliders, new ColliderComparer(transform.position));

		foreach (Collider2D blockCollider in colliders) {
			if (blockCollider == playerCollider || !(blockCollider is BoxCollider2D) ||
			    blockCollider.enabled == false)
				continue;

			BoxCollider2D boxCollider = blockCollider as BoxCollider2D;
			Vector3 collisionOffset;
			if (GetIntersection(playerCollider, boxCollider, out collisionOffset)) {
				if (boxCollider.isTrigger) {
					if (!triggersHit.Contains(boxCollider)) {
						triggersHit.Add(boxCollider);
					}
				} else {
					if (!collidersHit.Contains(boxCollider)) {
						collidersHit.Add(boxCollider);
					}

					transform.position += collisionOffset;
				}
			}
		}
	}

	private bool GetIntersection(BoxCollider2D a, BoxCollider2D b, out Vector3 collisionOffset) {
		Rect aBounds = new Rect(a.transform.position.x - (a.size.x / 2f) + a.offset.x - COLLISION_SKIN_WIDTH,
		                        a.transform.position.y - (a.size.y / 2f) + a.offset.y - COLLISION_SKIN_WIDTH,
		                        a.size.x + (COLLISION_SKIN_WIDTH * 2), a.size.y + (COLLISION_SKIN_WIDTH * 2));

		Rect bBounds = new Rect(b.transform.position.x - (b.size.x * b.gameObject.transform.localScale.x / 2f) + b.offset.x,
		                        b.transform.position.y - (b.size.y * b.gameObject.transform.localScale.y / 2f) + b.offset.y,
		                        b.size.x * b.gameObject.transform.localScale.x, 
		                        b.size.y * b.gameObject.transform.localScale.y);

		float top = Mathf.Min(aBounds.yMax, bBounds.yMax);
		float bottom = Mathf.Max(aBounds.yMin, bBounds.yMin);
		float left = Mathf.Max(aBounds.xMin, bBounds.xMin);
		float right = Mathf.Min(aBounds.xMax, bBounds.xMax);

		collisionOffset = Vector3.zero;

		if (top < bottom || left > right) {
			return false;
		}

		float xAxisOffset = right - left;
		float yAxisOffset = top - bottom;

		Vector3 bToADir = (a.transform.position - b.transform.position);
		Vector3 xAxisOffsetDir = new Vector3(Mathf.Sign(bToADir.x), 0);
		Vector3 yAxisOffsetDir = new Vector3(0, Mathf.Sign(bToADir.y));

		if (xAxisOffset <= yAxisOffset) {
			collisionOffset = xAxisOffsetDir * xAxisOffset;
		} else {
			collisionOffset = yAxisOffsetDir * yAxisOffset;
		}

		return true;
	}
	
	private bool CheckRight() {
		BoxCollider2D box = GetBoxCollider();
		Vector3 rightSide = transform.position + new Vector3(box.offset.x, box.offset.y);

		Vector3 topRight = rightSide + (new Vector3(box.size.x, box.size.y) / 2) + 
							new Vector3(HORIZONTAL_RAY_OFFSET, -VERTICAL_RAY_OFFSET);
		Vector3 bottomRight = rightSide + (new Vector3(box.size.x, -box.size.y) / 2) + 
							new Vector3(HORIZONTAL_RAY_OFFSET, VERTICAL_RAY_OFFSET);

		Vector3 rayOffset = new Vector3(HORIZONTAL_RAY_OFFSET, 0);

		return CheckForCollision(topRight, topRight + rayOffset) || 
			   CheckForCollision(bottomRight, bottomRight + rayOffset);
	}
	
	private bool CheckLeft() {
		BoxCollider2D box = GetBoxCollider();
		Vector3 leftSide = transform.position + new Vector3(box.offset.x, box.offset.y);
		
		Vector3 topLeft = leftSide - (new Vector3(box.size.x, -box.size.y) / 2) + 
							new Vector3(-HORIZONTAL_RAY_OFFSET, -VERTICAL_RAY_OFFSET);
		Vector3 bottomLeft = leftSide - (new Vector3(box.size.x, box.size.y) / 2) + 
							new Vector3(-HORIZONTAL_RAY_OFFSET, VERTICAL_RAY_OFFSET);
		
		Vector3 rayOffset = new Vector3(-HORIZONTAL_RAY_OFFSET, 0);
		return CheckForCollision(topLeft, topLeft + rayOffset) || 
			   CheckForCollision(bottomLeft, bottomLeft + rayOffset);
	}

	private bool CheckAbove() {
		Vector2 rayStart, rayEnd = Vector2.zero;
		GetHeadRay(out rayStart, out rayEnd);
		return CheckForCollision(rayStart, rayEnd);
	}

	private void GetLeftSideRay(out Vector2 rayStart, out Vector2 rayEnd) {
		BoxCollider2D boxCollider = GetBoxCollider();
		rayStart = new Vector2(boxCollider.transform.position.x, boxCollider.transform.position.y)
					+ boxCollider.offset
					- (boxCollider.size / 2)
				    - new Vector2(GROUND_RAY_OFFSET, -GROUND_RAY_OFFSET);
		rayEnd = rayStart + (Vector2.up * (boxCollider.size.y - GROUND_RAY_OFFSET * 2));
	}

	private void GetRightSideRay(out Vector2 rayStart, out Vector2 rayEnd) {
		BoxCollider2D boxCollider = GetBoxCollider();
		rayStart = new Vector2(boxCollider.transform.position.x, boxCollider.transform.position.y)
					+ boxCollider.offset
					+ (boxCollider.size / 2)
					- new Vector2(-GROUND_RAY_OFFSET, GROUND_RAY_OFFSET);
		rayEnd = rayStart + (-Vector2.up * (boxCollider.size.y - GROUND_RAY_OFFSET * 2));
	}

	private void GetHeadRay(out Vector2 rayStart, out Vector2 rayEnd) {
		BoxCollider2D boxCollider = GetBoxCollider();
		rayStart = new Vector2(boxCollider.transform.position.x, boxCollider.transform.position.y) 
					+ boxCollider.offset
					- new Vector2(boxCollider.size.x, -boxCollider.size.y) / 2
					- new Vector2(-GROUND_RAY_OFFSET, -GROUND_RAY_OFFSET);
		
		rayEnd = rayStart + (Vector2.right * (boxCollider.size.x - GROUND_RAY_OFFSET * 2));
	}
	
	private RaycastHit2D CheckForCollision(Vector3 rayStart, Vector3 rayEnd) {
		return CheckForCollision(new Vector2(rayStart.x, rayStart.y), new Vector2(rayEnd.x, rayEnd.y));
	}
	
	private RaycastHit2D CheckForCollision(Vector2 rayStart, Vector2 rayEnd) {
		Debug.DrawLine (rayStart, rayEnd);
		return Physics2D.Linecast(rayStart, rayEnd);
	}

	private BoxCollider2D GetBoxCollider() {
		return (BoxCollider2D) GetComponent<Collider2D>();
	}

	private void GetGroundRay(out Vector2 rayStart, out Vector2 rayEnd) {
		BoxCollider2D boxCollider = GetBoxCollider();
		rayStart = new Vector2(boxCollider.transform.position.x, boxCollider.transform.position.y) 
				+ boxCollider.offset
				- (boxCollider.size / 2) 
				- new Vector2(-GROUND_RAY_OFFSET, GROUND_RAY_OFFSET);
		
		rayEnd = rayStart + (Vector2.right * (boxCollider.size.x - GROUND_RAY_OFFSET * 2));
	}
	
	private bool IsGrounded () {
		Vector2 rayStart, rayEnd = Vector2.zero;
		GetGroundRay(out rayStart, out rayEnd);
		return CheckForCollision(rayStart, rayEnd) && gravityScale > 0;
	}
	
	public void AddBoost(Boost boost) {
		m_activeBoosts.Add(boost);
	}
	
	public void AddKey () {
		m_keyRing++;
		LevelController.Instance.keysCollected++;
	}
	
	public bool RemoveKey() {
		if (m_keyRing > 0) {
			m_keyRing--;
			LevelController.Instance.keysCollected--;
			return true;
		} else {
			return false;
		}
	}

	public void ActivateDisguise(float duration){
		StartCoroutine(TriggerDisguise(duration));
	}

	public IEnumerator TriggerDisguise(float duration){
		foreach(AbsToggleBlock affectedBlock in GameObject.FindObjectsOfType<AbsToggleBlock>()){
			affectedBlock.ToggleBlock();
		}
		yield return new WaitForSeconds(duration);
		foreach(AbsToggleBlock affectedBlock in GameObject.FindObjectsOfType<AbsToggleBlock>()){
			affectedBlock.ToggleBlock();
		}
	}	
}
