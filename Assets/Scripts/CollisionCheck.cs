using UnityEngine;
using System.Collections;

public class CollisionCheck : MonoBehaviour {
	public string tagComp = "";
	public bool isMeeting {
		get {
			return _isMeeting;
		}
	}

	public Collision collMeeting {
		get {
			return col;
		}
	}
	
	public bool _isMeeting = false;
	private Collision col = null;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnCollisionEnter(Collision coll)
	{
		if (coll.collider.tag == tagComp) {
			_isMeeting = true;
			col = coll;
		}
	}
	
	void OnCollisionStay(Collision coll)
	{
		if (coll.collider.tag == tagComp) {
			_isMeeting = true;
			col = coll;
		}
	}
	
	void OnCollisionExit(Collision coll)
	{
		if (coll.collider.tag == tagComp) {
			_isMeeting = false;
			col = coll;
		}
	}
}
