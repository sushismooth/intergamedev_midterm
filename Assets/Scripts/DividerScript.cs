using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DividerScript : MonoBehaviour
{

	public Transform player;
	public Material mat;
	private float alpha;
	private float distance;
	
	void Update ()
	{
		distance = transform.position.z - player.position.z;
		if (distance >= 5)
		{
			alpha = 0;
		} 
		else if (distance <= 0)
		{
			alpha = 0.2f;
		}
		else if (distance > 0 && distance < 5)
		{
			alpha = (Mathf.Abs(distance - 5)) / 25;
		}
		mat.color = new Color(0,0,0,alpha);
	}
}
