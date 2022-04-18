using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	
	public static List<GameObject> activeProps; // the currently active props on the stage.
	private static List<Act> acts; // a list of all the levels in the game.
	
	private static LevelLoader loader;
	
	// TODO: just make these ints? wat?
	private static Level currentLevel;
	private static Act currentAct;
	
	private static bool timerActive; // whether the timer is running or not.
	private static float timer;
	
	public const float TOP_BOUNDARY = 20.0f; // the "top" of the level. Y coordinate.
	public const float SIDE_BOUNDARY = 0f; // the "side" of the level. X coordinate.
	public const float EXIT_SPEED = 16.0f; // exit speed when props leave the stage.
	public const float ENTRY_SPEED = 16.0f; // entry speed when props enter the stage.
	
	
    // Start is called before the first frame update
    void Start()
    {
		acts = new List<Act>();
		activeProps = new List<GameObject>();
		loader = gameObject.AddComponent<LevelLoader>();
		
		timerActive = false;
		timer = 0;
		
		// populate our list of levels.
		buildLevelList(acts);
		
		// disable all the props before loading the first level.
		clearLevel();
		currentLevel = null;
		currentAct = null;
    }
	
	// Update is called once per frame
    void Update()
    {
        
		// DEBUG. GUI will be the one calling this on game start. woo
		if (currentLevel == null) {
			loadLevel(1, 1);
		}
		
		// count the timer, if a level is active.
		if (timerActive) {
			timer += Time.deltaTime;
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
	private static void buildLevelList(List<Act> actList) {
		
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
					
					Level level = new Level(levelNum, levelObject);
					act.levels.Add(level);
					totalLevels++;
				}
				
			}
			
		}
		
		Debug.Log("Loaded " + totalActs + " acts with " + totalLevels + " levels."); // DEBUG
		
	}
	
	// disable all the props. called only once at start of game.
	private static void clearLevel() {
		
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
		loader.load(level.props);
	}
	
	// called by loader when finished loading
	public static void loadComplete() {
		startTimer();
	}
	
	// being unloading whatever level is currently loaded.
	private static void unloadLevel() {
		loader.unload(activeProps);
	}
	
	// called by loader when finished unloading
	public static void unloadComplete() {
		// DEBUG: for now just load the next level.
		goNextLevel();
	}
	
	// start the level timer from zero.
	private static void startTimer() {
		timer = 0;
		timerActive = true;
		Debug.Log("Timer started!");
	}
	
	// stop the level timer.
	private static void stopTimer() {
		timerActive = false;
		Debug.Log("Timer stopped at " + timer + "!");
	}
	
	// called by other objects in the game to say the players won!
	public static void win() {
		
		stopTimer();
		currentLevel.newTime(timer);
		
		// TODO: GUI will pop in here to show "YOU WIN" and buttons for next level etc.
		// TODO: how do i tell the GUI the level is finished?
		
		unloadLevel();
		
	}
	
	// GUI STUFF
	
	// GAME STATE
	// you can use these to control game state!
	
	// load a specific level.
	// takes two integers, act number and level number respectively.
	// GUI uses this to start specific levels, like from a "level select" menu.
	// so to start the game, just call loadLevel(1, 1)
	public static void loadLevel(int actNum, int levelNum) {
		currentAct = acts[actNum-1];
		currentLevel = currentAct.levels[levelNum-1];
		loadLevel(currentLevel);
	}
	
	// load the next level.
	// uses the LevelManager internal list to figure out which level comes next.
	// GUI calls this for the "Next level" button or something similar.
	public static void goNextLevel() {
		
		currentLevel = currentAct.getNextLevel(currentLevel);
		if (currentLevel == null) {
			// there's no more Levels in the Act. go to next Act.
			int nextActIndex = acts.IndexOf(currentAct);
			nextActIndex++;
			
			if (acts.Count <= nextActIndex) {
				// there's no more Acts in the game.
				// DEBUG: loop back to Act 1, Level 1.
				// end-of-game stuff will go here.
				currentAct = acts[0];
				currentLevel = currentAct.levels[0];
			}
			else {
				// next Act is present. start at Level 1.
				// end-of-act stuff will go here.
				currentAct = acts[nextActIndex];
				currentLevel = currentAct.levels[0];
			}
		}
		
		loadLevel(currentLevel);
		
	}
	
	// GETTERS
	// hit me up if you want anything else!
	
	// number of the current Level.
	public static int getCurrentLevel() {
		return currentLevel.number;
	}
	// number of the current Act.
	public static int getCurrentAct() {
		return currentAct.number;
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
		return currentAct.levels.Count - currentLevel.number;
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
	internal GameObject parent; // reference to the object in the Hierarchy
	internal int number;
	internal List<GameObject> props; // references to all the props in this Level. i.e. all children of the parent.
	
	// starts at 0. when the players complete a level faster than they have before, this keeps track of it!
	internal float bestTime; 
	
	internal Level(int number, GameObject parent) {
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
			propObject.AddComponent<StageProp>();
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
	internal Vector3 originalPosition;
	internal Quaternion originalRotation;
	
    // Start is called before the first frame update
    void Start()
    {
        this.originalPosition = this.transform.position;
		this.originalRotation = this.transform.rotation;
		
		this.transform.position = new Vector3(this.transform.position.x, LevelManager.TOP_BOUNDARY, this.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
