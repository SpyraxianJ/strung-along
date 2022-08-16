using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{

    public Image staminaBar;
    float maxScale;

    [Space]

    public Image staminaRootImage;

    [Space]

    public float timeUntilfade;
    float fadetimer;
    [Tooltip("This is the curve for the opacity of the stamina bar, it starts right after the last stamina visual update so if you don't want it to start fading instantly, give a buffer in the curve")]
    public AnimationCurve fadeAnimation;

    // Start is called before the first frame update
    void Start()
    {
        // Comment this line out to disable the auto-scale set, which requires setting maxScale to public so it can actually be changed
        maxScale = staminaBar.transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {

        staminaBar.color = new Color(staminaBar.color.r, staminaBar.color.g, staminaBar.color.b, fadeAnimation.Evaluate((fadetimer / timeUntilfade)));
        staminaRootImage.color = new Color(staminaRootImage.color.r, staminaRootImage.color.g, staminaRootImage.color.b, fadeAnimation.Evaluate((fadetimer / timeUntilfade)));

        if (fadetimer > 0)
        {
            fadetimer -= Time.deltaTime;
        }

    }

    public void UpdateStaminaVisual(float value) 
    {
        fadetimer = timeUntilfade;
        staminaBar.transform.localScale = new Vector3(staminaBar.transform.localScale.x, Mathf.Lerp(0, maxScale, value), staminaBar.transform.localScale.z);
    }
}
