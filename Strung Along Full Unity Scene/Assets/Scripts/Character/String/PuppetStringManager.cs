using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuppetStringManager : MonoBehaviour
{

    [Header("References")]

    public GameObject puppet1;
    public GameObject puppet2;
    public GameObject puppet1Effective;
    public GameObject puppet2Effective;
    public GameObject stringRoot1;
    public GameObject stringRoot2;
    public PuppetString string1Ref;
    public PuppetString string2Ref;
    public Text debugTangleDisplay;

    [Space]

    GameObject debug;
    GameObject debug2;

    [Space]

    [Header("State")]

    [Tooltip("This is how tangled the strings currently are, public for exposed debug purposes")]
    public float tangle;
    public float maxTangle;
    public bool bolConnected;
    public Vector3 effectiveRoot;

    [Space]

    public Vector3 puppet1LastFrame;
    public Vector3 puppet2LastFrame;
    public Vector3 effectiveRootLastFrame;

    [Space]

    [Header("Variables")]

    public float lerpToEffectiveRootSpeed;
    [Range(1, 2)]
    public float puppetDistanceEffectiveRootFactor = 1.2f;
    [Range(1, 3)]
    [Tooltip("This one helps the tangle point go up when highly tangled o7")]
    public float puppetDistanceEffectiveRootFactorTangled = 1.8f;
    [Range(0, 1)]
    public float effectiveRootPuppetPositionInfluence;

    [Space]

    [Header("Debug")]

    public float debugeffRange;


    // Start is called before the first frame update
    void Start()
    {
        InitialiseStrings();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (debugTangleDisplay != null) {
            debugTangleDisplay.text = "Tangle: " + Mathf.Round(tangle * 100)/100 + " (maximum tangle at " + maxTangle + ")";
        }

        if (bolConnected == true)
        {

            float startTangle = tangle;

            Vector3 targetEffectiveRoot = new Vector3(((puppet1Effective.transform.position + puppet2Effective.transform.position) / 2).x, 
                Vector3.Lerp((stringRoot1.transform.position + stringRoot2.transform.position) / 2, new Vector3((puppet1Effective.transform.position + puppet2Effective.transform.position).x, (stringRoot1.transform.position + stringRoot2.transform.position).y, (puppet1Effective.transform.position + puppet2Effective.transform.position).z) / 2, effectiveRootPuppetPositionInfluence).y, 
                ((puppet1Effective.transform.position + puppet2Effective.transform.position) / 2).z);
            //targetEffectiveRoot =
            //    new Vector3(targetEffectiveRoot.x,
            //    Mathf.Lerp(Mathf.Max(puppet1Effective.transform.position.y, puppet2Effective.transform.position.y), Mathf.Min(stringRoot1.transform.position.y, stringRoot2.transform.position.y), -Mathf.Pow(Mathf.Lerp(puppetDistanceEffectiveRootFactor, puppetDistanceEffectiveRootFactorTangled, Mathf.Abs(tangle) / Mathf.Max(0.0001f, maxTangle)), -Vector3.Distance(puppet1Effective.transform.position, puppet2Effective.transform.position)) + 1),
            //   targetEffectiveRoot.z);
            targetEffectiveRoot = new Vector3(targetEffectiveRoot.x, targetEffectiveRoot.y - 2, targetEffectiveRoot.z);

           // end of the mathf.Lerp line uses -A^{-x}+1 where A = puppetDistanceEffectiveRootFactor, put it in https://www.desmos.com/calculator to see it
           // It's used as a budget way of simulating the phenomena of how the string effective root gets lower the closer the two objects or puppets are to each other
           // Much of this was made using observations I found from playing with strings a lot, some of it is probably wrong, but it's the best guess I have

           // hight is lerped between square root of the two puppet's distance apart, further out, the lower it gets
           effectiveRoot = Vector3.Lerp(effectiveRoot, targetEffectiveRoot, lerpToEffectiveRootSpeed * Time.fixedDeltaTime);

            StringTick(puppet1Effective, stringRoot1, puppet2Effective, stringRoot2, string1Ref.transform, string2Ref.transform, puppet1LastFrame, debug);
            StringTick(puppet2Effective, stringRoot2, puppet1Effective, stringRoot1, string2Ref.transform, string1Ref.transform, puppet2LastFrame, debug2);

            if (startTangle > 0)
            {
                if (tangle <= 0) {
                    bolConnected = false;
                    tangle = 0;
                }
            }
            else if (startTangle < 0)
            {
                if (tangle >= 0)
                {
                    bolConnected = false;
                    tangle = 0;
                }
            }
            else
            {

                // I feel like there should be some fancy thing to determine which direction to set the tangle to and all that, but in theory just being empty should cover every edge case I can think of??

            }

        }
        else
        {
            // repeat code i know it's bad but brain has reached it's limit and won't think of a cleaner solution, will fix later if nessassary
            StringTick(puppet1Effective, stringRoot1, puppet2Effective, stringRoot2, string1Ref.transform, string2Ref.transform, puppet1LastFrame, debug);
            StringTick(puppet2Effective, stringRoot2, puppet1Effective, stringRoot1, string2Ref.transform, string1Ref.transform, puppet2LastFrame, debug2);
        }

        // End variable updates

        puppet1LastFrame = puppet1Effective.transform.position;
        puppet2LastFrame = puppet2Effective.transform.position;
        effectiveRootLastFrame = effectiveRoot;

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
            differenceLastFrame = differenceLastFrame - (effectiveRoot - effectiveRootLastFrame);

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

            //debugObj.transform.position = referencePos;

            // Now to determine if we tangled or not

            // only uses x and z
            Vector3 stringDirection = puppet.transform.position - referencePos;
            stringDirection = new Vector3(stringDirection.x, 0, stringDirection.z);
            Vector3 rotateVector = new Vector3(stringDirection.normalized.z, 0, -stringDirection.normalized.x);


            //Debug.Log(Vector3.Project(differenceLastFrame, rotateVector));

            // THIS IS THE FINAL VALUE THAT SHOWS HOW MUCH WE ROTATED YES I FINALLY DID IT OMG IT ACTUALLY WORKS I THINK AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
            float rotateAroundValue = Vector3.Dot(differenceLastFrame, rotateVector);

            //Debug.Log(rotateAroundValue);

            // I never thought I'd see the day...
            // tangle = tangle + rotateAroundValue;
            // actually not quite, we need to multiply this value based on the distance we are away (exponential decay) to get a more accurate value
            // Actually that's wrong, it /2 * pi * r
            // Ok that's also wrong but I give up for now, I'll figure it out later when it's not 5:41am

            float finalRotateValue = rotateAroundValue / (Mathf.Max(0.000000001f, 2 * Mathf.PI * stringDirection.magnitude));

            // I never thought I'd see the day...
            tangle += finalRotateValue;

            // bad, i'll fix this if it becomes a problem
            PuppetController puppetCont = puppet.GetComponent<PuppetController>();

            if (puppetCont != null) {
                puppetCont.effectiveRoot = effectiveRoot;
            }

        }
    }

    public void StringCollision()
    {

    }

}
