using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSmoothness : MonoBehaviour
{
    public float smoothness = 30;
	
	public float GetSmoothness() {
		return smoothness;
	}
	
	public void SetSmoothness(float smoothness) {
		this.smoothness = smoothness;
	}
}
