using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcessMesh
{
	private const float EPSILON = 0.001f;
	
	class PointComparer : IComparer<Vector2> {
    public int Compare(Vector2 v1, Vector2 v2) {
			if (v1 == null || v2 == null)
			{
				return 0;
			}
          
			// "CompareTo()" method
			return (Math.Atan2(v1.y, v1.x)).CompareTo(Math.Atan2(v2.y, v2.x));
          
		}
	}
	
	private static List<Vector2> Unique(List<Vector2> list) {
		List<Vector2> uniqueList = new List<Vector2>();
		for (int i = 0; i < list.Count; i++) {
			bool inUniqueList = false;
			for (int j = 0; j < uniqueList.Count; j++) {
				inUniqueList = inUniqueList || list[i].x == uniqueList[j].x && list[i].y == uniqueList[j].y;
			}
			if (!inUniqueList) uniqueList.Add(list[i]);
		}
		return uniqueList;
	}
	
	private static (List<(int, int)>, List<List<int>>) GetPairsAndFaces(Mesh mesh) {
		List<(int, int)> pairs = new List<(int, int)>();
		List<List<int>> faces = new List<List<int>>();
		
		for (int i = 0; i < mesh.triangles.Length; i += 3) {
			int numZero = 0;
			bool reverse = false;
			List<int> indices = new List<int>();
			for (int j = 0; j < 3; j++) {
				if (mesh.vertices[mesh.triangles[i+j]].x < EPSILON) {
					numZero++;
					indices.Add(mesh.triangles[i+j]);
				} else {
					if (j == 1) {
						reverse = true;
					}
				}
			}
			if (numZero == 2) {
				if (reverse) {
					pairs.Add((indices[1], indices[0]));
				} else {					
					pairs.Add((indices[0], indices[1]));
				}
			}
			if (numZero == 3) {
				faces.Add(indices);
			}
		}
		return (pairs, faces);
	}
	
	private static int GetFirst(Mesh mesh, List<(int, int)> pairs) {
		int first;
		for (int i = 0; i < pairs.Count; i++) {
			first = pairs[i].Item1;
			bool found = false;
			for (int j = 0; j < pairs.Count; j++) {
				if (mesh.vertices[pairs[j].Item2] == mesh.vertices[first]) {
					found = true;
					break;
				}
			}
			if (!found) return first;
		}
		return pairs[0].Item1;
	}
	
    public static (Vector2[], List<(float, float)>, List<(Vector2, Vector2)>, (Vector3[], Vector2[], int[])) GetPointsUVsAndCap(Mesh mesh) {
		List<Vector2> points = new List<Vector2>();
		List<(float, float)> UVs = new List<(float, float)>();
		List<(Vector2, Vector2)> normals = new List<(Vector2, Vector2)>();
		List<Vector3> capVertices = new List<Vector3>();
		List<Vector2> capUV = new List<Vector2>();
		List<int> capTriangles = new List<int>();
		
		(List<(int, int)>, List<List<int>>) result = GetPairsAndFaces(mesh);
		List<(int, int)> pairs = result.Item1;
		List<List<int>> faces = result.Item2;
		
		if (pairs.Count == 0) {
			return (null, null, null, (null, null, null));
		}
		

		int first = GetFirst(mesh, pairs);
		points.Add(new Vector2(mesh.vertices[first].z, mesh.vertices[first].y));
		UVs.Add((0, mesh.uv[first].y));
		normals.Add((new Vector2(0, 0), new Vector2(mesh.normals[first].z, mesh.normals[first].y)));
		//UVs.Add((mesh.uv[pairs[0].Item2].y, 0));
		bool done = false;
		int i = 0;
		int steps = 0;
		while (!done && steps < 1000) {
			steps += 1;
			if (i == pairs.Count) {
				done = true;
			} else {
				Vector2 newPoint = new Vector2(mesh.vertices[pairs[i].Item1].z, mesh.vertices[pairs[i].Item1].y);
				if (points[points.Count-1] == newPoint) {
					newPoint = new Vector2(mesh.vertices[pairs[i].Item2].z, mesh.vertices[pairs[i].Item2].y);
					UVs[UVs.Count-1] = (UVs[UVs.Count-1].Item1, mesh.uv[pairs[i].Item1].y);
					UVs.Add((mesh.uv[pairs[i].Item2].y, 0));
					normals[normals.Count-1] = (normals[normals.Count-1].Item1, new Vector2(mesh.normals[pairs[i].Item1].z, mesh.normals[pairs[i].Item1].y));
					normals.Add((new Vector2(mesh.normals[pairs[i].Item2].z, mesh.normals[pairs[i].Item2].y), new Vector2(0, 0)));
					pairs.RemoveAt(i);
					points.Add(newPoint);
					if (newPoint == points[0]) {
						done = true;
					}
					i = 0;
					continue;
				}
				newPoint = new Vector2(mesh.vertices[pairs[i].Item2].z, mesh.vertices[pairs[i].Item2].y);
				if (points[points.Count-1] == newPoint) {
					newPoint = new Vector2(mesh.vertices[pairs[i].Item1].z, mesh.vertices[pairs[i].Item1].y);
					pairs.RemoveAt(i);
					points.Add(newPoint);
					if (newPoint == points[0]) {
						done = true;
					}
					i = 0;
					continue;
				}
				i++;
			}
		}
		
		Dictionary<int, int> newIndex = new Dictionary<int, int>();
		for (int j = 0; j < faces.Count; j++) {
			for (int k = 0; k < 3; k++) {
				if (!newIndex.ContainsKey(faces[j][k])) {
					capVertices.Add(mesh.vertices[faces[j][k]]);
					capUV.Add(mesh.uv[faces[j][k]]);
					newIndex[faces[j][k]] = capVertices.Count-1;
				}
				capTriangles.Add(newIndex[faces[j][k]]);
			}
		}
		
		return (points.ToArray(), UVs, normals, (capVertices.ToArray(), capUV.ToArray(), capTriangles.ToArray()));
	}
}
