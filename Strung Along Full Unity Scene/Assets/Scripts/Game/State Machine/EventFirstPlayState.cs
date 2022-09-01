using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class EventFirstPlayState : GameState
{
    public override void EnterState(GameStateManager ctx) {
		Debug.Log("Playing first level intro cinematic.");
		
		ctx._lights.DimMains();
		ctx._camera.SwitchCam(2);
		
		ctx._cinematics.PlayCutscene(ctx._cinematics._intro);
	
		// teleport under their string roots
		ctx._player1.transform.position = ctx._player1.GetComponent<PuppetController>().thisStringRoot.transform.position + new Vector3(0, -4, 0);
		ctx._player2.transform.position = ctx._player2.GetComponent<PuppetController>().thisStringRoot.transform.position + new Vector3(0, -4, 0);

	}

	public override void UpdateState(GameStateManager ctx) {
		
		
		if (ctx._cinematics._director.state == PlayState.Playing) {
			// disable movement.
			ctx._player1.GetComponent<PuppetController>().move = Vector2.zero;
			ctx._player2.GetComponent<PuppetController>().move = Vector2.zero;
			// they can still jump though because its funny
			ctx._player1.GetComponent<Rigidbody>().velocity.Scale( new Vector3(0f, 1f, 0f) );
			ctx._player2.GetComponent<Rigidbody>().velocity.Scale( new Vector3(0f, 1f, 0f) );
			
			// make players look forwards.
			Vector3 lookPosition = new Vector3( ctx._camera.Position().x, 0f, ctx._camera.Position().z );
			ctx._player1.GetComponent<PuppetController>().visualReference.transform.LookAt(lookPosition, Vector3.up);
			ctx._player2.GetComponent<PuppetController>().visualReference.transform.LookAt(lookPosition, Vector3.up);
			
			// zero out the contextual move timer.
			ctx._player1.GetComponent<PuppetContextualTutorial>().movementTimer = 0f;
			ctx._player2.GetComponent<PuppetContextualTutorial>().movementTimer = 0f;
			
		} else {
			ctx._lights.BrightenMains();
			ctx.SwitchState(ctx.PlayingState);
		}
		
		
		
		
	}
	
}
