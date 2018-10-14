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
	public int currentLevel = 1;
	private int maxLevels = 5;
	public int successesPerLevel;

	public GameObject player;
	public PlayerController playerScript;
	public GameObject partner;
	public PartnerController partnerScript;
	private Vector3 playerStartPos = new Vector3(0f, 1.1f, 15f);
	private Vector3 ballStartPos = new Vector3(0f, 1f, 17.5f);
	
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
		playerScript = player.GetComponent<PlayerController>();
		partnerScript = partner.GetComponent<PartnerController>();
		
		NewLevel();
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log(gameState);
		
		Ray groundedRay = new Ray(ball.transform.position, Vector3.down);
		float groundedRayDistance = 0.55f;
		
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
		
		successText.GetComponent<Text>().text = "Current Level: " + currentLevel + "\n" + "Successful Throws: " + successThrows + "\n" + "Successful Catches: " +
			successCatches;
		
		CheckLevelProgress();
	}

	void CheckLevelProgress()
	{
		if (successThrows >= successesPerLevel && successCatches >= successesPerLevel)
		{
			successThrows = 0;
			successCatches = 0;
			currentLevel++;
			if (currentLevel > maxLevels)
			{
				EndGame();
				return;
			}
			NewLevel();
		}
	}
	
	void NewLevel()
	{
		gameState = THROW;
		
		float playerScale = 0.5f + (currentLevel * (0.5f / maxLevels));
		float partnerScale = 0.7f + (currentLevel * (0.3f / maxLevels));
		
		player.transform.localScale = new Vector3(playerScale, playerScale, playerScale);
		player.transform.position = playerStartPos;
		playerScript.hasBall = false;
		playerScript.maxMoveSpeed = 5 + (currentLevel * 1);
		playerScript.maxPower = 8 + (currentLevel * 2);
		
		partner.transform.localScale = new Vector3(partnerScale, partnerScale, partnerScale);
		partner.transform.position = new Vector3(0f, 1.1f, 16.5f + (currentLevel * 7f));
		partnerScript.hasBall = false;
		partnerScript.isPatrolling = false;
		partnerScript.moveSpeed = 3 + (currentLevel * 1);
		partnerScript.minZ = 13f + (currentLevel * 7f);
		partnerScript.maxZ = 20f + (currentLevel * 7f);
		
		ball.transform.position = ballStartPos;
		ball.transform.parent = null;
		ball.GetComponent<Rigidbody>().isKinematic = false;
		ballHeld = 0;

	}

	void EndGame()
	{
		
	}
}
