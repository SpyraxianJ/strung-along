using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
	
	public static event Action onLoadComplete;
	public static event Action onUnloadComplete;
	
	// set on initialization: static throughout gameplay.
	private StringRoot p1Anchor;
	private StringRoot p2Anchor;
	
	// set each time a level is loaded: dynamic.
	private List<GameObject> workingProps = null;
	private Spawnpoint p1Spawn = null;
	private Spawnpoint p2Spawn = null;
	
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void initPlayerRefs(StringRoot p1Anchor, StringRoot p2Anchor) {
		this.p1Anchor = p1Anchor;
		this.p2Anchor = p2Anchor;
	}
	
	// activate the props for the new level.
	// THREE PHASES:
	// 1. prop minigame (most props)
	// 2. move puppets to spawnpoints
	// 3. props that arent suitable for the minigame (like ground hazards)
	public void load(List<GameObject> props, Spawnpoint p1, Spawnpoint p2) {
		
		workingProps = props;
		p1Spawn = p1;
		p2Spawn = p2;
		
		// only load props that aren't tagged to be loaded after puppets are moved.
		foreach (GameObject prop in workingProps) {
			
			if (!prop.GetComponent<StageProp>().afterPuppetSpawn) {
				prop.SetActive(true);
				
				MoveProp moverComponent = prop.AddComponent<MoveProp>();
				moverComponent.target = prop.GetComponent<StageProp>().originalPosition;
				moverComponent.enabled = true;
				
				LevelManager.activeProps.Add(prop);
			}
			
			
		}
		
		StartCoroutine( waitLoadBefore() );
		
	}
	
	IEnumerator waitLoadBefore() {
		
		foreach (GameObject prop in workingProps) {
			yield return new WaitUntil( () => prop.GetComponent<MoveProp>() == null);
		}
		
		movePuppets();
		
	}
	
	private void movePuppets() {
		
		// set temporary string parameters while moving em around
		p1Anchor.stringLength = 4f;
		p1Anchor.elasticString = false;
		p2Anchor.stringLength = 4f;
		p2Anchor.elasticString = false;
		
		MoveProp moverComponent;
		// move p1 anchor point
		moverComponent = p1Anchor.gameObject.AddComponent<MoveProp>();
		moverComponent.target = p1Spawn.anchorPoint.position;
		moverComponent.enabled = true;
		// move p2 anchor point
		moverComponent = p2Anchor.gameObject.AddComponent<MoveProp>();
		moverComponent.target = p2Spawn.anchorPoint.position;
		moverComponent.enabled = true;
		
		StartCoroutine( waitMovePuppets() );
		
	}
	
	IEnumerator waitMovePuppets() {
		yield return new WaitUntil( () => p1Anchor.GetComponent<MoveProp>() == null);
		yield return new WaitUntil( () => p2Anchor.GetComponent<MoveProp>() == null);
		
		// update string parameters
		p1Anchor.stringLength = p1Spawn.stringLength;
		p1Anchor.elasticString = p1Spawn.elasticString;
		p2Anchor.stringLength = p2Spawn.stringLength;
		p2Anchor.elasticString = p2Spawn.elasticString;
		
		loadAfter();
		
	}
	
	private void loadAfter() {
		
		foreach (GameObject prop in workingProps) {
			
			if (prop.GetComponent<StageProp>().afterPuppetSpawn) {
				prop.SetActive(true);
				
				MoveProp moverComponent = prop.AddComponent<MoveProp>();
				moverComponent.target = prop.GetComponent<StageProp>().originalPosition;
				moverComponent.enabled = true;
				
				LevelManager.activeProps.Add(prop);
			}
			
			
		}
		
		StartCoroutine( waitLoadAfter() );
		
	}
	
	IEnumerator waitLoadAfter() {
		
		foreach (GameObject prop in workingProps) {
			yield return new WaitUntil( () => prop.GetComponent<MoveProp>() == null);
		}
		
		// all done, clear references.
		workingProps = null;
		p1Spawn = null;
		p2Spawn = null;
		// now let everyone know the level is loaded and ready to go!
		onLoadComplete?.Invoke();
		
	}
	
	// clear the stage of everything on it.
	public void unload(List<GameObject> activeProps) {
		
		workingProps = new List<GameObject>(activeProps);
		
		foreach (GameObject prop in workingProps) {
			MoveProp moverComponent = prop.AddComponent<MoveProp>();
			Vector3 targetPosition;
			
			if (prop.TryGetComponent<Collider>(out Collider comp) ) {
				comp.enabled = false;
			}
			
			if (prop.GetComponent<StageProp>().afterPuppetSpawn) {
				targetPosition = prop.transform.position - new Vector3(LevelManager.SIDE_BOUNDARY, 0, 0);
			}
			else {
				targetPosition = prop.transform.position + new Vector3(0, LevelManager.TOP_BOUNDARY, 0);
			}
			
			moverComponent.target = targetPosition;
			moverComponent.enabled = true;
			
		}
		
		StartCoroutine( waitExit() );
		
	}
	
	
	
	// sends control back to LevelManager when all props have exited the stage.
	IEnumerator waitExit() {
		
		foreach (GameObject prop in workingProps) {
			yield return new WaitUntil( () => prop.GetComponent<MoveProp>() == null);
			
			if (prop.TryGetComponent<Collider>(out Collider comp) ) {
				comp.enabled = true;
			}
			
			LevelManager.activeProps.Remove(prop);
			prop.SetActive(false);
		}
		
		workingProps = null;
		
		onUnloadComplete?.Invoke();
		
	}
	
}
