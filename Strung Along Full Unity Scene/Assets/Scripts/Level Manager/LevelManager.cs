using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
	
	public static List<GameObject> activeProps; // the currently active props on the stage.
	private static List<Act> acts; // a list of all the acts in the game. access levels through acts[#].levels[#]
	private static Level currentLevel; // reference to the currently playing level.
	private static LevelLoader loader; // class for handling moving objects on and off the stage.
	
	private static bool timerActive; // whether the timer is running or not.
	private static float timer;
	
	private static int levelState; // game state indicators
	private const int GAME_START = -1;
	private const int NO_LEVEL = 0;
	private const int LEVEL_LOADING = 1;
	private const int LEVEL_PLAYING = 2;
	
	public const float TOP_BOUNDARY = 24f; // the "top" of the level. Y coordinate.
	public const float SIDE_BOUNDARY = 34f; // the "side" of the level. X coordinate.
	public const float EXIT_SPEED = 16.0f; // exit speed when props leave the stage.
	public const float ENTRY_SPEED = 0.2f; // entry speed when props enter the stage.
	public const float ENTRY_SPEED_MAX = 20f; // maximum speed allowed when entering the stage.
	// the tag props will use for being brought on after the puppets are moved to their spawnpoint.
	public const string PROP_DELAY_TAG = "AfterPuppetSpawn";
	
	[Header("References")]
	public GameObject player1; // refs to the player objects
	public GameObject player2;
	public StringRoot p1Anchor; // refs to string anchor locations
	public StringRoot p2Anchor;
	public bool p1AtGoal;
	public bool p2AtGoal;
	[Space]
	[Header("Events")]
	public UnityEvent onLevelComplete;
	public UnityEvent onLevelFailure;
	
	
    // Start is called before the first frame update
    void Start()
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
		// TODO: when both little dudes die, the level is failed.
		//Player1.onDeath
		//Player2.onDeath
		
		// init timer
		timerActive = false;
		timer = 0;
		
		// init win state
		p1AtGoal = false;
		p2AtGoal = false;
		levelState = GAME_START;
		
		// init list of levels
		buildLevelList(acts);
		
		// disable all props before loading the first level
		clearLevel();
		currentLevel = null;
    }
	
	// Update is called once per frame
    void Update()
    {
        
		// DEBUG. GUI will be the one calling this on game start. woo
		if (levelState == GAME_START) {
			loadLevel(1, 1);
		}
		
		// count the timer, if a level is active.
		if (levelState == LEVEL_PLAYING && timerActive) {
			timer += Time.deltaTime;
		}
		
		// check if little dudes have won!
		if (levelState == LEVEL_PLAYING && p1AtGoal && p2AtGoal) {
			onLevelComplete.Invoke();
		}
		
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
		
		// iterate over every GameObject in the Hierarchy. see this is why we only call this once.
		foreach (GameObject gObject in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]) {
			
			// check if the first 3 characters are "act".
			// TODO: this means we can't have any other GameObject starting with "act". i'll have to think about this.
			if ( gObject.name.Length > 3 && gObject.name.Substring(0, 3).Equals("act") ) {
				
				// we have a match! add it to the Act list.
				int actNum = int.Parse( gObject.name.Substring(3) );
				Act act = new Act(gObject, actNum);
				actList.Add(act);
				totalActs++;
				
				// now iterate through the children and add them as levels.
				for (int i = 0; i < gObject.transform.childCount; i++) {
					
					GameObject levelObject = gObject.transform.GetChild(i).gameObject;
					int levelNum = int.Parse(levelObject.name);
					
					Level level = new Level(act, levelNum, levelObject);
					act.levels.Add(level);
					totalLevels++;
				}
				
			}
			
		}
		
		actList.Reverse();
		
		Debug.Log("Loaded " + totalActs + " acts with " + totalLevels + " levels."); // DEBUG
		
	}
	
	// disable all the props. called only once at start of game.
	private void clearLevel() {
		
		foreach (Act act in acts) {
			foreach (Level level in act.levels) {
				foreach (GameObject prop in level.props) {
					prop.SetActive(false);
				}
			}
		}
		
	}
	
	// begin loading a specific level by object reference.
	private static void loadLevel(Level level) {
		levelState = LEVEL_LOADING;
		loader.load(level.props, level.p1Spawn, level.p2Spawn);
		
	}
	
	// subscribed to LevelLoader load event
	private void loadComplete() {
		// start the level!
		levelState = LEVEL_PLAYING;
		startTimer();
	}
	
	// being unloading whatever level is currently loaded.
	private void unloadLevel() {
		levelState = LEVEL_LOADING;
		loader.unload(activeProps);
		// TODO: reset all the props, like levers  that have been pushed.
		// make a disabled duplicate of each prop in its Prop class?
	}
	
	// subscribed to LevelLoader unload event
	private void unloadComplete() {
		// DEBUG: for now just load the next level.
		levelState = NO_LEVEL;
		goNextLevel();
	}
	
	// start the level timer from zero.
	private void startTimer() {
		timer = 0;
		timerActive = true;
		Debug.Log("Timer started!");
	}
	
	// stop the level timer.
	private void stopTimer() {
		timerActive = false;
		Debug.Log("Timer stopped at " + timer + "!");
	}
	
	// subscribed to Goal touch event
	private void updateGoalState(bool enterGoal, bool isPlayer2) {
		
		if (enterGoal) {
			
			if (isPlayer2) {
				p2AtGoal = true;
			} else {
				p1AtGoal = true;
			}
			
		} else {
			
			if (isPlayer2) {
				p2AtGoal = false;
			} else {
				p1AtGoal = false;
			}
			
		}
		
	}
	
	
	// subscribed to LevelManager OnLevelComplete UnityEvent
	public void winLevel() {
		
		stopTimer();
		currentLevel.newTime(timer);
		p1AtGoal = false;
		p2AtGoal = false;
		
		unloadLevel();
		
	}
	
	// subscribed to LevelManager OnLevelFailure UnityEvent
	public void failLevel() {
		
		stopTimer();
		// send to GUI. restart level, quit, that sorta stuff
		// TODO: restart level function!
		
	}
	
	// GUI STUFF
	
	// GAME STATE
	// you can use these to control game state!
	
	// load a specific level.
	// takes two integers, act number and level number respectively.
	// GUI uses this to start specific levels, like from a "level select" menu.
	// so to start the game, just call loadLevel(1, 1)
	public static void loadLevel(int actNum, int levelNum) {
		currentLevel = acts[actNum-1].levels[levelNum-1];
		loadLevel(currentLevel);
	}
	
	// load the next level.
	// uses the LevelManager internal list to figure out which level comes next.
	// GUI calls this for the "Next level" button or something similar.
	public static void goNextLevel() {
		
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
	
	// TODO: retry the current level.
	public static void retryLevel() {
		
		// unloadLevel();
		// reset level to how it started
		// load the same level again
		
	}
	
	
	// GETTERS
	// hit me up if you want anything else!
	
	// number of the current Level.
	public static int getCurrentLevel() {
		return currentLevel.number;
	}
	// number of the current Act.
	public static int getCurrentAct() {
		return currentLevel.act.number;
	}
	// total number of Acts in the game.
	public static int getActCount() {
		return acts.Count;
	}
	// number of levels in the given Act.
	public static int getActLevelCount(int actNumber) {
		return acts[actNumber-1].levels.Count;
	}
	// number of levels left (after the current Level) in the current Act.
	public static int getActRemainingLevels() {
		return currentLevel.act.levels.Count - currentLevel.number;
	}
	// the amount of time the level has been playing for.
	// stops when the level is completed, and resets when a new level is loaded.
	public static float getTime() {
		return timer;
	}
	// if the timer is running or not.
	public static bool isRunning() {
		return timerActive;
	}
	// TODO: score stuff? not sure what the designers have planned

}


// internal data class for Acts.
internal class Act
{
	internal GameObject parent; // reference to the object in the Hierarchy
	internal int number;
	internal List<Level> levels;
	
	internal Act(GameObject parent, int number) {
		this.parent = parent;
		this.number = number;
		
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
internal class Level
{
	internal Act act; // reference to the Act this Level is part of
	internal GameObject parent; // reference to the object in the Hierarchy
	internal int number;
	internal List<GameObject> props; // references to all the props in this Level. i.e. all children of the parent.
	internal Goal p1Goal;
	internal Goal p2Goal;
	internal Spawnpoint p1Spawn;
	internal Spawnpoint p2Spawn;
	
	// starts at 0. when the players complete a level faster than they have before, this keeps track of it!
	internal float bestTime; 
	
	internal Level(Act act, int number, GameObject parent) {
		this.act = act;
		this.number = number;
		this.parent = parent;
		
		props = new List<GameObject>();
		buildPropList(props);
	}
	
	// iterate through the children of the parent object and populate the prop list.
	// we also attach StageProp here.
	private void buildPropList(List<GameObject> propList) {
		
		for (int i = 0; i < parent.transform.childCount; i++) {
			GameObject propObject = parent.transform.GetChild(i).gameObject;
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
			Debug.LogError("Act " + act.number + " Level " + number + " has no P1 spawnpoint set!");
		}
		if (p2Spawn == null) {
			Debug.LogError("Act " + act.number + " Level " + number + " has no P2 spawnpoint set!");
		}
		if (p1Goal == null) {
			Debug.LogError("Act " + act.number + " Level " + number + " has no P1 goal set!");
		}
		if (p2Goal == null) {
			Debug.LogError("Act " + act.number + " Level " + number + " has no P2 goal set!");
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

// internal data class for stage props. we store a bunch of info about positioning.
// this is also where the prop is teleported off-stage before the game begins.
internal class StageProp : MonoBehaviour
{
	public Vector3 originalPosition;
	public Quaternion originalRotation;
	public bool afterPuppetSpawn = false;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void init() {
		// MonoBehaviours don't let me use constructors so i have to do this lmao
		
		this.originalPosition = this.transform.position;
		this.originalRotation = this.transform.rotation;
		this.afterPuppetSpawn = gameObject.CompareTag(LevelManager.PROP_DELAY_TAG);
		
		
		if (afterPuppetSpawn) {
			// if from the right:
			this.transform.position = new Vector3(LevelManager.SIDE_BOUNDARY, this.transform.position.y, this.transform.position.z);
		} else {
			// if from the top:
			this.transform.position = new Vector3(this.transform.position.x, LevelManager.TOP_BOUNDARY, this.transform.position.z);
		}
	}
	
}
