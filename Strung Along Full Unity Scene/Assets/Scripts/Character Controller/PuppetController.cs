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

    public float airborneAcceleration;
    [Range(0, 1)]
    [Tooltip("This is only used for decreasing speed when it's over the maximum")]
    public float airborneDeceleration;
    [Range(0, 1)]
    [Tooltip("This is the natural drag that's always applied")]
    public float airborneDecay;
    [Range(0, 20)]
    public float airborneMaxSpeed;

    [Space]

    [Tooltip("How string the inital force of the jump should be, affects the shortest possible jump")]
    public float initalJumpVelocity;
    [Tooltip("How strong the boost from holding up as they are jumping will be")]
    public float jumpBoostForce;
    [Tooltip("How long the puppet will have an upwards boost from holding jump")]
    public float jumpBoostTime;
    float jumpBoostTimer; // how much time we have left of the jump boost, set to 0 when jump is released
    [Tooltip("The force applied upwards when jump is held, be careful not to set too high or the puppet will be able to fly")]
    [Range(0, 20)]
    public float jumpHoldForce;
    [Tooltip("The force applied downwards when jump is not held")]
    [Range(-20, 0)]
    public float jumpReleaseForce;
    [Tooltip("The drag applied to the puppet during the rise when jump is not held")]
    [Range(0, 1)]
    public float jumpReleaseRisingDrag;
    public float maxFallSpeed = 100;

    [Space]
    [Range(0, 50)]
    public float gravity = 9.8f;

    [Space]

    [Tooltip("Time in second that you can be off the ground before while still being able to jump")]
    [Range(0, 1)]
    public float coyoteTime;

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
    float timeSinceGrounded;
    bool hasJumped;

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

        Vector2 input = Vector2.zero;

        // Getting the player input and putting it inside a variable to reduce points of variance between the two player input possibilities
        if (secondPlayer)
        {
            input = new Vector2(Input.GetAxis("Horizontal2"), Input.GetAxis("Vertical2"));
        }
        else
        {
            input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }

        if (grounded)
        {
            hasJumped = false;

            // grounded movement

            // force calculation stuff is done on each axis independantly for this enviroment for the time being, I might change it later idk

            if (input.x != 0)
            {
                if (input.normalized.x * groundedMaxSpeed > rb.velocity.x) // tl;dr we want to move faster right than we are already moving
                {
                    rb.velocity = rb.velocity + Vector3.right * groundedAcceleration * Time.fixedDeltaTime * 100;
                }
                else if (input.normalized.x * groundedMaxSpeed < rb.velocity.x) // tl;dr we want to move faster left than we are already moving
                {
                    rb.velocity = rb.velocity + Vector3.left * groundedAcceleration * Time.fixedDeltaTime * 100;
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
                    rb.velocity = rb.velocity + Vector3.forward * groundedAcceleration * Time.fixedDeltaTime * 100;
                }
                else if (input.normalized.y * groundedMaxSpeed < rb.velocity.z) // tl;dr we want to move faster back than we are already moving
                {
                    rb.velocity = rb.velocity + Vector3.back * groundedAcceleration * Time.fixedDeltaTime * 100;
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
        else
        {
            // Air movement stuff, using simpler calculations since precision isn't as important

            rb.AddForce(new Vector3(input.normalized.x, 0, input.normalized.y) * airborneAcceleration, ForceMode.Acceleration);

            // deceleration (force soft, hard stopping in the air sounds ugly, but I can add if it if nessassary)

            if (new Vector2(rb.velocity.x, rb.velocity.z).magnitude >= airborneMaxSpeed)
            {
                rb.velocity = new Vector3(rb.velocity.x * (1 - airborneDeceleration), rb.velocity.y, rb.velocity.z * (1 - airborneDeceleration)); // Uses 1 - groundedDeceleration to make the variable more intuitive for designers to adjust
            }

            // Timer increment
            timeSinceGrounded = timeSinceGrounded + Time.fixedDeltaTime;

        }

        // Jump

        if (forceAirborneTimer > 0)
        {
            forceAirborneTimer = forceAirborneTimer - Time.fixedDeltaTime;
        }
        else {
            Jump();
        }

        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

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

        if (grounded) {
            timeSinceGrounded = 0;
        }
    }

    public void Jump() {

        string txtadd = "";

        // Side note: this is a really stupid way of doing this

        if (secondPlayer == true)
        {
            txtadd = "2";
        }

        if (Input.GetAxis("Jump" + txtadd) > 0)
        {

            bool canJump = false;

            if (grounded) {
                canJump = true;
            }

            if (timeSinceGrounded <= coyoteTime && hasJumped == false) {
                canJump = true;
            }

            if (canJump == true) {
                hasJumped = true;

                rb.velocity = new Vector3(rb.velocity.x, initalJumpVelocity, rb.velocity.z);
                grounded = false;
                gameObject.transform.position = transform.position + Vector3.up * 0.05f;
                forceAirborneTimer = 0.1f;
                jumpBoostTimer = jumpBoostTime;

            }

        }

        // Jump specific

        if (jumpBoostTimer > 0) {
            if (Input.GetAxis("Jump" + txtadd) > 0)
            {
                jumpBoostTimer = jumpBoostTimer - Time.fixedDeltaTime;
                rb.AddForce(Vector3.up * Mathf.Lerp(0, jumpBoostForce, jumpBoostTimer / jumpBoostTime));
            }
            else
            {
                // Once jump is released, you can no longer continue rising
                jumpBoostTimer = 0;
            }
        }

        // General airborne

        if (Input.GetAxis("Jump" + txtadd) > 0)
        {
            rb.AddForce(Vector3.up * jumpHoldForce);
        }
        else
        {
            rb.AddForce(Vector3.up * jumpReleaseForce);
            if (rb.velocity.y > 0) {
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * (1 - jumpReleaseRisingDrag * Time.fixedDeltaTime * 100), rb.velocity.z);
            }
        }

        if (rb.velocity.y < -maxFallSpeed) {
            rb.AddForce(Vector3.up * jumpReleaseForce);
        }

    }

}