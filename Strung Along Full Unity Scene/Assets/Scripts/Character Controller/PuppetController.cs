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
    public PuppetStringManager stringManager;
    public StringRoot thisStringRoot;
    public StaminaBar staminaUI;
    public GameObject visualReference;
    public PuppetAudio audioManager;
    public Animator puppetAnimator;
    public PuppetContextualTutorial conTut;
    public HandIKHandler ikHandler;
    public ClimbingIK climbIK;

    [Space]

    public GameObject jumpParticles;
    public GameObject landParticles;

    [Space]

    [SerializeField]
    private int playerIndex = 0;

    private Rigidbody rb;

    public Vector2 move;
    public bool movePressed;
    public bool jumpPressed;
    public bool jumpReleased = true;
    public bool grabPressed;

    private float speed = 0;
    [Tooltip("Determines if the puppet is currently on the ground or not, public for unity inspector debugging purposes, can be made private later without issue")]
    public bool isGrounded = true; // Keep public, other things use this as a reference
    public bool isClimbing = false;
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

    [Space]

    [Tooltip("These are the layers that the puppet will consider ground when standing on them")]
    public LayerMask groundMask;

    [Space]

    [Tooltip("This is the amount that this puppet it pushed downwards into the ground every frame, allows for travelling down slopes or stairs without going airborne, but too high will result in strange teleporting")]
    [Range(0, 3)]
    public float groundedDownPerFrame;
    public Vector3 effectiveRoot;

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
    public Collider colliderThis;
    float grabbedObjectDistance;
    float grabbedObjectHeight;
    float grabStartHeight;

    // Private

    float forceAirborneTimer;
    float timeSinceGrounded;
    bool hasJumped;

    [Space]

    public TempGrab tempGrab;
    float airTimer;

    public int GetPlayerIndex()
    {
        return playerIndex;
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


    private void Update()
    {

        puppetAnimator.SetFloat("ForceAir", forceAirborneTimer);
        AnimationTick();

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (move != Vector2.zero) {
            conTut.movementTimer = 0;
        }

        if (isClimbing == false) // Do normal stuff
        {
            climbIK.enabled = false;
            GroundDetection();
            HandleMovement();
            HandleJump(); //don't remove
            HandleGrab();
        }
        else // Pause all normal movement stuff, just climb
        {
            climbIK.enabled = true;
            isGrounded = false;
            HandleMovement();
            ClimbTick();
            conTut.climbTimer = 0;
        }

        //AnimationTick();

        //Debug.Log(move.x + " " + move.y);

        // Temp gross grab compatability
        if (tempGrab.grabbed != null && isClimbing == true)
        {
            GrabRelease();
        }

        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        // Animator Variables

        if (puppetAnimator != null) {
            puppetAnimator.SetFloat("Speed", new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude);
            puppetAnimator.SetBool("Grounded", isGrounded);
            puppetAnimator.SetFloat("YVelocity", rb.velocity.y);
            puppetAnimator.SetFloat("ForceAir", forceAirborneTimer);
            if (grabbing == true)
            {
                puppetAnimator.SetBool("GrabbingObject", true);
                Vector3 a = (grabbingObject.gameObject.transform.position - transform.position);
                float difference = Vector3.Distance(new Vector3(a.x, 0, a.z).normalized, new Vector3(move.x, 0, move.y).normalized);
                puppetAnimator.SetFloat("ObjectRelativeMovement", difference);
                //Debug.Log(difference);
            }
            else
            {
                puppetAnimator.SetBool("GrabbingObject", false);
                puppetAnimator.SetFloat("ObjectRelativeMovement", 0);
            }
            if (isClimbing) {

            }
            else
            {

            }
            puppetAnimator.SetBool("Climbing", isClimbing);
        }

    }

    public void AnimationTick()
    {

        if (grabbing == false)
        {

            if (tempGrab.grabbed != null)
            {
                Vector3 a = (tempGrab.grabbed.gameObject.transform.position - transform.position);
                float difference = Vector3.Distance(new Vector3(a.x, 0, a.z).normalized, new Vector3(move.x, 0, move.y).normalized);
                visualReference.transform.rotation = Quaternion.RotateTowards(visualReference.transform.rotation, Quaternion.LookRotation(new Vector3(a.x, 0, a.z), transform.up), visualAirRotateSpeed * Time.deltaTime);
            }
            else
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

        if (grabbing)
        {
            // disable rotation
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

        // Check if we can grab an object, if we can, ignore the climb part

        RaycastHit hit;

        if (Physics.Raycast(transform.position + Vector3.up, visualReference.transform.forward, out hit, grabDistance, grabbingMask) && grabbing == false)
        {

            grabbingObject = hit.collider;
            grabbing = true;
            Physics.IgnoreCollision(grabbingObject, colliderThis, true);
            //transform.position = hit.point - (visualReference.transform.forward) * grabDistance;
            Debug.Log("Started Grabbing " + grabbingObject.gameObject);
            grabbedObjectDistance = Vector3.Distance(hit.point, grabbingObject.gameObject.transform.position + Vector3.up); // Not doing the thing >:(((
            grabbingObject.gameObject.layer = 11;
            grabbingObject.attachedRigidbody.freezeRotation = true;
            grabbedObjectHeight = grabbingObject.gameObject.transform.position.y;
            grabStartHeight = transform.position.y;

        }
        else 
        {
            if (stamina > 0)
            {
                // First, we need to find out where we are in relation to our own string
                SetClimbValue();

                isClimbing = true;

                //rb.velocity = Vector3.zero; // makes climbing feel a little worse, but prevents some exploits, consider turning of if desired.

                if (isGrounded)
                {
                    conTut.jumpTimer += 1;
                }

            }
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

        if (grabbingObject != null) {
            Physics.IgnoreCollision(grabbingObject, colliderThis, false);
            grabbing = false;
            grabbingObject.gameObject.layer = 9;
            grabbingObject.attachedRigidbody.freezeRotation = true;
        }
        grabbingObject = null;
        grabbedObjectDistance = 0;

    }

    public void ClimbTick() {
        airTimer = 0;
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
