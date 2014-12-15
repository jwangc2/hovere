using UnityEngine;
using System.Collections;

public class InputWalk : CharMovement {

	public ColliderCheck[] footChecks;
	public ColliderCheck bodyCheck;

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
			this.velocity = Vector3.zero;
		} else if (info.nameHash == walkState) {
			this.velocity = (cc.transform.forward * 1f);
		} else if (info.nameHash == sprintState) {
			this.velocity = cc.transform.forward * 6f;
		} else if (info.nameHash == wallrunState) {
			this.velocity = cc.transform.forward * 6f;
			this.velocity.y = 0f;
		}

		if (Input.GetButtonDown("Jump") && OnGround())
		{			this.velocity = cc.transform.up * 150f;
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
