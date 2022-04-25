using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// tracks whether the object is currently touched by the designated player.
//

public class Goal : MonoBehaviour
{
	
	public LevelManager manager;
	[Tooltip("Determines if this is the goal for Player 2. Otherwise Player 1.")]
	public bool player2Goal;
	[Tooltip("Does the player have to remain touching the goal to win? If not, the goal only needs to be touched once: the player doesn't have to remain touching the goal.")]
	public bool persistent = true;
	
	private Color originalColor;
	private bool active = true; // if not persistent, turns inactive after first touch.
	private GameObject targetPlayer; // reference to the player GameObject this goal waits for.
	
	
    // Start is called before the first frame update
    void Start()
    {
        
		// the goal can only be triggered by the right player!
		if (player2Goal) {
			targetPlayer = manager.player2;
		} else {
			targetPlayer = manager.player1;
		}
		
		originalColor = gameObject.GetComponent<Renderer>().material.color;
		
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	void OnTriggerEnter(Collider other) {
		
		if (other.gameObject == targetPlayer && active) {
			LevelManager.goals++;
			StartCoroutine ( pulseAnimation() );
			StartCoroutine( fadeAnimation(false) );
			
			// if not persistent, turn the goal off after first touch.
			if (!persistent) {
				active = false;
			}
		}
		
	}
	
	void OnTriggerExit(Collider other) {
		
		if (other.gameObject == targetPlayer && persistent && active) {
			LevelManager.goals--;
			StartCoroutine( fadeAnimation(true) );
		}
		
	}
	
	// the goal fades out and turns green when touched.
	// fades back in and returns to original color when stopped touching.
	// if true, fades out. if false, fades in.
	IEnumerator fadeAnimation(bool reverse) {
		
		Renderer renderer = gameObject.GetComponent<Renderer>();
		Color toColor;
		
		if (!reverse) {
			// turn green and fade out
			renderer.material.color = new Color(0, 1, 0, renderer.material.color.a);
			toColor = renderer.material.color;
			
			for (float alpha = 1f; alpha >= 0.3f; alpha -= 0.01f) {
				toColor.a = alpha;
				renderer.material.color = toColor;
				yield return null;
			}
		} else {
			// change back and fade in
			renderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, renderer.material.color.a);
			toColor = renderer.material.color;
			
			for (float alpha = toColor.a; alpha < 1f; alpha += 0.01f) {
				toColor.a = alpha;
				renderer.material.color = toColor;
				yield return null;
			}
		}
		
		
	}
	
	// make the goal object pulse larger then back to original size.
	// some visual feedback!
	IEnumerator pulseAnimation() {
		
		float pulseSize = 0.5f; // pulse to 50% larger on goal touch.
		float pulseIncrement = 0.05f; // how much to change size on each frame.
		
		Vector3 scaleChange = new Vector3(pulseIncrement, pulseIncrement, pulseIncrement);
		
		for (float scaleFactor = 0f; scaleFactor < pulseSize; scaleFactor += pulseIncrement) {
			
			gameObject.transform.localScale += scaleChange;
			yield return null;
		}
		for (float scaleFactor = pulseSize; scaleFactor > 0f; scaleFactor -= pulseIncrement) {
			
			gameObject.transform.localScale -= scaleChange;
			yield return null;
		}
	}
	
	
}
