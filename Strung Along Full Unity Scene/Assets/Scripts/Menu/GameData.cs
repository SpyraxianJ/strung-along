using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]

public class GameData
{
    public float SavedTimer;

    public GameData(Game game)
    {
        SavedTimer = game.SavedTimer;
    }
}
