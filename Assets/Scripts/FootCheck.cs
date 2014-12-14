using UnityEngine;
using System.Collections;

public class FootCheck : MonoBehaviour {
	public bool isGrounded {
		get {
			return _isGrounded;
		}
	}

	public bool _isGrounded = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Ground")
			_isGrounded = true;
	}

	void OnTriggerStay(Collider other)
	{
		if (other.tag == "Ground")
			_isGrounded = true;
	}

	void OnTriggerExit(Collider other)
	{
		if (other.tag == "Ground")
			_isGrounded = false;
	}
}
