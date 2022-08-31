using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Reaction : MonoBehaviour, IResettable
{
	public abstract void Fire(float lerp);
	public abstract void Reset();
}
