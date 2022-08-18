using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoMove : MonoBehaviour
{
    private bool flicker1, flicker2, lightning;
    public float moveVelocity, lightIntensity = 7f, lightningIntensity = 0f;
    public Vector3 targetPos;
    public Light stageOne, stageTwo, lightningLight;
    public float randTime;
    public AudioSource thunderAudioPlayer;
    public AudioClip thunderSound;

    private void Start()
    {
        Invoke("CallLightning", 6f);
        Invoke("StopLightning", 6.2f);
        //InvokeRepeating("CallLightning", 6f, 6f);
        //InvokeRepeating("StopLightning", 6.2f, 6f);
    }

    private void Update()
    {
        //move the title logo into position
        transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * moveVelocity);
        int timeVar = (int)Mathf.Round(Time.time * 10.0f);

        if (timeVar % 10 == 0)
            ToggleFlicker(1);
        if (timeVar % 8 == 0)
            ToggleFlicker(2);

        //make the stage lights flicker
        if (flicker1)
        {
            if (timeVar % 3 == 0)
                stageOne.intensity = Random.Range(0f, lightIntensity);
        }
        else
            stageOne.intensity = lightIntensity;
        if (flicker2)
        {
            if (timeVar % 4 == 0)
                stageTwo.intensity = Random.Range(0f, lightIntensity);
        }
        else
            stageTwo.intensity = lightIntensity;

        //turn off lightning
        if (lightning)
        {
            if (timeVar % 1 == 0)
                lightningLight.intensity = Random.Range(0f, 10f);
        }
        else
                lightningLight.intensity = lightningIntensity;
    }

    public void ToggleFlicker(int num)
    {
        if (num == 1)
            flicker1 = !flicker1;
        else if (num == 2)
            flicker2 = !flicker2;
    }

    public void CallLightning()
    {
        lightning = true;
        randTime = Random.Range(6f, 10f);
        thunderAudioPlayer.PlayOneShot(thunderSound);
        Invoke("CallLightning", randTime);
    }
    public void StopLightning()
    {
        lightning = false;
        Invoke("StopLightning", randTime);
    }
}
