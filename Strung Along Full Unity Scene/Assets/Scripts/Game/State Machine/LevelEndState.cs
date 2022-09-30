using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndState : GameState
{
	float _victoryTimer;
	
    public override void EnterState(GameStateManager ctx) {
		if (ctx._currentLevelWon) {
			_victoryTimer = 0f;
			ctx._currentLevelWon = false;
			// TODO: play puppet jump animation
			// TODO: make goal masks go big
			// TODO: flash lights in front of stage
			ctx._tutorial.EndTutorial(true);
		} else {
			_victoryTimer = ctx._victoryTime;
			ctx._tutorial.EndTutorial(false);
		}
	}

	public override void UpdateState(GameStateManager ctx) {
		
		if (_victoryTimer < ctx._victoryTime) {
			_victoryTimer += Time.deltaTime;
		} else {
			ctx.SwitchState(ctx.UnloadingState);
		}
		
	}
	
}
