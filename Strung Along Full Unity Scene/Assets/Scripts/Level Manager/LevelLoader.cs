using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
	
	public static event Action onLoadComplete;
	public static event Action onUnloadComplete;
	
	private List<GameObject> workingProps;
	
	
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
		
		workingProps = props;
		
		// only load props that aren't tagged to be loaded after puppets are moved.
		foreach (GameObject prop in workingProps) {
			
			if (!prop.GetComponent<StageProp>().afterPuppetSpawn) {
				prop.SetActive(true);
				prop.AddComponent<LevelEnterTop>();
			}
			
			
		}
		
		StartCoroutine( waitLoadBefore() );
		
	}
	
	
	IEnumerator waitLoadBefore() {
		
		foreach (GameObject prop in workingProps) {
			yield return new WaitUntil( () => prop.GetComponent<LevelEnterTop>() == null);
		}
		
		
		movePuppets();
		
	}
	
	private void movePuppets() {
		
		// do the thing
		
		//StartCoroutine( waitMovePuppets() );
		loadAfter();
		
	}
	
	IEnumerator waitMovePuppets() {
		
		// TODO: target puppets instead asdavghsdas
		foreach (GameObject prop in workingProps) {
			yield return new WaitUntil( () => prop.GetComponent<LevelEnterTop>() == null);
		}
		
		//onLoadComplete?.Invoke();
		loadAfter();
		
	}
	
	private void loadAfter() {
		
		foreach (GameObject prop in workingProps) {
			
			if (prop.GetComponent<StageProp>().afterPuppetSpawn) {
				prop.SetActive(true);
				prop.AddComponent<LevelEnterTop>();
			}
			
			
		}
		
		StartCoroutine( waitLoadAfter() );
		
	}
	
	IEnumerator waitLoadAfter() {
		
		foreach (GameObject prop in workingProps) {
			yield return new WaitUntil( () => prop.GetComponent<LevelEnterTop>() == null);
		}
		
		
		onLoadComplete?.Invoke();
		
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
	
	
	
	// sends control back to LevelManager when all props have exited the stage.
	IEnumerator waitExit(List<GameObject> props) {
		
		List<GameObject> propsIterator = new List<GameObject>(props);
		
		
		foreach (GameObject prop in propsIterator) {
			yield return new WaitUntil( () => prop.GetComponent<LevelExitTop>() == null);
		}
		
		onUnloadComplete?.Invoke();
		
	}
	
}
