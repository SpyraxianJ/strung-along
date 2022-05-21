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

    public GameData(float time, int level, int act)
    {
        SavedTimer = time;
        SavedLevel = level;
        SavedAct = act;
    }
}
