using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour {
    
    public GameObject fade;
    private bool inTransition = false;
    public float fadeAlpha = 0;
    public float transitionTime;

    void Update()
    {
        if (inTransition)
        {
            if (fadeAlpha >= 255)
            {
                SceneManager.LoadScene("GameScene");
            }
            fadeAlpha += (255 / transitionTime) * Time.deltaTime;
            float fadeAlphaNormalized = fadeAlpha / 255;
            fade.GetComponent<Image>().color = new Color(0,0,0,fadeAlphaNormalized);
            Debug.Log(fadeAlpha);
        }
    }
    
    private void OnTriggerEnter(Collider collisionInfo)
    {
        inTransition = true;
    }
}
