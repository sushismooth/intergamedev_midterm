using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//put on player character
//controls movement and rotation of player character
public class PlayerController : MonoBehaviour
{

	public float moveSpeed;
	private Vector3 inputVector;
	private Rigidbody myRbody;

	private Camera cam;
	private Vector3 mousePosition;
	private Vector3 mouseAwayFromCamera;
	private Vector3 mouseWorldPosition;
	
	// Use this for initialization
	void Start ()
	{
		myRbody = GetComponent<Rigidbody>();
		cam = Camera.main;
	}
	
	// Update is called once per frame
	void Update ()
	{
		//get inputs
		float vertical = Input.GetAxis("Vertical");
		float horizontal = Input.GetAxis("Horizontal");
		
		//change inputVector accordingly
		inputVector = transform.forward * vertical;
		//inputVector += transform.right * horizontal;
		
		//cursor to world location
		mousePosition = Input.mousePosition;
		mousePosition.z = 1;
		mouseAwayFromCamera = cam.ScreenToWorldPoint(mousePosition);
		
		//find direction between two points
		Vector3 raycastDirection = mouseAwayFromCamera - cam.transform.position;
		
		//shoot raycast from camera
		RaycastHit rayHit;
		if (Physics.Raycast(cam.transform.position, raycastDirection, out rayHit, Mathf.Infinity))
		{
			Debug.DrawRay(cam.transform.position, raycastDirection * rayHit.distance, Color.yellow);
			mouseWorldPosition = rayHit.point;
			Debug.Log(mouseWorldPosition);
		}
		else
		{
			Debug.DrawRay(cam.transform.position, raycastDirection * 1000, Color.white);
		}

		//turn player towards mouseWorldPoint
		Vector3 playerLookAt = mouseWorldPosition;
		playerLookAt.y = transform.position.y;
		transform.LookAt(playerLookAt);
	}

	void FixedUpdate()
	{
		myRbody.velocity = (inputVector * moveSpeed);
	}
}
