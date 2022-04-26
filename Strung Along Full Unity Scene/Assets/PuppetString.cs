using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppetString : MonoBehaviour
{

    public PuppetStringManager manager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        manager.bolConnected = true;
        manager.effectiveRoot = collision.GetContact(0).point;

        // on collsiion we need to determine if our rotation is clockwise or anti-clockwise so we know which say tangles, and which untangles
    }
}
