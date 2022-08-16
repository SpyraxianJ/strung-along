using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinIndicator : MonoBehaviour
{

    public PuppetStringManager manager;
    public SpriteRenderer spriteRenderer;
    public Color uiColor;
    public Color uiTightColor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (manager.tangle == 0)
        {
            spriteRenderer.enabled = false;
        }
        else {
            spriteRenderer.enabled = true;
            Color current = Color.Lerp(uiColor, uiTightColor, (Mathf.Abs(manager.tangle) / Mathf.Max(0.0001f, manager.maxTangle / 2)) - 1);
            spriteRenderer.color = new Color(current.r, current.g, current.b, Mathf.Abs(manager.tangle) / Mathf.Max(0.0001f, manager.maxTangle/2));
        }

        if (manager.tangle > 0)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }

        //transform.position = new Vector3(manager.effectiveRoot.x, Mathf.Lerp(manager.puppet1.transform.position.y, manager.puppet2.transform.position.y, 0.5f) + 0.1f, manager.effectiveRoot.z);
        transform.position = Vector3.Lerp(manager.puppet1.transform.position, manager.puppet2.transform.position, 0.5f);
        transform.rotation = Quaternion.Euler(new Vector3(90, manager.tangle * 100f, transform.rotation.z));

    }
}
