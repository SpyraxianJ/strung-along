using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
	// level manager state machine
	public enum State {
		GameStart,
		NoLevel,
		LevelLoading,
		LevelPlaying
	}
	// puppet state machine
	[Serializable]
	public struct Puppet {
		public bool atGoal;
		public bool alive;
	}
	
	[Header("References")]
	public GameObject player1; // refs to the player objects
	public GameObject player2;
	public StringRoot p1Anchor; // refs to string anchor locations
	public StringRoot p2Anchor;
	[Header("Initialization")]
	public int initialAct;
	public int initialLevel;
	[Space]
	[Header("Events")]
	public UnityEvent onLevelComplete;
	public UnityEvent onLevelFailure;
	public UnityEvent onLevelUnloaded;
	public UnityEvent onPuppet1Death;
	public UnityEvent onPuppet2Death;
	[Space]
	[Header("Debug")]
	public State state;
	public bool win;
	public Level currentLevel; // reference to the currently playing level.
	public static List<GameObject> activeProps; // the currently active props on the stage.
	private LevelLoader loader; // class for handling moving objects on and off the stage.
	public bool timerActive; // whether the timer is running or not.
	public float timer;
	public Puppet p1;
	public Puppet p2;
	
	public List<Act> acts; // a list of all the acts in the game. access levels through acts[#].levels[#]
	
	
	//
	// INITIALIZATION
	//
	
    void Awake()
    {
		// init references
		acts = new List<Act>();
		activeProps = new List<GameObject>();
		loader = gameObject.AddComponent<LevelLoader>();
		loader.initPlayerRefs(p1Anchor, p2Anchor);
		
		// init event subscriptions
		LevelLoader.onLoadComplete += loadComplete;
		LevelLoader.onUnloadComplete += unloadComplete;
		Goal.onPlayerGoal += updateGoalState;
		
		// init timer
		timerActive = false;
		timer = 0;
		
		// init win state
		p1.atGoal = false;
		p2.atGoal = false;
		p1.alive = true;
		p2.alive = true;
		state = State.GameStart;
		win = false;
		
		// init list of levels
		buildLevelList(acts);
		
		// disable all props before loading the first level
		clearLevel();
		currentLevel = null;
    }
	
	// build an internal hierarchy of the game's levels.
	// this is done by searching through the Unity Engine Hierarchy and:
	// 1. finding GameObjects that begin with "act".
	// 2. grabbing all the children of that object.
	// 3. adding said children to the level list.
	// it's clowny but very modular, as it doesn't rely on any hardcoded values or tagging.
	// you can create a whole new act with tons of levels just by creating GameObjects in the Unity Editor!
	// ONLY CALL THIS ONCE, ON Start(). IT'S SLOW AS FUCK
	private void buildLevelList(List<Act> actList) {
		
		int totalActs = 0; // DEBUG
		int totalLevels = 0; // DEBUG
		bool debugLevel = false; // DEBUG
		string debugString = "TEST"; // DEBUG
		
		// DEBUG
		// if there's a GameObject named "TEST", just load that and nothing else. useful for testing!
		foreach (GameObject gObject in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]) {
			if ( gObject.name.Equals(debugString) ) {
				debugLevel = true;
				
				Act act = gObject.AddComponent<Act>();
				act.init(1);
				actList.Add(act);
				totalActs++;
				
				Level level = gObject.AddComponent<Level>();
				level.init(act, 1);
				act.levels.Add(level);
				totalLevels++;
				
				Debug.Log("Loaded TEST level. When you're done, rename it to the level number and drop it under an 'act' GameObject!");
				break;
			}
		}
		
		// iterate over every GameObject in the Hierarchy. see this is why we only call this once.
		foreach (GameObject gObject in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]) {
			
			// check if the first 3 characters are "act".
			// TODO: this means we can't have any other GameObject starting with "act". i'll have to think about this.
			if ( gObject.name.Length > 3 && gObject.name.Substring(0, 3).Equals("act") ) {
				
				if (debugLevel) {
					gObject.SetActive(false);
					continue;
				}
				
				// we have a match! add it to the Act list.
				int actNum = int.Parse( gObject.name.Substring(3) );
				Act act = gObject.AddComponent<Act>();
				act.init(actNum);
				actList.Add(act);
				totalActs++;
				
				// now iterate through the children and add them as levels.
				for (int i = 0; i < gObject.transform.childCount; i++) {
					
					GameObject levelObject = gObject.transform.GetChild(i).gameObject;
					int levelNum = int.Parse(levelObject.name);
					
					Level level = levelObject.AddComponent<Level>();
					level.init(act, levelNum);
					act.levels.Add(level);
					totalLevels++;
				}
			}
		}
		// the act list is created in the wrong order for some reason.
		actList.Reverse();
		Debug.Log("Loaded " + totalActs + " acts with " + totalLevels + " levels."); // DEBUG
	}
	
	// disable all the props. called only once at start of game.
	private void clearLevel() {
		
		foreach (Act act in acts) {
			foreach (Level level in act.levels) {
				foreach (GameObject prop in level.props) {
					prop.SetActive(false);
					loader.toggleColliders(prop, false);
					loader.toggleActivators(prop, false);
					loader.toggleReactors(prop, false);
				}
			}
		}
	}
	
	
	//
	// RUNTIME
	//
	void Update()
    {
		// DEBUG. GUI will be the one calling this on game start. woo
		if (state == State.GameStart) {
			state = State.NoLevel;
			loadLevel(initialAct, initialLevel);
		}
		
		// count the timer if it's on.
		if (timerActive) {
			timer += Time.deltaTime;
		}
		
		if (state == State.LevelPlaying) {
			// check if both players at goal
			if (p1.atGoal && p2.atGoal) {
				state = State.LevelLoading;
				onLevelComplete.Invoke();
			}
			
			// check if either player has died
			if (!p1.alive || !p2.alive) {
				state = State.LevelLoading;
				onLevelFailure.Invoke();
			}
		}
    }
	
	//
	// LEVEL LOADING: NoLevel
	//
	private void loadLevel(Level level) {
		state = State.LevelLoading;
		loader.load(level.props, level.p1Spawn, level.p2Spawn);
	}
	public void loadLevel(int actNum, int levelNum) {
		currentLevel = acts[actNum-1].levels[levelNum-1];
		loadLevel(currentLevel);
	}
	public void loadNextLevel() {
		int nextActIndex = acts.IndexOf(currentLevel.act);
		currentLevel = currentLevel.act.getNextLevel(currentLevel);
		if (currentLevel == null) {
			// there's no more Levels in the Act. go to next Act.
			nextActIndex++;
			
			if (acts.Count <= nextActIndex) {
				// there's no more Acts in the game.
				// DEBUG: loop back to Act 1, Level 1.
				// end-of-game stuff will go here.
				currentLevel = acts[0].levels[0];
			}
			else {
				// next Act is present. start at Level 1.
				// end-of-act stuff will go here.
				currentLevel = acts[nextActIndex].levels[0];
			}
		}
		
		loadLevel(currentLevel);
	}
	public void loadSameOrNext() {
		
		if (win) {
			loadNextLevel();
		} else {
			loadLevel(currentLevel);
		}
		win = false;
		
		
	}
	// subscribed to LevelLoader load event
	private void loadComplete() {
		state = State.LevelPlaying;
		startTimer();
		Debug.Log("Act " + currentLevel.act.actNumber + " Level " + currentLevel.levelNumber + " start!");
	}
	
	//
	// LEVEL FUNCTIONS: LevelPlaying
	//
	// start the level timer from zero.
	private void startTimer() {
		timer = 0;
		timerActive = true;
	}
	// stop the level timer.
	private void stopTimer() {
		timerActive = false;
	}
	// subscribed to Goal touch event
	private void updateGoalState(bool enterGoal, bool isPlayer2) {
		if (enterGoal) {
			if (isPlayer2) {
				p2.atGoal = true;
			} else {
				p1.atGoal = true;
			}
		} else {
			
			if (isPlayer2) {
				p2.atGoal = false;
			} else {
				p1.atGoal = false;
			}
		}
	}
	public void killPuppet(PuppetController pup) {
		if (pup.gameObject == player1) {
			p1.alive = false;
			onPuppet1Death.Invoke();
		}
		if (pup.gameObject == player2) {
			p2.alive = false;
			onPuppet2Death.Invoke();
		}
		Debug.Log("Puppet died!! How could you?!?");
		StartCoroutine( waitForSeconds(2) );
		
		
	}
	public void endLevel(bool win) {
		stopTimer();
		this.win = win;
		
		p1.atGoal = false;
		p2.atGoal = false;
		Debug.Log("Level end! Win: " + win + ". Time: " + timer);
	}
	public void respawnPuppets() {
		p1.alive = true;
		p2.alive = true;
	}
	
	//
	// LEVEL UNLOADING
	//
	public void unloadLevel() {
		state = State.LevelLoading;
		loader.unload(activeProps);
	}
	// subscribed to LevelLoader unload event
	private void unloadComplete() {
		state = State.NoLevel;
		onLevelUnloaded.Invoke();
	}

	//
	// GETTERS
	//
	// number of the current Level.
	public int getCurrentLevel() {
		return currentLevel.levelNumber;
	}
	// number of the current Act.
	public int getCurrentAct() {
		return currentLevel.act.actNumber;
	}
	// total number of Acts in the game.
	public int getActCount() {
		return acts.Count;
	}
	// number of levels in the given Act.
	public int getActLevelCount(int actNumber) {
		return acts[actNumber-1].levels.Count;
	}
	// number of levels left (after the current Level) in the current Act.
	public int getActRemainingLevels() {
		return currentLevel.act.levels.Count - currentLevel.levelNumber;
	}
	// the amount of time the level has been playing for.
	// stops when the level is completed, and resets when a new level is loaded.
	public float getTime() {
		return timer;
	}
	// if the timer is running or not.
	public bool isRunning() {
		return timerActive;
	}
	// TODO: score stuff? not sure what the designers have planned

	
	IEnumerator waitForState(State waitState) {
		yield return new WaitUntil( () => this.state == waitState);
	}
	IEnumerator waitForSeconds(float sec) {
		yield return new WaitForSeconds(sec);
	}
	
	
}




// internal data class for Acts.
public class Act : MonoBehaviour
{
	[Header("Debug")]
	public int actNumber;
	public List<Level> levels;
	
	internal void init(int actNumber) {
		this.actNumber = actNumber;
		levels = new List<Level>();
	}
	
	
	// returns the next level in the list for this Act.
	// if this level isn't found, returns null. (Act is over)
	internal Level getNextLevel(Level level) {
		
		int nextLevelIndex = levels.IndexOf(level);
		nextLevelIndex++;
		
		if (levels.Count <= nextLevelIndex) {
			return null;
		}
		else {
			return levels[nextLevelIndex];
		}
		
	}
	
}

// internal data class for Levels.
public class Level : MonoBehaviour
{
	[Header("Debug")]
	public Act act; // reference to the Act this Level is part of
	public int levelNumber;
	public List<GameObject> props; // references to all the props in this Level
	public Goal p1Goal;
	public Goal p2Goal;
	public Spawnpoint p1Spawn;
	public Spawnpoint p2Spawn;
	
	// starts at 0. when the players complete a level faster than they have before, this keeps track of it!
	public float bestTime; 
	
	internal void init(Act act, int levelNumber) {
		this.act = act;
		this.levelNumber = levelNumber;
		
		props = new List<GameObject>();
		buildPropList(props);
	}
	
	// iterate through the children of the parent object and populate the prop list.
	// we also attach StageProp here.
	private void buildPropList(List<GameObject> propList) {
		
		for (int i = 0; i < transform.childCount; i++) {
			GameObject propObject = transform.GetChild(i).gameObject;
			propList.Add(propObject);
			
			StageProp stagePropComponent = propObject.AddComponent<StageProp>();
			stagePropComponent.init();
			
			if (propObject.GetComponent<Spawnpoint>() ) {
				// the prop is a spawnpoint. but whose?!
				Spawnpoint newSpawn = propObject.GetComponent<Spawnpoint>();
				
				if (newSpawn.isPlayer2) {
					p2Spawn = newSpawn;
				} else {
					p1Spawn = newSpawn;
				}
			}
			
			if (propObject.GetComponent<Goal>() ) {
				// the prop is a goal. but whose?!
				Goal newGoal = propObject.GetComponent<Goal>();
				
				if (newGoal.isPlayer2) {
					p2Goal = newGoal;
				} else {
					p1Goal = newGoal;
				}
			}
			
			
		}
		
		// if a level is missing a key component, yell at the level designer.
		if (p1Spawn == null) {
			Debug.LogError("Act " + act.actNumber + " Level " + levelNumber + " has no P1 spawnpoint set!");
		}
		if (p2Spawn == null) {
			Debug.LogError("Act " + act.actNumber + " Level " + levelNumber + " has no P2 spawnpoint set!");
		}
		if (p1Goal == null) {
			Debug.LogError("Act " + act.actNumber + " Level " + levelNumber + " has no P1 goal set!");
		}
		if (p2Goal == null) {
			Debug.LogError("Act " + act.actNumber + " Level " + levelNumber + " has no P2 goal set!");
		}
		
	}
	
	// a level completion time has been recorded. if it's the fastest, keep record of it.
	// returns true if it's a new record, false if not.
	internal bool newTime(float newTime) {
		if (newTime > bestTime) {
			bestTime = newTime;
			return true;
		} else {
			return false;
		}
	}
	
	internal float getBestTime() {
		return bestTime;
	}
	
}

// internal data class for stage props.
// we store a bunch of info about positioning, what direction the prop enters from, and when.
// you can override the default direction and timing of props with the StagePropOverride component.
public class StageProp : MonoBehaviour
{
	[Header("Debug")]
	public Vector3 originalPosition;
	public LevelLoader.Direction stageMoveDirection;
	public LevelLoader.Timing stageMoveTiming;
	
	internal void init() {
		
		this.originalPosition = this.transform.position;
		
		if (TryGetComponent<StagePropOverride>(out StagePropOverride spo) ) {
			stageMoveDirection = spo.stageMoveDirection;
			stageMoveTiming = spo.stageMoveTiming;
			Destroy(spo);
		} else {
			stageMoveDirection = LevelLoader.Direction.Top;
			stageMoveTiming = LevelLoader.Timing.BeforePuppets;
		}
	}
	
}