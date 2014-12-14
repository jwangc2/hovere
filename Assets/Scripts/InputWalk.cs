using UnityEngine;
using System.Collections;

public class InputWalk : CharMovement {

	public FootCheck footCollider;

	Animator animator;
	float walk = 0.0f;
	float sprint = 0.0f;
	float dir = 0.0f;
	// Use this for initialization
	protected override void Start () {
		base.Start ();
		animator = this.gameObject.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		// Get the inputs
		float h = Input.GetAxisRaw("Horizontal");
		float v = Input.GetAxis("Vertical");
		bool sp = Input.GetButton("Fire1");

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
		if (animator)
		{
			animator.SetFloat("Walk", walk);
			animator.SetFloat("Sprint", sprint);
			animator.SetFloat("Turn", dir);
			animator.SetBool("OnGround", this.cc.isGrounded || footCollider.isGrounded);
		}
	}
}
