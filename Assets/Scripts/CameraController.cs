using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

	public GameObject player;
	public GameObject cameraOrbit;
	private Vector3 position;

	private float targetCamDistance;
	private float currentCamDistance = -5;

	void Start()
	{
		targetCamDistance = currentCamDistance;
	}
	
	// Update is called once per frame
	void Update()
	{
		//position = player.transform.position + new Vector3(0f, 0f, -8f);
		//position.y = 10f;
		//transform.position = position;

		float horizontal = Input.GetAxis("Mouse X");

		Vector3 cameraOrbitPos = player.transform.position;
		cameraOrbitPos.y = 10;
		cameraOrbit.transform.position = cameraOrbitPos;

		//horizontal = Input.GetAxis("Horizontal");
		
		if (Input.GetMouseButton(1))
		{
			cameraOrbit.transform.eulerAngles += new Vector3(0, horizontal * -1.5f, 0);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
		Vector3 camPos = Camera.main.transform.position;
		
		if (mouseScroll > 0f && currentCamDistance < 8)
		{
			targetCamDistance += 1;
			currentCamDistance += 1;
		}
		else if (mouseScroll < 0f && currentCamDistance > -20)
		{
			targetCamDistance -= 1;
			currentCamDistance -= 1;
		}

		Camera.main.transform.position = Vector3.Lerp(camPos, camPos + (transform.forward * targetCamDistance), 0.25f);
		targetCamDistance = targetCamDistance * 3/4;
		//Debug.Log(currentCamDistance);
	}
}
