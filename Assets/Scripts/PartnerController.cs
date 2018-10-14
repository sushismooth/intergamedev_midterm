using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PartnerController : MonoBehaviour {

	public GameObject GameState;
	private GameController gameStateScript;
	private int THROW = 1;
	private int CATCH = 2;

	public float moveSpeed = 10f;
	private Rigidbody myRbody;
	private float gravity;

	public GameObject player;
	private Vector3 playerPos;
	private Bounds playerBounds;
	
	public GameObject ball;
	private Rigidbody ballRbody;
	public bool hasBall;
	private float timeSincePickup;
	private float pickupWaitTime = 1f;

	public float minZ;
	public float maxZ;
	private int currentPatrolTarget = 1;
	private Vector3 patrolPos1;
	private Vector3 patrolPos2;
	public bool isPatrolling = false;
	private bool wasPatrolling = false;

	public bool isChargingThrow;
	private Vector3 throwTarget;
	private float throwDistance;
	private float throwPower;
	private float requiredPower;
	public float powerGrowthRate;
	private float throwAngle = 45f;
	private float throwRadianAngle;
	public float endTime;
	public float airTime;
	
	public GameObject throwTrajectory;
	private LineRenderer trajectoryLR;
	public int lineResolution;
	public GameObject trajectoryHit;
	public float trajectoryAlphaCutoff;
	
	// Use this for initialization
	void Start () {
		gameStateScript = GameState.GetComponent<GameController>();
		myRbody = GetComponent<Rigidbody>();
		ballRbody = ball.GetComponent<Rigidbody>();
		trajectoryLR = throwTrajectory.GetComponent<LineRenderer>();
		playerBounds = new Bounds(new Vector3( 0f, 0.5f, 10f), new Vector3(50f, 2f, 20f));
		
		gravity = Math.Abs(Physics.gravity.y);
	}
	
	// Update is called once per frame
	void Update () {
		if (gameStateScript.gameState == CATCH && !hasBall && gameStateScript.ballGrounded && !gameStateScript.ballInAir)
		{
			Vector3 ballPosition = ball.transform.position;
			ballPosition.y = transform.position.y;
			transform.LookAt(ballPosition);
		}

		if (hasBall && !isChargingThrow)
		{
			if (timeSincePickup >= pickupWaitTime && transform.position.z > minZ && transform.position.z < maxZ)
			{
				ThrowBall();
			}
			else
			{
				if (transform.position.z < minZ)
				{
					transform.LookAt(new Vector3(transform.position.x, transform.position.y, 100f));
				}
				if (transform.position.z > maxZ)
				{
					transform.LookAt(new Vector3(transform.position.x, transform.position.y, -100f));
				}
			}
			timeSincePickup += Time.deltaTime;
		}

		if (isChargingThrow)
		{
			ChargeThrow();
		}
		
		if (gameStateScript.lastHeldBall == 2)
		{
			ReduceTrajectory();
			SetTrajectoryGradient();	
		}

		if (isPatrolling)
		{
			StartPatrol();
			CheckPatrolTarget();
			Debug.DrawLine(patrolPos1, patrolPos2, Color.yellow); //show patrol path
		}

		Debug.DrawLine(new Vector3(-25, 0.1f, minZ), new Vector3(25, 0.1f, minZ), Color.red); //show bounds
		Debug.DrawLine(new Vector3(-25, 0.1f, maxZ), new Vector3(25, 0.1f, maxZ), Color.red);

		wasPatrolling = isPatrolling;
	}

	void ThrowBall()
	{
		
		throwPower = 0;
		requiredPower = 0;
		airTime = 0;
		
		playerPos = player.transform.position;
		playerPos.y = 0;
		
		//find target
		float distanceFromPlayer = Vector3.Distance(transform.position, player.transform.position); //find distance between two character for randomPoint
		Vector2 randomPoint = Random.insideUnitCircle * (distanceFromPlayer/2);	
		throwTarget = playerPos + new Vector3( randomPoint.x, 0, randomPoint.y);
		int tries = 0;
		while (!playerBounds.Contains(throwTarget) && tries < 100)
		{
			randomPoint = Random.insideUnitCircle * (distanceFromPlayer/2);	
			throwTarget = playerPos + new Vector3( randomPoint.x, 0, randomPoint.y);
			tries++;
		}
		float distance = Vector2.Distance(new Vector2(ball.transform.position.x, ball.transform.position.z), new Vector2(throwTarget.x, throwTarget.z)); //calculate change in x
		
		
		//find required velocity
		throwRadianAngle = throwAngle * Mathf.Deg2Rad;
		requiredPower = 1 / Mathf.Cos(throwRadianAngle) * Mathf.Sqrt((0.5f * gravity * distance * distance / (distance * Mathf.Tan(throwRadianAngle) + ball.transform.position.y)));
		
		isChargingThrow = true;
		timeSincePickup = 0f;
	}
	
	void ChargeThrow()
	{
		if (throwPower < requiredPower)
		{
			throwPower += powerGrowthRate * Time.deltaTime;
			if (throwPower >= requiredPower)
			{
				throwPower = requiredPower;
				ReleaseThrow();
				isChargingThrow = false;
			}
		}
		Vector3 throwTargetLookAt = throwTarget;
		throwTargetLookAt.y = transform.position.y;
		transform.LookAt(throwTargetLookAt);
		
		throwTrajectory.transform.position = ball.transform.position;
		throwTrajectory.transform.eulerAngles = this.transform.eulerAngles - new Vector3(0, 90, 0);
		RenderArc();
	}

	void ReleaseThrow()
	{
		isChargingThrow = false;
		hasBall = false;
		ball.transform.parent = null;
		ballRbody.isKinematic = false;
		gameStateScript.ballHeld = 0;
		
		Vector3 throwAngleV3 = new Vector3(throwAngle * -1, transform.eulerAngles.y, 0);
		ballRbody.transform.eulerAngles = throwAngleV3;
		ballRbody.velocity = ball.transform.forward * throwPower;

		gameStateScript.ballInAir = true;
		gameStateScript.gameState = THROW;
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

	void StartPatrol()
	{
		if (isPatrolling && !wasPatrolling) //check if patrol just started
		{
			patrolPos1 = new Vector3(Random.Range(-20f, 0f), transform.position.y, Random.Range(minZ, maxZ));
			patrolPos2 = new Vector3(Random.Range(patrolPos1.x + 10, 20f), transform.position.y, Random.Range(minZ, maxZ));
		}
	}

	void CheckPatrolTarget()
	{
		if (transform.position.x < patrolPos1.x && currentPatrolTarget == 1)
		{
			currentPatrolTarget = 2;
		}
		else if (transform.position.x > patrolPos2.x && currentPatrolTarget == 2)
		{
			currentPatrolTarget = 1;
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject == ball && gameStateScript.gameState == CATCH)
		{
			if (gameStateScript.ballInAir)
			{
				gameStateScript.successThrows += 1;
			}
			hasBall = true;
			isPatrolling = false;
			gameStateScript.ballInAir = false;
			gameStateScript.ballHeld = 2;
			gameStateScript.lastHeldBall = 2;
			ball.transform.parent = this.gameObject.transform;
			ballRbody.isKinematic = true;
			ball.transform.localPosition = Vector3.forward * 1.1f * ball.transform.localScale.x;
			ball.transform.eulerAngles = Vector3.zero;
			
			trajectoryLR.positionCount = 0;
		}
	}

	void FixedUpdate()
	{
		if(gameStateScript.gameState == CATCH && !hasBall && gameStateScript.ballGrounded && !gameStateScript.ballInAir) //move to pickup ball
		{
			myRbody.velocity = transform.forward * moveSpeed;
		} 
		else if (gameStateScript.gameState == CATCH && hasBall && (transform.position.z > maxZ || transform.position.z < minZ) && !isChargingThrow)
		{
			myRbody.velocity = transform.forward * moveSpeed;
		} 
		else if (isPatrolling)
		{
			if (currentPatrolTarget == 1)
			{
				transform.LookAt(patrolPos1);
				myRbody.velocity = transform.forward * moveSpeed;
			}
			else if (currentPatrolTarget == 2)
			{
				transform.LookAt(patrolPos2);
				myRbody.velocity = transform.forward * moveSpeed;
			}
		}
	}
}
