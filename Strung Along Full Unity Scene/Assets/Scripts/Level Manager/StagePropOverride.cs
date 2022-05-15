using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagePropOverride : MonoBehaviour
{
	[Header("Override Options")]
	[Tooltip("The direction the prop enters and exits the stage.")]
    public LevelLoader.Direction stageMoveDirection = LevelLoader.Direction.Top;
	[Tooltip("The prop enters the stage before or after the puppet anchor points move.")]
	public LevelLoader.Timing stageMoveTiming = LevelLoader.Timing.BeforePuppets;
}
