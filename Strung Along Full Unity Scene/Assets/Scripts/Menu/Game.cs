using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Game : MonoBehaviour
{
    public float SavedTimer = 0.0f;
    public int SavedLevel = 1;
    public int SavedAct = 1;

    public void SaveGame()
    {
        SavedTimer = LevelManager.getTime();
        SavedLevel = LevelManager.getCurrentLevel();
        SavedAct = LevelManager.getCurrentAct();
        SaveSystem.SaveData(this);
    }

    public void LoadGame()
    {
        if(SaveSystem.loadData() != null)
        {
            GameData data = SaveSystem.loadData();
            SavedTimer = data.SavedTimer;
            SavedLevel = data.SavedLevel;
            SavedAct = data.SavedAct;
        }
    }
}
