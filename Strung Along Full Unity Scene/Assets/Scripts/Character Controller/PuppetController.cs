using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppetController : MonoBehaviour
{

    [Header("References")]

    [Tooltip("This is the other playable puppet, it should never be empty otherwise it will probably cause errors")]
    public PuppetController otherPuppet;
    //public stringthingidk string;

    [Space]

    public Rigidbody rb;

    [Space]

    [Header("State")]

    [Tooltip("Used to determine if this puppet is the second player, if true it will use the second player's controls and other play-specific things")]
    public bool secondPlayer;
    [Tooltip("Determines if the puppet is currently on the ground or not, public for unity inspector debugging purposes, can be made private later without issue")]
    public bool grounded;

    [Space]

    [Header("Movement Attributes")]

    public float groundedAcceleration;
    [Range(0, 1)]
    public float groundedDeceleration;
    [Range(0, 20)]
    public float groundedMaxSpeed;

    [Space]

    [Header("Minor Variables")]

    [Tooltip("If this is true, grounded speed can't go past the max speed, if false, the puppet just decelerate using the deceleration variables when past max speed")]
    public bool groundedMaxSpeedHardCap;
    [Tooltip("This is the number of raycasts used to do ground detection, increase this if ground detection is wonky, but too many rays may become preformance heavy")]
    public int groundedRayNumber;
    [Tooltip("This is how far down the raycasts for ground detection extent from the puppet, higher values allow for stepping up larger steps")]
    public float groundRayLength;
    [Tooltip("This is how wide the ground detection rays are around the puppet, should be scaled with the capsule radius")]
    public float groundedRayRadius;

    [Space]

    [Tooltip("These are the layers that the puppet will consider ground when standing on them")]
    public LayerMask groundMask;

    [Space]

    [Tooltip("This is the amount that this puppet it pushed downwards into the ground every frame, allows for travelling down slopes or stairs without going airborne, but too high will result in strange teleporting")]
    [Range(0, 3)]
    public float groundedDownPerFrame;

    // Private

    float forceAirborneTimer;

    // Start is called before the first frame update
    void Start()
    {
        if (otherPuppet == null)
        {
            Debug.LogWarning("No second player connected to " + this + ", this is likely to cause errors");
        }
        else {
            if (otherPuppet.secondPlayer == secondPlayer) {
                Debug.LogWarning("The other puppet connected to " + this + " is using the same controls as it's assigned other puppet, consider changing one of them or reassigning the other puppet");
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        GroundDetection();

        // Getting the player input and putting it inside a variable to reduce points of variance between the two player input possibilities
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (grounded)
        {
            // grounded movement

            // force calculation stuff is done on each axis independantly for this enviroment for the time being, I might change it later idk

            if (input.x != 0) {
                if (input.normalized.x * groundedMaxSpeed > rb.velocity.x) // tl;dr we want to move faster right than we are already moving
                {
                    rb.velocity = rb.velocity + Vector3.right * groundedAcceleration;
                }
                else if (input.normalized.x * groundedMaxSpeed < rb.velocity.x) // tl;dr we want to move faster left than we are already moving
                {
                    rb.velocity = rb.velocity + Vector3.left * groundedAcceleration;
                }
                else
                {
                    rb.velocity = new Vector3(rb.velocity.x * (1 - groundedDeceleration), rb.velocity.y, rb.velocity.z);
                }
            }
            else
            {
                rb.velocity = new Vector3(rb.velocity.x * (1 - groundedDeceleration), rb.velocity.y, rb.velocity.z);
            }

            if (input.y != 0)
            {
                if (input.normalized.y * groundedMaxSpeed > rb.velocity.z) // tl;dr we want to move faster forward than we are already moving
                {
                    rb.velocity = rb.velocity + Vector3.forward * groundedAcceleration;
                }
                else if (input.normalized.y * groundedMaxSpeed < rb.velocity.z) // tl;dr we want to move faster back than we are already moving
                {
                    rb.velocity = rb.velocity + Vector3.back * groundedAcceleration;
                }
                else
                {
                    rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z * (1 - groundedDeceleration));
                }
            }
            else
            {
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z * (1 - groundedDeceleration));
            }

            // Max speed calculation

            if (new Vector2(rb.velocity.x, rb.velocity.y).magnitude >= groundedMaxSpeed)
            {
                if (groundedMaxSpeedHardCap)
                {
                    rb.velocity = rb.velocity.normalized * groundedMaxSpeed;
                }
                else
                {
                    rb.velocity = rb.velocity * (1 - groundedDeceleration); // Uses 1 - groundedDeceleration to make the variable more intuitive for designers to adjust
                }
            }


        }

    }

    public void GroundDetection()
    {
        // Ground Detection

        if (forceAirborneTimer > 0)
        {
            forceAirborneTimer = forceAirborneTimer - Time.fixedDeltaTime;
        }

        if (grounded)
        {
            transform.position = transform.position - transform.up * groundedDownPerFrame;
        }

        if (forceAirborneTimer > 0)
        {
            // Ground detection is not allowed
            grounded = false;
        }
        else
        {
            // Ground detection is allowed
            List<RaycastHit> groundRays = new List<RaycastHit>();

            RaycastHit hit;
            Debug.DrawRay(transform.position + transform.up * groundRayLength, -transform.up * groundRayLength, Color.magenta);
            if (Physics.Raycast(transform.position + transform.up * groundRayLength, -transform.up, out hit, groundRayLength, groundMask))
            {
                groundRays.Add(hit);
            }

            if (groundedRayNumber > 0)
            {
                for (int i = 0; i < groundedRayNumber; i++)
                {
                    hit = new RaycastHit();
                    float rot = (Mathf.PI * i * 2) / groundedRayNumber;
                    Vector3 pos = new Vector3(Mathf.Cos(rot), 0, Mathf.Sin(rot)) * groundedRayRadius;
                    Physics.Raycast(transform.position + pos + transform.up * groundRayLength, -transform.up, out hit, groundRayLength, groundMask);
                    Debug.DrawRay(transform.position + pos + transform.up * groundRayLength, -transform.up * groundRayLength, new Color(i / groundedRayNumber, 1, (groundedRayNumber - i) / groundedRayNumber));
                    if (Physics.Raycast(transform.position + pos + transform.up * groundRayLength, -transform.up, out hit, groundRayLength, groundMask))
                    {
                        groundRays.Add(hit);
                    }
                }
            }

            if (groundRays.Count > 0)
            {

                if (grounded)
                {
                    // Reverse the downwards from being grounded, ONLY if we were grounded earlier, thus can confirm we did this earlier this frame
                    transform.position = transform.position + transform.up * groundedDownPerFrame;
                }

                bool lastGrounded = false;
                if (grounded)
                {
                    lastGrounded = true;
                }
                grounded = true;
                float avg = 0;
                for (int i = 0; i < groundRays.Count; i++)
                {
                    avg = avg + groundRays[i].point.y;
                }
                avg = avg / groundRays.Count;
                if (lastGrounded)
                {
                    gameObject.transform.position = new Vector3(gameObject.transform.position.x, avg, gameObject.transform.position.z);
                }
                else
                {
                    gameObject.transform.position = new Vector3(gameObject.transform.position.x, avg, gameObject.transform.position.z);
                }
                rb.velocity = new Vector3(rb.velocity.x, -1f, rb.velocity.z);
            }
            else
            {
                if (grounded)
                {
                    // Reverse the downwards from being grounded, ONLY if we were grounded earlier, thus can confirm we did this earlier this frame
                    transform.position = transform.position + transform.up * groundedDownPerFrame;
                }
                grounded = false;
            }
        }
    }


}
