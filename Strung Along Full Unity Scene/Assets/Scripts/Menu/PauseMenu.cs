using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject PauseMenuUI, LevelSelectUI;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) ){

            if (GameIsPaused)
            {
                Resume();
                //Debug.Log("Game resumed at " + Timer.GetTimer());
            }
            else
            {
                Pause();
                //Debug.Log("Game paused at " + Timer.GetTimer());
            }
        }
    }

    public void Resume()
    {
        Time.timeScale = 1.0f;
        GameIsPaused = false;
        CloseLevelSelectUI();
		PauseMenuUI.SetActive(false);
    }

    public void Pause()
    {
        PauseMenuUI.SetActive(true);
        Time.timeScale = 0.0f;
        GameIsPaused = true;
    }
	
    public void OpenLevelSelectUI()
    {
        LevelSelectUI.SetActive(true);
    }

    public void CloseLevelSelectUI()
    {
        LevelSelectUI.SetActive(false);
    }
}
