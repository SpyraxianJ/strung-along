using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CopyOpacity : MonoBehaviour
{

    // this is me being lazy sorry

    public Image thisImg;
    public Image follow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        thisImg.color = new Color(thisImg.color.r, thisImg.color.g, thisImg.color.b, follow.color.a);
    }
}
