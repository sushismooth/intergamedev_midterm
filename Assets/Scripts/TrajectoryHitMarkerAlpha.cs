using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//adjusts alpha of hit marker material based on distance from ball
//closer = more visible, further = more transparent
public class TrajectoryHitMarkerAlpha : MonoBehaviour
{
	public GameObject GameState;
	private GameController gameStateScript;

	public GameObject player;
	private PlayerController playerScript;

	public GameObject partner;
	private PartnerController partnerScript;
	
	public Transform ball;
	private SpriteRenderer mySpriteRenderer;
	private Material myMaterial;
	private float alpha;
	public float maxRenderDistance;
	public float maxDistanceForFullAlpha;
	private bool timeout;
	public float timeoutDuration;
	private float timeoutTimer;

	void Awake()
	{
		mySpriteRenderer = GetComponent<SpriteRenderer>();
		myMaterial = mySpriteRenderer.material;
		gameStateScript = GameState.GetComponent<GameController>();
		playerScript = player.GetComponent<PlayerController>();
		partnerScript = partner.GetComponent<PartnerController>();
		timeoutTimer = timeoutDuration;
	}
	void Update ()
	{
		if ((gameStateScript.ballGrounded || (gameStateScript.ballHeld > 0 && !playerScript.isChargingThrow && !partnerScript.isChargingThrow)) && mySpriteRenderer.enabled)
		{
			timeout = true;
		}

		if (timeout){
			timeoutTimer -= Time.deltaTime;
			alpha = timeoutTimer / timeoutDuration;
			if (timeoutTimer <= 0)
			{
				mySpriteRenderer.enabled = false;
				timeoutTimer = timeoutDuration;
				timeout = false;
			}
		}
		else
		{
			alpha = Vector3.Distance(transform.position, ball.position) - maxDistanceForFullAlpha;
			if (alpha > maxRenderDistance)
			{
				alpha = maxRenderDistance;
			}
			else if (alpha < 0)
			{
				alpha = 0;
			}
			alpha = 1 - alpha / maxRenderDistance;
		}
		myMaterial.color = new Color(1,1,1,alpha);
	}
}
