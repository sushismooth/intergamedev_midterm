using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

	public GameObject player;
	private Vector3 position;
	
	// Update is called once per frame
	void Update ()
	{
		position = player.transform.position + new Vector3(0f, 0f, -8f);
		position.y = 10f;
		transform.position = position;
	}
}
