using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventActChangeState : GameState
{
    public override void EnterState(GameStateManager ctx) {
		ctx._cinematics.PlayCutscene(ctx._cinematics._actChange);
		
		if (ctx._skipActChanges) {
			ctx._cinematics.SkipCutscene();
		}

	}

	public override void UpdateState(GameStateManager ctx) {
		if ( ctx._cinematics.Playing() ) {
			//ctx._cinematics.DisablePlayers(ctx._player1, ctx._player2);
			//ctx._cinematics.ForceLook(ctx._player1, ctx._camera.Position());
			//ctx._cinematics.ForceLook(ctx._player2, ctx._camera.Position());
		} else {
			ctx.SwitchState(ctx.EmptyState);
		}
		
	}
	
}
