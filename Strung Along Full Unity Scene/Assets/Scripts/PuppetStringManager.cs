using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppetStringManager : MonoBehaviour
{

    [Header("References")]

    public GameObject puppet1;
    public GameObject puppet2;
    public GameObject stringRoot1;
    public GameObject stringRoot2;
    public PuppetString string1Ref;
    public PuppetString string2Ref;

    [Space]

    public GameObject debug;
    public GameObject debug2;

    [Space]

    [Header("State")]

    [Tooltip("This is how tangled the strings currently are, public for exposed debug purposes")]
    public float fltTangle;
    public bool bolConnected;
    public Vector3 effectiveRoot;

    [Space]

    public Vector3 puppet1LastFrame;
    public Vector3 puppet2LastFrame;

    [Space]

    [Header("Variables")]

    public float lerpToEffectiveRootSpeed;
    [Range(1, 2)]
    public float puppetDistanceEffectiveRootFactor = 1.5f;
    [Range(0, 1)]
    public float effectiveRootPuppetPositionInfluence;


    // Start is called before the first frame update
    void Start()
    {
        InitialiseStrings();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (bolConnected == true) {

            Vector3 targetEffectiveRoot = Vector3.Lerp((stringRoot1.transform.position + stringRoot2.transform.position)/2, (puppet1.transform.position + puppet2.transform.position)/2, effectiveRootPuppetPositionInfluence);
            targetEffectiveRoot = 
                new Vector3(targetEffectiveRoot.x, 
                Mathf.Lerp(Mathf.Max(puppet1.transform.position.y, puppet2.transform.position.y), Mathf.Min(stringRoot1.transform.position.y, stringRoot2.transform.position.y), -Mathf.Pow(puppetDistanceEffectiveRootFactor, -Vector3.Distance(puppet1.transform.position, puppet2.transform.position)) + 1), 
                targetEffectiveRoot.z);

            // end of the mathf.Lerp line uses -A^{-x}+1 where A = puppetDistanceEffectiveRootFactor, put it in https://www.desmos.com/calculator to see it
            // It's used as a budget way of simulating the phenomena of how the string effective root gets lower the closer the two objects or puppets are to each other
            // Much of this was made using observations I found from playing with strings a lot, some of it is probably wrong, but it's the best guess I have

            // hight is lerped between square root of the two puppet's distance apart, further out, the lower it gets
            effectiveRoot = Vector3.Lerp(effectiveRoot, targetEffectiveRoot, lerpToEffectiveRootSpeed * Time.fixedDeltaTime);

        }

        StringTick(puppet1, stringRoot1, puppet2, stringRoot2, string1Ref.transform, string2Ref.transform, puppet1LastFrame, debug);
        StringTick(puppet2, stringRoot2, puppet1, stringRoot1, string2Ref.transform, string1Ref.transform, puppet2LastFrame, debug2);

        // End variable updates

        puppet1LastFrame = puppet1.transform.position;
        puppet2LastFrame = puppet2.transform.position;

    }

    // On start-up, this should be done AFTER the puppets get into position
    public void InitialiseStrings()
    {
        string1Ref.manager = this;
        string2Ref.manager = this;
    }

    public void StringTick(GameObject puppet, GameObject root, GameObject otherpuppet, GameObject otherRoot, Transform reference, Transform otherReference, Vector3 lastFrame, GameObject debugObj)
    {
        if (bolConnected == false)
        {
            reference.rotation = Quaternion.FromToRotation(Vector3.up, puppet.transform.position - root.transform.position);
            reference.position = (puppet.transform.position + root.transform.position) / 2;
            reference.localScale = new Vector3(reference.localScale.x, (puppet.transform.position - root.transform.position).magnitude, reference.localScale.z);
        }
        else {
            reference.rotation = Quaternion.FromToRotation(Vector3.up, puppet.transform.position - effectiveRoot);
            reference.position = (puppet.transform.position + effectiveRoot) / 2;
            reference.localScale = new Vector3(reference.localScale.x, (puppet.transform.position - effectiveRoot).magnitude, reference.localScale.z);

            // calculate how much they have tangled/untangled

            // difference from last frame
            Vector3 differenceLastFrame = puppet.transform.position - lastFrame;

            // We need to find the string height at this point
            float refHeight = puppet.transform.position.y;
            // This is the point we are calculating out tangling around
            Vector3 referencePos = Vector3.zero;

            if (refHeight >= effectiveRoot.y)
            {
                referencePos = effectiveRoot;
            }
            else
            {
                Vector3 otherRootPuppetDifference = otherpuppet.transform.position - effectiveRoot;

                if (Mathf.Abs(otherRootPuppetDifference.y) > 0.05f)
                {
                    // in this, x = x, y = z
                    Vector2 travelPerY = new Vector2(-otherRootPuppetDifference.x / otherRootPuppetDifference.y, -otherRootPuppetDifference.z / otherRootPuppetDifference.y);

                    float heightDifferenceFromRoot = effectiveRoot.y - refHeight;
                    referencePos = new Vector3(effectiveRoot.x + travelPerY.x * heightDifferenceFromRoot, refHeight, effectiveRoot.z + travelPerY.y * heightDifferenceFromRoot);

                }
                else // too close to dividing by 0, just use the effective root
                {
                    referencePos = effectiveRoot;
                }

            }

            debugObj.transform.position = referencePos;

        }
    }

    public void StringCollision()
    {

    }

}
