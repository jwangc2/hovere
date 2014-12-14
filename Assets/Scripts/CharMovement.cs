using UnityEngine;
using System.Collections;

public class CharMovement : MonoBehaviour {

	public CharacterController cc;
	public float grav = 9.8f;

	protected Vector3 velocity = Vector3.zero;

	// Use this for initialization
	protected virtual void Start () {
	
	}
	
	// Update is called once per frame
	protected virtual void FixedUpdate () {
		Move();
	}

	protected virtual void Move() {
		float dt = Time.fixedDeltaTime;
		this.velocity += Vector3.down * grav * dt;
		
		// For custom movement after applying gravity
		Step();
		
		// If on the ground, reset velocity
		if (cc.isGrounded)
			this.velocity.y = 0f;
		
		cc.Move(velocity * dt);
	}

	protected virtual void Step() {
	}
}
