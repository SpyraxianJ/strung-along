using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSystem : MonoBehaviour
{
	public GameStateManager _ctx;
	public Animator crossFade, musicCrossFade;
	
    // Start is called before the first frame update
    void Awake()
    {
		StartCoroutine(LaunchScene());
	}

	public IEnumerator LaunchScene()
	{
		yield return new WaitForSeconds(0f);
		startNewGame();
	}

	public void startNewGame() {
		_ctx.StartGame();
	}

	public void quitGame() {
		_ctx.RequestQuit();
	}
	
	public void saveGameData() {
		GameData currentGameData = new GameData(_ctx.GetCurrentLevel(), _ctx.GetCurrentAct(), _ctx.GetPlaytime() );
		SaveSystem.SaveData(currentGameData);
	}
	
	public void loadGameData() {
		
		GameData loadGameData = SaveSystem.LoadData();
		if (loadGameData == null) {
			// couldnt find save file.
			_ctx.RequestQuit();
		}
		else {
			_ctx.RequestLevel(loadGameData.SavedAct, loadGameData.SavedLevel);
			_ctx.SetPlaytime(loadGameData.SavedPlaytime);
		}
		
		// new level set, tell levelmanager to unload current level
		
	}
	
}
