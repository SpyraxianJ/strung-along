using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]

public class GameData
{
    public int SavedLevel;
    public int SavedAct;
	public float SavedPlaytime;

    public GameData(int level, int act, float time)
    {
        SavedLevel = level;
        SavedAct = act;
		SavedPlaytime = time;
    }
}
