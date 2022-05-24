using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
	
	public static event Action onLoadComplete;
	public static event Action onUnloadComplete;
	
	public const float TOP_BOUNDARY = 24f; // the "top" of the level. Y coordinate.
	public const float BOTTOM_BOUNDARY = -10f;
	public const float SIDE_BOUNDARY = 34f; // the "side" of the level. X coordinate.
	
	public const float ENTRY_SPEED = 0.2f; // entry speed when props enter the stage.
	public const float EXIT_SPEED = 0.2f; // exit speed when props leave the stage.
	public const float SPEED_MAX = 30f; // maximum speed allowed when moving props.
	
	public enum Direction {
		Top,
		Side,
		Bottom
	}
	
	public enum Timing {
		BeforePuppets,
		AfterPuppets
	}
	
	[Header("Debug")]
	// set on initialization: static throughout gameplay.
	public StringRoot p1Anchor;
	public StringRoot p2Anchor;
	private StringRoot[] anchors;
	
	// set each time a level is loaded: dynamic.
	public List<GameObject> workingProps = null;
	private Spawnpoint p1Spawn = null;
	private Spawnpoint p2Spawn = null;
	
	public void initPlayerRefs(StringRoot p1Anchor, StringRoot p2Anchor) {
		this.p1Anchor = p1Anchor;
		this.p2Anchor = p2Anchor;
		this.anchors = new StringRoot[] {p1Anchor, p2Anchor};
	}
	
	// activate the props for the new level.
	// THREE PHASES:
	// 1. StageProps with BeforePuppets
	// 2. move puppets to spawnpoints
	// 3. StageProps with AfterPuppets
	public void load(List<GameObject> props, Spawnpoint p1, Spawnpoint p2) {
		workingProps = props;
		p1Spawn = p1;
		p2Spawn = p2;
		
		// before enabling, place all props offstage to make it look like they're moving on
		foreach (GameObject prop in workingProps) {
			switch (prop.GetComponent<StageProp>().stageMoveDirection) {
				case Direction.Top:
					prop.transform.position += new Vector3(0, TOP_BOUNDARY, 0);
					break;
				case Direction.Side:
					prop.transform.position += new Vector3(SIDE_BOUNDARY, 0, 0);
					break;
				case Direction.Bottom:
					prop.transform.position += new Vector3(0, BOTTOM_BOUNDARY, 0);
					break;
			}
		}
		
		// PHASE 1
		foreach (GameObject prop in workingProps) {
			if (prop.GetComponent<StageProp>().stageMoveTiming == Timing.BeforePuppets) {
				prop.SetActive(true);
				
				toggleColliders(prop, true);
				
				MoveProp moverComponent = prop.AddComponent<MoveProp>();
				moverComponent.target = prop.GetComponent<StageProp>().originalPosition;
				moverComponent.moveSpeed = ENTRY_SPEED;
				moverComponent.maxSpeed = SPEED_MAX;
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
		// PHASE 2
		foreach(StringRoot anchor in anchors) {
			// set temporary string parameters while moving em around
			anchor.stringLength = 4f;
			anchor.elasticString = false;
		}
		
		MoveProp moverComponent;
		// move p1 anchor point
		moverComponent = p1Anchor.gameObject.AddComponent<MoveProp>();
		moverComponent.target = p1Spawn.transform.position;
		moverComponent.moveSpeed = ENTRY_SPEED;
		moverComponent.maxSpeed = SPEED_MAX;
		moverComponent.enabled = true;
		// move p2 anchor point
		moverComponent = p2Anchor.gameObject.AddComponent<MoveProp>();
		moverComponent.target = p2Spawn.transform.position;
		moverComponent.moveSpeed = ENTRY_SPEED;
		moverComponent.maxSpeed = SPEED_MAX;
		moverComponent.enabled = true;
		
		StartCoroutine( waitMovePuppets() );
		
	}
	
	IEnumerator waitMovePuppets() {
		yield return new WaitUntil( () => p1Anchor.GetComponent<MoveProp>() == null);
		yield return new WaitUntil( () => p2Anchor.GetComponent<MoveProp>() == null);
		yield return new WaitForSeconds(1.25f);
		
		// update string parameters
		p1Anchor.stringLength = p1Spawn.stringLength;
		p1Anchor.elasticString = p1Spawn.elasticString;
		p2Anchor.stringLength = p2Spawn.stringLength;
		p2Anchor.elasticString = p2Spawn.elasticString;
		// untangle them if they got tangled. this might be fucked we'll see.
		p1Anchor.manager.tangle = 0;
		p1Anchor.manager.bolConnected = false;
		
		loadAfter();
		
	}
	
	private void loadAfter() {
		// PHASE 3
		foreach (GameObject prop in workingProps) {
			
			if (prop.GetComponent<StageProp>().stageMoveTiming == Timing.AfterPuppets) {
				prop.SetActive(true);
				toggleColliders(prop, true);
				
				MoveProp moverComponent = prop.AddComponent<MoveProp>();
				moverComponent.target = prop.GetComponent<StageProp>().originalPosition;
				moverComponent.moveSpeed = ENTRY_SPEED;
				moverComponent.maxSpeed = SPEED_MAX;
				moverComponent.enabled = true;
				
				LevelManager.activeProps.Add(prop);
			}
			
			
		}
		
		StartCoroutine( waitLoadAfter() );
		
	}
	
	IEnumerator waitLoadAfter() {
		foreach (GameObject prop in workingProps) {
			yield return new WaitUntil( () => prop.GetComponent<MoveProp>() == null);
			
			toggleActivators(prop, true);
			toggleReactors(prop, true);
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
			
			
			toggleColliders(prop, false);
			toggleActivators(prop, false);
			toggleReactors(prop, false);
			
			
			MoveProp moverComponent = prop.AddComponent<MoveProp>();
			Vector3 targetPosition = new Vector3();
			switch (prop.GetComponent<StageProp>().stageMoveDirection) {
				case Direction.Top:
					targetPosition = prop.transform.position + new Vector3(0, TOP_BOUNDARY, 0);
					break;
				case Direction.Side:
					targetPosition = prop.transform.position - new Vector3(SIDE_BOUNDARY, 0, 0);
					break;
				case Direction.Bottom:
					targetPosition = prop.transform.position + new Vector3(0, BOTTOM_BOUNDARY, 0);
					break;
			}
			moverComponent.target = targetPosition;
			moverComponent.moveSpeed = LevelLoader.EXIT_SPEED;
			moverComponent.maxSpeed = LevelLoader.SPEED_MAX;
			moverComponent.enabled = true;
			
		}
		
		StartCoroutine( waitExit() );
		
	}
	
	// sends control back to LevelManager when all props have exited the stage.
	IEnumerator waitExit() {
		foreach (GameObject prop in workingProps) {
			yield return new WaitUntil( () => prop.GetComponent<MoveProp>() == null);
			
			LevelManager.activeProps.Remove(prop);
			prop.SetActive(false);
			
			// set prop position back to default
			prop.transform.position = prop.GetComponent<StageProp>().originalPosition;
			
		}
		
		workingProps = null;
		onUnloadComplete?.Invoke();
		
	}
	
	// helper methods for toggling components off. this is so weird shit doesnt happen
	public void toggleColliders(GameObject obj, bool res) {
		Collider[] comps = obj.GetComponents<Collider>();
		foreach (Collider comp in comps) {
			comp.enabled = res;
		}
	}
	public void toggleActivators(GameObject obj, bool res) {
		Activator[] comps = obj.GetComponents<Activator>();
		foreach (Activator comp in comps) {
			if (!res) { comp.reset(); }
			comp.enabled = res;
		}
	}
	public void toggleReactors(GameObject obj, bool res) {
		Reactor[] comps = obj.GetComponents<Reactor>();
		foreach (Reactor comp in comps) {
			if (!res) { comp.reset(); }
			comp.enabled = res;
		}
	}
	
	// reset the level without moving props on and off the stage.
	public void resetLevel(List<GameObject> activeProps) {
		workingProps = new List<GameObject>(activeProps);
		
		foreach (GameObject prop in workingProps) {
			
			toggleActivators(prop, false);
			toggleReactors(prop, false);
			toggleActivators(prop, true);
			toggleReactors(prop, true);
			
		}
		
		
		
	}
	
	
}
