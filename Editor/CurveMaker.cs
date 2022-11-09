﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class CurveMaker : EditorWindow
{
	// number of nodes to create
	private int numNodes = 3;
	// parent object of the nodes
	private GameObject nodeParent = null;
	// List of node objects
	private List<GameObject> nodes = new List<GameObject>(); 
	// position in list of nodes to add/delete
	private int nodePosition = 1;
	
	// strings for gui displays
	string[] shapeModeStrings = {"rectangle", "tube"};
	string[] angleModeStrings = {"linear", "smooth"};
	string[] uvLabelStrings = {"Length", "Width", "Height"};
	// postion for gui scrollViews
	private Vector2 scrollPosition = Vector2.zero;
	private Vector2 innerScrollPosition = Vector2.zero;
	
	// message for gui
	private string message = "";
	
    // Add menu named "Marble Curves to the Window menu
    [MenuItem("Tools/Marble Curves", false, 120)]
    static void InitWindow()
    {
        // Get existing open window or if none, make a new one:
        CurveMaker window = (CurveMaker)EditorWindow.GetWindow(typeof(CurveMaker));
		window.titleContent = new GUIContent("Marble Curves");
        window.Show();
    }

	// get the position, direction, or normal vector from a node
	private Vector3 NodePosition(int i) {
		return nodes[i].GetComponent<Transform>().position;
	}
	private Vector3 NodeDirection(int i) {
		return nodes[i].GetComponent<Transform>().right;
	}
	private Vector3 NodeNormal(int i) {
		return nodes[i].GetComponent<Transform>().up;
	}
	
	//get an array containing all the position, direction, or normal vectors from the nodes list
	private Vector3[] NodePositions() {
		Vector3[] positions = new Vector3[nodes.Count];
		for (int i = 0; i < nodes.Count; i++) {
			positions[i] = NodePosition(i);
		}
		return positions;
	}
	private Vector3[] NodeDirections() {
		Vector3[] directions = new Vector3[nodes.Count];
		for (int i = 0; i < nodes.Count; i++) {
			directions[i] = NodeDirection(i);
		}
		return directions;
	}
	private Vector3[] NodeNormals() {
		Vector3[] directions = new Vector3[nodes.Count];
		for (int i = 0; i < nodes.Count; i++) {
			directions[i] = NodeNormal(i);
		}
		return directions;
	}
	
	// get a list of only 1 dimension of the position or direction vectors from the nodes list
	private float[] NodePositionsDimension(int dimension) {
		float[] positions = new float[nodes.Count];
		for (int i = 0; i < nodes.Count; i++) {
			positions[i] = nodes[i].GetComponent<Transform>().position[dimension];
		}
		return positions;
	}
	private float[] NodeDirectionsDimension(int dimension, float[] scale) {
		float[] directions = new float[nodes.Count];
		for (int i = 0; i < nodes.Count; i++) {
			directions[i] = nodes[i].GetComponent<Transform>().right[dimension]*scale[i];
		}
		return directions;
	}
	
	// get an array of all the smoothness values from the nodes in the nodes list
	private float[] NodeSmoothValues() {
		float[] smoothnesses = new float[nodes.Count];
		for (int i = 0; i < nodes.Count; i++) {
			smoothnesses[i] = nodes[i].GetComponent<NodeSmoothness>().GetSmoothness();
		}
		return smoothnesses;
	}

	// rename all the nodes in the nodes list starting at position so their number matches their position
	private void RenameNodes(int position) {
		for (int i = position; i < nodes.Count; i++) {
			nodes[i].name = "Node" + (i + 1).ToString();
		}
	}
	
	// order the node objects in the heirarchy by the order they appear in the nodes list
	private void OrderNodes() {
		for (int i = 0; i < nodes.Count; i++) {
			nodes[i].transform.SetSiblingIndex(i);
		}
	}
	
	// load the currently selected game object into nodeParent and its children into the nodes list
	private void LoadNodes() {
		nodeParent = Selection.activeGameObject;
		if (nodeParent == null) return;
		
		if (nodeParent.GetComponent<NodeInfo>() == null) {
			nodeParent = Selection.activeGameObject.transform.parent.gameObject;
			if (nodeParent == null || nodeParent.GetComponent<NodeInfo>() == null) {
				message = "No Nodes Selected";
				return;
			}
		}
		nodes = new List<GameObject>();
		for (int i = 0; i < nodeParent.transform.childCount; i++) {
			nodes.Add(nodeParent.transform.GetChild(i).gameObject);
		}
		RenameNodes(0);
		message = "Nodes Loaded Successfully";
	}
	
	// delete the node in the nodes list at nodePosition
	private void DeleteNode() {
		if (nodePosition-1 >= nodes.Count || nodePosition-1 < 0) {
			message = "No node at selected position";
			return;
		}
		if (nodes.Count == 2) {
			message = "Can't have less than 2 nodes";
			return;
		}
		DestroyImmediate(nodes[nodePosition-1]);
		nodes.RemoveAt(nodePosition-1);
		RenameNodes(nodePosition-1);
		message = "Node deleted";
	}
	
	// add a node to the end of the nodes list
	private void AddNode() {
		InsertNode(nodes.Count);
	}
	
	// insert a node into the nodes list at position position
	private void InsertNode(int position) {
		if (position <= 0) {
			position = 1;
		}
		if (position > nodes.Count) {
			position = nodes.Count;
			nodes.Add(new GameObject("Node" + (position+1).ToString()));
		} else {
			nodes.Insert((position), new GameObject("Node" + (position+1).ToString()));
		}
		NodeSmoothness ns = nodes[position].AddComponent(typeof(NodeSmoothness)) as NodeSmoothness;
		ns.SetSmoothness(nodeParent.GetComponent<NodeInfo>().nodeSmoothness);
		SpriteRenderer sr = nodes[position].AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
		sr.sprite = Resources.Load<Sprite>("nodeIconMiddle");
		nodes[position].transform.parent = nodeParent.transform;
		RenameNodes(position);
		OrderNodes();
		if (nodes.Count == 1) {
			nodes[position].transform.position = nodeParent.transform.position;
			return;
		}
		if (position == 0) {
			nodes[position].transform.position = nodes[1].transform.position - nodes[1].transform.right*10;
			nodes[position].transform.rotation = nodes[1].transform.rotation;
			return;
		}
		if (position == nodes.Count-1) {
			nodes[position].transform.position = nodes[position-1].transform.position + nodes[position-1].transform.right*10;
			nodes[position].transform.rotation = nodes[position-1].transform.rotation;
			return;
		}
		float nodeSmoothness = nodeParent.GetComponent<NodeInfo>().nodeSmoothness;
		(Func<float, float>, Func<float, float>) xs = Bezier.LambdasFromNodes(
				new float[] {nodes[position-1].transform.position.x, nodes[position+1].transform.position.x}, 
				new float[] {nodes[position-1].transform.right.x*nodeSmoothness, nodes[position+1].transform.right.x*nodeSmoothness});
		(Func<float, float>, Func<float, float>) ys = Bezier.LambdasFromNodes(
				new float[] {nodes[position-1].transform.position.y, nodes[position+1].transform.position.y}, 
				new float[] {nodes[position-1].transform.right.y*nodeSmoothness, nodes[position+1].transform.right.y*nodeSmoothness});
		(Func<float, float>, Func<float, float>) zs = Bezier.LambdasFromNodes(
				new float[] {nodes[position-1].transform.position.z, nodes[position+1].transform.position.z}, 
				new float[] {nodes[position-1].transform.right.z*nodeSmoothness, nodes[position+1].transform.right.z*nodeSmoothness});
				
		nodes[position].transform.position = new Vector3(xs.Item1(0.5f), ys.Item1(0.5f), zs.Item1(0.5f));
		nodes[position].transform.right = (new Vector3(xs.Item2(0.5f), ys.Item2(0.5f), zs.Item2(0.5f))).normalized;
	}

	// draw the GUI elements
    void OnGUI()
    {
		// make it so that the GUI can scroll
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		
		// input for number of nodes to create
		numNodes = EditorGUILayout.IntField("Number of Nodes:", numNodes);
		
		if (GUILayout.Button("Create New Nodes")) {
			nodeParent = new GameObject("Nodes");
			nodeParent.AddComponent(typeof(NodeInfo));
			Vector3 cameraPosition = UnityEditor.SceneView.lastActiveSceneView.camera.transform.position + 
			                         UnityEditor.SceneView.lastActiveSceneView.camera.transform.forward*50;
			nodeParent.transform.position = new Vector3((int) cameraPosition.x, (int) cameraPosition.y, (int) cameraPosition.z);
			nodes = new List<GameObject>(); 
			for (int i = 0; i < numNodes; i++) {
				AddNode();
			}
		}
		if (GUILayout.Button("Load Selected Nodes")) {
			LoadNodes();
		}
		
		nodePosition = EditorGUILayout.IntField("Position:", nodePosition);
		if (GUILayout.Button("Delete Node")) {
			DeleteNode();
		}
		if (GUILayout.Button("Add Node")) {
			InsertNode(nodePosition-1);
		}
		
		EditorGUILayout.Space();
		
		if (nodeParent == null) {
			EditorGUILayout.LabelField(message);
			EditorGUILayout.EndScrollView();
			return;
		}
		NodeInfo ni = nodeParent.GetComponent<NodeInfo>();
		if (ni == null) {
			EditorGUILayout.LabelField(message);
			EditorGUILayout.EndScrollView();
			return;
		}
		
		EditorGUILayout.BeginHorizontal();
		UnityEditor.EditorGUIUtility.labelWidth = 1;
		EditorGUILayout.LabelField("Shape:");
		ni.shapeMode = GUILayout.SelectionGrid(ni.shapeMode, shapeModeStrings, 2);
		EditorGUILayout.EndHorizontal();
		
		
		EditorGUILayout.BeginHorizontal();
		if (ni.shapeMode == 0) {
			UnityEditor.EditorGUIUtility.labelWidth = 50;
			ni.width = EditorGUILayout.FloatField("Width:", ni.width);
		} else {
			UnityEditor.EditorGUIUtility.labelWidth = 80;
			ni.innerRadius = EditorGUILayout.FloatField("Inner Radius:", ni.innerRadius);
		}
		UnityEditor.EditorGUIUtility.labelWidth = 80;
		ni.widthOffset = EditorGUILayout.FloatField("Width Offset:", ni.widthOffset);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		if (ni.shapeMode == 0) {
			UnityEditor.EditorGUIUtility.labelWidth = 50;
			ni.height = EditorGUILayout.FloatField("Height:", ni.height);
		} else {
			UnityEditor.EditorGUIUtility.labelWidth = 80;
			ni.outerRadius = EditorGUILayout.FloatField("Outer Radius:", ni.outerRadius);
		}
		UnityEditor.EditorGUIUtility.labelWidth = 80;
		ni.heightOffset = EditorGUILayout.FloatField("Height Offset:", ni.heightOffset);
		EditorGUILayout.EndHorizontal();
		
		UnityEditor.EditorGUIUtility.labelWidth = 140;
		ni.lengthStepSize = EditorGUILayout.FloatField("Length Step Size:", ni.lengthStepSize);
		ni.lengthStepSize = Math.Max(ni.lengthStepSize, 0.01f);
		if (ni.shapeMode == 0) {
			ni.widthStepSize = EditorGUILayout.FloatField("Width Step Size:", ni.widthStepSize);
		} else {
			ni.divisions = EditorGUILayout.IntField("Divisions:", ni.divisions);
		}
		ni.widthStepSize = Math.Max(ni.widthStepSize, 0.01f);
		ni.roundLength = Math.Abs(EditorGUILayout.IntField("Round Tile to nearest:", ni.roundLength));
		
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("UVs:");
		for (int i = 0; i < 3; i++) {
			UnityEditor.EditorGUIUtility.labelWidth = 90;
			EditorGUILayout.BeginHorizontal();
			ni.uvScale[i] = EditorGUILayout.FloatField(uvLabelStrings[i] + "- Scale:", ni.uvScale[i]);
			UnityEditor.EditorGUIUtility.labelWidth = 40;
			if (i != 2) ni.uvOffset[i] = EditorGUILayout.FloatField("Offset:", ni.uvOffset[i]);
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.Space();
		
		UnityEditor.EditorGUIUtility.labelWidth = 140;
		EditorGUILayout.BeginHorizontal();
		ni.nodeSmoothness = EditorGUILayout.FloatField("Smoothness Value:", ni.nodeSmoothness);
		if (GUILayout.Button("Set All")) {
			for (int i = 0; i < nodes.Count; i++) {
				NodeSmoothness ns = nodes[i].GetComponent<NodeSmoothness>();
				ns.SetSmoothness(ni.nodeSmoothness);
			}
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.LabelField("Node Smoothness Values:");
		UnityEditor.EditorGUIUtility.labelWidth = 15;
		innerScrollPosition = EditorGUILayout.BeginScrollView(innerScrollPosition, GUILayout.Height(65));
		EditorGUILayout.BeginHorizontal();
		for (int i = 0; i < nodes.Count; i++) {
			NodeSmoothness ns = nodes[i].GetComponent<NodeSmoothness>();
			ns.SetSmoothness(EditorGUILayout.FloatField((i+1).ToString() + ":", ns.GetSmoothness()));
			if ((i+1) % 3 == 0) {
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
			}
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndScrollView();
		
		if (GUILayout.Button("Auto-Position Internal Nodes")) {
			float smoothStart = nodes[0].GetComponent<NodeSmoothness>().GetSmoothness();
			float smoothEnd = nodes[nodes.Count-1].GetComponent<NodeSmoothness>().GetSmoothness();
			float[] dxs = Bezier.Spline(NodePositionsDimension(0), 
										NodeDirection(0).x*smoothStart, 
										NodeDirection(nodes.Count-1).x*smoothEnd);
			float[] dys = Bezier.Spline(NodePositionsDimension(1), 
										NodeDirection(0).y*smoothStart, 
										NodeDirection(nodes.Count-1).y*smoothEnd);
			float[] dzs = Bezier.Spline(NodePositionsDimension(2), 
										NodeDirection(0).z*smoothStart, 
										NodeDirection(nodes.Count-1).z*smoothEnd);
										
			for (int i = 0; i < nodes.Count; i++) {
				Vector3 d = new Vector3(dxs[i], dys[i], dzs[i]);
				nodes[i].transform.right = d.normalized;
				nodes[i].GetComponent<NodeSmoothness>().SetSmoothness(d.magnitude);
			}
		}
		
		EditorGUILayout.BeginHorizontal();
		UnityEditor.EditorGUIUtility.labelWidth = 50;
		EditorGUILayout.LabelField("Angle Interpolation:");
		ni.angleInterpolationMode = GUILayout.SelectionGrid(ni.angleInterpolationMode, angleModeStrings, 2);
		EditorGUILayout.EndHorizontal();
		
		UnityEditor.EditorGUIUtility.labelWidth = 50;
		EditorGUILayout.LabelField("Include faces:");
		EditorGUILayout.BeginHorizontal();
		ni.topFace = EditorGUILayout.Toggle("Top:", ni.topFace);
		ni.leftFace = EditorGUILayout.Toggle("Left:", ni.leftFace);
		ni.startFace = EditorGUILayout.Toggle("Start:", ni.startFace);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		ni.bottomFace = EditorGUILayout.Toggle("Bottom:", ni.bottomFace);
		ni.rightFace = EditorGUILayout.Toggle("Right:", ni.rightFace);
		ni.endFace = EditorGUILayout.Toggle("End:", ni.endFace);
		EditorGUILayout.EndHorizontal();
		
		if (GUILayout.Button("Make Curve")) {
			
			if (nodeParent == null) {
				message = "Selected nodes have been deleted";
			} else {			
				(Func<float, float>, Func<float, float>) xs = Bezier.LambdasFromNodes(NodePositionsDimension(0), NodeDirectionsDimension(0, NodeSmoothValues()));
				(Func<float, float>, Func<float, float>) ys = Bezier.LambdasFromNodes(NodePositionsDimension(1), NodeDirectionsDimension(1, NodeSmoothValues()));
				(Func<float, float>, Func<float, float>) zs = Bezier.LambdasFromNodes(NodePositionsDimension(2), NodeDirectionsDimension(2, NodeSmoothValues()));

				ParametricCurve curve = new ParametricCurve(xs.Item1, xs.Item2, ys.Item1, ys.Item2, zs.Item1, zs.Item2, t => 0);

				curve.SetShape(ni.shapeMode);
				curve.SetTRange(0, nodes.Count - 1);
				curve.SetStepLength(ni.lengthStepSize);
				curve.SetBoxSize(ni.width, ni.widthStepSize, ni.height);
				curve.SetRadius(ni.innerRadius, ni.outerRadius);
				curve.SetDivisions(ni.divisions);
				curve.SetOffset(ni.widthOffset, ni.heightOffset);
				curve.SetFaces(ni.topFace, ni.bottomFace, ni.leftFace, ni.rightFace, ni.startFace, ni.endFace);
				curve.SetRound(ni.roundLength);
				curve.SetUVScale(ni.uvScale);
				curve.SetUVOffset(ni.uvOffset);
				curve.SetNormals(NodeNormals());
				curve.SetAngleInterpolationMode(ni.angleInterpolationMode);
				
				float[] angles = new float[nodes.Count];
				float[] angleChanges = new float[nodes.Count];
				for (int i = 0; i < nodes.Count; i++) {
					angles[i] = curve.AngleFromVector(nodes[i].transform.up, i);
					angleChanges[i] = 0;
				}
				(Func<float, float>, Func<float, float>) thetas = Bezier.LambdasFromNodes(angles, angleChanges);
				curve.SetTheta(thetas.Item1);
			
				curve.makeVertsAndFaces();
			
				GameObject EmptyObj = new GameObject("Curve");
			
				Mesh mesh = new Mesh();
				MeshFilter mf = EmptyObj.AddComponent(typeof(MeshFilter)) as MeshFilter;
				MeshCollider mc = EmptyObj.AddComponent<MeshCollider>();
				mesh.vertices = curve.vertices;
				mesh.uv = curve.uvs;
				mesh.triangles = curve.triangles;
				mesh.name = "curveMesh";
				mf.sharedMesh = mesh;
				mc.sharedMesh = mesh;
				
				MeshRenderer mr = EmptyObj.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
				mr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
			
				message = "Curve Created Successfully";
			}
		}
		
		EditorGUILayout.LabelField(message);

		EditorGUILayout.EndScrollView();
		
		EditorGUILayout.Space();
		
    }
}
