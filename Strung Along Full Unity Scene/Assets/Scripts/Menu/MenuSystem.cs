using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSystem : MonoBehaviour
{
	public LevelManager manager;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void saveGameData() {
		GameData currentGameData = new GameData(manager.getTime(), manager.getCurrentLevel(), manager.getCurrentAct() );
		SaveSystem.SaveData(currentGameData);
	}
	
	public void loadGameData() {
		// stop the level currently playing
		manager.endLevel(false);
		
		GameData loadGameData = SaveSystem.LoadData();
		if (loadGameData == null) {
			// couldnt find save file, just start new game.
			manager.setNextLevel(1, 1);
			manager.setTimer(0);
		}
		else {
			manager.setNextLevel(loadGameData.SavedAct, loadGameData.SavedLevel);
			manager.setTimer(loadGameData.SavedTimer);
		}
		
		// new level set, tell levelmanager to unload current level
		manager.unloadLevel();
		
	}
	
}
