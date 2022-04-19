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
    private bool grounded = true;
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
            if (jumpPressed)
            {
                Jump();
            }
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
        GroundDetection();
        HandleMovement();
        HandleJump();

        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }

    public void GroundDetection()
    {
        // Ground Detection

        if (grounded)
        {
            hasJumped = false;
            transform.position -= (transform.up * groundedDownPerFrame);
        }

        if (forceAirborneTimer > 0)
        {
            // Ground detection is not allowed
            forceAirborneTimer -= Time.fixedDeltaTime;
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
                bool lastGrounded = false;

                if (grounded)
                {
                    // Reverse the downwards from being grounded, ONLY if we were grounded earlier, thus can confirm we did this earlier this frame
                    transform.position = transform.position + transform.up * groundedDownPerFrame;
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
                    transform.position += (transform.up * groundedDownPerFrame);
                }
                grounded = false;
            }
        }

        if (grounded)
        {
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

        if (grounded)
        {
            if (speed < relativeMaxSpeed)
            {
                speed += relativeMaxSpeed / groundedMaxSpeed;
            }
            else if (speed > relativeMaxSpeed - 1 && speed < relativeMaxSpeed + 1)
            {
                //hold speed
            }
            else
            {
                speed -= stopCurve.Evaluate(speed / groundedMaxSpeed);
            }
            animator.SetFloat("Speed", speed);

            if (movementDirection != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotateSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (movementDirection != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, (rotateSpeed/airRotateModifier) * Time.deltaTime);
            }
        }
    }

    void HandleJump()
    {
        if (jumpPressed)
        {
            Jump();
        }
    }
    public void Jump()
    {
        if ((grounded || timeSinceGrounded <= coyoteTime) && hasJumped == false)
        {
            grounded = false;
            hasJumped = true;
            rb.velocity = new Vector3(rb.velocity.x, initalJumpVelocity, rb.velocity.z);
            transform.position += Vector3.up * 0.05f;
            forceAirborneTimer = 0.1f;
            jumpBoostTimer = jumpBoostTime;

        }

        // Jump specific

        if (jumpBoostTimer > 0)
        {
            if (jumpPressed)
            {
                jumpBoostTimer -= Time.fixedDeltaTime;
                rb.AddForce(Vector3.up * Mathf.Lerp(0, jumpBoostForce, jumpBoostTimer / jumpBoostTime));
            }
            else
            {
                // Once jump is released, you can no longer continue rising
                jumpBoostTimer = 0;
            }
        }

        // General airborne

        if (jumpPressed)
        {
            rb.AddForce(Vector3.up * jumpHoldForce);
        }
        else
        {
            rb.AddForce(Vector3.up * jumpReleaseForce);
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * (1 - jumpReleaseRisingDrag * Time.fixedDeltaTime * 100), rb.velocity.z);
            }
        }

        if (rb.velocity.y < -maxFallSpeed)
        {
            rb.AddForce(Vector3.up * jumpReleaseForce);
        }

    }
}
