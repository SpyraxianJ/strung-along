using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
	
	public static event Action onLoadComplete;
	public static event Action onUnloadComplete;
	
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	// activate the props for the new level.
	// 1. enable all the new objects
	// 2. bring them onto the stage
	// 3. add them to the active props list
	public void load(List<GameObject> props) {
		
		foreach (GameObject prop in props) {
			prop.SetActive(true);
			prop.AddComponent<LevelEnterTop>();
		}
		
		StartCoroutine( waitEnter(props) );
	}
	
	// clear the stage of everything on it.
	// 1. move all active objects off the stage
	// 2. disable those objects
	// 3. remove them from the active props list
	public void unload(List<GameObject> activeProps) {
		
		List<GameObject> activePropsIterator = new List<GameObject>(activeProps);
		
		foreach (GameObject prop in activePropsIterator) {
			prop.AddComponent<LevelExitTop>();
		}
		
		StartCoroutine( waitExit(activeProps) );
		
	}
	
	// sends control back to LevelManager when all props reach target positions.
	IEnumerator waitEnter(List<GameObject> props) {
		
		foreach (GameObject prop in props) {
			yield return new WaitUntil( () => prop.GetComponent<LevelEnterTop>() == null);
		}
		
		onLoadComplete?.Invoke();
		// TODO: then move the puppets. then move the AFTER props.
		
	}
	
	// sends control back to LevelManager when all props have exited the stage.
	IEnumerator waitExit(List<GameObject> props) {
		
		List<GameObject> propsIterator = new List<GameObject>(props);
		
		
		foreach (GameObject prop in propsIterator) {
			yield return new WaitUntil( () => prop.GetComponent<LevelExitTop>() == null);
		}
		
		onUnloadComplete?.Invoke();
		
	}
	
}
