using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]

public class GameData
{
    public float SavedTimer;
    public int SavedLevel;
    public int SavedAct;

    public GameData(Game game)
    {
        SavedTimer = game.SavedTimer;
        SavedLevel = game.SavedLevel;
        SavedAct = game.SavedAct;
    }
}
