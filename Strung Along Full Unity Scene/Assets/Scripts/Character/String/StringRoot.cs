using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringRoot : MonoBehaviour
{

    [Header("Referneces")]

    public Rigidbody connectedObject;
    public PuppetController connectedPuppet;
    public Transform connectedPoint;
    public Transform connectedVisualPoint;
    public PuppetStringManager manager;
    public LineRenderer lineVisual;

    [Space]

    [Header("String Properties")]

    [Tooltip("This is how far the connectedObject can go from this object before being pulled back")]
    public float stringLength;
    [Tooltip("This is how far the connectedObject can go from this object before it starts to be pulled, ignored on Y")]
    public float stringStretchLength;
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

    [Tooltip("Ignore this, it's used as a debug visual and can be freely removed without effecting functionality")]
    public float debugDistance;
    public float debugeffRange;
    float stretchTime;
    Transform angleRef;
    Transform angleRef2;

    // Start is called before the first frame update
    void Awake()
    {
        connectedPuppet = connectedObject.gameObject.GetComponent<PuppetController>();
		
		// DEBUG harper here, making unity happy about these empty refs. delete when you've got it all sorted <3
		GameObject angleRefObj = new GameObject("angleRef");
		GameObject angleRef2Obj = new GameObject("angleRef2");
		angleRef = angleRefObj.transform;
		angleRef2 = angleRef2Obj.transform;
		
    }

    // Update is called once per frame
    void FixedUpdate()
    {
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

        float distance = Vector3.Distance(new Vector3(effectiveRoot.x, connectedPoint.position.y, effectiveRoot.z), connectedPoint.position);
        float baseDistance = Vector3.Distance(new Vector3(transform.position.x, connectedPoint.position.y, transform.position.z), connectedPoint.position);

        // 0 == 1 is to pause it
        if (stringLength - baseDistance <= 0 && 0 == 1) // pretend we aren't tangled this frame, since our bounding area isn't limited by out tangled range
        {

            //Debug.Log("lol2 " + effectiveLength);

            effectiveRoot = transform.position;
            effectiveLength = stringLength;

            distance = Vector3.Distance(effectiveRoot, connectedPoint.position);
            baseDistance = Vector3.Distance(transform.position, connectedPoint.position);
        }

        //float distanceExtended = Mathf.Max(distance, baseDistance); // to make sure tangling doesn't ever give us MORE reach
        debugDistance = distance;
        debugeffRange = effectiveLength;

        if (distance > effectiveLength)
        {


            if (connectedPuppet != null)
            {
                connectedPuppet.beingPulled = true;
            }


            Vector3 difference = (effectiveRoot - connectedPoint.position);
            difference = new Vector3(difference.x, 0, difference.z);

            // this entire section is a nightmare sorry

            Vector3 crossOut = Vector3.Cross(connectedObject.velocity, difference);

            angleRef.transform.rotation = Quaternion.LookRotation(crossOut); // This' Z
            angleRef.transform.position = connectedPoint.position;

            angleRef2.transform.rotation = Quaternion.LookRotation(difference); // This' X
            angleRef2.transform.position = connectedPoint.position;

            // this is all disgusting

            //Debug.Log("VelocityBefore: " + connectedObject.velocity);

            Vector3 oldVel = connectedObject.velocity;

            connectedObject.velocity =
                Vector3.Project(connectedObject.velocity, angleRef2.transform.up) +
                Vector3.Project(connectedObject.velocity, angleRef2.transform.right);

            connectedObject.velocity = connectedObject.velocity * (1 - (Time.fixedDeltaTime * 0.5f));

            connectedObject.velocity = new Vector3(connectedObject.velocity.x, oldVel.y, connectedObject.velocity.z);

            //Debug.Log("VelocityAfter: " + connectedObject.velocity);

            //connectedObject.velocity = Vector3.Project(connectedObject.velocity, new Vector3(difference.normalized.y, -difference.normalized.x));

            float oldY = connectedObject.transform.position.y;
            Vector3 vector = (connectedObject.gameObject.transform.position - connectedPoint.transform.position);

            vector = (connectedObject.gameObject.transform.position - connectedPoint.transform.position);

            connectedObject.gameObject.transform.position = new Vector3(vector.x, vector.y, vector.z) - (difference.normalized * effectiveLength) + effectiveRoot;
            connectedObject.transform.position = new Vector3(connectedObject.transform.position.x, oldY, connectedObject.transform.position.z);

            if (connectedPuppet != null)
            {
                if (connectedPuppet.otherPuppet.grabbingObject.gameObject == connectedPuppet.gameObject)
                {
                    connectedPuppet.otherPuppet.GrabRelease(); // prevent overpulling
                    Debug.Log("Force release");
                }
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

        if (elasticString)
        {

            PuppetController puppet = connectedObject.GetComponent<PuppetController>();

            if (distance > stringStretchLength)
            {

                Vector3 difference = (effectiveRoot - connectedPoint.position);
                difference = new Vector3(difference.x, 0, difference.z);

                if (puppet != null)
                {
                    float y = connectedObject.velocity.y;
                    connectedObject.velocity = connectedObject.velocity * Mathf.Lerp(1f, 0.8f, puppet.timeSinceSlingshot - 1);
                    connectedObject.velocity = new Vector3(connectedObject.velocity.x, y, connectedObject.velocity.z);

                }

                connectedObject.AddForce(difference * stringForcePerUnit);

                if (puppet != null)
                {
                    // send pulled message
                    puppet.beingPulled = true;
                }

            }

            if (puppet != null)
            {
                // send not pulled message
                puppet.beingPulled = false;
            }

        }
        else
        {
            stretchTime = -1;
        }

        // Do all the stuff on the Y axis here

        // real root instead of effective used to simplify Y axis stuf and make knot position visual only
        if (connectedObject.transform.position.y < transform.position.y - effectiveLength)
        {
            connectedObject.velocity = new Vector3(connectedObject.velocity.x, 0, connectedObject.velocity.z);
            connectedObject.transform.position = new Vector3(connectedObject.transform.position.x, transform.position.y - effectiveLength, connectedObject.transform.position.z);
            // If we are pulled up, pull in a bit as well

            Vector3 difference = (effectiveRoot - connectedPoint.position);
            difference = new Vector3(difference.x, 0, difference.z);

            connectedObject.AddForce(difference * 3f);
            connectedObject.velocity = connectedObject.velocity * Mathf.Min(Mathf.Abs((1 - Time.fixedDeltaTime)), 1);
        }

        if (manager.tangle != 0) // This is done bc we might overwrite this earlier, just putting it back for the line render or anythign else that might need it
        {
            effectiveRoot = manager.effectiveRoot;
        }

        lineVisual.SetPosition(0, transform.position);
        lineVisual.SetPosition(1, effectiveRoot);
        lineVisual.SetPosition(2, connectedVisualPoint.position);


    }

}
