using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartnerController : MonoBehaviour {

	public GameObject GameState;
	private GameController gameStateScript;
	private int THROW = 1;
	private int CATCH = 2;

	private float moveSpeed = 10f;
	private Rigidbody myRbody;
	
	public GameObject ball;
	private Rigidbody ballRbody;
	private bool hasBall;
	
	// Use this for initialization
	void Start () {
		gameStateScript = GameState.GetComponent<GameController>();
		myRbody = GetComponent<Rigidbody>();
		ballRbody = ball.GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		if (gameStateScript.gameState == CATCH && !hasBall && gameStateScript.ballGrounded && !gameStateScript.ballInAir)
		{
			Vector3 ballPosition = ball.transform.position;
			ballPosition.y = transform.position.y;
			transform.LookAt(ballPosition);
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
			gameStateScript.ballHeld = 2;
			ball.transform.parent = this.gameObject.transform;
			ballRbody.isKinematic = true;
			ball.transform.localPosition = Vector3.forward * 1.1f;
			ball.transform.eulerAngles = Vector3.zero;
		}
	}

	void FixedUpdate()
	{
		if(gameStateScript.gameState == CATCH && !hasBall && gameStateScript.ballGrounded && !gameStateScript.ballInAir)
		{
			myRbody.velocity = transform.forward * moveSpeed;
		}
	}
}
