using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringRoot : MonoBehaviour
{

    [Header("Referneces")]

    public Rigidbody connectedObject;
    public PuppetController connectedPuppet;
    public PuppetStringManager manager;
    public LineRenderer lineVisual;

    [Space]

    [Header("String Properties")]

    [Tooltip("This is how far the connectedObject can go from this object before being pulled back")]
    public float stringLength;
    [Tooltip("This is the smallest the string can go from being tangled, should be smaller than string length")]
    public float minimumStringLength;

    [Space]

    [Tooltip("If enabled, the string will have flex, otherwise it will not")]
    public bool elasticString;

    [Space]

    [Tooltip("This is how much force the string applies for every meter the connectedObject is away from the root past the stringLength")]
    public float stringForcePerUnit;
    [Tooltip("This is the time it takes once the string starts being stretched before it reaches the maximum pulling force, set to 0 to ingore this feature")]
    public float timeUntilMaxForce;
    [Tooltip("Only used when timeUntilMaxForce is > 0, it's the curve that the string force will take during this time before it reaches it's maximum force")]
    public AnimationCurve stretchOverTime;

    [Space]

    [Tooltip("How fast it moves to it's new position")]
    public float lerpSpeed;
    [Tooltip("The position this anchor point will try to move to")]
    public Vector3 targetPosition;

    [Space]

    [Tooltip("Ignore this, it's used as a debug visual and can be freely removed without effecting functionality")]
    public float debugDistance;
    public float debugeffRange;
    float stretchTime;

    // Start is called before the first frame update
    void Awake()
    {
        connectedPuppet = connectedObject.gameObject.GetComponent<PuppetController>();
        targetPosition = transform.position; // turn off to manually set on startup rather than being based on current position
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.fixedDeltaTime);

        // Used for tangleing
        Vector3 effectiveRoot = transform.position;
        float effectiveLength = stringLength;

        if (manager.tangle != 0) {

            // Set new root
            //Vector3 puppetAverage = (manager.leftString.connectedObject.transform.position + manager.rightString.connectedObject.transform.position) / 2;

            // This line is a bit weird to I'll try explain
            // Without this, when the puppets are fully tangled, their reference points will be set right next to each other and completely independant from the original StringRoot location
            // To fix this, the point at which the tangle lerp is converging to it the DIRECTION of the two puppets, with the min, max magic on top, to get the smallest between current position and max string length
            //Vector3 puppetMaxTanglePoint = -puppetAverage.normalized * Mathf.Min(stringLength, puppetAverage.magnitude) + (manager.leftString.transform.position + manager.rightString.transform.position) / 2;

            //effectiveRoot = Vector3.Lerp((manager.leftString.transform.position + manager.rightString.transform.position) / 2, puppetMaxTanglePoint, (float)Mathf.Abs(manager.tangleCount) / manager.maxTangles);
            // Additional note: This could have a per-frame lerp function for smoother movements, especially when on the non-elastic option;

            effectiveRoot = manager.effectiveRoot;

            // Set new length
            effectiveLength = Mathf.Lerp(stringLength, minimumStringLength, (float)Mathf.Abs(manager.tangle) / manager.maxTangle);

        }

        float distance = Vector3.Distance(effectiveRoot, connectedObject.transform.position);
        float baseDistance = Vector3.Distance(transform.position, connectedObject.transform.position);
        //distance = Mathf.Max(distance, baseDistance); // to make sure tangling doesn't ever give us MORE reach
        debugDistance = distance;
        debugeffRange = effectiveLength;

        if (distance > effectiveLength || baseDistance > stringLength)
        {

            if (connectedPuppet != null)
            {
                connectedPuppet.beingPulled = true;
            }


            Vector3 difference = (effectiveRoot - connectedObject.transform.position);

            if (elasticString)
            {

                if (stretchTime < 0)
                {
                    stretchTime = 0 + Time.fixedDeltaTime;
                }
                else
                {
                    stretchTime = stretchTime + Time.fixedDeltaTime;
                }

                float pullForce = 0;

                if (timeUntilMaxForce > 0) // Ensures we don't divide by 0, or anything less than 0
                {
                    pullForce = difference.magnitude * stringForcePerUnit * stretchOverTime.Evaluate(stretchTime / timeUntilMaxForce);
                }
                else
                {
                    pullForce = difference.magnitude * stringForcePerUnit;
                }

                connectedObject.AddForce(difference.normalized * pullForce, ForceMode.Acceleration);

            }
            else
            {
                // These two lines took more time to write than like, almost the entire rest of this script and I'm really not proud of that.

                // So, I have absolutely 0 idea why this works mathematically (using new Vector3(difference.normalized.y, -difference.normalized.x) to rotate it by 90),
                // I just did a few examples manually on paper, found the pattern, put it in and it ended up working :)
                // (which is to flip x and y but invert the sign of the original x value)
                connectedObject.velocity = Vector3.Project(connectedObject.velocity, new Vector3(difference.normalized.y, -difference.normalized.x));

                connectedObject.gameObject.transform.position = -(difference.normalized * effectiveLength) + effectiveRoot;
            }

        }
        else
        {
            // -1 is used to avoid any strange floating point errors from causing any funny business
            stretchTime = -1;
            if (connectedPuppet != null) {
                connectedPuppet.beingPulled = false;
            }
        }

        lineVisual.SetPosition(0, transform.position);
        lineVisual.SetPosition(1, effectiveRoot);
        lineVisual.SetPosition(2, connectedObject.transform.position);


    }

    public void SetAnchorPoint(Vector3 newPoint) {
        targetPosition = newPoint;
    }

}
