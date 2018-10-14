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
	public GameObject partner;
	private PartnerController partnerScript;
	private int THROW = 1;
	private int CATCH = 2;
	
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

	public GameObject ball;
	private Rigidbody ballRbody;
	public bool hasBall;
	public bool isChargingThrow;
	public float endTime;
	public float airTime;
	private float throwPower;
	public float throwAngle;
	private float throwRadianAngle;
	public float maxPower;
	public float powerGrowthRate;

	public GameObject throwTrajectory;
	private LineRenderer trajectoryLR;
	public int lineResolution;
	public GameObject trajectoryHit;
	public float trajectoryAlphaCutoff;
	
	
	// Use this for initialization
	void Start ()
	{
		gameStateScript = GameState.GetComponent<GameController>();
		partnerScript = partner.GetComponent<PartnerController>();
		myRbody = GetComponent<Rigidbody>();
		ballRbody = ball.GetComponent<Rigidbody>();
		trajectoryLR = throwTrajectory.GetComponent<LineRenderer>();
		
		cam = Camera.main;
		FLOOR_LAYERMASK = 1 << 9;

		gravity = Mathf.Abs(Physics.gravity.y);
	}

	void Update()
	{
		MouseToWorldPosition();
		PlayerMovement();
		ThrowBall();
		if (gameStateScript.lastHeldBall == 1)
		{
		ReduceTrajectory();
        SetTrajectoryGradient();	
		}
		CheckBallGrounded();

		//Debug.DrawRay(ball.transform.position, ball.transform.forward * 100, Color.white);
	}

	void FixedUpdate()
	{
		if (isChargingThrow)
		{
			moveSpeed = moveSpeed / 2;
		}
		myRbody.velocity = (inputVector * moveSpeed); //change player velocity according to inputs
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject == ball && gameStateScript.gameState == THROW) //when player collides with ball
		{
			if (gameStateScript.ballInAir)
			{
				gameStateScript.successCatches += 1;
			}
			
			hasBall = true;
			partnerScript.isPatrolling = true;
			gameStateScript.ballInAir = false;
			gameStateScript.ballHeld = 1;
			gameStateScript.lastHeldBall = 1;
			ball.transform.parent = this.gameObject.transform; //attach ball to player, remove physics
			ballRbody.isKinematic = true;
			ball.transform.localPosition = Vector3.forward * (1.1f * ((transform.localScale.x/2) + 0.5f) * ball.transform.localScale.x);
			ball.transform.eulerAngles = Vector3.zero;

			trajectoryLR.positionCount = 0;
		}
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

	void ThrowBall()
	{
		if (hasBall && Input.GetMouseButtonDown(0) && !isChargingThrow) //on mouse down
		{
			isChargingThrow = true;
			throwPower = 0f;
			airTime = 0;
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

			throwTrajectory.transform.position = ball.transform.position; //place origin of trajectory at ball position
			throwTrajectory.transform.eulerAngles = this.transform.eulerAngles - new Vector3(0, 90, 0); //angle trajectory with facing of player/ball
			RenderArc();
		} 
		else if (hasBall && Input.GetMouseButtonUp(0) && isChargingThrow) //on mouse release
		{
			
			isChargingThrow = false;
			hasBall = false;
			ball.transform.parent = null;
			ballRbody.isKinematic = false;
			gameStateScript.ballHeld = 0;

			Vector3 throwAngleV3 = new Vector3(throwAngle * -1, transform.eulerAngles.y, 0); //Vector3 including throwAngle and playerRotation
			ballRbody.transform.eulerAngles = throwAngleV3; //rotate ball based on throwAngle
			ballRbody.velocity = ball.transform.forward * throwPower;

			gameStateScript.ballInAir = true;
			gameStateScript.gameState = CATCH;
		}
	}

	void ReduceTrajectory()
	{
		if (gameStateScript.ballInAir && airTime < endTime)
		{
			float maxX = trajectoryLR.GetPosition(trajectoryLR.positionCount - 1).x; //find x value of last point in trajectory
			float currentX = airTime / endTime * maxX;	//find x value of ball based on predicted endTime and current airTime

			if (trajectoryLR.GetPosition(0).x < currentX) //if x value of first point in trajectory is less than currentX, remove it
			{
				Vector3[] newArcArray = new Vector3[trajectoryLR.positionCount - 1]; //populate new array with all elements besides first
				for (int i = 0; i < newArcArray.Length; i++)
				{
					newArcArray[i] = trajectoryLR.GetPosition(i + 1);
				}

				trajectoryLR.positionCount -= 1;
				trajectoryLR.SetPositions(newArcArray); //set points of lineRenderer to new array
			}
			airTime += Time.deltaTime;
		}
	}

	void SetTrajectoryGradient() //set gradient of trajectory based on size and ball position
	{
		Gradient gradient = new Gradient();
		GradientColorKey[] colorKeys = new GradientColorKey[2];
		colorKeys[0].color = Color.white;
		colorKeys[0].time = 0.0f;
		colorKeys[1].color = Color.white;
		colorKeys[1].time = 1.0f;
		GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
		alphaKeys[0].alpha = 1.0f;
		alphaKeys[0].time = airTime/endTime;
		if (airTime + trajectoryAlphaCutoff <= endTime)
		{
			alphaKeys[1].alpha = 0.0f;
			alphaKeys[1].time = airTime + trajectoryAlphaCutoff / endTime;
		} else if (airTime + trajectoryAlphaCutoff > endTime)
		{
			alphaKeys[1].alpha = airTime + trajectoryAlphaCutoff - endTime / trajectoryAlphaCutoff;
            alphaKeys[1].time = 1.0f;
		}

		gradient.SetKeys(colorKeys,alphaKeys);
		trajectoryLR.colorGradient = gradient;
	}

	void CheckBallGrounded()
	{
		if (gameStateScript.ballGrounded) //if ball is grounded, disable trajectory and hit marker
		{
			trajectoryLR.enabled = false;
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
		trajectoryHit.transform.position = hitPosition; //set hit marker position to end of trajectory
	}

	Vector3[] CalculateArcArray() //create array of Vector3 positions for arc
	{
		Vector3[] arcArray = new Vector3[lineResolution + 1]; //declare Vector3 array for lineRenderer points
		throwRadianAngle = Mathf.Deg2Rad * throwAngle; //convert angle to radians for trigonometric functions
		//float maxDistance = (throwPower * throwPower * Mathf.Sin(2 * throwRadianAngle)) / gravity; //calculates distance of throw if starting y and ending y are both 0
		endTime = ((-throwPower * Mathf.Sin(throwRadianAngle)) - Mathf.Sqrt((throwPower * Mathf.Sin(throwRadianAngle))  //finds change in time between origin of ball (y-intercept) and positive x-intercept.
		          * (throwPower * Mathf.Sin(throwRadianAngle)) - (4 * (-gravity/2) * ball.transform.position.y))) / -gravity; //uses trajectory formula combined with quadratic equation. assumes no air resistance
		float endDistance = throwPower * endTime * Mathf.Cos(throwRadianAngle); //finds x value/horizontal distance travelled in endTime;

		for (int i = 0; i <= lineResolution; i++)
		{
			float time = ((float) i / (float) lineResolution);
			arcArray[i] = CalculateArcPoint(time, endDistance); //populate arcArray with equally horizontally spaced points 
		}
		return arcArray;
	}

	Vector3 CalculateArcPoint(float time, float endDistance)
	{
		float x = time * endDistance;
		float y = x * Mathf.Tan(throwRadianAngle) - ((gravity * x * x) / (2 * throwPower * throwPower * Mathf.Cos(throwRadianAngle) * Mathf.Cos(throwRadianAngle))); //find y value for each point based on x
		return new Vector3(x, y);
	}
}
