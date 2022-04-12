using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringManager : MonoBehaviour
{

    [Header("References")]

    public StringRoot leftString;
    public StringRoot rightString;

    [Space]

    public int tangleCount;
    [Range(1, 10)]
    public int maxTangles = 1;

    // Don't like this variable name but it's what I had in my prototype and I can't think of a better one rn, feel free to change
    public bool rightIsRight;

    // Start is called before the first frame update
    void Start()
    {
        leftString.manager = this;
        rightString.manager = this;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (leftString.connectedObject.transform.position.x < rightString.connectedObject.transform.position.x)
        {
            if (rightIsRight)
            {
                // expected
            }
            else
            {
                if (leftString.connectedObject.transform.position.z < rightString.connectedObject.transform.position.z)
                {
                    rightIsRight = true;
                    tangleCount = tangleCount - 1;
                    Debug.Log("tangle -1 d");
                }
                else
                {
                    rightIsRight = true;
                    tangleCount = tangleCount + 1;
                    Debug.Log("tangle +1 c");
                }
            }
        }
        else
        {
            if (rightIsRight == false)
            {
                // expected
            }
            else
            {
                if (leftString.connectedObject.transform.position.z < rightString.connectedObject.transform.position.z)
                {
                    rightIsRight = false;
                    tangleCount = tangleCount + 1;
                    Debug.Log("tangle +1 a");
                }
                else
                {
                    rightIsRight = false;
                    tangleCount = tangleCount - 1;
                    Debug.Log("tangle -1 b");
                }
            }
        }

        if (tangleCount > maxTangles) {
            tangleCount = maxTangles;
        }
        if (tangleCount < -maxTangles) {
            tangleCount = -maxTangles;
        }

    }
}
