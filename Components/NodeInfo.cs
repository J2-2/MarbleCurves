using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteInEditMode]
public class NodeInfo : MonoBehaviour
{
	public int shapeMode;
	public float width = 8;
	public float height = 1;
	public float innerRadius = 4;
	public float outerRadius = 5;
	public GameObject customObject = null;
	public float fitOffset = 0;
	public bool stretchToCurve = false;
	public int divisions = 16;
	public float widthOffset = 0;
	public float heightOffset = 0;
    public float lengthStepSize = 1;
	public float widthStepSize = 1;
	public int roundLength = 4;
	public Vector3 uvScale = new Vector3(1,1,1);
	public Vector3 uvOffset = new Vector3(0,0,0);
	public float nodeSmoothness = 30;
	public int angleInterpolationMode = 1;
	public bool topFace = true;
	public bool bottomFace = true;
	public bool leftFace = true;
	public bool rightFace = true;
	public bool startFace = true;
	public bool endFace = true;
	public bool preview = true;

	void Update() {
		if (!Application.isEditor || Application.isPlaying) {
			return;
		}

		LineRenderer line = gameObject.GetComponent<LineRenderer>();

		if (!preview) {
			DestroyImmediate(line);
			return;
		}

		if (line == null) {
			gameObject.AddComponent<LineRenderer>();
			line = gameObject.GetComponent<LineRenderer>();
		}

		List<GameObject> nodes =  getNodes();

		(Func<float, float>, Func<float, float>) xs = Bezier.LambdasFromNodes(NodePositionsDimension(0, nodes), NodeDirectionsDimension(0, NodeSmoothValues(nodes), nodes));
		(Func<float, float>, Func<float, float>) ys = Bezier.LambdasFromNodes(NodePositionsDimension(1, nodes), NodeDirectionsDimension(1, NodeSmoothValues(nodes), nodes));
		(Func<float, float>, Func<float, float>) zs = Bezier.LambdasFromNodes(NodePositionsDimension(2, nodes), NodeDirectionsDimension(2, NodeSmoothValues(nodes), nodes));

		drawCurve(xs.Item1, ys.Item1, zs.Item1, nodes.Count, NodePosition(0, nodes), line);
	}

	private Vector3 NodePosition(int i, List<GameObject> nodes) {
		return nodes[i].GetComponent<Transform>().position;
	}

	private List<GameObject> getNodes() {
		List<GameObject> nodes = new List<GameObject>();
		for (int i = 0; i < gameObject.transform.childCount; i++) {
			nodes.Add(gameObject.transform.GetChild(i).gameObject);
		}
		return nodes;
	}
	private float[] NodePositionsDimension(int dimension, List<GameObject> nodes) {
		float[] positions = new float[nodes.Count];
		for (int i = 0; i < nodes.Count; i++) {
			positions[i] = nodes[i].GetComponent<Transform>().position[dimension] - nodes[0].GetComponent<Transform>().position[dimension];
		}
		return positions;
	}

		private float[] NodeDirectionsDimension(int dimension, float[] scale, List<GameObject> nodes) {
		float[] directions = new float[nodes.Count];
		for (int i = 0; i < nodes.Count; i++) {
			directions[i] = nodes[i].GetComponent<Transform>().right[dimension]*scale[i];
		}
		return directions;
	}

		private float[] NodeSmoothValues(List<GameObject> nodes) {
		float[] smoothnesses = new float[nodes.Count];
		for (int i = 0; i < nodes.Count; i++) {
			smoothnesses[i] = nodes[i].GetComponent<NodeSmoothness>().GetSmoothness();
		}
		return smoothnesses;
	}

	    private void drawCurve(Func<float, float> x, Func<float, float> y, Func<float, float> z, int length, Vector3 offset, LineRenderer line) {
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.widthMultiplier = 0.2f;
        line.positionCount = length*10+1;
        for (int i = 0; i < line.positionCount; i++) {
            float t = (((float) i )/(line.positionCount-1))*(length-1);
            line.SetPosition(i, new Vector3(x(t), y(t), z(t)) + offset);
        }
    }
}
