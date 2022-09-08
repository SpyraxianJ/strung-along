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
			if (act.activeSelf) {
				_acts.Add( act.AddComponent<Act>() );
			}
		}
    }
	
	public Level GetFirstLevel() {
		return _acts[0]._levels[0];
	}
	
	// get the level after this level. null if next act or end of game.
	public Level GetLevelAfter(Level level) {
		Act parentAct = level.transform.parent.GetComponent<Act>();
		int nextIndex = parentAct._levels.IndexOf(level) + 1;
		
		if (nextIndex < parentAct._levels.Count) {
			return parentAct._levels[nextIndex];
		} else {
			return null;
		}
	}
	
	// get the first level of the act after this level's act. null if end of game.
	public Level GetActAfter(Level level) {
		Act parentAct = level.transform.parent.GetComponent<Act>();
		int nextIndex = _acts.IndexOf(parentAct) + 1;
		
		if (nextIndex < _acts.Count) {
			return _acts[nextIndex]._levels[0];
		} else {
			// next act doesn't exist, end of game.
			return null;
		}
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
			if (level.activeSelf) {
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
	public GridManager _grid;
	
	void Start() {
		_props = new List<StageProp>();
		
		for (int i = 0; i < transform.childCount; i++) {
			GameObject prop = transform.GetChild(i).gameObject;
			if (prop.activeSelf) {
				
				if ( prop.TryGetComponent(out StageProp sp) ) {
					_props.Add(sp);
				} else {
					_props.Add( prop.AddComponent<StageProp>() );
				}
				
				if ( prop.TryGetComponent(out Spawnpoint spawn) ) {
					// the prop is a spawnpoint. but whose?!
					if (spawn._isPlayer2) {
						_p2Spawn = spawn;
					} else {
						_p1Spawn = spawn;
					}
				} else if ( prop.TryGetComponent(out Goal goal) ) {
					// the prop is a goal. but whose?!
					if (goal._isPlayer2) {
						_p2Goal = goal;
					} else {
						_p1Goal = goal;
					}
				} else if ( prop.TryGetComponent(out GridManager grid) ) {
					// the prop is the GridManager object!
					_grid = grid;
				}
				
				prop.GetComponent<StageProp>().Init();
				
			}
		}
		
		
	}
	
	public void ToggleColliders(bool toggle) {
		foreach (StageProp prop in _props) {
			prop.ToggleColliders(toggle);
		}
	}
	
	public void Reset() {
		_p1Goal.Reset();
		_p2Goal.Reset();
		
		foreach (StageProp prop in _props) {
			IResettable[] resets = prop.GetComponents<IResettable>();
			foreach (IResettable comp in resets) {
				comp.Reset();
			}
		}
	}
	
	
	
}

