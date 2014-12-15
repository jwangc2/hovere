using UnityEngine;
using System.Collections;

public class InputWalk : CharMovement {

	#region Public and Private Variables
	public ColliderCheck[] footChecks;
	public CollisionCheck bodyCheck;
	public Animator animator;

	float walk = 0.0f;
	float sprint = 0.0f;
	float dir = 0.0f;

	bool onwall = false;

	private int idleState;
	private int walkState;
	private int fallState;
	private int sprintState;
	private int wallrunState;

	#endregion


	#region Unity Callbacks
	// Use this for initialization
	protected override void Start () {
		base.Start ();

		// Determine the ID's related to each state
		idleState = Animator.StringToHash("Base Layer.Idle");
		walkState = Animator.StringToHash("Base Layer.Walk");
		fallState = Animator.StringToHash("Base Layer.Falling");
		sprintState = Animator.StringToHash("Base Layer.SprintState");
		wallrunState = Animator.StringToHash("Base Layer.WallRun");
	}
	
	// Update is called once per frame
	void Update () {
		// Get the inputs
		float h = Input.GetAxisRaw("Horizontal");
		float v = Input.GetAxis("Vertical");
		bool sp = Input.GetButton("Sprint");

		// Calculate some of the state variables
		walk = v * v;
		sprint = 0f;
		if (sp)
		{
			sprint = v * v;
		}

		// Turning (interpolated for smoothness)
		dir = dir + (h - dir) * 0.1f;
	}

	protected override void FixedUpdate() {
		base.FixedUpdate();

		// Check physics stuff during FixedUpdate()
		onwall = bodyCheck.isMeeting;

		// Apply the state variables
		if (animator)
		{
			animator.SetFloat("Walk", walk);
			animator.SetFloat("Sprint", sprint);
			animator.SetFloat("Turn", dir);
			animator.SetBool("OnGround", OnGround());
			animator.SetBool("OnWall", onwall);
		}


	}

	#endregion


	#region Inherited Methods

	protected override void Move() {
		base.Move();

		// Match the animator position with the cc position as soon as the cc moves
		UpdateAnimator();
	}

	protected override void Step() {
		base.Step();

		float dt = Time.fixedDeltaTime;

		// Make sure we are in the right orientation (mainly for the forward orientation)
		UpdateCharacterController();
		onwall = bodyCheck.isMeeting; // Recheck this

		// Get the state info and act according the current state
		AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
		if (info.nameHash == idleState) {
			// If we're trying to turn while sprinting, jank it
			TurnInPlace(1f, dt);

			// Stay still
			this.velocity.x = 0f;
			this.velocity.z = 0f;
		} 
		else if (info.nameHash == walkState) {
			// Move forward at a speed of 1
			Vector3 spd = cc.transform.forward * 1f;
			this.velocity = new Vector3(spd.x, this.velocity.y, spd.z);
		} 
		else if (info.nameHash == sprintState) {
			// If we're trying to turn while sprinting, jank it
			TurnInPlace(2f, dt);

			// Move forward at a speed of 6
			Vector3 spd = cc.transform.forward * 6f;
			this.velocity = new Vector3(spd.x, this.velocity.y, spd.z);
		} 
		else if (info.nameHash == wallrunState && onwall) {
			// Check for collision info (we need the normal)
			Collision c = bodyCheck.collMeeting;
			if (c != null)
			{
				Vector3 norm = c.contacts[0].normal;
				Vector3 cross = Vector3.Cross(cc.transform.forward, norm); // crossing with this will det. left vs right running

				// Determine the new direction to move in
				Vector3 newDir = Vector3.Cross(norm, Vector3.up * Mathf.Sign(cross.y)).normalized;

				// Look somewhat diagonally
				animator.transform.LookAt(animator.transform.position + newDir + norm * -0.5f);

				// Move parallel to the wall with a speed of 6
				this.velocity = newDir * 6f;
			}
			else
			{
				// Best guess, just keep moving forward
				this.velocity = cc.transform.forward * 6f;
			}

			// Friction (ideal)
			this.velocity.y = 0f;
		}

		// If the player presses the jump key
		if (Input.GetButtonDown("Jump") && (OnGround() || onwall))
		{
			// Nomrally jump with vert. speed of 7
			this.velocity.y = 7f;

			// If we're jumping off a wall
			if (onwall)
			{
				Collision c = bodyCheck.collMeeting;
				if (c != null)
				{
					// y-velocity of 10
					this.velocity.y = 10f;

					// jump away from the wall with a speed of 3
					Vector3 norm = c.contacts[0].normal;
					this.velocity += norm * 3f;

					// turn to face where you will be moving
					Vector3 look = animator.transform.position + this.velocity;
					look.y = animator.transform.position.y;                     // but make sure the y-axis is locked
					animator.transform.LookAt(look);

					// Get off the wall a bit (to unregister OnWall)
					cc.transform.position += norm * 1f;
					animator.SetBool("OnWall", false);
				}
			}
			else
			{
				// Get off the ground and make sure it doesn't register as being on the ground (and setting y-velocity to 0)
				cc.transform.position += Vector3.up * 0.5f;
				cc.Move(Vector3.zero);
			}
		}
	}

	protected override bool OnGround() 
	{
		// Check each of the foot checkers with a gigantic OR gate
		bool footOn = false;
		foreach (ColliderCheck collCheck in footChecks)
		{
			if (collCheck.isMeeting)
			{
				footOn = true;
				break;
			}
		}

		return (base.OnGround() || footOn);
	}

	#endregion


	#region Helper Functions

	// Matches the animator position to the cc position
	void UpdateAnimator()
	{
		animator.transform.position = cc.transform.position;
	}

	void UpdateCharacterController()
	{
		cc.transform.rotation = animator.transform.rotation;
	}

	void TurnInPlace(float degrees, float dt)
	{
		// If we're trying to turn while doing something, jank it
		animator.transform.RotateAround(animator.transform.up, dir * degrees * dt);
		UpdateCharacterController();
	}

	#endregion

}
