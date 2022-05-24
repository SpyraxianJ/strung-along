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
	// TODO: add support for changing default prop stage entry direction.
	public bool loadTestLevel;
	public GameObject testLevel;
	[Space]
	[Header("Events")]
	public UnityEvent onLevelComplete;
	public UnityEvent onLevelFailure;
	public UnityEvent onLevelUnloaded;
	[Space]
	[Header("Debug")]
	public Puppet p1; // puppet 1 status
	public Puppet p2; // puppet 2 status
	public State state; // current gamestate
	public Level currentLevel; // reference to the currently playing level
	public Level nextLevel; // reference to the next level if it exists.
	public static List<GameObject> activeProps; // the currently active props on the stage
	private LevelLoader loader; // class for handling moving objects on and off the stage
	public bool timerActive; // whether the timer is running or not
	public float timer; // current timer value
	public GrimReaper reaper; // reference to attached grim reaper
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
		reaper = GetComponent<GrimReaper>();
		
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
		
		// init list of levels
		buildLevelList(acts);
		clearLevel();
		currentLevel = null;
		nextLevel = null;
    }
	
	void Start() {
		
		
		
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
		
		int totalActs = 0;
		int totalLevels = 0;
		
		// check if we're loading the test level.
		if (loadTestLevel && testLevel != null) {
			Act act = testLevel.AddComponent<Act>();
			act.init(1);
			actList.Add(act);
			totalActs++;
			
			Level level = testLevel.AddComponent<Level>();
			level.init(act, 1);
			act.levels.Add(level);
			totalLevels++;
			Debug.Log(this + ": Loaded test level '" + testLevel.name + "'. Uncheck 'Load Test Level' on LevelManager to load game as usual.");
		} else if (loadTestLevel) {
			Debug.LogWarning(this + ": 'Load Test Level' is checked but no level is given. Things might be funky.");
			loadTestLevel = false;
		} else if (testLevel != null) {
			testLevel.SetActive(false);
		}
		
		// iterate over every GameObject in the Hierarchy. see this is why we only call this once.
		foreach (GameObject gObject in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]) {
			
			// check if the first 3 characters are "act".
			if ( gObject.name.Length > 3 && gObject.name.Substring(0, 3).Equals("act") ) {
				
				if (loadTestLevel) {
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
		Debug.Log(this + ": Loaded " + totalActs + " acts with " + totalLevels + " levels.");
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
				loader.resetLevel(activeProps);
				
				if (!p1.alive) {
					Debug.Log(this + ": Player 1 died ;_;");
					p1.alive = true;
					reaper.respawn( player1.GetComponent<PuppetController>() );
				}
				if (!p2.alive) {
					Debug.Log(this + ": Player 2 died ;_;");
					p2.alive = true;
					reaper.respawn( player2.GetComponent<PuppetController>() );
				}
				
			}
			
			
			
			
		}
    }
	
	//
	// LEVEL LOADING: NoLevel
	//
	private void loadLevel(Level level) {
		currentLevel = level;
		nextLevel = null;
		
		if (currentLevel == null) {
			// just do nothing!
		} else {
			state = State.LevelLoading;
			loader.load(level.props, level.p1Spawn, level.p2Spawn);
		}
		
	}
	public void loadNextLevel() {
		loadLevel(nextLevel);
	}
	public void loadFirstLevel() {
		loadLevel(acts[initialAct-1].levels[initialLevel-1]);
	}
	public void setNextLevel(int actNum, int levelNum) {
		
		if (actNum == 0 || levelNum == 0) {
			nextLevel = null;
		} else {
			nextLevel = acts[actNum-1].levels[levelNum-1];
			if (nextLevel == null) {
				Debug.LogError(this + ": couldn't find Act " + actNum + " Level " + levelNum + ".");
			}
		}
	
	}
	// subscribed to LevelLoader load event
	private void loadComplete() {
		state = State.LevelPlaying;
		resetTimer();
		startTimer();
		Debug.Log(this + ": Act " + currentLevel.act.actNumber + " Level " + currentLevel.levelNumber + " start!");
	}
	
	//
	// LEVEL FUNCTIONS: LevelPlaying
	//
	// reset the level timer.
	private void resetTimer() {
		timer = 0;
	}
	// set the level timer to a value.
	public void setTimer(float time) {
		timer = time;
	}
	// start the level timer.
	private void startTimer() {
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
		
		reaper.kill(pup);
		
		
		
		
		
		
	}
	public void endLevel(bool win) {
		stopTimer();
		
		if (win) {
			nextLevel = currentLevel.next();
		} else {
			nextLevel = currentLevel;
		}
		
		
		p1.atGoal = false;
		p2.atGoal = false;
		p1.alive = true;
		p2.alive = true;
		Debug.Log(this + ": Level ended! Win: " + win + ". Time: " + timer);
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
	// number of the current Level. returns -1 if no level is loaded.
	public int getCurrentLevel() {
		return currentLevel != null ? currentLevel.levelNumber : -1;
	}
	// number of the current Act. returns -1 if no level is loaded.
	public int getCurrentAct() {
		return currentLevel != null ? currentLevel.act.actNumber : -1;
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
		
		// TODO: handle end of act.
		// TODO: handle end of game.
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
			//stagePropComponent.init();
			
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
			Debug.LogError(this + ": Act " + act.actNumber + " Level " + levelNumber + " has no P1 spawnpoint set!");
		}
		if (p2Spawn == null) {
			Debug.LogError(this + ": Act " + act.actNumber + " Level " + levelNumber + " has no P2 spawnpoint set!");
		}
		if (p1Goal == null) {
			Debug.LogError(this + ": Act " + act.actNumber + " Level " + levelNumber + " has no P1 goal set!");
		}
		if (p2Goal == null) {
			Debug.LogError(this + ": Act " + act.actNumber + " Level " + levelNumber + " has no P2 goal set!");
		}
		
	}
	
	
	public Level next() {
		return act.getNextLevel(this);
		
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
	
	void Awake() {
		
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
