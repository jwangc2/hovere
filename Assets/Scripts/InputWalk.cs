using UnityEngine;
using System.Collections;

public class InputWalk : CharMovement {

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

	// Use this for initialization
	protected override void Start () {
		base.Start ();

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

		// Determine the speeds
		walk = v * v;
		sprint = 0f;
		if (sp)
		{
			sprint = v * v;
		}

		// Turning
		dir = dir + (h - dir) * 0.1f;
	}

	protected override void FixedUpdate() {
		base.FixedUpdate();

		onwall = bodyCheck.isMeeting;
		if (animator)
		{
			animator.SetFloat("Walk", walk);
			animator.SetFloat("Sprint", sprint);
			animator.SetFloat("Turn", dir);
			animator.SetBool("OnGround", OnGround());
			animator.SetBool("OnWall", onwall);
		}


	}

	protected override void Move() {
		base.Move();

		animator.transform.position = cc.transform.position;
	}

	protected override void Step() {
		base.Step();

		cc.transform.rotation = animator.transform.rotation;

		AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
		if (info.nameHash == idleState) {
			this.velocity.x = 0f;
			this.velocity.z = 0f;
		} else if (info.nameHash == walkState) {
			this.velocity = (cc.transform.forward * 1f);
		} else if (info.nameHash == sprintState) {
			this.velocity = cc.transform.forward * 6f;
		} else if (info.nameHash == wallrunState && bodyCheck.isMeeting) {
			Collision c = bodyCheck.collMeeting;

			if (c != null)
			{
				Vector3 norm = c.contacts[0].normal;
				Vector3 cross = Vector3.Cross(cc.transform.forward, norm);

				Vector3 newDir = Vector3.Cross(norm, Vector3.up * Mathf.Sign(cross.y)).normalized;

				animator.transform.LookAt(animator.transform.position + newDir + norm * -0.5f);
				this.velocity = newDir * 6f;
			}
			else
			{
				this.velocity = cc.transform.forward * 6f;
			}
			this.velocity.y = 0f;
		}

		if (Input.GetButtonDown("Jump") && (OnGround() || onwall))
		{
			this.velocity.y = 150f;
			if (onwall)
			{
				Collision c = bodyCheck.collMeeting;
				if (c != null)
				{
					Vector3 norm = c.contacts[0].normal;
					this.velocity.y = 10f;
					this.velocity += norm * 10f;
					Vector3 look = animator.transform.position + this.velocity;
					look.y = animator.transform.position.y;
					animator.transform.LookAt(look);
				}
			}
		}
	}

	protected override bool OnGround() 
	{
		bool footOn = true;
		foreach (ColliderCheck collCheck in footChecks)
		{
			if (!collCheck.isMeeting)
			{
				footOn = false;
				break;
			}
		}
		return (base.OnGround() || footOn);
	}

}
