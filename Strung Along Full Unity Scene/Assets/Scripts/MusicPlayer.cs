using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource act1, act2, act3;
    public GameStateManager gameStateManager;

    // Update is called once per frame
    void Update()
    {
        switch (gameStateManager.GetCurrentAct())
        {
            case 1:
                act1.gameObject.SetActive(true);
                act2.gameObject.SetActive(false);
                act3.gameObject.SetActive(false);
                break;
            case 2:
                act1.gameObject.SetActive(false);
                act2.gameObject.SetActive(true);
                act3.gameObject.SetActive(false);
                break;
            case 3:
                act1.gameObject.SetActive(false);
                act2.gameObject.SetActive(false);
                act3.gameObject.SetActive(true);
                break;
            default:
                act1.gameObject.SetActive(false);
                act2.gameObject.SetActive(false);
                act3.gameObject.SetActive(false);
                break;
        }
    }
}
