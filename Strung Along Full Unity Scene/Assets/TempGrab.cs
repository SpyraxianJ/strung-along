using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempGrab : MonoBehaviour
{

    PlayerControls controls;
    public Rigidbody grabbed;
    public SphereCollider grabDetect;
    public Vector3 lastFrame;

    public bool grabPressed = false;

    [Space]

    public bool secondPlayer;

    void Awake()
    {
        if (secondPlayer == false)
        {
            controls = new PlayerControls();
            controls.Player.Grab.performed += ctx =>
            {
                grabPressed = ctx.ReadValueAsButton();
                Debug.Log(ctx.ReadValueAsButton());
            };
            controls.Player.Grab.canceled += ctx =>
            {
                grabPressed = false;
            };
        }
        else
        {
            controls = new PlayerControls();
            controls.Player.Grab2.performed += ctx =>
            {
                grabPressed = ctx.ReadValueAsButton();
                Debug.Log(ctx.ReadValueAsButton());
            };
            controls.Player.Grab2.canceled += ctx =>
            {
                grabPressed = false;
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (grabDetect.enabled == true && grabbed == null) {
            grabDetect.enabled = false;
        }

        if (grabbed != null)
        {
            grabbed.isKinematic = true;
        }
        if (grabPressed == true)
        {
            grabDetect.enabled = true;
        }
        if (grabPressed == false)
        {
            grabDetect.enabled = false;
        }
        if (grabPressed == true && grabbed != null)
        {
            grabDetect.enabled = true;
            if (grabbed != null) {
                Vector3 move = transform.position - lastFrame;
                grabbed.position += new Vector3(move.x, 0, move.z); // y movement
            }
        }
        if (grabPressed == false && grabbed != null)
        {
            grabDetect.enabled = false;
            grabbed.isKinematic = true;
            grabbed = null;
        }

        // end
        lastFrame = transform.position;

    }

    private void OnTriggerEnter(Collider other)
    {
        grabbed = other.GetComponent<Rigidbody>();
        grabbed.isKinematic = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (grabbed != null) {
            grabbed.isKinematic = true;
        }
        grabbed = null;
    }
}
