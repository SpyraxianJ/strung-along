using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingIK : MonoBehaviour
{

    public HandIKHandler IK;
    public StringRoot root;
    public float distance = 0.8f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IK.IKLeft == false)
        {
            if (root.manager.tangle != 0)
            {
                IK.leftHand.position = Vector3.MoveTowards(root.connectedPoint.position, root.manager.effectiveRoot, distance);
            }
            else
            {
                IK.leftHand.position = Vector3.MoveTowards(root.connectedPoint.position, root.transform.position, distance);
            }
        }

        if (IK.IKRight == false)
        {
            if (root.manager.tangle != 0)
            {
                IK.rightHand.position = Vector3.MoveTowards(root.connectedPoint.position, root.manager.effectiveRoot, distance);
            }
            else
            {
                IK.rightHand.position = Vector3.MoveTowards(root.connectedPoint.position, root.transform.position, distance);
            }
        }
    }
}
