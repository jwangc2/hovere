using UnityEngine;
using System.Collections;

public class CamPOV : MonoBehaviour {

	public Transform target;
	// Use this for initialization
	void Start () {
		Update ();
	}
	
	// Update is called once per frame
	void Update () {
		if (target)
		{
			this.transform.position = target.position;
			this.transform.rotation = target.rotation;
		}
	}
}
