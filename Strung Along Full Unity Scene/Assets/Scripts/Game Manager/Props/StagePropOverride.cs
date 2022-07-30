using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

By default, props enter the stage from the Top and do so Before the puppet anchor points are moved.
You can use this class to override these defaults. Just slap it on a GameObject in the level.
Exit timings are all the same, but still adhere to direction overrides.

stageMoveDirection
	Top: default. enter and exit from the top of the stage.
	Side: enter from the right and exit to the left of the stage. very snazzy.
	Bottom: enter and exit from the bottom of the stage.

This is mainly useful if you want to bring a hazardous object on stage, but want to do so after
all the other props are onstage and the puppets are in a safe position.

*/

public class StagePropOverride : MonoBehaviour
{
	[Header("Override Options")]
	[Tooltip("The direction the prop enters and exits the stage.")]
    public GameStateManager.Direction stageMoveDirection = GameStateManager.Direction.Top;
}
