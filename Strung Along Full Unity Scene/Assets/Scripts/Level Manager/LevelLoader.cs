using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
	
	public static event Action onLoadComplete;
	public static event Action onUnloadComplete;
	
	private StringRoot p1Anchor;
	private StringRoot p2Anchor;
	
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
		
		
		//p1Anchor.AddComponent<MoveToPosition>();
		//p2Anchor.AddComponent<MoveToPosition>();
		
		p1Anchor.SetAnchorPoint(p1Spawn.anchorPoint.position);
		p1Anchor.connectedObject.gameObject.transform.position = p1Spawn.puppet.position;
		p1Anchor.stringLength = p1Spawn.stringLength;
		p1Anchor.elasticString = p1Spawn.elasticString;
		
		p2Anchor.SetAnchorPoint(p2Spawn.anchorPoint.position);
		p2Anchor.connectedObject.gameObject.transform.position = p2Spawn.puppet.position;
		p2Anchor.stringLength = p2Spawn.stringLength;
		p2Anchor.elasticString = p2Spawn.elasticString;
		
		loadAfter();
		//StartCoroutine( waitMovePuppets() );
		
	}
	
	IEnumerator waitMovePuppets() {
		
		// TODO: target puppets instead asdavghsdas
		
		//yield return new WaitUntil( () => p1Anchor.GetComponent<MoveToPosition>() == null);
		//yield return new WaitUntil( () => p2Anchor.GetComponent<MoveToPosition>() == null);
		// meaningless thing to make compiler shut up
		yield return new WaitUntil( () => p2Anchor.GetComponent<LevelEnterTop>() == null);
		
		loadAfter();
		
	}
	
	private void loadAfter() {
		
		foreach (GameObject prop in workingProps) {
			
			if (prop.GetComponent<StageProp>().afterPuppetSpawn) {
				prop.SetActive(true);
				//prop.AddComponent<LevelEnterRight>();
				prop.AddComponent<LevelEnterTop>();
			}
			
			
		}
		
		StartCoroutine( waitLoadAfter() );
		
	}
	
	IEnumerator waitLoadAfter() {
		
		foreach (GameObject prop in workingProps) {
			yield return new WaitUntil( () => prop.GetComponent<LevelEnterTop>() == null);
		}
		
		// all done, clear references.
		workingProps = null;
		p1Spawn = null;
		p2Spawn = null;
		// now let everyone know the level is loaded and ready to go!
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
