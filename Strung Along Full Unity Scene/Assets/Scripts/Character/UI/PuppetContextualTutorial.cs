using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuppetContextualTutorial : MonoBehaviour
{

    public float lerpSpeed;

    [Space]

    [Tooltip("How long without moving for the moving tutorial to appear")]
    public float movementTime;
    public float movementTimer;
    public Image movementImage;

    [Space]

    [Tooltip("How long without moving for the moving tutorial to appear")]
    public float climbTime;
    public float climbTimer;
    public Image climbImage;

    [Space]

    [Tooltip("How long without moving for the moving tutorial to appear")]
    public float jumpTime;
    public float jumpTimer;
    public Image jumpImage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        movementTimer += Time.fixedDeltaTime;
        //climbTimer += Time.fixedDeltaTime; Done through PuppetController
        //jumpTimer += Time.fixedDeltaTime; Done through puppet controller

        if (jumpTimer > jumpTime)
        {
            jumpImage.color = Color.Lerp(jumpImage.color, new Color(1, 1, 1, 1), lerpSpeed * Time.fixedDeltaTime);
        }
        else
        {
            jumpImage.color = Color.Lerp(jumpImage.color, new Color(1, 1, 1, 0), lerpSpeed * Time.fixedDeltaTime);
        }

        if (climbTimer > climbTime)
        {
            climbImage.color = Color.Lerp(climbImage.color, new Color(1, 1, 1, 1), lerpSpeed * Time.fixedDeltaTime);
        }
        else
        {
            climbImage.color = Color.Lerp(climbImage.color, new Color(1, 1, 1, 0), lerpSpeed * Time.fixedDeltaTime);
        }

        if (movementTimer > movementTime)
        {
            movementImage.color = Color.Lerp(movementImage.color, new Color(1, 1, 1, 1), lerpSpeed * Time.fixedDeltaTime);
        }
        else
        {
            movementImage.color = Color.Lerp(movementImage.color, new Color(1, 1, 1, 0), lerpSpeed * Time.fixedDeltaTime);
        }


    }

}
