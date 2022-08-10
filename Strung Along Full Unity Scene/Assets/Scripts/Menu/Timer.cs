using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;

public class Timer : MonoBehaviour
{
    private bool timerActive = false;
    private static float currentTime;
    private float lastSavedTime = 0;
    private float startTime = 0;
    public  TMP_Text currentTimeText;

    void Update()
    {
        GameData data = SaveSystem.LoadData();
        lastSavedTime = data.SavedPlaytime;

        TimerGoesOn();
    }

    public static float GetTimer()
    {
        return currentTime;
    }

    public void StartTimer()
    {
        timerActive = true;
    }

    public void StopTimer()
    {
        timerActive = false;
    }

    public void ResetTimer()
    {
        currentTime = startTime;
        timerActive = false;
    }

    public void LoadFromLastGame()
    {
        currentTime = lastSavedTime;
    }

    public void TimerGoesOn()
    {
        if (timerActive)
        {
            currentTime += Time.deltaTime;
        }

        TimeSpan time = TimeSpan.FromSeconds(currentTime);
        currentTimeText.text = time.Minutes + ":" + time.Seconds;
    }
}
