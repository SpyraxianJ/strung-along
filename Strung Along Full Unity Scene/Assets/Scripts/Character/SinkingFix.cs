using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkingFix : MonoBehaviour
{

    public float height;
    public bool dont;

    // I hate this script but I have no idea why the character are sinking, so uhhh, here it is

    // Start is called before the first frame update
    void Start()
    {
        height = transform.localPosition.y;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (dont == false)
            transform.localPosition = new Vector3(transform.localPosition.x, height, transform.localPosition.z);
    }
    void FixedUpdate()
    {
        if (dont == false)
            transform.localPosition = new Vector3(transform.localPosition.x, height, transform.localPosition.z);
    }
}
