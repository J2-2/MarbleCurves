using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ParametricCurve {
	
	// parametric functions
	private Func<float, float> x, dx, y, dy, z, dz, theta;
	
	// bounds and step size for t
	private float lower, upper, step;
	private bool evenStepLength;
	private float stepLength;
	
	private Vector3[] normals;
	private int angleInterpolationMode;
	
	//private float length;
	
	private float width, widthStep, height;
	
	private float innerRadius, outerRadius;
	
	private Vector2[] customPoints;
	private List<(float, float)> customUVs;
	private List<(Vector2, Vector2)> customNormals;
	private (Vector3[], Vector2[], int[]) customCap;
	
	private GameObject fitObject;
	private float fitOffset;
	private bool fitStretch;
	
	private int divisions;
	
	private float masterWidthOffset, masterHeightOffset;
	
	private bool topFace, bottomFace, leftFace, rightFace, startFace, endFace;
	
	private int roundLength;
	
	private Vector3 uvScale;
	private Vector3 uvOffset;
	
	private int shapeMode;
	
	private const int NUM_INTERVALS = 5000;
	
	private const float EPSILON = 0.001f;
	
	
	// the array of vertices for the mesh
	public Vector3[] vertices;
	public Vector2[] uvs;
	public int[] triangles;
	public Vector3[] meshNormals;
	public Vector4[] meshTangents;

	// constructor 
    public ParametricCurve(Func<float, float> x, Func<float, float> dx, 
	                       Func<float, float> y, Func<float, float> dy,
						   Func<float, float> z, Func<float, float> dz,
						   Func<float, float> theta) {
		this.x = x;
		this.dx = dx;
		this.y = y;
		this.dy = dy;
		this.z = z;
		this.dz = dz;
		this.theta = theta;
		
		lower = 0;
		upper = 2.05f;
		step = 0.1f;
		
		width = 4;
		widthStep = 1;
		height = 2;
		
		innerRadius = 4;
		outerRadius = 5;
		divisions = 32;
		
		angleInterpolationMode = 1;
		
		evenStepLength = true;
	    stepLength = 1;
		uvScale = new Vector3(1, 1, 1);
		uvOffset = new Vector3(0, 0);
		
		masterWidthOffset = 0;
		masterHeightOffset = 0;
		
		topFace = true;
		bottomFace = true;
		leftFace = true;
		rightFace = true;
		startFace = true;
		endFace = true;
		
		roundLength = 4;
		
		shapeMode = 1;
	}
	
	public void SetShape(int shapeMode) {
		this.shapeMode = shapeMode;
	}
	
	public void SetAngleInterpolationMode(int angleInterpolationMode) {
		this.angleInterpolationMode = angleInterpolationMode;
	}
	
	public void SetNormals(Vector3[] normals) {
		this.normals = normals;
	}
	
	public void SetTheta(Func<float, float> theta) {
		this.theta = theta;
	}
	
	public void SetTRange(float lower, float upper) {
		this.lower = lower;
		this.upper = upper;
	}
	
	public void SetStepLength(float length) {
		stepLength = length;
	}
	
	public void SetBoxSize(float width, float widthStep, float height) {
		this.width = width;
		this.widthStep = widthStep;
		this.height = height;
	}
	
	public void SetRadius(float innerRadius, float outerRadius) {
		this.innerRadius = innerRadius;
		this.outerRadius = outerRadius;
	}
	
	public void SetCustom(Vector2[] customPoints, List<(float, float)> customUVs, List<(Vector2, Vector2)> customNormals, (Vector3[], Vector2[], int[]) customCap) {
		this.customPoints = customPoints;
		this.customUVs = customUVs;
		this.customNormals = customNormals;
		this.customCap = customCap;
	}
	
	public void SetFitObject(GameObject fitObject, float fitOffset, bool fitStretch) {
		this.fitObject = fitObject;
		this.fitOffset = fitOffset;
		this.fitStretch = fitStretch;
	}
	
	
	public void SetDivisions(int divisions) {
		this.divisions = divisions;
	}
	
	public void SetOffset(float widthOffset, float heightOffset) {
		masterWidthOffset = widthOffset;
		masterHeightOffset = heightOffset;
	}
	
	public void SetRound(int round) {
		roundLength = round;
	}
	
	public void SetUVScale(Vector3 uvScale) {
		this.uvScale = uvScale; 
	}
	
	public void SetUVOffset(Vector3 uvOffset) {
		this.uvOffset = uvOffset; 
	}
	
	public void SetFaces(bool top, bool bottom, bool left, bool right, bool start, bool end) {
		topFace = top;
		bottomFace = bottom;
		leftFace = left;
		rightFace = right;
		startFace = start;
		endFace = end;
	}
	
	public Vector3 position(float t) {
		return new Vector3(x(t), y(t), z(t));
	}
	
	// tangent vector at the point t
	public Vector3 direction(float t) {
		Vector3 v = new Vector3(dx(t), dy(t), dz(t));
		return v.normalized; 
	}
	
	// vector perpendicular to t at angle angle
	public Vector3 perp(float t, float angle = 0) {
		Vector3 u, v, w;
		if (dy(t) == 0) {
			u = new Vector3(0, 1, 0);
		} else {
			u = new Vector3(-dx(t),(float) ((Math.Pow(dx(t), 2) + Math.Pow(dz(t), 2)) / dy(t)) , -dz(t));
			if (dy(t) < 0) {
				u = u * (-1);
			}
			Vector3.Normalize(u);
		}
		v = Vector3.Cross(direction(t), u);
		Vector3.Normalize(v);

		w = ((float) Math.Sin(theta(t) + angle)) * u + ((float) Math.Cos(theta(t) + angle)) * v;
		return w.normalized;
	}
	
	public float AngleFromVector(Vector3 v, float t) {
		Vector3 u = perp(t, (float) (Math.PI/2));
		Vector3.Normalize(v);
		
		double theta = Math.Acos(Vector3.Dot(u, v));
		if (Vector3.Dot(perp(t, (float) ((Math.PI*2 - theta) + ((Math.PI/2)))), v) > Vector3.Dot(u, v)) {
			theta = Math.PI*2 - theta;
			//theta = -theta;
		}
		return (float) theta;
	}
	
	public Vector3 PlaneProject(Vector3 prevNorm, Vector3 currDirection) {
		//Vector3 toProject = (prevPoint + prevNorm) - currPoint;
		Vector3 toProject = prevNorm;
		return toProject - Vector3.Dot(toProject, currDirection.normalized)*currDirection.normalized;
	}
	
	public float ArcLength(float t1, float t2, int numIntervals) {
		if (t1 == t2) {
			return 0;
		}
		float stepSize = (t2 - t1)/numIntervals;
		float prev = (float) (Math.Sqrt(Math.Pow(dx(t1), 2) + Math.Pow(dy(t1), 2) + Math.Pow(dz(t1), 2)));
		float curr;
		float sum = 0;
		for (float t = t1 + stepSize; t <= t2; t += stepSize) {
			curr = (float) (Math.Sqrt(Math.Pow(dx(t), 2) + Math.Pow(dy(t), 2) + Math.Pow(dz(t), 2)));
			float midValue = (t + (t - stepSize)) / 2;
			sum += (float) ((prev + 4*Math.Sqrt(Math.Pow(dx(midValue), 2) + Math.Pow(dy(midValue), 2) + Math.Pow(dz(midValue), 2)) + curr) * (stepSize/6));;
			prev = curr;
		}
		return sum;
	}
	
	public (List<float>, float) getTSteps() {
		float totalArcLength = ArcLength(lower, upper, NUM_INTERVALS);
		float lengthAdjustment = 1;
		if (roundLength != 0) {
			lengthAdjustment = (float) (totalArcLength / (roundLength*Math.Round(totalArcLength/roundLength)));
		}
		
		List<float> stepList = new List<float>();
		stepList.Add(lower);
		float stepSize = (upper - lower)/(NUM_INTERVALS);
		float prev = (float) (Math.Sqrt(Math.Pow(dx(lower), 2) + Math.Pow(dy(lower), 2) + Math.Pow(dz(lower), 2)));
		float curr;
		float target = stepLength*lengthAdjustment;
		float sum = 0;
		for (float t = lower + stepSize; t <= upper; t += stepSize) {
			curr = (float) (Math.Sqrt(Math.Pow(dx(t), 2) + Math.Pow(dy(t), 2) + Math.Pow(dz(t), 2)));
			float midValue = (t + (t - stepSize)) / 2;
			float newSum = sum + (float) ((prev + 4*Math.Sqrt(Math.Pow(dx(midValue), 2) + Math.Pow(dy(midValue), 2) + Math.Pow(dz(midValue), 2)) + curr) * (stepSize/6));
			if (newSum > target) {
				if (newSum - target < target - sum) {
					stepList.Add(t);
				} else {
					stepList.Add(t - stepSize);
				}
				target += stepLength*lengthAdjustment;
			}
			sum = newSum;
			prev = curr;
			if (Math.Abs(totalArcLength - target) < EPSILON) {
				stepList.Add(upper);
				return (stepList, 1);
			}
		}
		stepList.Add(upper);
		
		return (stepList, (sum - (target - stepLength*lengthAdjustment)) / stepLength*lengthAdjustment);
	}
	
	public List<float> GetWidthSteps() {
		List<float> widthStepList = new List<float>();
		
		float currWidth = 0;
		bool done = false;
		while (!done) {
			if (currWidth >= width - EPSILON) {
				widthStepList.Add(width);
				done = true;
			} else {
				widthStepList.Add(currWidth);
				currWidth += widthStep;
			}
		}
		
		return widthStepList;
	}
	
	public float Interpolate(float x) {
		return (float) Math.Pow(Math.Sin(x * (Math.PI/2)),2);
	}
	
	public Vector3[] GetStepNormals(List<float> tSteps, Vector3[] nodeNormals) {
		Vector3[] finalNormals = new Vector3[tSteps.Count];
		
		List<int> divisionIndices = new List<int>();
		divisionIndices.Add(0);
		for (int i = 0; i < tSteps.Count - 1; i++) {
			if (tSteps[i] >= divisionIndices.Count) {
				divisionIndices.Add(i);
			}
		}
		divisionIndices.Add(tSteps.Count - 1);
		
		Vector3[,] unmergedNormals = new Vector3[tSteps.Count, 2];
		for (int i = 0; i < divisionIndices.Count - 1; i++) {
			for (int j = divisionIndices[i]; j < divisionIndices[i+1]; j++) {
				Vector3 prevNormal = (j == divisionIndices[i]) ? nodeNormals[i] : unmergedNormals[j-1,0];
				unmergedNormals[j,0] = PlaneProject(prevNormal, direction(tSteps[j]));
				
				int k = divisionIndices[i+1] - (j - divisionIndices[i]) - 1;
				prevNormal = (j == divisionIndices[i]) ? nodeNormals[i+1] : unmergedNormals[k+1,1];
				unmergedNormals[k,1] = PlaneProject(prevNormal, direction(tSteps[k]));
			}
			
			for (int j = divisionIndices[i]; j < divisionIndices[i+1]; j++) {
				float mergeCoefficient = tSteps[j]-i;
				if (angleInterpolationMode == 1) mergeCoefficient = Interpolate(mergeCoefficient);
				finalNormals[j] = (1-mergeCoefficient)*unmergedNormals[j,0] + mergeCoefficient*unmergedNormals[j,1];
				//finalNormals[j] = unmergedNormals[j,1];
			}
		}
		finalNormals[finalNormals.Length-1] = nodeNormals[nodeNormals.Length-1];
		
		return finalNormals;
	}
	
	public void makeVertsAndFaces() {
		
		if (shapeMode == 0) {
			makeVertsAndFacesBox();
		} else if (shapeMode == 1) {
			makeVertsAndFacesTube();
		} else {
			makeVertsAndFacesCustom();
		}
	}
	
	public Mesh FitToCurve() {
		Mesh mesh = fitObject.GetComponent<MeshFilter>().sharedMesh;
		Mesh newMesh = new Mesh();
		
		Dictionary<float, float> XToTMap = new Dictionary<float, float>();
		for (int i = 0; i < mesh.vertices.Length; i++) {
			if (!XToTMap.ContainsKey(mesh.vertices[i].x + fitOffset)) {
				XToTMap[mesh.vertices[i].x + fitOffset] = -1;
			}
		}
		
		List<float> xValues = new List<float>(XToTMap.Keys);
		xValues.Sort();
		
		float scale = 1;
		
		
		if (fitStretch) {
			scale = (ArcLength(lower, upper, NUM_INTERVALS) - fitOffset) / (xValues[xValues.Count-1] - fitOffset);
			
		}
		
		float stepSize = (upper - lower)/(NUM_INTERVALS);
		float prev = (float) (Math.Sqrt(Math.Pow(dx(0), 2) + Math.Pow(dy(0), 2) + Math.Pow(dz(0), 2)));
		float curr;
		int target = 0;
		float sum = 0;
		float t = stepSize;
		while (target < xValues.Count) {
			curr = (float) (Math.Sqrt(Math.Pow(dx(t), 2) + Math.Pow(dy(t), 2) + Math.Pow(dz(t), 2)));
			float midValue = (t + (t - stepSize)) / 2;
			float newSum = sum + (float) ((prev + 4*Math.Sqrt(Math.Pow(dx(midValue), 2) + Math.Pow(dy(midValue), 2) + Math.Pow(dz(midValue), 2)) + curr) * (stepSize/6));
			while (newSum > (xValues[target] - fitOffset)*scale + fitOffset) {
				if (newSum - ((xValues[target] - fitOffset)*scale + fitOffset) < (xValues[target] - fitOffset)*scale + fitOffset - sum) {
					XToTMap[xValues[target]] = t;
				} else {
					XToTMap[xValues[target]] = t - stepSize;
				}
				target += 1;
				if (target == xValues.Count) {
					break;
				}
			}
			t += stepSize;
			sum = newSum;
			prev = curr;
		}
		
		if (XToTMap[xValues[xValues.Count-1]] > upper) {
			return null;
		}
		//for (int i = 0; i < xValues.Count; i++) {
		//	Debug.Log("x value: " + xValues[i].ToString() + ", t value: " + XToTMap[xValues[i]]);
		//}
		
		//Dictionary<float, Vector3> XToNormalMap = new Dictionary<float, Vector3>();
		
		//List<float> tSteps = new List<float>();
		//for (int i = 0; i < xValues.Count; i++) {
		//	tSteps.Add(XToTMap[xValues[i]]);
		//}
		//tSteps.Add(upper);
		//Vector3[] interpNormals = GetStepNormals(tSteps, normals);
		
		List<float> tSteps = new List<float>();
		for (float i = lower; i < upper; i+= 0.01f) {
			tSteps.Add(i);
		}
		tSteps.Add(upper);
		Vector3[] interpNormals = GetStepNormals(tSteps, normals);
		
		//for (int i = 0; i < xValues.Count; i++) {
		//	XToNormalMap[xValues[i]] = interpNormals[i];
		//}
		
		Vector3[] newVertices = new Vector3[mesh.vertices.Length];
		Vector2[] newUV = new Vector2[mesh.uv.Length];
		int[] newTriangles = new int[mesh.triangles.Length];
		Vector3[] newNormals = new Vector3[mesh.normals.Length];
		Vector4[] newTangents = new Vector4[mesh.tangents.Length];
		
		for (int i = 0; i < newVertices.Length; i++) {
			t = XToTMap[mesh.vertices[i].x + fitOffset];
			//Vector3 offsetY = XToNormalMap[mesh.vertices[i].x + fitOffset].normalized;
			Vector3 offsetY = interpNormals[(int) Math.Floor(t*100)].normalized;
			Vector3 offsetX = Vector3.Cross(direction(t), offsetY).normalized;
			newVertices[i] = position(t) + (mesh.vertices[i].y + masterHeightOffset)*offsetY + (mesh.vertices[i].z + masterWidthOffset)*offsetX;
			newUV[i] = mesh.uv[i];
			newNormals[i] = mesh.normals[i].x*(direction(t).normalized) + mesh.normals[i].y*offsetY + mesh.normals[i].z*offsetX;
			Vector3 tangent = mesh.tangents[i].x*(direction(t).normalized) + mesh.tangents[i].y*offsetY + mesh.tangents[i].z*offsetX;
			newTangents[i] = new Vector4(tangent.x, tangent.y, tangent.z, mesh.tangents[i].w);
		}
		for (int i = 0; i < newTriangles.Length; i++) {
			newTriangles[i] = mesh.triangles[i];
		}
		newMesh.vertices = newVertices;
		newMesh.uv = newUV;
		newMesh.triangles = newTriangles;
		newMesh.normals = newNormals;
		newMesh.tangents = newTangents;
		return newMesh;
	}
	
	private void makeVertsAndFacesCustom() {
		
		//Vector2[] customPoints = {new Vector2(-4, 0), new Vector2(-5,0), new Vector2(-6,0), new Vector2(-6,2), new Vector2(-5,2), new Vector2(-4,1), new Vector2(4,1), new Vector2(5,2), new Vector2(6,2), new Vector2(6,0), new Vector2(5, 0), new Vector2(4, 0), new Vector2(-4, 0)};
		//float[] customUV = {2, 3, 4, 6, 7, 8, 16, 17, 18, 20, 21, 22, 30};
		
		int numTSteps = (int) ((upper + step/2 - lower)/step) + 1;
		List<float> stepList = null;
		float lastUVAdjustment = 1;
		if (evenStepLength) {
			(List<float>, float) stepInfo = getTSteps();
			stepList = stepInfo.Item1;
			lastUVAdjustment = stepInfo.Item2;
			numTSteps = stepList.Count;
		}
		
		Vector3[] stepNormals = GetStepNormals(stepList, normals);
		
		int numWidthSteps = customPoints.Length;
		
		vertices = new Vector3[numTSteps*numWidthSteps*2 + customCap.Item1.Length*2];
		uvs = new Vector2[vertices.Length];
		meshNormals = new Vector3[vertices.Length];
		meshTangents = new Vector4[vertices.Length];
		int[,,] points = new int[numTSteps, numWidthSteps, 2];
		
		float totalArcLength = 0;
		float lengthAdjustment = 1;
		if (!evenStepLength) {
			totalArcLength = ArcLength(lower, upper, NUM_INTERVALS*(numTSteps-1));
			if (roundLength != 0) {
				lengthAdjustment = (float) (roundLength*Math.Round(totalArcLength/roundLength) / totalArcLength);
			}
		}
		
		// vertices
		float tPrev = 0;
		
		float arcLengthSum = 0;
		int k = 0;
		for (int i = 0; i < numTSteps; i++) {
			float t = i*step + lower;
			if (evenStepLength) t = stepList[i];
			
			Vector3 currPosition = position(t);
			Vector3 currDirection = direction(t);
			Vector3 normalDirection = currDirection.normalized;
			
			Vector3 offsetHeight = stepNormals[i].normalized;
			Vector3 offset = Vector3.Cross(currDirection, offsetHeight).normalized;
			
			float arcLength = 0;
			if (!evenStepLength) {
				arcLength = ArcLength(tPrev, t, NUM_INTERVALS);
				arcLengthSum += arcLength;
			}
			tPrev = t;
			
			for (int j = 0; j < numWidthSteps; j++) {
				
				Vector3 finalOffset = offset*(customPoints[j].x + masterWidthOffset) + offsetHeight*(customPoints[j].y + masterHeightOffset);
				Vector3 finalPosition = currPosition + finalOffset;
				int firstK = k;
				points[i,j,0] = k;
				if (evenStepLength) {
					float uvAdjustment = 1;
					if (i == numTSteps-1) {
						uvAdjustment = lastUVAdjustment;
					}
					//uvs[k] = new Vector2(0, 0);
					uvs[k] = new Vector2(((i-1)*stepLength + stepLength*uvAdjustment)*uvScale[0] + uvOffset[0], customUVs[j].Item1);
				} else {
					//uvs[k] = new Vector2(0, 0);
					uvs[k] = new Vector2(arcLengthSum*lengthAdjustment*uvScale[0] + uvOffset[0], customUVs[j].Item1);
				}
				meshNormals[k] = customNormals[j].Item1.y*offsetHeight + customNormals[j].Item1.x*offset;
				meshTangents[k] = new Vector4(normalDirection.x, normalDirection.y, normalDirection.z, 1);
				vertices[k++] = finalPosition;
				
				points[i,j,1] = k;
				uvs[k] = new Vector2(uvs[k-1].x, customUVs[j].Item2);
				meshNormals[k] = customNormals[j].Item2.y*offsetHeight + customNormals[j].Item2.x*offset;
				meshTangents[k] = new Vector4(normalDirection.x, normalDirection.y, normalDirection.z, 1);
				vertices[k++] = finalPosition;
			}
		}
		
		int capIndexStart = k;
		Vector3 offsetY = normals[0].normalized;
		Vector3 offsetX = Vector3.Cross(direction(0), offsetY).normalized;
		for (int i = 0; i < customCap.Item1.Length; i++) {
			vertices[k] = position(0) + (customCap.Item1[i].y + masterHeightOffset)*offsetY + (customCap.Item1[i].z + masterWidthOffset)*offsetX;
			meshNormals[k] = (-direction(0)).normalized;
			meshTangents[k] = new Vector4(offsetX.x, offsetX.y, offsetX.z, 1);
			uvs[k++] = customCap.Item2[i];
		}
		
		int capIndexEnd = k;
		offsetY = normals[normals.Length-1].normalized;
		offsetX = Vector3.Cross(direction(normals.Length-1), offsetY).normalized;
		for (int i = 0; i < customCap.Item1.Length; i++) {
			vertices[k] = position(normals.Length-1) + (customCap.Item1[i].y + masterHeightOffset)*offsetY + (customCap.Item1[i].z + masterWidthOffset)*offsetX;
			meshNormals[k] = direction(normals.Length-1);
			meshTangents[k] = new Vector4(offsetX.x, offsetX.y, offsetX.z, 1);
			uvs[k++] = customCap.Item2[i];
		}
		
		triangles = new int[(numTSteps*numWidthSteps*4-4)*3 + customCap.Item3.Length*2];
		
		// faces
		k = 0;
		for (int i = 0; i < numTSteps; i++) {
			for (int j = 0; j < numWidthSteps; j++) {
					
				if (i > 0 && j > 0) {
					triangles[k++] = points[i,j,0];
					triangles[k++] = points[i,j-1,1];
					triangles[k++] = points[i-1,j,0];
				}
				if (i < numTSteps-1 && j < numWidthSteps-1) {
					triangles[k++] = points[i,j,1];
					triangles[k++] = points[i,j+1,0];
					triangles[k++] = points[i+1,j,1];
				}
			}
		}
		
		for (int i = 0; i < customCap.Item3.Length; i++) {
			triangles[k++] = customCap.Item3[i] + capIndexStart;
		}
		
		for (int i = 0; i < customCap.Item3.Length; i += 3) {
			for (int j = 2; j >= 0; j--) {
				triangles[k++] = customCap.Item3[i+j] + capIndexEnd;
			}
		}
	}
	
	private void makeVertsAndFacesBox() {

		int numTSteps = (int) ((upper + step/2 - lower)/step) + 1;
		List<float> stepList = null;
		float lastUVAdjustment = 1;
		if (evenStepLength) {
			(List<float>, float) stepInfo = getTSteps();
			stepList = stepInfo.Item1;
			lastUVAdjustment = stepInfo.Item2;
			numTSteps = stepList.Count;
		}
		
		Vector3[] stepNormals = GetStepNormals(stepList, normals);
		
		List<float> widthStepList = GetWidthSteps();
		int numWidthSteps = widthStepList.Count;
	
		vertices = new Vector3[numTSteps*numWidthSteps*(Convert.ToInt32(topFace) + Convert.ToInt32(bottomFace)) + 
		                       numTSteps*2*(Convert.ToInt32(leftFace) + Convert.ToInt32(rightFace)) + 
							   numWidthSteps*2*(Convert.ToInt32(startFace) + Convert.ToInt32(endFace))];
		uvs = new Vector2[vertices.Length];
		meshNormals = new Vector3[vertices.Length];
		meshTangents = new Vector4[vertices.Length];
		int[,,] points = new int[numTSteps, numWidthSteps, 6];
		
		float totalArcLength = 0;
		float lengthAdjustment = 1;
		if (!evenStepLength) {
			totalArcLength = ArcLength(lower, upper, NUM_INTERVALS*(numTSteps-1));
			if (roundLength != 0) {
				lengthAdjustment = (float) (roundLength*Math.Round(totalArcLength/roundLength) / totalArcLength);
			}
		}

		// vertices
		float tPrev = 0;
		
		float arcLengthSum = 0;
		int k = 0;
		for (int i = 0; i < numTSteps; i++) {
			float t = i*step + lower;
			if (evenStepLength) t = stepList[i];
			
			Vector3 currPosition = position(t);
			Vector3 currDirection = direction(t);
			Vector3 normalDirection = currDirection.normalized;
			
			Vector3 offsetHeight = stepNormals[i].normalized;
			Vector3 offset = Vector3.Cross(currDirection, offsetHeight).normalized;
			
			float arcLength = 0;
			if (!evenStepLength) {
				arcLength = ArcLength(tPrev, t, NUM_INTERVALS);
				arcLengthSum += arcLength;
			}
			tPrev = t;
			
			for (int j = 0; j < numWidthSteps; j++) {
				float widthPosition = widthStepList[j];
				
				Vector3 finalOffset = offset * (widthPosition - (width/ 2)) + offset*masterWidthOffset + offsetHeight*masterHeightOffset;
				Vector3 finalPosition = currPosition + finalOffset;
				
				Vector2 originalUV;
				if (evenStepLength) {
					float uvAdjustment = 1;
					if (i == numTSteps-1) {
						uvAdjustment = lastUVAdjustment;
					}
					originalUV = new Vector2(((i-1)*stepLength + stepLength*uvAdjustment)*uvScale[0] + uvOffset[0], -widthPosition*uvScale[1] - uvOffset[1]);
				} else {
					originalUV = new Vector2(arcLengthSum*lengthAdjustment*uvScale[0] + uvOffset[0], -widthPosition*uvScale[1] - uvOffset[1]);
				}
				if (topFace) {
					points[i,j,0] = k;
					uvs[k] = originalUV;
					meshNormals[k] = offsetHeight;
					meshTangents[k] = new Vector4(normalDirection.x, normalDirection.y, normalDirection.z, 1);
					vertices[k++] = finalPosition;
				}
				
				if (bottomFace) {
					points[i,j,1] = k;
					if (evenStepLength) {
						float uvAdjustment = 1;
						if (i == numTSteps-1) {
							uvAdjustment = lastUVAdjustment;
						}
						uvs[k] = new Vector2(((i-1)*stepLength + stepLength*uvAdjustment)*uvScale[0] + uvOffset[0], widthPosition*uvScale[1] + uvOffset[1]);
					} else {
						uvs[k] = new Vector2(arcLengthSum*lengthAdjustment*uvScale[0] + uvOffset[0], widthPosition*uvScale[1] + uvOffset[1]);
					}
					meshNormals[k] = -offsetHeight;
					meshTangents[k] = new Vector4(normalDirection.x, normalDirection.y, normalDirection.z, 1);
					vertices[k++] = finalPosition - offsetHeight*height;
				}
				
				if (((j == 0) && rightFace) || ((j == numWidthSteps-1) && leftFace)) {
					points[i,j,2] = k;
					if (j == 0) {
						meshNormals[k] = -offset;
						uvs[k] = new Vector2(originalUV.x, uvOffset[2] + height*uvScale[2]);
					} else {
						meshNormals[k] = offset;
						uvs[k] = new Vector2(originalUV.x, uvOffset[2]);
					}
					meshTangents[k] = new Vector4(normalDirection.x, normalDirection.y, normalDirection.z, 1);
					vertices[k++] = finalPosition - offsetHeight*height;
					
					points[i,j,4] = k;
					
					if (j == 0) {
						meshNormals[k] = -offset;
						
						uvs[k] = new Vector2(originalUV.x, uvOffset[2]);
					} else {
						meshNormals[k] = offset;
						uvs[k] = new Vector2(originalUV.x, uvOffset[2] + height*uvScale[2]);
					}
					meshTangents[k] = new Vector4(normalDirection.x, normalDirection.y, normalDirection.z, 1);
					vertices[k++] = finalPosition;
				}
				
				if (((i == 0) && startFace) || ((i == numTSteps-1) && endFace)) {
					points[i,j,3] = k;
					uvs[k] = new Vector2(uvOffset[2], originalUV.y);
					if (i == 0) {
						meshNormals[k] = (-currDirection).normalized;
					} else {
						meshNormals[k] = currDirection.normalized;
					}
					meshTangents[k] = new Vector4(offsetHeight.x, offsetHeight.y, offsetHeight.z, 1);
					vertices[k++] = finalPosition - offsetHeight*height;
					
					points[i,j,5] = k;
					uvs[k] = new Vector2(uvOffset[2] + height*uvScale[2], originalUV.y);
					if (i == 0) {
						meshNormals[k] = (-currDirection).normalized;
					} else {
						meshNormals[k] = currDirection.normalized;
					}
					meshTangents[k] = new Vector4(offsetHeight.x, offsetHeight.y, offsetHeight.z, 1);
					vertices[k++] = finalPosition;
				}
				
			}
		}

		triangles = new int[((numTSteps*numWidthSteps*2 - numTSteps*2 - numWidthSteps*2)*Convert.ToInt32(topFace) +
		                    (numTSteps*numWidthSteps*2 - numTSteps*2 - numWidthSteps*2)*Convert.ToInt32(bottomFace) +
							numTSteps*4*Convert.ToInt32(leftFace) + numTSteps*4*Convert.ToInt32(rightFace) + 
							numWidthSteps*4*Convert.ToInt32(startFace) + numWidthSteps*4*Convert.ToInt32(endFace))*3];
		
		// faces
		k = 0;
		for (int i = 0; i < numTSteps; i++) {
			for (int j = 0; j < numWidthSteps; j++) {
				if (i == 0 && startFace) {
					if (j > 0) {
						triangles[k++] = points[i,j,5];
						triangles[k++] = points[i,j-1,5];
						triangles[k++] = points[i,j,3];
					}
					if (j < numWidthSteps-1) {
						triangles[k++] = points[i,j,5];
						triangles[k++] = points[i,j,3];
						triangles[k++] = points[i,j+1,3];
					}
				}
				if (i == numTSteps-1 && endFace) {
					if (j > 0) {
						triangles[k++] = points[i,j,5];
						triangles[k++] = points[i,j,3];
						triangles[k++] = points[i,j-1,5];
					}
					if (j < numWidthSteps-1) {
						triangles[k++] = points[i,j,5];
						triangles[k++] = points[i,j+1,3];
						triangles[k++] = points[i,j,3];
					}
				}
				if (j == 0 && rightFace) {
					if (i > 0) { 
						triangles[k++] = points[i,j,4];
						triangles[k++] = points[i,j,2];
						triangles[k++] = points[i-1,j,4];
					}
					if (i < numTSteps-1) {
						triangles[k++] = points[i,j,4];
						triangles[k++] = points[i+1,j,2];
						triangles[k++] = points[i,j,2];
					}
				}
				if (j == numWidthSteps-1 && leftFace) {
					if (i > 0) {
						triangles[k++] = points[i,j,4];
						triangles[k++] = points[i-1,j,4];
						triangles[k++] = points[i,j,2];
					}
					if (i < numTSteps-1) {
						triangles[k++] = points[i,j,4];
						triangles[k++] = points[i,j,2];
						triangles[k++] = points[i+1,j,2];
					}
				}
				if (i > 0 && j > 0) {
					if (topFace) {
						triangles[k++] = points[i,j,0];
						triangles[k++] = points[i,j-1,0];
						triangles[k++] = points[i-1,j,0];
					}
					if (bottomFace) {
						triangles[k++] = points[i,j,1];
						triangles[k++] = points[i-1,j,1];
						triangles[k++] = points[i,j-1,1];
					}
				}
				if (i < numTSteps-1 && j < numWidthSteps-1) {
					if (topFace) {
						triangles[k++] = points[i,j,0];
						triangles[k++] = points[i,j+1,0];
						triangles[k++] = points[i+1,j,0];
					}
					if (bottomFace) {
						triangles[k++] = points[i,j,1];
						triangles[k++] = points[i+1,j,1];
						triangles[k++] = points[i,j+1,1];
					}
				}
			}
		}
	}
	
	private void makeVertsAndFacesTube() {
		
		int numTSteps = (int) ((upper + step/2 - lower)/step) + 1;
		List<float> stepList = null;
		float lastUVAdjustment = 1;
		if (evenStepLength) {
			(List<float>, float) stepInfo = getTSteps();
			stepList = stepInfo.Item1;
			lastUVAdjustment = stepInfo.Item2;
			numTSteps = stepList.Count;
		}
		
		Vector3[] stepNormals = GetStepNormals(stepList, normals);
		
		//List<float> widthStepList = GetWidthSteps();
		int numWidthSteps = divisions;
	
		vertices = new Vector3[numTSteps*(numWidthSteps+1)*(Convert.ToInt32(topFace) + Convert.ToInt32(bottomFace)) +  
							   (numWidthSteps+1)*2*(Convert.ToInt32(startFace) + Convert.ToInt32(endFace))];
		uvs = new Vector2[vertices.Length];
		meshNormals = new Vector3[vertices.Length];
		meshTangents = new Vector4[vertices.Length];
		int[,,] points = new int[numTSteps, numWidthSteps+1, 4];
		
		float totalArcLength = 0;
		float lengthAdjustment = 1;
		if (!evenStepLength) {
			totalArcLength = ArcLength(lower, upper, NUM_INTERVALS*(numTSteps-1));
			if (roundLength != 0) {
				lengthAdjustment = (float) (roundLength*Math.Round(totalArcLength/roundLength) / totalArcLength);
			}
		}
		
		float circumference = (float) (innerRadius*Math.PI*2);
		if (roundLength != 0) {
				circumference = (float) (roundLength*Math.Round(circumference/roundLength));
			}

		// vertices
		float tPrev = 0;
		float arcLengthSum = 0;
		int k = 0;
		for (int i = 0; i < numTSteps; i++) {
			float t = i*step + lower;
			if (evenStepLength) t = stepList[i];
			
			Vector3 currPosition = position(t);
			Vector3 currDirection = direction(t);
			Vector3 normalDirection = currDirection.normalized;
			
			Vector3 offsetY = stepNormals[i].normalized;
			Vector3 offsetX = Vector3.Cross(currDirection, offsetY).normalized;
			
			float arcLength = 0;
			if (!evenStepLength) {
				arcLength = ArcLength(tPrev, t, NUM_INTERVALS);
				arcLengthSum += arcLength;
			}
			tPrev = t;
			
			for (int j = 0; j < numWidthSteps+1; j++) {
				//float widthPosition = widthStepList[j];
				float widthPosition = ((float) j)/numWidthSteps;
				
				// offset = offset_x*math.cos((j*2*math.pi)/divisions) + offset_y*math.sin((j*2*math.pi)/divisions)
				Vector3 offset = offsetX * (float) Math.Cos(widthPosition*2*Math.PI) + offsetY * (float) Math.Sin(widthPosition*2*Math.PI);	
				Vector3 normalOffset = offset.normalized;
				Vector3 masterOffset = offsetX * masterWidthOffset  + offsetY * masterHeightOffset;
				Vector3 finalPosition = currPosition + innerRadius*offset + masterOffset;
				
				Vector2 originalUV;
				if (evenStepLength) {
					float uvAdjustment = 1;
					if (i == numTSteps-1) {
						uvAdjustment = lastUVAdjustment;
					}
					originalUV = new Vector2(((i-1)*stepLength + stepLength*uvAdjustment)*uvScale[0] + uvOffset[0], widthPosition*circumference*uvScale[1] + uvOffset[1]);
				} else {
					originalUV = new Vector2(arcLengthSum*lengthAdjustment*uvScale[0] + uvOffset[0], widthPosition*circumference*uvScale[1] + uvOffset[1]);
				}
				
				if (topFace) {
					points[i,j,0] = k;
					uvs[k] = originalUV;
					meshNormals[k] = -offset;
					meshTangents[k] = new Vector4(normalDirection.x, normalDirection.y, normalDirection.z, 1);
					vertices[k++] = finalPosition;
				}
				if (bottomFace) {
					points[i,j,1] = k;
					uvs[k] = originalUV;
					meshNormals[k] = offset;
					meshTangents[k] = new Vector4(normalDirection.x, normalDirection.y, normalDirection.z, 1);
					vertices[k++] = finalPosition + offset*(outerRadius - innerRadius);
				}
				
				if (((i == 0) && startFace)|| ((i == numTSteps-1) && endFace)) {
					points[i,j,2] = k;
					uvs[k] = new Vector2(uvOffset[2] + (outerRadius - innerRadius)*uvScale[2], originalUV.y);
					if (i == 0) {
						meshNormals[k] = (-currDirection).normalized;
					} else {
						meshNormals[k] = currDirection.normalized;
					}
					meshTangents[k] = new Vector4(normalOffset.x, normalOffset.y, normalOffset.z, 1);
					vertices[k++] = finalPosition + offset*(outerRadius - innerRadius);
					
					points[i,j,3] = k;
					uvs[k] = new Vector2(uvOffset[2], originalUV.y);
					if (i == 0) {
						meshNormals[k] = (-currDirection).normalized;
					} else {
						meshNormals[k] = currDirection.normalized;
					}
					meshTangents[k] = new Vector4(normalOffset.x, normalOffset.y, normalOffset.z, 1);
					vertices[k++] = finalPosition;
				}
			}
		}
		
		triangles = new int[((numTSteps*numWidthSteps*2 - (numWidthSteps)*2)*Convert.ToInt32(topFace) +
		                    (numTSteps*numWidthSteps*2 - (numWidthSteps)*2)*Convert.ToInt32(bottomFace) + 
							(numWidthSteps)*4*Convert.ToInt32(startFace) + (numWidthSteps)*4*Convert.ToInt32(endFace))*3];
		
		// faces
		k = 0;
		for (int i = 0; i < numTSteps; i++) {
			for (int j = 0; j < numWidthSteps; j++) {	
				if (i == 0 && startFace) {
					if (j > 0) {
						triangles[k++] = points[i,j,3];
						triangles[k++] = points[i,j-1,3];
						triangles[k++] = points[i,j,2];
					}
					triangles[k++] = points[i,j,3];
					triangles[k++] = points[i,j,2];
					triangles[k++] = points[i,j+1,2];
					if (j == numWidthSteps-1) {
						triangles[k++] = points[i,j+1,3];
						triangles[k++] = points[i,j,3];
						triangles[k++] = points[i,j+1,2];
					}
				}
				if (i == numTSteps-1 && endFace) {
					if (j > 0) {
						triangles[k++] = points[i,j,3];
						triangles[k++] = points[i,j,2];
						triangles[k++] = points[i,j-1,3];
					}
					triangles[k++] = points[i,j,3];
					triangles[k++] = points[i,j+1,2];
					triangles[k++] = points[i,j,2];
					if (j == numWidthSteps-1) {
						triangles[k++] = points[i,j+1,3];
						triangles[k++] = points[i,j+1,2];
						triangles[k++] = points[i,j,3];
					}
				}
				if (i > 0) {
					if (j > 0)  {
						if (topFace) {
							triangles[k++] = points[i,j,0];
							triangles[k++] = points[i,j-1,0];
							triangles[k++] = points[i-1,j,0];
						}
						if (bottomFace) {
							triangles[k++] = points[i,j,1];
							triangles[k++] = points[i-1,j,1];
							triangles[k++] = points[i,j-1,1];
						}
					}
					if (j == numWidthSteps-1) {
						if (topFace) {
							triangles[k++] = points[i,j+1,0];
							triangles[k++] = points[i,j,0];
							triangles[k++] = points[i-1,j+1,0];
						}
						if (bottomFace) {
							triangles[k++] = points[i,j+1,1];
							triangles[k++] = points[i-1,j+1,1];
							triangles[k++] = points[i,j,1];
						}
					}
				}
				if (i < numTSteps-1) {	
				    if (topFace) {
						triangles[k++] = points[i,j,0];
						triangles[k++] = points[i,j+1,0];
						triangles[k++] = points[i+1,j,0];
					}
					if (bottomFace) {
						triangles[k++] = points[i,j,1];
						triangles[k++] = points[i+1,j,1];
						triangles[k++] = points[i,j+1,1];
					}
				}
			}
		}
	}
}
