using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeInfo : MonoBehaviour
{
	public int shapeMode;
	public float width = 8;
	public float height = 1;
	public float innerRadius = 4;
	public float outerRadius = 5;
	public int divisions = 32;
	public float widthOffset = 0;
	public float heightOffset = 0;
    public float lengthStepSize = 1;
	public float widthStepSize = 1;
	public int roundLength = 4;
	public Vector3 uvScale = new Vector3(1,1,1);
	public Vector3 uvOffset = new Vector2(0,0);
	public float nodeSmoothness = 30;
	public int angleInterpolationMode = 1;
	public bool topFace = true;
	public bool bottomFace = true;
	public bool leftFace = true;
	public bool rightFace = true;
	public bool startFace = true;
	public bool endFace = true;
}
