using UnityEngine;
using System.Collections;

public class ColliderCheck : MonoBehaviour {
	public string tagComp = "";
	public bool isMeeting {
		get {
			return _isMeeting;
		}
	}

	public bool _isMeeting = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == tagComp)
			_isMeeting = true;
	}

	void OnTriggerStay(Collider other)
	{
		if (other.tag == tagComp)
			_isMeeting = true;
	}

	void OnTriggerExit(Collider other)
	{
		if (other.tag == tagComp)
			_isMeeting = false;
	}
}
