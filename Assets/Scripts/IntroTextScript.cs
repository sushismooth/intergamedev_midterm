using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroTextScript : MonoBehaviour {

	// Use this for initialization
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.W))
		{
			Destroy(this.gameObject);
			Destroy(this);
		}
	}
}
