using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//put on player character
//controls movement and rotation of player character
public class PlayerController : MonoBehaviour
{

	private float moveSpeed;
	private Vector3 inputVector;
	private Rigidbody myRbody;

	private Camera cam;
	private Vector3 mousePosition;
	private Vector3 mouseAwayFromCamera;
	private Vector3 mouseWorldPosition;
	private int FLOOR_LAYERMASK;

	public GameObject ball;
	private Rigidbody ballRbody;
	private bool hasBall;
	private bool isChargingThrow;
	private float throwPower;
	private float maxPower = 100;
	private float powerGrowthRate = 40;
	
	
	// Use this for initialization
	void Start ()
	{
		myRbody = GetComponent<Rigidbody>();
		ballRbody = ball.GetComponent<Rigidbody>();
		cam = Camera.main;
		FLOOR_LAYERMASK = 1 << 9;
	}
	
	void Update ()
	{	
		//find the world location on the floor plane where the mouse cursor is pointing at
		//cursor to world location
		mousePosition = Input.mousePosition;
		mousePosition.z = 1;
		mouseAwayFromCamera = cam.ScreenToWorldPoint(mousePosition);
		
		//find direction between two points
		Vector3 raycastDirection = mouseAwayFromCamera - cam.transform.position;
		
		//shoot raycast from camera
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

		//turn player towards mouseWorldPoint
		Vector3 playerLookAt = mouseWorldPosition;
		playerLookAt.y = transform.position.y;
		transform.LookAt(playerLookAt);
		
		//determine movespeed from distance between player and mouseWorldPoint
		float distanceFromMouse = Vector3.Distance(transform.position, playerLookAt);
		if (distanceFromMouse < 0.5)
		{
			moveSpeed = 0;
		} 
		else if (distanceFromMouse > 10)
		{
			moveSpeed = 10;
		}
		else
		{
			moveSpeed = distanceFromMouse;
		}
		
		//get inputs
		float vertical = Input.GetAxis("Vertical");
		float horizontal = Input.GetAxis("Horizontal");
		
		//change inputVector accordingly
		inputVector = transform.forward * vertical;
		//inputVector += transform.right * horizontal;

		if (hasBall && Input.GetMouseButtonDown(0) && !isChargingThrow)
		{
			isChargingThrow = true;
			throwPower = 0f;
		} 
		else if (hasBall && Input.GetMouseButton(0) && isChargingThrow)
		{
			if (throwPower <= maxPower)
			{
				throwPower += Time.deltaTime * powerGrowthRate;
			}
			else if (throwPower > maxPower)
			{
				throwPower = maxPower;
			}
			Debug.Log(throwPower);
		} 
		else if (hasBall && Input.GetMouseButtonUp(0) && isChargingThrow)
		{
			float ballAngle;
			Vector3 ballAngleV3;
			
			isChargingThrow = false;
			hasBall = false;
			ball.transform.parent = null;
			ballRbody.isKinematic = false;

			ballAngle = throwPower / maxPower * 60;
			ballAngleV3 = new Vector3(ballAngle * -1, transform.eulerAngles.y, transform.eulerAngles.z);
			ballRbody.transform.eulerAngles = ballAngleV3;
			ballRbody.AddForce(ball.transform.forward * throwPower * 10);

		}

		Debug.DrawRay(ball.transform.position, ball.transform.forward * 100, Color.white);

	}

	void FixedUpdate()
	{
		myRbody.velocity = (inputVector * moveSpeed);
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject == ball)
		{
			hasBall = true;
			ball.transform.parent = this.gameObject.transform;
			ballRbody.isKinematic = true;
			ball.transform.localPosition = Vector3.forward * 1.1f;
			ball.transform.eulerAngles = Vector3.zero;
		}
	}
}
