using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveOnStartup : MonoBehaviour
{

    // Start is called before the first frame update
    void Awake()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
