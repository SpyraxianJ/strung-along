using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDatabase : MonoBehaviour
{
	
	public List<Act> _acts;
	
    void Start() {
		_acts = new List<Act>();
		
		// get all children of the object and add them as acts
        for (int i = 0; i < transform.childCount; i++) {
			GameObject act = transform.GetChild(i).gameObject;
			if (act.activeInHierarchy) {
				_acts.Add( act.AddComponent<Act>() );
			}
		}
		
		
    }
	
	public Level GetFirstLevel() {
		return transform.GetChild(0).GetChild(0).GetComponent<Level>();
	}
	
	public Level GetLevelAfter(Level level) {
		return level.transform.parent.GetChild(  level.transform.GetSiblingIndex() + 1 ).GetComponent<Level>();
		// TODO: return NULL if it's a new act and react accordingly!
	}
	
	public Level GetByIndex(int act, int level) {
		return transform.GetChild(act - 1).GetChild(level - 1).GetComponent<Level>();
	}
	
	public int GetLevelNum(Level level) {
		return level.transform.GetSiblingIndex() + 1;
	}
	
	public int GetActNum(Level level) {
		return level.transform.parent.GetSiblingIndex() + 1;
	}
	

}

public class Act : MonoBehaviour
{
	public List<Level> _levels;
	
	void Start() {
		_levels = new List<Level>();
		
		for (int i = 0; i < transform.childCount; i++) {
			GameObject level = transform.GetChild(i).gameObject;
			if (level.activeInHierarchy) {
				_levels.Add( level.AddComponent<Level>() );
			}
		}
		
	}
	
}

public class Level : MonoBehaviour
{
	public List<StageProp> _props;
	public Goal _p1Goal;
	public Goal _p2Goal;
	public Spawnpoint _p1Spawn;
	public Spawnpoint _p2Spawn;
	
	void Start() {
		_props = new List<StageProp>();
		
		for (int i = 0; i < transform.childCount; i++) {
			GameObject prop = transform.GetChild(i).gameObject;
			if (prop.activeInHierarchy) {
				_props.Add( prop.AddComponent<StageProp>() );
				
				if (prop.GetComponent<Spawnpoint>() ) {
					// the prop is a spawnpoint. but whose?!
					Spawnpoint newSpawn = prop.GetComponent<Spawnpoint>();
					if (newSpawn._isPlayer2) {
						_p2Spawn = newSpawn;
					} else {
						_p1Spawn = newSpawn;
					}
				} else if (prop.GetComponent<Goal>() ) {
					// the prop is a goal. but whose?!
					Goal newGoal = prop.GetComponent<Goal>();
					if (newGoal._isPlayer2) {
						_p2Goal = newGoal;
					} else {
						_p1Goal = newGoal;
					}
				}
				
			}
		}
		
		
	}
	
	public void FreezeAll() {
		foreach (StageProp prop in _props) {
			prop.Freeze();
		}
	}
	
	public void UnfreezeAll() {
		foreach (StageProp prop in _props) {
			prop.Unfreeze();
		}
	}
	
	
	
}

public class StageProp : MonoBehaviour
{
	public Vector3 originalPosition;
	public GameStateManager.Direction stageMoveDirection;
	
	void Start() {
		
		this.originalPosition = this.transform.position;
		
		if (TryGetComponent<StagePropOverride>(out StagePropOverride spo) ) {
			stageMoveDirection = GameStateManager.Direction.Top;
			Destroy(spo);
		} else {
			stageMoveDirection = GameStateManager.Direction.Top;
		}
		
		this.gameObject.SetActive(false);
		
	}
	
	public void Freeze() {
		
		Collider[] colliders = GetComponents<Collider>();
		foreach (Collider comp in colliders) {
			comp.enabled = false;
		}
		/**
		Activator[] activators = GetComponents<Activator>();
		foreach (Activator comp in activators) {
			comp.reset();
			comp.enabled = false;
		}
		
		Reactor[] reactors = GetComponents<Reactor>();
		foreach (Reactor comp in reactors) {
			comp.reset();
			comp.enabled = false;
		}
		**/
	}
	
	public void Unfreeze() {
		
		Collider[] colliders = GetComponents<Collider>();
		foreach (Collider comp in colliders) {
			comp.enabled = true;
		}
		/**
		Activator[] activators = GetComponents<Activator>();
		foreach (Activator comp in activators) {
			comp.enabled = true;
		}
		
		Reactor[] reactors = GetComponents<Reactor>();
		foreach (Reactor comp in reactors) {
			comp.enabled = true;
		}
		**/
	}
	
}