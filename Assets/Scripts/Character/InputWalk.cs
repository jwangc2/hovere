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

	bool touchingWall = false;
	bool canWallRun = false;

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
		touchingWall = bodyCheck.isMeeting;
		Collision c = bodyCheck.collMeeting;
		bool vertCheck = false;
		bool horzCheck = false;

		if (touchingWall && c != null)
		{
			// Find the normal and it's right and up equivalent
			Vector3 norm = c.contacts[0].normal;
			Vector3 normRight = Vector3.Cross(norm, Vector3.up);
			Vector3 normUp = Vector3.Cross(norm, Vector3.left);

			// Find the right and up components (relative to surf. normal) of velocity using dot projection
			Vector3 compRight = TKVecMath.DotProj(velocity, normRight);
			Vector3 compUp = TKVecMath.DotProj(velocity, normUp);

			// Subtract right component to get the "vertical velocity", then compare the angle to ensure proper entry
			Vector3 vertVel = velocity - compRight;
			float vertAngle = Vector3.Angle(vertVel, norm * -1f);
			vertCheck = (vertAngle <= 70f);

			// Do something similar for the "horizontal velocity"
			Vector3 horzVel = velocity - compUp;
			float horzAngle = Vector3.Angle(horzVel, norm * -1f);
			horzCheck = (horzAngle >= 35f && horzAngle < 90f);
		}


		canWallRun = (touchingWall && vertCheck && horzCheck);

		// Apply the state variables
		if (animator)
		{
			animator.SetFloat("Walk", walk);
			animator.SetFloat("Sprint", sprint);
			animator.SetFloat("Turn", dir);
			animator.SetBool("OnGround", OnGround());
			animator.SetBool("TouchingWall", touchingWall);
			animator.SetBool("CanWallRun", canWallRun);
		}


	}

	#endregion


	#region Inherited Methods

	protected override void Move() {
		base.Move();

		// Get the state info and act according the current state
		AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

		if (info.nameHash != idleState)
		{
			// Match the animator position with the cc position as soon as the cc moves
			UpdateAnimator();
		}
	}

	protected override void Step() {
		base.Step();

		float dt = Time.fixedDeltaTime;

		// Make sure we are in the right orientation (mainly for the forward orientation)
		UpdateCharacterController();
		touchingWall = bodyCheck.isMeeting; // Recheck this

		// Get the state info and act according the current state
		AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
		int currentState = info.nameHash;

		if (currentState == idleState) {
			// If we're trying to turn while sprinting, jank it
			TurnInPlace(1f, dt);

			// Stay still
			this.velocity.x = 0f;
			this.velocity.z = 0f;
		}
		else if (currentState == fallState) {
			Vector3 look = animator.transform.forward;
			look.y = 0f;

			animator.transform.LookAt(animator.transform.position + look);
		}
		else if (currentState == walkState) {
			// Move forward at a speed of 1
			Vector3 spd = cc.transform.forward * 1f;
			this.velocity = new Vector3(spd.x, this.velocity.y, spd.z);
		} 
		else if (currentState == sprintState) {
			// If we're trying to turn while sprinting, jank it
			TurnInPlace(2f, dt);

			// Move forward at a speed of 6
			Vector3 spd = cc.transform.forward * 6f;
			this.velocity = new Vector3(spd.x, this.velocity.y, spd.z);
		} 
		else if (currentState == wallrunState && touchingWall) {
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
		bool onwall = (currentState == wallrunState);
		bool onground = OnGround();
		if (Input.GetButtonDown("Jump"))
		{
			if (onground) // AKA a normal jump
			{
				// Nomrally jump with vert. speed of 7
				this.velocity.y = 7f;
				
				// Get off the ground and make sure it doesn't register as being on the ground (and setting y-velocity to 0)
				cc.transform.position += Vector3.up * 0.5f;
				cc.Move(Vector3.zero);
			}
			else // If off the ground
			{

				// ...and touching the wall
				if (touchingWall)
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

						// Get off the wall a bit (to unregister touchingWall)
						cc.transform.position += norm * 1f;
						animator.SetBool("touchingWall", false);
					}
				}
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
