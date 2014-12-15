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
		
		// If on the ground, reset velocity
		if (OnGround() && this.velocity.y < 0f)
		{
			this.velocity.y = 0f;
		}

		// For custom movement after applying gravity
		Step();

		// Debug.Log("Speed: " + this.velocity.magnitude);
		cc.Move(velocity * dt);

		this.velocity += Vector3.down * grav * dt;
	}

	protected virtual void Step() {
	}

	protected virtual bool OnGround() {
		return cc.isGrounded;
	}
}
