using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGameListener : MonoBehaviour
{
    public string gameName = "StageScene";
    public Canvas pressAnyKeyCanvas;
    private bool fadeIn = false, fadeOut = false, menuDone = false;
    public Animator crossFade, musicCrossFade;

    private void Start()
    {
        Invoke("ShowUI", 6f);
    }

    public IEnumerator LaunchGame()
    {
        crossFade.SetTrigger("StartCrossFade");
        musicCrossFade.SetTrigger("StartCrossFade");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(gameName);
    }

    private void Update()
    {
        if (Input.anyKeyDown && menuDone)
            StartCoroutine(LaunchGame());

        if (fadeIn)
        {
            if (pressAnyKeyCanvas.GetComponent<CanvasGroup>().alpha < 1)
            {
                pressAnyKeyCanvas.GetComponent<CanvasGroup>().alpha += Time.deltaTime;
                if (pressAnyKeyCanvas.GetComponent<CanvasGroup>().alpha >= 1)
                {
                    fadeIn = false;
                    menuDone = true;
                }    
            }
        }
    }

    public void ShowUI()
    {
        fadeIn = true;
    }
	
}
