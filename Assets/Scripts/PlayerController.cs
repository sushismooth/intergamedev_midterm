using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA;


//put on player character
//controls movement and rotation of player character
public class PlayerController : MonoBehaviour
{
	public GameObject GameState;
	private GameController gameStateScript;
	private int THROW = 1;
	private int CATCH = 2;
	
	private float moveSpeed;
	private Vector3 inputVector;
	private Rigidbody myRbody;
	private float gravity;

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
	public float throwAngle = 45;
	private float throwRadianAngle;
	public float maxPower = 10;
	public float powerGrowthRate = 4;

	public GameObject throwTrajectory;
	private LineRenderer trajectoryLR;
	public int lineResolution;
	public GameObject trajectoryHit;
	
	
	// Use this for initialization
	void Start ()
	{
		gameStateScript = GameState.GetComponent<GameController>();
		myRbody = GetComponent<Rigidbody>();
		ballRbody = ball.GetComponent<Rigidbody>();
		trajectoryLR = throwTrajectory.GetComponent<LineRenderer>();
		
		cam = Camera.main;
		FLOOR_LAYERMASK = 1 << 9;

		gravity = Mathf.Abs(Physics.gravity.y);
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
		
		//get W/S inputs - since people don't run sideways
		float vertical = Input.GetAxis("Vertical");
		inputVector = transform.forward * vertical;

		if (hasBall && Input.GetMouseButtonDown(0) && !isChargingThrow) //on mouse down
		{
			isChargingThrow = true;
			throwPower = 0f;
		} 
		else if (hasBall && Input.GetMouseButton(0) && isChargingThrow) //during mouse hold
		{
			if (throwPower < maxPower)
			{
				throwPower += Time.deltaTime * powerGrowthRate;
			}
			else if (throwPower > maxPower)
			{
				throwPower = maxPower;
			}

			throwTrajectory.transform.position = ball.transform.position;
			throwTrajectory.transform.eulerAngles = this.transform.eulerAngles - new Vector3(0, 90, 0);
			RenderArc();
		} 
		else if (hasBall && Input.GetMouseButtonUp(0) && isChargingThrow) //on mouse release
		{
			Vector3 throwAngleV3;
			
			isChargingThrow = false;
			hasBall = false;
			ball.transform.parent = null;
			ballRbody.isKinematic = false;

			//throwPower / maxPower * 60;
			throwAngleV3 = new Vector3(throwAngle * -1, transform.eulerAngles.y, transform.eulerAngles.z);
			ballRbody.transform.eulerAngles = throwAngleV3;
			ballRbody.velocity = ball.transform.forward * throwPower;
			//ballRbody.AddForce(ball.transform.forward * throwPower * 10);

			gameStateScript.ballInAir = true;
			gameStateScript.gameState = CATCH;
		}

		Debug.DrawRay(ball.transform.position, ball.transform.forward * 100, Color.white);

	}

	void FixedUpdate()
	{
		myRbody.velocity = (inputVector * moveSpeed);
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject == ball && gameStateScript.gameState == THROW) //when player collides with ball
		{
			hasBall = true;
			ball.transform.parent = this.gameObject.transform;
			ballRbody.isKinematic = true;
			ball.transform.localPosition = Vector3.forward * 1.1f;
			ball.transform.eulerAngles = Vector3.zero;
		}
	}

	void RenderArc() //set appropriate settings for line renderer
	{
		trajectoryLR.enabled = true;
		trajectoryHit.GetComponent<SpriteRenderer>().enabled = true;
		trajectoryLR.positionCount = lineResolution + 1;
		trajectoryLR.SetPositions(CalculateArcArray());
		Vector3 hitPosition = trajectoryLR.transform.TransformPoint(trajectoryLR.GetPosition(lineResolution));
		hitPosition.y = 0.01f;
		trajectoryHit.transform.position = hitPosition;
	}

	Vector3[] CalculateArcArray() //create array of Vector3 positions for arc
	{
		Vector3[] arcArray = new Vector3[lineResolution + 1];
		throwRadianAngle = Mathf.Deg2Rad * throwAngle;
		//float maxDistance = (throwPower * throwPower * Mathf.Sin(2 * throwRadianAngle)) / gravity;
		float endTime = ((-throwPower * Mathf.Sin(throwRadianAngle)) - Mathf.Sqrt((throwPower * Mathf.Sin(throwRadianAngle)) * (throwPower * Mathf.Sin(throwRadianAngle)) - (4 * (-gravity/2) * ball.transform.position.y))) / -gravity;
		float endDistance = throwPower * endTime * Mathf.Cos(throwRadianAngle);

		for (int i = 0; i <= lineResolution; i++)
		{
			float time = ((float) i / (float) lineResolution);
			arcArray[i] = CalculateArcPoint(time, endDistance);
		}

		return arcArray;
	}

	Vector3 CalculateArcPoint(float time, float endDistance)
	{
		float x = time * endDistance;
		float y = x * Mathf.Tan(throwRadianAngle) - ((gravity * x * x) / (2 * throwPower * throwPower * Mathf.Cos(throwRadianAngle) * Mathf.Cos(throwRadianAngle)));
		return new Vector3(x, y);
	}
}
