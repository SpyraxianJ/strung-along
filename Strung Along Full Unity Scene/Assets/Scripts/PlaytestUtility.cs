using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaytestUtility : MonoBehaviour
{

    PlayerControls controls;
    public string restartScene;

    // Start is called before the first frame update
    void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Restart.performed += ctx =>
        {
            SceneManager.LoadScene(restartScene);
        };
    }

    void OnEnable()
    {
        controls.Player.Enable();
    }
    void OnDisable()
    {
        controls.Player.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        // piss code
        if (Input.GetKey(KeyCode.P) && Input.GetKey(KeyCode.I) && Input.GetKey(KeyCode.S))
        {
            System.Diagnostics.Process.Start(Application.dataPath.Replace("_Data", ".exe"));
            Application.Quit();
        }
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
