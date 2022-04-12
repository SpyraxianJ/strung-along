using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	
	private static List<GameObject> activeProps; // the currently active props on the stage.
	private static List<Act> acts; // a list of all the levels in the game.
	// TODO: just make these ints? wat?
	private static Level currentLevel;
	private static Act currentAct;
	
	public const float TOP_BOUNDARY = 20.0f; // the "top" of the level. Y coordinate.
	public const float SIDE_BOUNDARY = 0f; // the "side" of the level. X coordinate.
	public const float EXIT_SPEED = 16.0f; // exit speed when props leave the stage.
	public const float ENTRY_SPEED = 16.0f; // entry speed when props enter the stage.
	
	
    // Start is called before the first frame update
    void Start()
    {
		acts = new List<Act>();
		activeProps = new List<GameObject>();
		
		// populate our list of levels.
		buildLevelList(acts);
		
		// disable all the props before loading the first level...
		clearLevel();
		currentLevel = null;
		currentAct = null;
		
		// DEBUG
		// GUI will usually be the one calling this.
        loadLevel(1, 1);
		
    }
	
	// Update is called once per frame
    void Update()
    {
        
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
	
	// disable all the props at the start of the game.
	private static void clearLevel() {
		
		foreach (Act act in acts) {
			foreach (Level level in act.levels) {
				foreach (GameObject prop in level.props) {
					prop.SetActive(false);
				}
			}
		}
		
		
	}
	
	
	// unload the current level (if there is one) and load in the new one.
	// takes two integers, act number and level number respectively.
	// GUI uses this to start levels!
	public static void loadLevel(int actNum, int levelNum) {
		currentAct = acts[actNum-1];
		currentLevel = currentAct.levels[levelNum-1];
		loadLevel(currentLevel);
	}
	
	// unloads the current level and loads in the new one.
	// this version of the function takes a Level object and loads it.
	private static void loadLevel(Level level) {
		
		// clear the stage of everything on it.
		// 1. move all active objects off the stage
		// 2. disable those objects
		// 3. clear the active props list
		foreach (GameObject prop in activeProps) {
			prop.AddComponent<LevelExitTop>();
		}
		activeProps.Clear();
		
		// TODO: calling loadLevel while props are exiting or entering will make them freeze.
		// in general calling loadLevel too quickly makes weird shit happen.
		
		// activate the props for the new level.
		// 1. enable all the new objects
		// 2. bring them onto the stage!
		// 3. add them to the active props list
		foreach (GameObject prop in level.props) {
			prop.SetActive(true);
			prop.AddComponent<LevelEnterTop>();
			activeProps.Add(prop);
		}
		
	}
	
	// called by other objects in the game to say the players won!
	public static void win() {
		
		// TODO: score stuff, animations for winning, etc.
		
		// let's figure out which Level comes next.
		currentLevel = currentAct.getNextLevel(currentLevel);
		if (currentLevel == null) {
			// if there's no more levels in this Act, move to the next Act.
			int nextActIndex = acts.IndexOf(currentAct);
			nextActIndex++;
			
			if (acts.Count <= nextActIndex) {
				// DEBUG
				// if there's no more Acts, loop back to the start for now.
				// end-of-game stuff will go here.
				currentAct = acts[0];
				currentLevel = currentAct.levels[0];
			}
			else {
				// otherwise the next Level is the start of the next Act.
				// end-of-act stuff will go here.
				currentAct = acts[nextActIndex];
				currentLevel = currentAct.levels[0];
			}
			
			
		}
		
		loadLevel(currentLevel);
		
	}
	
	
	// GETTERS FOR GUI STUFF
	//
	// number of the current Level.
	public static int getCurrentLevel() {
		return currentLevel.number;
	}
	// number of the current Act.
	public static int getCurrentAct() {
		return currentAct.number;
	}
	// number of levels in the given Act.
	public static int getActLevelCount(int actNumber) {
		return acts[actNumber-1].levels.Count;
	}
	// number of levels left (after the current Level) in the current Act.
	public static int getActRemainingLevels() {
		return currentAct.levels.Count - currentLevel.number;
	}
	// TODO: timer? score?

}


// internal data class for Acts.
internal class Act
{
	internal GameObject parent;
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
	internal GameObject parent;
	internal int number;
	internal List<GameObject> props;
	// TODO: properties for marking a level as "complete"? Boolean for yes/no, int for scores and stuff?
	
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
