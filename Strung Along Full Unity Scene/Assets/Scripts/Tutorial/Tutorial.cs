using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{

	[Tooltip("How long to wait before showing the first tutorial.")]
	public float _initialDelay = 5f;
	[Tooltip("Ordered list of tutorials to show in this level.")]
	public TutorialListener.Tutorials[] _tuts;
	[Tooltip("Spot the tutorial rat will run to.")]
	public Vector3 _ratPosition = new Vector3(0f, 0f, -4f);
	
	void OnDrawGizmosSelected() {
		Gizmos.DrawWireCube(_ratPosition + new Vector3(0, 0.5f, 0), Vector3.one);
	}
	
}
