using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;

public class LogRecorder : MonoBehaviour
{

    public string txtDocumentName;

    [Space]

    public PuppetController p1;
    public PuppetController p2;
    public GameStateManager gamestate;

    [Space]

    public float positionTimer = 1f;
    float timer;
    int trueTimer;
    float levelTimer;
    float framerate;

    [Space]

    public bool ignore;

    [Space]

    public Image imgIcon;

    // Start is called before the first frame update
    void Start()
    {

        Directory.CreateDirectory(Application.streamingAssetsPath + "/Logs/");
        txtDocumentName = Application.streamingAssetsPath + "/Logs/" + (System.DateTime.Now + "").Replace(" ", "_").Replace(":", ";").Replace("/", "-") + ".txt";
        UpdateRecord();

    }

    void UpdateRecord() {
        if (PlayerPrefs.GetInt("Record") > 0)
        {
            ignore = false;
            imgIcon.gameObject.SetActive(true);
        }
        else
        {
            ignore = true;
            imgIcon.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timer = timer + Time.fixedDeltaTime;
        if (timer >= positionTimer)
        {
            timer = 0;
            Tick(p1.transform.position, p1.rb.velocity, true);
            Tick(p2.transform.position, p2.rb.velocity, false);
        }
        trueTimer = trueTimer + 1;
        levelTimer += Time.fixedDeltaTime;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            Debug.LogError("Record Key Hit");

            if (PlayerPrefs.GetInt("Record") > 0)
            {
                PlayerPrefs.SetInt("Record", -10);
            }
            else
            {
                PlayerPrefs.SetInt("Record", 10);
            }

            UpdateRecord();

        }

        framerate = (1.0f / Time.deltaTime);
    }

    public void Death(Vector3 position, Vector3 velocity, bool p1)
    {

        if (ignore != true) {
            if (!File.Exists(txtDocumentName))
            {
                File.WriteAllText(txtDocumentName, "Start at '" + System.DateTime.Now + "'\n");
            }

            File.AppendAllText(txtDocumentName, "Death\t" + TabbedVector(position) + "\t" + TabbedVector(velocity) + "\t" + p1 + "\t" + gamestate._currentLevel.name + "\t" + trueTimer + "\t" + levelTimer + "\n");
        }

    }

    public void Tick(Vector3 position, Vector3 velocity, bool p1)
    {

        if (ignore != true) {
            if (!File.Exists(txtDocumentName))
            {
                File.WriteAllText(txtDocumentName, "Start at '" + System.DateTime.Now + "'\n");
            }

            File.AppendAllText(txtDocumentName, "Tick\t" + TabbedVector(position) + "\t" + TabbedVector(velocity) + "\t" + p1 + "\t" + gamestate._currentLevel.name + "\t" + trueTimer + "\t" + levelTimer + "\t" + framerate + "\n");
        }

    }

    public void Complete(string level)
    {

        if (ignore != true) {
            if (!File.Exists(txtDocumentName))
            {
                File.WriteAllText(txtDocumentName, "Start at '" + System.DateTime.Now + "'\n");
            }

            File.AppendAllText(txtDocumentName, "Level\t" + level + "\tComplete\t" + System.DateTime.Now + "\t" + levelTimer + "\n");

            levelTimer = 0;
        }

    }

    public string TabbedVector(Vector3 vector) {
        return vector.x + "\t" + vector.y + "\t" + vector.z + "\t";
    }
}
