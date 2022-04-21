using System; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testMove : MonoBehaviour
{
    public float gravity = 9.8f;

    private Animator animator;
    private PlayerControls controls;
    public AnimationCurve stopCurve;
    private Rigidbody rb;

    private bool movePressed;
    private Vector2 move;
    private bool jumpPressed;

    private float speed = 0;
    private bool isGrounded = true;
    private bool jumpReleased = true;
    private float groundedMaxSpeed = 50;

    [Header("State")]

    [Tooltip("Used to determine if this puppet is the second player, if true it will use the second player's controls and other play-specific things")]
    public bool secondPlayer;

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
    public float initalJumpForce;
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
    [Tooltip("The time that has to expire before you can jump again")]
    public float jumpDisabledTime;
    private float jumpGracePeriod;

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
    private int landingRayNumber = 10;
    [Tooltip("This is how far down the raycasts for ground detection extent from the puppet, higher values allow for stepping up larger steps")]
    public float groundRayLength;
    private float landingRayLength = 1f;
    [Tooltip("This is how wide the ground detection rays are around the puppet, should be scaled with the capsule radius")]
    public float groundedRayDiameter;
    [Tooltip("This is how wide the landing detection rays are around the puppet, MUST BE LARGER THAN groundedRayRadius by 0.05")]
    public float landingRayDiameter;

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
    float rotateSpeed = 720;
    float airRotateModifier = 9;


    void Awake()
    {
        controls = new PlayerControls();
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
    }
    void OnEnable()
    {
        controls.Player.Enable();
    }
    void OnDisable()
    {
        controls.Player.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float fallSpeedMod = 1;

        animator.applyRootMotion = isGrounded; //only use root motion when grounded

        GroundDetection();
        HandleMovement();
        HandleJump();
        Debug.Log(rb.velocity.x + " " + rb.velocity.y + " " + rb.velocity.z);
        if (rb.velocity.y < 0) //slightly faster falling to make jumping feel better
            fallSpeedMod = 1.5f;

        rb.AddForce(Vector3.down * gravity * fallSpeedMod, ForceMode.Acceleration);
    }

    public void GroundDetection()
    {
        // Ground Detection

        if (isGrounded)
        {
            if (hasJumped)
            {
                jumpGracePeriod += Time.fixedDeltaTime;
            }
            if (jumpGracePeriod > jumpDisabledTime)
            {
                hasJumped = false;
                jumpGracePeriod = 0;
            }

            transform.position -= (transform.up * groundedDownPerFrame);
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
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            }
            else
            {
                if (isGrounded)
                {
                    // Reverse the downwards from being grounded, ONLY if we were grounded earlier, thus can confirm we did this earlier this frame
                    transform.position += (transform.up * groundedDownPerFrame);
                }
                isGrounded = false;
            }
        }

        if (isGrounded)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isLanding", true);
            timeSinceGrounded = 0;
        }
    }
    
    void HandleMovement()
    {
        Vector3 movementDirection = new Vector3(move.x, 0, move.y);
        movementDirection.Normalize();

        float relativeMaxSpeedX = Math.Abs(groundedMaxSpeed * move.x);
        float relativeMaxSpeedY = Math.Abs(groundedMaxSpeed * move.y);
        int relativeMaxSpeed = (int)Math.Floor(new Vector2(relativeMaxSpeedX, relativeMaxSpeedY).magnitude);
        Debug.Log(relativeMaxSpeed);

        if (isGrounded)
        {
            animator.SetBool("isLanding", false);

            if (speed < relativeMaxSpeed)
            {
                speed += relativeMaxSpeed / groundedMaxSpeed;
            }
            else if (speed > relativeMaxSpeed - 1 && speed < relativeMaxSpeed + 1)
            {
                //hold speed (prevents glitchy animation effect)
            }
            else
            {
                speed -= stopCurve.Evaluate(speed/groundedMaxSpeed);
            }
            animator.SetFloat("speed", speed, 0.05f, Time.deltaTime);

            if (movementDirection != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotateSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (rb.velocity.y < -0.5)
            {
                animator.SetBool("isJumping", false);
                animator.SetBool("isFalling", true);
            }
            //much slower rotation while in air
            if (movementDirection != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, (rotateSpeed/airRotateModifier) * Time.deltaTime);
            }

            rb.AddForce(new Vector3(move.normalized.x, 0, move.normalized.y) * airborneAcceleration, ForceMode.Acceleration);

            // deceleration (force soft, hard stopping in the air sounds ugly, but I can add if it if nessassary)

            if (new Vector2(rb.velocity.x, rb.velocity.z).magnitude >= airborneMaxSpeed)
            {
                rb.velocity = new Vector3(rb.velocity.x * (1 - airborneDeceleration), rb.velocity.y, rb.velocity.z * (1 - airborneDeceleration)); // Uses 1 - groundedDeceleration to make the variable more intuitive for designers to adjust
            }

            timeSinceGrounded += Time.fixedDeltaTime;
            animator.SetFloat("airtime", timeSinceGrounded, 0.05f, Time.deltaTime);

            List<RaycastHit> landingRays = objectRayGenerator(landingRayNumber, landingRayLength, landingRayDiameter, false);

            if ((landingRays.Count > 0 && animator.GetBool("isFalling")))
            {
                animator.SetBool("isFalling", false);
                animator.SetBool("isLanding", true);
            }
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
            rb.AddForce(Vector3.up * initalJumpForce, ForceMode.Impulse);
            forceAirborneTimer = 0.1f;
            jumpBoostTimer = jumpBoostTime;
            animator.SetBool("isLanding", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isJumping", true);
        }
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
}
