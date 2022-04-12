using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// Causes the player to win when the object is physically touched by the player.
//
//

public class WinTouch : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	void OnCollisionEnter() {
		// TODO: only the player can trigger this collision.
		// TODO: check if either or both players must touch 1.at the same time or 2. at any point.
		LevelManager.win();
	}
	
}
