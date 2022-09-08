using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PuppetController : MonoBehaviour
{
    PlayerControls controls;

    [Header("References")]

    [Tooltip("This is the other playable puppet, it should never be empty otherwise it will probably cause errors")]
    public PuppetController otherPuppet;
	[HideInInspector]
    public PuppetStringManager stringManager;
    public StringRoot thisStringRoot;
    StaminaBar staminaUI;
    public GameObject visualReference;
    PuppetAudio audioManager;
    Animator puppetAnimator;
    PuppetContextualTutorial conTut;
    HandIKHandler ikHandler;
    ClimbingIK climbIK;
	[HideInInspector]
    public GridManager gridManager;

    [Space]

    public GameObject jumpParticles;
    public GameObject landParticles;

    [Space]


    private Rigidbody rb;

    public Vector2 move;
    public bool movePressed;
    public bool jumpPressed;
    public bool jumpReleased = true;
    public bool grabPressed;

    float speed = 0;
    [Tooltip("Determines if the puppet is currently on the ground or not, public for unity inspector debugging purposes, can be made private later without issue")]
    public bool isGrounded = true; // Keep public, other things use this as a reference
    public bool isClimbing = false;
    public float groundedMaxSpeed = 10;

    [Space]

    [Tooltip("This player can grab grab the other player")]
    public bool canSlingshot;
    public float SlingshotForce;
    public float SlingshotForceYMulti;

    [Space]

    [Header("State")]

    [Tooltip("Used to determine if this puppet is the second player, if true it will use the second player's controls and other play-specific things")]
    public bool secondPlayer;
    //private variables to be changed depening on player
    string playerHorizontalInput;
    string playerVerticalInput;
    string playerJumpInput;
    [HideInInspector]
    public bool beingPuppetPulled;
    public bool beingPulled;
    float distanceToHook;

    [Tooltip("This is how much we have climbed up our string currently, used to make it so the string can move and we move with it.")]
    // Goes between 0-1, knot is always at 0.5 if it exists
    float climbValue;

    [Space]

    [Tooltip("max of 1")]
    public float stamina;
    [Tooltip("How fast their stamina drains as they climb, this is basically how much stamina they lose per second")]
    public float staminaDrain;
    [Tooltip("How long it takes to regen all stamina while on the ground")]
    public float staminaRegen;

    [Space]

    public GridPoint gridPoint1;
    public GridPoint gridPoint2;

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
    public float jumpBoostTimer; // how much time we have left of the jump boost, set to 0 when jump is released
    [Tooltip("The force applied upwards when jump is held, be careful not to set too high or the puppet will be able to fly")]
    [Range(0, 20)]
    public float jumpHoldForce;
    [Tooltip("The force applied downwards when jump is not held")]
    [Range(-50, 0)]
    public float jumpReleaseForce;
    [Tooltip("The drag applied to the puppet during the rise when jump is not held")]
    [Range(0, 1)]
    public float jumpReleaseRisingDrag;
    [Range(0, 1)]
    public float jumpRisingDrag;
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
    [Tooltip("How smooth the force that keeps the player on the lines will be, setting to 1 basically allows you to move anywhere")]
    [Range(0f, 1f)]
    public float lineForceSmoothness = 0.6f;
    [Tooltip("How easy it is to change lines, boosting this will make it so angles further than 45 degrees from the intended direction may still change to that line over parralel lines")]
    [Range(0f, 5f)]
    public float directionChangeBoost = 1f;

    [Space]

    [Tooltip("These are the layers that the puppet will consider ground when standing on them")]
    public LayerMask groundMask;

    [Space]

    [Tooltip("This is the amount that this puppet it pushed downwards into the ground every frame, allows for travelling down slopes or stairs without going airborne, but too high will result in strange teleporting")]
    [Range(0, 3)]
    public float groundedDownPerFrame;
    public Vector3 effectiveRoot;

    [Space]

    [Tooltip("This is the distance the player can be from a point to travel along a different line from the one they are currently on")]
    public float pointRedirectDistance;

    [Space]

    [Header("Animation Variables")]

    public float visualRotateSpeed;
    public float visualAirRotateSpeed;

    [Space]

    [Header("Grabing Variables")]

    [Tooltip("How far away can the puppet grab objects from")]
    public float grabDistance;

    [Tooltip("This distance that the puppet holds objects at, should be the some or lower than the grab distance")]
    public float holdDistance;

    [Space]

    public bool grabbing;
    [Tooltip("Collider of the object that we are grabbing")]
    public Collider grabbingObject;

    public LayerMask grabbingMask;
    [Tooltip("Grabbing mask for when slingshot is active")]
    public LayerMask grabbingMaskSlingshot;
    public Collider colliderThis;
    float grabbedObjectDistance;
    float grabbedObjectHeight;
    float grabStartHeight;
    public float timeSinceSlingshot;

    // Private

    float forceAirborneTimer;
    float timeSinceGrounded;
    bool hasJumped;

    [Space]

    float airTimer;

    public int GetPlayerIndex()
    {
        return secondPlayer ? 1 : 0;
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
		conTut = GetComponent<PuppetContextualTutorial>();
		ikHandler = GetComponent<HandIKHandler>();
		climbIK = GetComponent<ClimbingIK>();
		puppetAnimator = visualReference.GetComponentInChildren<Animator>();
		stringManager = transform.parent.parent.GetComponent<PuppetStringManager>();
		staminaUI = GetComponentInChildren<StaminaBar>();
		audioManager = GetComponentInChildren<PuppetAudio>();

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

    void Update()
    {
        AnimationTick();

    }

    void FixedUpdate()
    {
		// handle movement
		HandleMovement();
		conTut.movementTimer = move != Vector2.zero ? 0 : conTut.movementTimer;
        if (isClimbing == false)
        {
            climbIK.enabled = false;
            GroundDetection();
            HandleJump(); //don't remove
            HandleGrab();
        }
        else // climbing means no ground movement and abilities
        {
            climbIK.enabled = true;
            isGrounded = false;
            ClimbTick();
            conTut.climbTimer = 0;
        }
		
		// force movement to grid
		if (gridManager && gridPoint1 && gridPoint2) {
            ForceToGrid();
        }
        else if (gridManager) {
            Debug.Log("Realigning " + this + " to grid.");
            EstimateGridPoints();
        } else {
			gridPoint1 = null;
			gridPoint2 = null;
		}
		
		// simulate gravity
        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        // Animator Variables
        if (puppetAnimator != null) {
            puppetAnimator.SetFloat("Speed", Mathf.Lerp(new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude, puppetAnimator.GetFloat("Speed"), 0.8f));
            puppetAnimator.SetBool("Grounded", isGrounded);
			puppetAnimator.SetBool("Climbing", isClimbing);
            puppetAnimator.SetFloat("YVelocity", rb.velocity.y);
            puppetAnimator.SetFloat("ForceAir", forceAirborneTimer);
            if (grabbing)
            {
                puppetAnimator.SetBool("GrabbingObject", true);
                Vector3 a = (grabbingObject.gameObject.transform.position - transform.position);
                float difference = Vector3.Distance(new Vector3(a.x, 0, a.z).normalized, new Vector3(move.x, 0, move.y).normalized);
                puppetAnimator.SetFloat("ObjectRelativeMovement", difference);
            }
            else
            {
                puppetAnimator.SetBool("GrabbingObject", false);
                puppetAnimator.SetFloat("ObjectRelativeMovement", 0);
            }
            puppetAnimator.SetBool("StringPulled", beingPulled);
            puppetAnimator.SetBool("PuppetPulled", beingPuppetPulled);

        }

        timeSinceSlingshot += Time.fixedDeltaTime;

    }

    public void AnimationTick()
    {

        if (grabbing == false)
        {
			if (isGrounded)
            {
                if (new Vector3(move.x, 0, move.y).magnitude > 0.05)
                {
                    visualReference.transform.rotation = Quaternion.RotateTowards(visualReference.transform.rotation, Quaternion.LookRotation(new Vector3(move.x, 0, move.y), transform.up), visualRotateSpeed * Time.deltaTime);
                }
            }
            else
            {
                if (new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude > 0.05)
                {
                    visualReference.transform.rotation = Quaternion.RotateTowards(visualReference.transform.rotation, Quaternion.LookRotation(new Vector3(rb.velocity.normalized.x, 0, rb.velocity.normalized.z), transform.up), visualAirRotateSpeed * Time.deltaTime);
                }
            }
			

        }
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
            conTut.climbTimer = 0;
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
        else {
            airTimer = airTimer + Time.fixedDeltaTime;

            // This is to prevent getting stuck hanging permanantly and out of stamina
            if (airTimer > 2) {
                if (stamina < 1)
                {
                    stamina += staminaRegen * Time.fixedDeltaTime * 0.1f;
                    staminaUI.UpdateStaminaVisual(stamina);
                }
                else
                {
                    stamina = 1;
                }
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
                else
                {
                    if (audioManager != null)
                    {
                        audioManager.Land();
                    }
                    puppetAnimator.Play("Land", 0, 0.5f);
                    Instantiate(landParticles, transform.position, Quaternion.identity);
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
        float relativeMaxSpeedX = Math.Abs(groundedMaxSpeed); // ugly sorry, leave for now bc I may use it later, if it's past May, I probably wont lol
        float relativeMaxSpeedY = Math.Abs(groundedMaxSpeed);

        if (grabbing)
        {
            relativeMaxSpeedX = relativeMaxSpeedX / 3;
            relativeMaxSpeedY = relativeMaxSpeedY / 3;
        }

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

            if (beingPuppetPulled) {
                effectiveMove = Vector2.zero;
            }

            if (effectiveMove.x != 0)
            {
                if (effectiveMove.x * relativeMaxSpeedX > rb.velocity.x && effectiveMove.x > 0) // tl;dr we want to move faster right than we are already moving
                {
                    rb.velocity += Vector3.right * groundedAcceleration * Time.fixedDeltaTime * 100;
                }
                else if (effectiveMove.x * relativeMaxSpeedX < rb.velocity.x && effectiveMove.x < 0) // tl;dr we want to move faster left than we are already moving
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
                if (effectiveMove.y * relativeMaxSpeedY > rb.velocity.z && effectiveMove.y > 0) // tl;dr we want to move faster forward than we are already moving
                {
                    rb.velocity += Vector3.forward * groundedAcceleration * Time.fixedDeltaTime * 100;
                }
                else if (effectiveMove.y * relativeMaxSpeedY < rb.velocity.z && effectiveMove.y < 0) // tl;dr we want to move faster back than we are already moving
                {
                    rb.velocity += Vector3.back * groundedAcceleration * Time.fixedDeltaTime * 100;
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

            if (new Vector2(rb.velocity.x, rb.velocity.z).magnitude >= groundedMaxSpeed)
            {
                if (groundedMaxSpeedHardCap)
                {
                    rb.velocity = rb.velocity * groundedMaxSpeed;
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

            conTut.climbTimer += Time.fixedDeltaTime;

            if (beingPulled == false)
            {
                // Air movement stuff, using simpler calculations since precision isn't as important

                if (new Vector2(rb.velocity.x, rb.velocity.z).magnitude < airborneMaxSpeed) // Can only accelerate in the air when velocity is low enough
                    rb.AddForce(new Vector3(move.normalized.x, 0, move.normalized.y) * airborneAcceleration, ForceMode.Acceleration);

                // deceleration (force soft, hard stopping in the air sounds ugly, but I can add if it if nessassary)

                if (new Vector2(rb.velocity.x, rb.velocity.z).magnitude >= airborneMaxSpeed)
                {
                    // Using project so the player can choose to slow down if they move that way, not forced to slow down, otherwise they are just prevented from accelerating
                    // rb.velocity = new Vector3(rb.velocity.x * (1 - airborneDeceleration), rb.velocity.y, rb.velocity.z * (1 - airborneDeceleration)); // Uses 1 - groundedDeceleration to make the variable more intuitive for designers to adjust
                    Vector3 vel = Vector3.Project(new Vector3(move.x, 0, move.y), new Vector3(rb.velocity.x * airborneDeceleration, 0, rb.velocity.z * airborneDeceleration));

                    // If this helps us reduce speed, not gain it, we can do it
                    if ((rb.velocity - vel).magnitude < rb.velocity.magnitude)
                    {
                        rb.velocity -= vel;
                    }
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

        // Please don't remove this part, it acts independently of the jump boost timer

        if (jumpPressed)
        {
            rb.AddForce(Vector3.up * jumpHoldForce * Mathf.Lerp(0, 10, timeSinceGrounded));
        }
        else
        {
            rb.AddForce(Vector3.up * jumpReleaseForce);
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * (1 - jumpReleaseRisingDrag * Time.fixedDeltaTime * 100), rb.velocity.z);
            }
        }

        if (rb.velocity.y > 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * (1 - jumpRisingDrag * Time.fixedDeltaTime * 100), rb.velocity.z);
        }

        // prevents falling too fast
        if (rb.velocity.y < -maxFallSpeed)
        {
            rb.AddForce(Vector3.up * jumpReleaseForce);
        }
    }

    public void StartJump()
    {
        conTut.jumpTimer = 0;
        airTimer = 0;
        if ((isGrounded || timeSinceGrounded <= coyoteTime) && hasJumped == false)
        {
            isGrounded = false;
            hasJumped = true;
            rb.velocity += Vector3.up * initialJumpForce;
            forceAirborneTimer = 0.1f;
            jumpBoostTimer = jumpBoostTime;
            // required for ensuring the puppet gets off the ground consistantly when jumping
            //gameObject.transform.position = transform.position + Vector3.up * 0.05f;
            if (audioManager != null){
                audioManager.Jump();
            }
            // Animator stuff
            if (puppetAnimator != null) {
                //puppetAnimator.SetTrigger("Jump");
                puppetAnimator.Play("JumpStart");
            }
            Instantiate(jumpParticles, transform.position, Quaternion.identity);
        }
    }

    // Grab stuff.
    // NOTE: A fair bit of this might seem redundant, and yeah it kind of is right now but it's foundation for when we want the ability to climb the other puppet's strings, so leave it in there for now.

    void HandleGrab()
    {
		
		// HARPER: debug grab ray
		Debug.DrawRay( transform.position + Vector3.up, visualReference.transform.forward * grabDistance, Color.red);

        if (grabbingObject == null) {
            grabbing = false;
        }

        if (grabbing)
        {
            // HARPER: send message to grabbed object each frame
			grabbingObject.gameObject.SendMessage("OnGrabbing", this, SendMessageOptions.DontRequireReceiver);


            // disable rotation
            if (Vector3.Distance(grabbingObject.gameObject.transform.position, ((visualReference.transform.forward) * grabbedObjectDistance * (1 + holdDistance)) + transform.position + (Vector3.up * grabbedObjectHeight)) > 1f) {
                // we are trying to move it more than a unit in a frame, cancel the grab
            }
            grabbingObject.gameObject.transform.position = ((visualReference.transform.forward) * grabbedObjectDistance * (1 + holdDistance)) + transform.position + (Vector3.up * grabbedObjectHeight);
			

            // Animator
            puppetAnimator.SetBool("GrabbingObject", true);

            // Hand IK

            if (ikHandler != null) {
                // Left
                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up * 1.7f + -visualReference.transform.right * 0.2f, visualReference.transform.forward, out hit, holdDistance + 2.5f, grabbingMask))
                {
                    ikHandler.leftHand.position = hit.point;
                    ikHandler.leftHand.rotation = Quaternion.LookRotation(Vector3.up);
                }
                else
                {
                    ikHandler.leftHand.position = ((visualReference.transform.forward) * (holdDistance + 0.5f)) + transform.position + Vector3.up * 1.7f;
                }

                // Right
                if (Physics.Raycast(transform.position + Vector3.up * 1.7f + visualReference.transform.right * 0.0f, visualReference.transform.forward, out hit, holdDistance + 2.5f, grabbingMask))
                {
                    ikHandler.rightHand.position = hit.point;
                    ikHandler.rightHand.rotation = Quaternion.LookRotation(Vector3.up);
                }
                else
                {
                    ikHandler.rightHand.position = ((visualReference.transform.forward) * (holdDistance + 0.5f)) + transform.position + Vector3.up * 1.7f;
                }

                ikHandler.IKLeft = true;
                ikHandler.IKRight = true;
            }
            else
            {
                Debug.LogError("No HandIK on " + gameObject);
            }

            if (MathF.Abs(transform.position.y - grabStartHeight) > 0.15f) {
                Debug.Log("Y position moving too much, letting go of object");
                GrabRelease();
            }

        }
        else {
            puppetAnimator.SetBool("GrabbingObject", false);
            if (ikHandler != null)
            {
                ikHandler.IKLeft = false;
                ikHandler.IKRight = false;
            }
            else {
                Debug.LogError("No HandIK on " + gameObject);
            }
        }

    }

    public void GrabStart()
    {

        // To ignore raycasts
        int layer = gameObject.layer;
        gameObject.layer = 2;

        // Check if we can grab an object, if we can, ignore the climb part

        RaycastHit playerHit;

        // We check players grabbing each other before anything else
        if (canSlingshot && Physics.Raycast(transform.position + Vector3.up, visualReference.transform.forward, out playerHit, grabDistance, grabbingMaskSlingshot, QueryTriggerInteraction.Collide) && grabbing == false)
        {
            if (playerHit.collider.gameObject == otherPuppet.gameObject)
            {


                if (otherPuppet.grabbingObject.gameObject == this.gameObject)
                {

                    // Add in any additional effects you want on grab release here
                    otherPuppet.GetComponent<Rigidbody>().velocity += (otherPuppet.transform.position - transform.position).normalized * 1f;
                    gameObject.layer = layer;

                    // Playing idle for now
                    puppetAnimator.Play("Movement");
                    otherPuppet.puppetAnimator.Play("Movement");

                    otherPuppet.GrabRelease();

                }
                else {
                    grabbingObject = playerHit.collider;
                    grabbing = true;
                    Physics.IgnoreCollision(grabbingObject, colliderThis, true);
                    //transform.position = hit.point - (visualReference.transform.forward) * grabDistance;
                    Debug.Log("Started Grabbing " + grabbingObject.gameObject);
                    grabbedObjectDistance = Vector3.Distance(playerHit.point, grabbingObject.gameObject.transform.position + Vector3.up); // Not doing the thing >:(((
                    grabbedObjectHeight = grabbingObject.gameObject.transform.position.y;
                    grabStartHeight = transform.position.y;
                    otherPuppet.beingPuppetPulled = false;
                }

            }
            else
            {
                Debug.LogWarning("The player just tried to grab something on the player layer that wasn't the other puppet, might be an issue: Was " + playerHit.collider.gameObject + " and should have been " + otherPuppet.gameObject);
            }
        }
        else {
            RaycastHit hit;

            if (Physics.Raycast(transform.position + Vector3.up, visualReference.transform.forward, out hit, grabDistance, grabbingMask, QueryTriggerInteraction.Collide) && grabbing == false)
            {

                grabbingObject = hit.collider;
                grabbing = true;
                Physics.IgnoreCollision(grabbingObject, colliderThis, true);
                Debug.Log("Started Grabbing " + grabbingObject.gameObject);
                grabbedObjectDistance = Vector3.Distance(hit.point, grabbingObject.gameObject.transform.position + Vector3.up); // Not doing the thing >:(((
                grabbingObject.gameObject.layer = 11;
                grabbedObjectHeight = grabbingObject.gameObject.transform.position.y;
                grabStartHeight = transform.position.y;
                // HELLO harper here. my objects want to know when they're being grabbed (and by who) so bam
                grabbingObject.gameObject.SendMessage("OnGrab", this, SendMessageOptions.DontRequireReceiver);

            }
            else
            {
                if (stamina > 0)
                {
                    // First, we need to find out where we are in relation to our own string
                    SetClimbValue();

                    //isClimbing = true;
                    //distanceToHook = Vector3.Distance(transform.position, effectiveRoot);

                    //rb.velocity = Vector3.zero; // makes climbing feel a little worse, but prevents some exploits, consider turning of if desired.

                    if (isGrounded)
                    {
                        conTut.jumpTimer += 1;
                    }

                }
            }
        }

        // Undoing the ignoreraycast change
        gameObject.layer = layer; // should be 6 but just in case

        if (grabbingObject != null)
        {
            grabbingObject.gameObject.transform.position = ((visualReference.transform.forward) * grabbedObjectDistance * (1 + holdDistance)) + transform.position + (Vector3.up * grabbedObjectHeight);
        }

    }

    public void GrabRelease()
    {
        if (isClimbing) {
            //rb.velocity *= 0.5f;
        }
        climbValue = 0;
        isClimbing = false;

        // grabbing stuff

        if (grabbingObject.gameObject == otherPuppet.gameObject)
        {
            otherPuppet.beingPuppetPulled = false;
            Physics.IgnoreCollision(grabbingObject, colliderThis, false);

            if (otherPuppet.beingPulled) {
                grabbingObject.attachedRigidbody.velocity = (otherPuppet.thisStringRoot.transform.position - grabbingObject.transform.position).normalized * SlingshotForce;
                grabbingObject.attachedRigidbody.velocity = new Vector3(grabbingObject.attachedRigidbody.velocity.x, grabbingObject.attachedRigidbody.velocity.y * SlingshotForceYMulti, grabbingObject.attachedRigidbody.velocity.z);
                grabbingObject.transform.position = grabbingObject.transform.position + (grabbingObject.attachedRigidbody.velocity * Time.fixedDeltaTime); // ensures we get lift if applicable
                otherPuppet.isGrounded = false;
                otherPuppet.timeSinceSlingshot = 0;
            }

        }
        else
        {
            if (grabbingObject != null)
            {
                Physics.IgnoreCollision(grabbingObject, colliderThis, false);
                grabbing = false;
                if (grabbingObject.gameObject.layer != 6)
                    grabbingObject.gameObject.layer = 9;
                //grabbingObject.attachedRigidbody.freezeRotation = true;
                grabbingObject.gameObject.SendMessage("OnReleased", this, SendMessageOptions.DontRequireReceiver);
            }
        }

        grabbing = false;
        grabbingObject = colliderThis;
        grabbedObjectDistance = 0;
    }

    public void ClimbTick() {
        airTimer = 0;

        // Swinging code is adapted from the old, inelastic string code

        Vector3 difference = (effectiveRoot - transform.position);

        Vector3 crossOut = Vector3.Cross(rb.velocity, difference);

        thisStringRoot.angleRef.transform.rotation = Quaternion.LookRotation(crossOut); // This' Z
        thisStringRoot.angleRef.transform.position = transform.position;

        thisStringRoot.angleRef2.transform.rotation = Quaternion.LookRotation(difference); // This' X
        thisStringRoot.angleRef2.transform.position = transform.position;

        Vector3 oldVel = rb.velocity;

        rb.velocity =
            Vector3.Project(rb.velocity, thisStringRoot.angleRef2.transform.up) +
            Vector3.Project(rb.velocity, thisStringRoot.angleRef2.transform.right);

        rb.velocity = rb.velocity * (1 - (Time.fixedDeltaTime * 0.5f));

        rb.velocity = new Vector3(rb.velocity.x, oldVel.y, rb.velocity.z);

        float oldY = rb.transform.position.y;
        Vector3 vector = (rb.gameObject.transform.position - rb.transform.position);

        vector = (rb.gameObject.transform.position - rb.transform.position);

        //rb.gameObject.transform.position = new Vector3(vector.x, vector.y, vector.z) - (difference.normalized * grabStartHeight) + effectiveRoot;
        rb.transform.position = new Vector3(rb.transform.position.x, oldY, rb.transform.position.z);





        //stamina -= staminaDrain * Time.fixedDeltaTime;

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

    public void EstimateGridPoints() // Used for getting the puppets to a grid point
    {


        float closest = 999;
        float second = 999;

        for (int i = 0; i < gridManager.points.Count; i++)
        {
            if (Vector3.Distance(transform.position, gridManager.points[i].transform.position) < closest)
            {
                second = closest;
                gridPoint2 = gridPoint1;
                closest = Vector3.Distance(transform.position, gridManager.points[i].transform.position);
                gridPoint1 = gridManager.points[i];
            }
            else {
                if (Vector3.Distance(transform.position, gridManager.points[i].transform.position) < second)
                {
                    second = Vector3.Distance(transform.position, gridManager.points[i].transform.position);
                    gridPoint2 = gridManager.points[i];
                }
            }
        }
    }

    public void ForceToGrid()
    {

        Debug.DrawLine(gridPoint1.transform.position, gridPoint1.transform.position + Vector3.up * 100f, Color.green);
        Debug.DrawLine(gridPoint2.transform.position, gridPoint2.transform.position + Vector3.up * 100f, Color.magenta);

        // Grid redirecting (Doing force to line after, so you can change your grid before getting forced onto it)

        GridPoint at = null;
        GridPoint notAt = null;
        GridPoint oldG1 = gridPoint1;

        if (Vector3.Distance(new Vector3(gridPoint1.transform.position.x, transform.position.y, gridPoint1.transform.position.z), transform.position) <= pointRedirectDistance)
        {
            at = gridPoint1;
            notAt = gridPoint2;
        }
        if (Vector3.Distance(new Vector3(gridPoint2.transform.position.x, transform.position.y, gridPoint2.transform.position.z), transform.position) <= pointRedirectDistance)
        {
            at = gridPoint2;
            notAt = gridPoint1;
        }

        GridPoint g2 = null;

        if (at != null)
        {
            // We are close enough to change to a different line

            float closest = 999;
            gridPoint1 = at;

            for (int i = 0; i < at.connectedPoints.Count; i++)
            {

                // Because points might not be evenly spread, we need to move based on distance to directions, not just positions

                Vector3 direction = at.connectedPoints[i].transform.position - gridPoint1.transform.position;
                direction = new Vector3(direction.x, 0, direction.z).normalized * 10f;

                float penalty = 0f;

                float angle = Vector3.Angle(gridPoint1.transform.position - gridPoint2.transform.position, gridPoint1.transform.position - at.connectedPoints[i].transform.position);
                if (angle > 170 && angle < 190)
                {
                    penalty = directionChangeBoost;
                }

                if (Vector3.Distance((transform.position + (new Vector3(move.x, 0, move.y) * Mathf.Min((6f - penalty), 1f)) - new Vector3(gridPoint1.transform.position.x, transform.position.y, gridPoint1.transform.position.z)), new Vector3(direction.x, transform.position.y, direction.z)) <= closest && at.connectedPoints[i] != at)
                {

                    closest = Vector3.Distance(transform.position - gridPoint1.transform.position, direction);
                    g2 = at.connectedPoints[i];
                }

            }

        }

        if (g2 != null)
        {
            // We tried to change lines

            float angle = Vector3.Angle(gridPoint1.transform.position - gridPoint2.transform.position, gridPoint1.transform.position - g2.transform.position);

            if (angle > 170 && angle < 190)
            {
                // We are moving roughly in the same direction, check again if we should ACTUALLY change lines
                Vector3 aimDirection = g2.transform.position - at.transform.position;
                Vector3 currentDirection = notAt.transform.position - at.transform.position;
                Vector3 playerTransform = transform.position - at.transform.position;

                aimDirection = new Vector3(aimDirection.x, 0, aimDirection.z);
                currentDirection = new Vector3(currentDirection.x, 0, currentDirection.z);
                playerTransform = new Vector3(playerTransform.x, 0, playerTransform.z);

                if (Vector3.Distance(aimDirection.normalized, playerTransform.normalized) > Vector3.Distance(currentDirection.normalized, playerTransform.normalized))
                {
                    Debug.Log("We are not close enough, not switching lines");
                }
                else
                {
                    gridPoint2 = g2;
                }

            }
            else
            {
                gridPoint2 = g2;
            }

            //Debug.Log(Vector3.Angle(gridPoint1.transform.position - gridPoint2.transform.position, gridPoint1.transform.position - g2.transform.position));

        }

        // After (potentally) changing line, make sure we are not beyond said line

        // Currently disabled, has issues that makes it feel weird but here if we need it later, has a strange "snapping" effect when moving between lines
        if (Vector3.Distance(new Vector3(gridPoint1.transform.position.x, transform.position.y, gridPoint1.transform.position.z), new Vector3(gridPoint2.transform.position.x, transform.position.y, gridPoint2.transform.position.z)) <
            Vector3.Distance(new Vector3(gridPoint1.transform.position.x, transform.position.y, gridPoint1.transform.position.z), transform.position))
        {
            //transform.position = new Vector3(gridPoint2.transform.position.x, transform.position.y, gridPoint2.transform.position.z);
        }

        if (Vector3.Distance(new Vector3(gridPoint1.transform.position.x, transform.position.y, gridPoint1.transform.position.z), new Vector3(gridPoint2.transform.position.x, transform.position.y, gridPoint2.transform.position.z)) <
            Vector3.Distance(new Vector3(gridPoint2.transform.position.x, transform.position.y, gridPoint2.transform.position.z), transform.position))
        {
            //transform.position = new Vector3(gridPoint1.transform.position.x, transform.position.y, gridPoint1.transform.position.z);
        }

        // Debug lighting up current line

        Debug.DrawLine(gridPoint1.transform.position, gridPoint2.transform.position);

        // it's my budget method of making a big line lmao
        for (int i = 0; i < 20; i++)
        {
            Debug.DrawLine(gridPoint1.transform.position + new Vector3(UnityEngine.Random.Range(-0.05f, 0.05f), UnityEngine.Random.Range(-0.05f, 0.05f), UnityEngine.Random.Range(-0.05f, 0.05f)), gridPoint2.transform.position + new Vector3(UnityEngine.Random.Range(-0.05f, 0.05f), UnityEngine.Random.Range(-0.05f, 0.05f), UnityEngine.Random.Range(-0.05f, 0.05f)));
        }

        // Force to current line

        Vector3 relativePosition = transform.position - gridPoint1.transform.position;

        Vector3 relativeLineVector = gridPoint1.transform.position - gridPoint2.transform.position;

        relativePosition = new Vector3(relativePosition.x, 0, relativePosition.z); // Don't want to touch the Y axis here
        relativeLineVector = new Vector3(relativeLineVector.x, 0, relativeLineVector.z); // or here

        Vector3 rbOld = rb.velocity;
        rb.velocity = Vector3.Project(rb.velocity, relativeLineVector);
        rb.velocity = new Vector3(rb.velocity.x, rbOld.y, rb.velocity.z);
        rb.velocity = Vector3.Lerp(rb.velocity, rbOld, 0.95f);

        Vector3 newPos = Vector3.Project(relativePosition, relativeLineVector) + gridPoint1.transform.position;

        // Force back if we have extended beyond our line

        //Debug.Log("1 to 2: " + Vector3.Distance(new Vector3(gridPoint1.transform.position.x, 0, gridPoint1.transform.position.z), new Vector3(gridPoint2.transform.position.x, 0, gridPoint2.transform.position.z)));
        //Debug.Log("P to 1: " + Vector3.Distance(new Vector3(gridPoint1.transform.position.x, 0, gridPoint1.transform.position.z), new Vector3(newPos.x, 0, newPos.z)));
        //Debug.Log("P to 2: " + Vector3.Distance(new Vector3(gridPoint2.transform.position.x, 0, gridPoint2.transform.position.z), new Vector3(newPos.x, 0, newPos.z)));

        if (Vector3.Distance(new Vector3(gridPoint1.transform.position.x, 0, gridPoint1.transform.position.z), new Vector3(gridPoint2.transform.position.x, 0, gridPoint2.transform.position.z)) <
            Vector3.Distance(new Vector3(gridPoint1.transform.position.x, 0, gridPoint1.transform.position.z), new Vector3(newPos.x, 0, newPos.z)))
        {
            newPos = new Vector3(gridPoint2.transform.position.x, newPos.y, gridPoint2.transform.position.z);
        }

        if (Vector3.Distance(new Vector3(gridPoint2.transform.position.x, 0, gridPoint2.transform.position.z), new Vector3(gridPoint1.transform.position.x, 0, gridPoint1.transform.position.z)) <
            Vector3.Distance(new Vector3(gridPoint2.transform.position.x, 0, gridPoint2.transform.position.z), new Vector3(newPos.x, 0, newPos.z)))
        {
            newPos = new Vector3(gridPoint1.transform.position.x, newPos.y, gridPoint1.transform.position.z);
        }

        transform.position = Vector3.Lerp(new Vector3(newPos.x, transform.position.y, newPos.z), transform.position, 0.9f);

    }

}
