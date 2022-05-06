using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Game : MonoBehaviour
{
    public float SavedTimer = 0.0f;

    public void SaveGame()
    {
        SavedTimer = Timer.GetTimer();
        SaveSystem.SaveData(this);
    }

    public void LoadGame()
    {
        if(SaveSystem.loadData() != null)
        {
            GameData data = SaveSystem.loadData();
            SavedTimer = data.SavedTimer;
        }
    }
}
