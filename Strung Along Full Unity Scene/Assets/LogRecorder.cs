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

    [Space]

    public float positionTimer = 1f;
    float timer;
    int trueTimer;

    [Space]

    public bool ignore;

    [Space]

    public Image imgIcon;

    // Start is called before the first frame update
    void Start()
    {

        Directory.CreateDirectory(Application.streamingAssetsPath + "/Logs/");
        txtDocumentName = Application.streamingAssetsPath + "/Logs/" + (System.DateTime.Now + "").Replace(" ", "_").Replace(":", ";").Replace("/", "-") + ".xls";
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
    }

    public void Death(Vector3 position, Vector3 velocity, bool p1)
    {

        if (ignore != true) {
            if (!File.Exists(txtDocumentName))
            {
                File.WriteAllText(txtDocumentName, "Start at '" + System.DateTime.Now + "'\n");
            }

            File.AppendAllText(txtDocumentName, "Death\t" + TabbedVector(position) + "\t" + TabbedVector(velocity) + "\t" + p1 + "\t" + trueTimer + "\n");
        }

    }

    public void Tick(Vector3 position, Vector3 velocity, bool p1)
    {

        if (ignore != true) {
            if (!File.Exists(txtDocumentName))
            {
                File.WriteAllText(txtDocumentName, "Start at '" + System.DateTime.Now + "'\n");
            }

            File.AppendAllText(txtDocumentName, "Tick\t" + TabbedVector(position) + "\t" + TabbedVector(velocity) + "\t" + p1 + "\t" + trueTimer + "\n");
        }

    }

    public void Complete(string level)
    {

        if (ignore != true) {
            if (!File.Exists(txtDocumentName))
            {
                File.WriteAllText(txtDocumentName, "Start at '" + System.DateTime.Now + "'\n");
            }

            File.AppendAllText(txtDocumentName, "Level\t" + level + "Complete\t" + System.DateTime.Now + "\n");
        }

    }

    public string TabbedVector(Vector3 vector) {
        return vector.x + "\t" + vector.y + "\t" + vector.z + "\t";
    }
}
