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
}
