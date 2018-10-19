using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndFade : MonoBehaviour
{
	private float fadeAlpha = 255;
	public float fadeTime;
	private Image myImage;
	
	void Start ()
	{
		myImage = GetComponent<Image>();

		myImage.color = new Color(0, 0, 0, 1);
	}
	
	// Update is called once per frame
	void Update ()
	{
		fadeAlpha -= 255 / fadeTime * Time.deltaTime;
		float fadeAlphaNormalized = fadeAlpha / 255;
		myImage.color = new Color(0,0,0,fadeAlphaNormalized);
	}
}
