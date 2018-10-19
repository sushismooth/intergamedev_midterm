using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionsButton : MonoBehaviour
{
	public GameObject instructionsText;
	public GameObject instructionsBG;

	void Start()
	{
		instructionsText.SetActive(false);
		instructionsBG.SetActive(false);
	}

	private void OnTriggerEnter(Collider collisionInfo)
	{
		instructionsText.SetActive(true);
		instructionsBG.SetActive(true);
	}

	private void OnTriggerExit(Collider collisionInfo)
	{
		instructionsText.SetActive(false);
		instructionsBG.SetActive(false);
	}
}
