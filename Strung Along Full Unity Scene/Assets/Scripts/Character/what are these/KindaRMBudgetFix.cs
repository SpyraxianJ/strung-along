using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KindaRMBudgetFix : MonoBehaviour
{

    public Vector3 value;
    public float timer;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.localPosition = Vector3.zero; // i'm crying
    }
}
