using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPlayerController : MonoBehaviour {

	private float moveSpeed;
	public float maxMoveSpeed;
	private Vector3 inputVector;
	private Rigidbody myRbody;
	private float gravity;

	private Camera cam;
	private Vector3 mousePosition;
	private Vector3 mouseAwayFromCamera;
	private Vector3 mouseWorldPosition;
	private int FLOOR_LAYERMASK;
	
	// Use this for initialization
	void Start () {
		myRbody = GetComponent<Rigidbody>();
		cam = Camera.main;
		FLOOR_LAYERMASK = 1 << 9;
	}
	
	// Update is called once per frame
	void Update () {
		MouseToWorldPosition();
		PlayerMovement();
	}
	
	void FixedUpdate()
	{
		myRbody.velocity = (inputVector * moveSpeed); //change player velocity according to inputs
	}
	
	void MouseToWorldPosition()
	{
		if (!Input.GetMouseButton(1))
		{
			//find the world location on the floor plane where the mouse cursor is pointing at
			//cursor to world location
			mousePosition = Input.mousePosition;
			mousePosition.z = 1;
			mouseAwayFromCamera = cam.ScreenToWorldPoint(mousePosition);

			//find direction between two points
			Vector3 raycastDirection = mouseAwayFromCamera - cam.transform.position;

			//shoot raycast from camera using direction
			RaycastHit rayHit;
			if (Physics.Raycast(cam.transform.position, raycastDirection, out rayHit, Mathf.Infinity, FLOOR_LAYERMASK))
			{
				Debug.DrawRay(cam.transform.position, raycastDirection * rayHit.distance, Color.yellow);
				mouseWorldPosition = rayHit.point;
			}
			else
			{
				Debug.DrawRay(cam.transform.position, raycastDirection * 1000, Color.white);
			}
		}
	}

	void PlayerMovement()
	{
		//turn player towards mouseWorldPoint
		mouseWorldPosition.y = transform.position.y;
		transform.LookAt(mouseWorldPosition);
		
		//determine movespeed from distance between player and mouseWorldPoint
		float distanceFromMouse = Vector3.Distance(transform.position, mouseWorldPosition);
		if (distanceFromMouse < 0.5)
		{
			moveSpeed = 0;
		} 
		else if (distanceFromMouse > 10)
		{
			moveSpeed = maxMoveSpeed;
		}
		else
		{
			moveSpeed = distanceFromMouse * maxMoveSpeed/10;
		}
		
		//get W/S inputs - since people don't run sideways
		float vertical = Input.GetAxis("Vertical");
		if (vertical < 0)
		{
			vertical = vertical * 0.5f; //slower when moving backwards
		}
		inputVector = transform.forward * vertical;
	}
}
