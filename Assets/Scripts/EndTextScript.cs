using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTextScript : MonoBehaviour
{
	private float zPos = -8f;
	
	void Update ()
	{
		if (zPos <= -6f)
		{
			zPos += Time.deltaTime/2;
		}
		transform.position = new Vector3(transform.position.x, transform.position.y, zPos);
	}
}
