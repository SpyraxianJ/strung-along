using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandIKHandler : MonoBehaviour
{

    [Header("Ik Variables")]

    public float lerpSpeed;

    [Space]

    [Header("Refernces")]

    public Transform leftHand;
    public Transform rightHand;

    [Space]

    public Animator animator;

    [Space]

    [Header("Controls")]

    public bool IKLeft;
    public bool IKRight;

    [Space]

    public float LeftIKCurrentWeight;
    public float RightIKCurrentWeight;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnAnimatorIK()
    {
        if (IKLeft)
        {
            LeftIKCurrentWeight = Mathf.Lerp(LeftIKCurrentWeight, 1, lerpSpeed * Time.deltaTime);
        }
        else 
        {
            LeftIKCurrentWeight = Mathf.Lerp(LeftIKCurrentWeight, 0, lerpSpeed * Time.deltaTime);
        }

        if (IKRight)
        {
            RightIKCurrentWeight = Mathf.Lerp(RightIKCurrentWeight, 1, lerpSpeed * Time.deltaTime);
        }
        else
        {
            RightIKCurrentWeight = Mathf.Lerp(RightIKCurrentWeight, 0, lerpSpeed * Time.deltaTime);
        }

        animator.SetIKPosition(AvatarIKGoal.RightHand, rightHand.position);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHand.position);
        animator.SetIKRotation(AvatarIKGoal.RightHand, rightHand.rotation);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHand.rotation);

        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, RightIKCurrentWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, RightIKCurrentWeight);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, LeftIKCurrentWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, LeftIKCurrentWeight);


    }
}
