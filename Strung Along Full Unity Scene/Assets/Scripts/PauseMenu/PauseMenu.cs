using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    PlayerControls controls;
    bool pausePressed;
    bool gamePaused = false;
    public GameObject pauseMenuUI;
    // Update is called once per frame

    private void Awake()
    {
        controls = new PlayerControls();
        pauseMenuUI.SetActive(false);

        controls.UI.Pause.performed += ctx =>
        {
            if (!gamePaused)
            {
                Pause();
            }
            else
            {
                Resume();
            }
            Debug.Log(pausePressed);
        };
    }

    private void Start()
    {
        
    }

    void OnEnable()
    {
        controls.UI.Enable();
    }
    void OnDisable()
    {
        controls.UI.Disable();
    }

    void Update()
    {

    }

    public void Resume(){
        pauseMenuUI.SetActive(false);
        //time back to normal;
        Time.timeScale = 1f;
        gamePaused = false;
    }

    void Pause(){
        pauseMenuUI.SetActive(true);
        //time freezed
        Time.timeScale = 0f;
        gamePaused = true;
    }
}
