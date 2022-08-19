using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WriteTakes : MonoBehaviour
{
    public TextMeshProUGUI takes, actScene;
    public GameStateManager gameManager;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameStateManager>();
    }

    private void Update()
    {
        takes.text = (gameManager.GetLevelAttempts() + 1).ToString();
        actScene.text = gameManager.GetCurrentAct() + "\n" + gameManager.GetCurrentLevel();
    }
}
