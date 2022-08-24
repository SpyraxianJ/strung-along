using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicIntro : MonoBehaviour
{
	
    void Start() {
		
    }
	
	public void Play() {
		StopAllCoroutines();
		StartCoroutine( PlayRoutine() );
	}
	
	IEnumerator PlayRoutine() {
		
		// wait 2 seconds.
		yield return new WaitForSeconds(2);
		
		// dim the stage lights.
		
		
		
		// show text 3 seconds: The audience is eager for a perfect show.
		// show text 4 seconds: Do not be meager, embrace the stage glow.
		// clear text.
		// show text 3 seconds: Follow your scripts and take your marked places.
		// show text 4 seconds: For fear of strings clipped and no friendly graces.
		
		
		
		// cinematic has ended.
		Destroy(this);
	}
	
	
}
