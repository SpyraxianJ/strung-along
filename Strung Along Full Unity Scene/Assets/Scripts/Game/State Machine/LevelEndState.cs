using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndState : GameState
{
	float _victoryTimer;

    public override void EnterState(GameStateManager ctx) {
		if (ctx._currentLevelWon) {
			_victoryTimer = 0f;

            try
            {
                ctx._player1.GetComponent<PuppetController>().VictoryAnimation("Victory1", 1.3f);
                ctx._player2.GetComponent<PuppetController>().VictoryAnimation("Victory2", 1.3f);
                Debug.Log("hehe");
            }
            catch (System.Exception)
            {
                Debug.LogWarning("LevelEndState could not find the puppet's animators to play the victory animation");
            }

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

            // Hi Tim here, this is a little bit of code to record when players win a level
            try
            {
                if (ctx.logRec != null)
                {
                    ctx.logRec.Complete(ctx._currentLevel.name);
                }
            }
            catch (System.Exception)
            {
                Debug.LogWarning("No log recorder found");
            }

        }
		
	}
	
}
