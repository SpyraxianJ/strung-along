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
    [Range(0, 30)]
    public float groundedMaxSpeed;
    [Tooltip("If this is true, grounded speed can't go past the max speed, if false, the puppet just decelerate using the deceleration variables when past max speed")]
    public bool groundedMaxSpeedHardCap;

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
}
