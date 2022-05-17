using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuppetController : MonoBehaviour
{
    PlayerControls controls;

    [Header("References")]

    [Tooltip("This is the other playable puppet, it should never be empty otherwise it will probably cause errors")]
    public PuppetController otherPuppet;
    public PuppetStringManager stringManager;
    public StringRoot thisStringRoot;
    public StaminaBar staminaUI;

    [Space]

    private Rigidbody rb;

    private bool movePressed;
    private Vector2 move;
    private bool jumpPressed;
    bool grabPressed;

    private float speed = 0;
    [Tooltip("Determines if the puppet is currently on the ground or not, public for unity inspector debugging purposes, can be made private later without issue")]
    private bool isGrounded = true;
    public bool isClimbing = false;
    private bool jumpReleased = true;
    public float groundedMaxSpeed = 10;

    [Space]

    [Header("State")]

    [Tooltip("Used to determine if this puppet is the second player, if true it will use the second player's controls and other play-specific things")]
    public bool secondPlayer;
    //private variables to be changed depening on player
    private string playerHorizontalInput;
    private string playerVerticalInput;
    private string playerJumpInput;
    [Tooltip("If true, movement is limited to prevent exploiting string mechanics")]
    public bool beingPulled;

    [Tooltip("This is how much we have climbed up our string currently, used to make it so the string can move and we move with it.")]
    // Goes between 0-1, knot is always at 0.5 if it exists
    public float climbValue;

    [Space]

    [Tooltip("max of 1")]
    public float stamina;
    [Tooltip("How fast their stamina drains as they climb, this is basically how much stamina they lose per second")]
    public float staminaDrain;
    [Tooltip("How long it takes to regen all stamina while on the ground")]
    public float staminaRegen;

    [Space]

    [Header("Movement Attributes")]

    public float groundedAcceleration;
    public float groundedDeceleration;
    public float pulledAcceleration;
    public float pulledDrag;
    public float pulledAirborneThreshold;

    [Space]

    [Header("Climbing Attributes")]

    public float climbingSpeed;

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
    public float initialJumpForce;
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
    [Tooltip("This is how wide the ground detection rays are around the puppet, should be scaled with the capsule radius")]
    public float groundedRayDiameter = 0.3f;
    [Tooltip("This is how wide the landing detection rays are around the puppet, MUST BE LARGER THAN groundedRayRadius by 0.05")]
    public float landingRayDiameter = 0.45f;

    [Space]

    [Tooltip("These are the layers that the puppet will consider ground when standing on them")]
    public LayerMask groundMask;

    [Space]

    [Tooltip("This is the amount that this puppet it pushed downwards into the ground every frame, allows for travelling down slopes or stairs without going airborne, but too high will result in strange teleporting")]
    [Range(0, 3)]
    public float groundedDownPerFrame;
    public Vector3 effectiveRoot;

    // Private

    float forceAirborneTimer;
    float timeSinceGrounded;
    bool hasJumped;

    [Space]

    public TempGrab tempGrab;


    void Awake()
    {

        if (secondPlayer == false)
        {
            controls = new PlayerControls();
            controls.Player.Jump.performed += ctx =>
            {
                jumpPressed = ctx.ReadValueAsButton();
                Debug.Log(ctx.ReadValueAsButton());
                if (jumpPressed && jumpReleased)
                {
                    StartJump();
                }
                jumpReleased = false;
            };
            controls.Player.Jump.canceled += ctx =>
            {
                jumpReleased = true;
                jumpBoostTimer = 0;
            };

            controls.Player.Move.performed += ctx =>
            {
                move = ctx.ReadValue<Vector2>();
                movePressed = true; //move.x != 0 || move.y != 0;
                                    //Debug.Log(movePressed);
            };
            controls.Player.Move.canceled += ctx =>
            {
                move = Vector2.zero;
                movePressed = false;
                //Debug.Log(movePressed);
            };
            controls.Player.Grab.performed += ctx =>
            {
                grabPressed = ctx.ReadValueAsButton();
                Debug.Log(ctx.ReadValueAsButton());
                GrabStart();
            };
            controls.Player.Grab.canceled += ctx =>
            {
                grabPressed = false;
                GrabRelease();
            };
        }
        else // THIS IS VERY TEMPORARY, just so the playtesters can have access to both players
        {
            controls = new PlayerControls();
            controls.Player.TempJump2.performed += ctx =>
            {
                jumpPressed = ctx.ReadValueAsButton();
                Debug.Log(ctx.ReadValueAsButton());
                if (jumpPressed && jumpReleased)
                {
                    StartJump();
                }
                jumpReleased = false;
            };
            controls.Player.TempJump2.canceled += ctx =>
            {
                jumpReleased = true;
                jumpBoostTimer = 0;
            };

            controls.Player.TempMove2.performed += ctx =>
            {
                move = ctx.ReadValue<Vector2>();
                movePressed = true; //move.x != 0 || move.y != 0;
                                    //Debug.Log(movePressed);
            };
            controls.Player.TempMove2.canceled += ctx =>
            {
                move = Vector2.zero;
                movePressed = false;
                //Debug.Log(movePressed);
            };
            controls.Player.Grab2.performed += ctx =>
            {
                grabPressed = ctx.ReadValueAsButton();
                Debug.Log(ctx.ReadValueAsButton());
                GrabStart();
            };
            controls.Player.Grab2.canceled += ctx =>
            {
                grabPressed = false;
                GrabRelease();
            };
        }

    }
    void OnEnable()
    {
        controls.Player.Enable();
    }
    void OnDisable()
    {
        controls.Player.Disable();
    }

    List<RaycastHit> objectRayGenerator(int rayNumber, float rayLength, float rayRadius, bool useOffset)
    {
        List<RaycastHit> rays = new List<RaycastHit>();

        if (rayNumber > 0)
        {
            for (int i = 0; i < rayNumber; i++)
            {
                float rot = (Mathf.PI * i * 2) / rayNumber;
                Vector3 pos = new Vector3(Mathf.Cos(rot), 0, Mathf.Sin(rot)) * rayRadius;
                Vector3 origin;

                if (useOffset)
                    origin = transform.position + pos + transform.up * rayLength;
                else
                    origin = transform.position + pos;


                //Physics.Raycast(transform.position + pos + transform.up * rayLength, -transform.up, out hit, rayLength, groundMask);
                Debug.DrawRay(origin, -transform.up * rayLength, Color.white);
                if (Physics.Raycast(origin, -transform.up, out RaycastHit hit, rayLength, groundMask))
                {
                    Debug.DrawRay(origin, -transform.up * rayLength, Color.green);
                    rays.Add(hit);
                }
            }
        }

        return rays;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

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
        if (isClimbing == false) // Do normal stuff
        {
            GroundDetection();
            HandleMovement();
            HandleJump(); //don't remove
        }
        else // Pause all normal movement stuff, just climb
        {
            Debug.Log("AA");
            isGrounded = false;
            HandleMovement();
            ClimbTick();
        }        
        
        //Debug.Log(move.x + " " + move.y);

        // Temp gross grab compatability
        if (tempGrab.grabbed != null && isClimbing == true)
        {
            GrabRelease();
        }

        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);


    }
    public void GroundDetection()
    {
        // Ground Detection

        if (beingPulled && isGrounded)
        {
            if (rb.velocity.y > pulledAirborneThreshold)
            {
                isGrounded = false;
                forceAirborneTimer = 0.05f;
            }
        }

        if (isGrounded)
        {
            transform.position = transform.position - transform.up * groundedDownPerFrame;

            if (stamina < 1)
            {
                stamina += staminaRegen * Time.fixedDeltaTime;
                staminaUI.UpdateStaminaVisual(stamina);
            }
            else
            {
                stamina = 1;
            }

        }

        if (forceAirborneTimer > 0)
        {
            // Ground detection is not allowed
            forceAirborneTimer -= Time.fixedDeltaTime;
            isGrounded = false;
        }
        else
        {
            // Ground detection is allowed
            List<RaycastHit> groundRays = objectRayGenerator(groundedRayNumber, groundRayLength, groundedRayDiameter, true);

            RaycastHit hit;
            Debug.DrawRay(transform.position + transform.up * groundRayLength, -transform.up * groundRayLength, Color.magenta);
            if (Physics.Raycast(transform.position + transform.up * groundRayLength, -transform.up, out hit, groundRayLength, groundMask))
            {
                groundRays.Add(hit);
            }

            if (groundRays.Count > 0)
            {
                bool lastGrounded = false;

                if (isGrounded)
                {
                    // Reverse the downwards from being grounded, ONLY if we were grounded earlier, thus can confirm we did this earlier this frame
                    transform.position = transform.position + transform.up * groundedDownPerFrame;
                    lastGrounded = true;
                }

                isGrounded = true;
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
                if (isGrounded)
                {
                    // Reverse the downwards from being grounded, ONLY if we were grounded earlier, thus can confirm we did this earlier this frame
                    transform.position = transform.position + transform.up * groundedDownPerFrame;
                }
                isGrounded = false;
            }

        }

        if (isGrounded) {
            timeSinceGrounded = 0;
        }
    }
    void HandleMovement()
    {
        float relativeMaxSpeedX = Math.Abs(groundedMaxSpeed * move.x);
        float relativeMaxSpeedY = Math.Abs(groundedMaxSpeed * move.y);

        if (isGrounded)
        {
            hasJumped = false;

            // grounded movement

            // force calculation stuff is done on each axis independantly for this enviroment for the time being, I might change it later idk

            Vector2 effectiveMove = move;

            if (beingPulled)
            {
                // If we are being pulled, decrease our ability to control grounded movement to prevent major string exploits

                //rb.velocity = rb.velocity * (1 - pulledDrag * Time.fixedDeltaTime);
                //rb.AddForce(new Vector3(move.x, 0, move.y) * pulledAcceleration);

                Vector3 difference = transform.position - effectiveRoot;
                effectiveMove = effectiveMove - new Vector2(difference.normalized.x / 2, difference.normalized.z / 2);

            }

            if (effectiveMove.x != 0)
            {
                if (effectiveMove.normalized.x * relativeMaxSpeedX > rb.velocity.x) // tl;dr we want to move faster right than we are already moving
                {
                    rb.velocity += Vector3.right * groundedAcceleration * Time.fixedDeltaTime * 100;
                }
                else if (effectiveMove.normalized.x * relativeMaxSpeedX < rb.velocity.x) // tl;dr we want to move faster left than we are already moving
                {
                    rb.velocity += Vector3.left * groundedAcceleration * Time.fixedDeltaTime * 100;
                }
                else
                {
                    rb.velocity = new Vector3(rb.velocity.x * (1 - groundedDeceleration), rb.velocity.y, rb.velocity.z);
                }
            }
            else
            {
                if (beingPulled == false)
                {
                    rb.velocity = new Vector3(rb.velocity.x * (1 - groundedDeceleration), rb.velocity.y, rb.velocity.z);
                }
            }

            if (effectiveMove.y != 0)
            {
                if (effectiveMove.normalized.y * relativeMaxSpeedY > rb.velocity.z) // tl;dr we want to move faster forward than we are already moving
                {
                    rb.velocity = rb.velocity + Vector3.forward * groundedAcceleration * Time.fixedDeltaTime * 100;
                }
                else if (effectiveMove.normalized.y * relativeMaxSpeedY < rb.velocity.z) // tl;dr we want to move faster back than we are already moving
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
                if (beingPulled == false)
                {
                    rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z * (1 - groundedDeceleration));
                }
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

            if (beingPulled) {
                //rb.AddForce(-(transform.position - effectiveRoot), ForceMode.Acceleration);
            }



        }
        else
        {

            if (beingPulled == false)
            {
                // Air movement stuff, using simpler calculations since precision isn't as important

                rb.AddForce(new Vector3(move.normalized.x, 0, move.normalized.y) * airborneAcceleration, ForceMode.Acceleration);

                // deceleration (force soft, hard stopping in the air sounds ugly, but I can add if it if nessassary)

                if (new Vector2(rb.velocity.x, rb.velocity.z).magnitude >= airborneMaxSpeed)
                {
                    rb.velocity = new Vector3(rb.velocity.x * (1 - airborneDeceleration), rb.velocity.y, rb.velocity.z * (1 - airborneDeceleration)); // Uses 1 - groundedDeceleration to make the variable more intuitive for designers to adjust
                }

            }
            else // You still have a bit of influence even when tangled
            {
                jumpBoostTimer = 0;
                rb.AddForce(new Vector3(move.normalized.x, 0, move.normalized.y) * airborneAcceleration * 0.6f, ForceMode.Acceleration);
            }

            // Timer increment
            timeSinceGrounded = timeSinceGrounded + Time.fixedDeltaTime;

        }
    }

    void HandleJump()
    {
        if (jumpPressed && jumpBoostTimer > 0)
        {
            jumpBoostTimer -= Time.fixedDeltaTime;
            rb.AddForce(Vector3.up * Mathf.Lerp(0, jumpBoostForce, jumpBoostTimer / jumpBoostTime));
        }
    }

    public void StartJump()
    {
        if ((isGrounded || timeSinceGrounded <= coyoteTime) && hasJumped == false)
        {
            isGrounded = false;
            hasJumped = true;
            rb.velocity += Vector3.up * initialJumpForce;
            forceAirborneTimer = 0.1f;
            jumpBoostTimer = jumpBoostTime;
            // required for ensuring the puppet gets off the ground consistantly when jumping
            //gameObject.transform.position = transform.position + Vector3.up * 0.05f;
        }
    }

    // Grab stuff.
    // NOTE: A fair bit of this might seem redundant, and yeah it kind of is right now but it's foundation for when we want the ability to climb the other puppet's strings, so leave it in there for now.

    public void GrabStart()
    {

        if (stamina > 0) {
            // First, we need to find out where we are in relation to our own string
            SetClimbValue();

            isClimbing = true;

            //rb.velocity = Vector3.zero; // makes climbing feel a little worse, but prevents some exploits, consider turning of if desired.
        }

    }

    public void GrabRelease()
    {
        if (isClimbing) {
            //rb.velocity *= 0.5f;
        }
        climbValue = 0;
        isClimbing = false;
    }

    public void ClimbTick() {

        // We don't want y velocity here, just keep it at 0 to be safe
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        SetClimbValue();

        if (stringManager.tangle != 0)
        {

            if (transform.position.y < stringManager.effectiveRoot.y)
            {
                // Under 0.5

                // temp simple patch
                transform.position = Vector3.MoveTowards(transform.position, stringManager.effectiveRoot, climbingSpeed * Time.fixedDeltaTime);
                //transform.position = Vector3.MoveTowards(transform.position, new Vector3(stringManager.effectiveRoot.x, transform.position.y, stringManager.effectiveRoot.z), Time.fixedDeltaTime);
                transform.position += (new Vector3(stringManager.effectiveRoot.x, transform.position.y, stringManager.effectiveRoot.z) - transform.position) * Time.fixedDeltaTime/2;

                // TEMP FAKE GRAVITY :))))
                rb.AddForce((stringManager.effectiveRoot - transform.position) * 2);
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            }
            else
            {
                // Over 0.5

                // Should do this for both when we can climb the other's string
                transform.position = Vector3.Lerp(stringManager.effectiveRoot, thisStringRoot.transform.position, (climbValue - 0.5f) * 5);

                transform.position = Vector3.MoveTowards(transform.position, thisStringRoot.transform.position, climbingSpeed * Time.fixedDeltaTime);
                transform.position += (new Vector3(thisStringRoot.transform.position.x, transform.position.y, thisStringRoot.transform.position.z) - transform.position) * Time.fixedDeltaTime/2;

            }

        }
        else {
            Debug.Log("AAAAAAAAAAAA");
            transform.position = Vector3.MoveTowards(transform.position, thisStringRoot.transform.position, climbingSpeed * Time.fixedDeltaTime);
            //transform.position = Vector3.MoveTowards(transform.position, new Vector3(thisStringRoot.transform.position.x, transform.position.y, thisStringRoot.transform.position.z), Time.fixedDeltaTime);
            transform.position += (new Vector3(thisStringRoot.transform.position.x, transform.position.y, thisStringRoot.transform.position.z) - transform.position) * Time.fixedDeltaTime/2;

            // TEMP FAKE GRAVITY :))))
            rb.AddForce((thisStringRoot.transform.position - transform.position) * 5);
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        }

        stamina -= staminaDrain * Time.fixedDeltaTime;

        staminaUI.UpdateStaminaVisual(stamina);

        if (stamina < 0) {
            GrabRelease();
        }

    }

    public void SetClimbValue()
    {

        if (stringManager.tangle != 0)
        {
            if (transform.position.y < stringManager.effectiveRoot.y)
            {
                // Under 0.5
                climbValue = Mathf.Lerp(0, 0.5f, (transform.position.y - transform.position.y) / (stringManager.effectiveRoot.y - transform.position.y));
            }
            else
            {
                // Over 0.5
                climbValue = Mathf.Lerp(0.5f, 1, (stringManager.effectiveRoot.y - transform.position.y) / (thisStringRoot.transform.position.y - transform.position.y));
            }
        }
        else
        {
            climbValue = (transform.position.y - transform.position.y) / (thisStringRoot.transform.position.y - transform.position.y);
        }

    }

}
