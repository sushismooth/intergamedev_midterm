using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	public int gameState;
	int THROW = 1;
	int CATCH = 2;
	public int successThrows = 0;
	public int successCatches = 0;

	public GameObject ball;
	public bool ballInAir;
	public bool ballGrounded;
	public int ballHeld; //0 = none; 1 = player, 2 = partner
	public int lastHeldBall; //1 = player, 2 = partner

	public GameObject successText;
	
	// Use this for initialization
	void Start ()
	{
		gameState = THROW;
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log(gameState);
		
		Ray groundedRay = new Ray(ball.transform.position, Vector3.down);
		float groundedRayDistance = 0.6f;
		
		Debug.DrawRay(groundedRay.origin, groundedRay.direction * groundedRayDistance, Color.yellow);
		if (Physics.Raycast(groundedRay, groundedRayDistance))
		{
			ballGrounded = true;
			ballInAir = false;
		}
		else
		{
			ballGrounded = false;
		}
		
		successText.GetComponent<Text>().text = "Successful Throws: " + successThrows + "\n" + "Successful Catches: " +
			successCatches;

	}
}
