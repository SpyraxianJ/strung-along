using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventGameEndState : GameState
{
    public override void EnterState(GameStateManager ctx) {
		Debug.Log("End of the game! Thanks for playing!!!!");

        // Tim stuff hehe

        ctx.gameEndVisual.SetActive(true);

		
		ctx._gameEnd = false;
		
		// we don't have an "end-of-game" planned yet, so just bring up level select.
		PauseMenu menu = MonoBehaviour.FindObjectOfType<PauseMenu>();
		menu.Pause();
		menu.OpenLevelSelectUI();
		
		ctx.SwitchState(ctx.EmptyState);
		
	}

	public override void UpdateState(GameStateManager ctx) {
		
	}
	
}
