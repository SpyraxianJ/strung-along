using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropButton : MonoBehaviour
{
	private GameObject switchObject;
	private float switchRestPos;
	private float switchPressPos = 0.3f;
	
    // Start is called before the first frame update
    void Start()
    {
        switchObject = transform.GetChild(0).gameObject;
		switchObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
		
		switchRestPos = switchObject.transform.localPosition.x;
    }
	
	void onEnable() {
		switchObject.GetComponent<Collider>().enabled = true;
	}
	
	void onDisable() {
		switchObject.GetComponent<Collider>().enabled = false;
	}

    // Update is called once per frame
    void Update()
    {
		
		if ( switchObject.transform.localPosition.x > switchRestPos ) {
			switchObject.transform.localPosition = new Vector3(switchRestPos, 0, 0);
		} else if ( switchObject.transform.localPosition.x <= switchPressPos ) {
			switchObject.transform.localPosition = new Vector3(switchPressPos, 0, 0);
		} else {
			switchObject.transform.localPosition = new Vector3(switchObject.transform.localPosition.x, 0, 0);
		}
		
    }
}
