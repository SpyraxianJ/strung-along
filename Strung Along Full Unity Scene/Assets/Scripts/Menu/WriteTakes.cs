using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class WriteTakes : MonoBehaviour
{
    public TextMeshProUGUI takes, actScene, time;
    public GameStateManager gameManager;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameStateManager>();
    }

    private void Update()
    {
        takes.text = (gameManager.GetLevelAttempts() + 1).ToString();
        actScene.text = gameManager.GetCurrentAct() + "\n" + gameManager.GetCurrentLevel();
        TimeSpan timespan = TimeSpan.FromSeconds(gameManager.GetLevelPlaytime());
        time.text = timespan.Minutes + " M " + timespan.Seconds + " S";

    }
}
