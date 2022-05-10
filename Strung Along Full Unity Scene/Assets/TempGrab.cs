using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempGrab : MonoBehaviour
{

    PlayerControls controls;
    public Rigidbody grabbed;
    public SphereCollider grabDetect;
    public Vector3 lastFrame;

    bool grabPressed = false;

    [Space]

    bool secondPlayer;

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
        else { 
        }
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
                grabbed.position += transform.position - lastFrame;
            }
        }
        if (grabPressed == false && grabbed != null)
        {
            grabDetect.enabled = false;
            grabbed.isKinematic = false;
            grabbed = null;
        }

        // end
        lastFrame = transform.position;

    }

    private void OnTriggerEnter(Collider other)
    {
        grabbed = other.GetComponent<Rigidbody>();
        Debug.Log("piss");
        grabbed.isKinematic = true;
    }
}
