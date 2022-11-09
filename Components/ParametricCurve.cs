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
	
	private int divisions;
	
	private float masterWidthOffset, masterHeightOffset;
	
	private bool topFace, bottomFace, leftFace, rightFace, startFace, endFace;
	
	private int roundLength;
	
	private Vector3 uvScale;
	private Vector2 uvOffset;
	
	private int shapeMode;
	
	private const int NUM_INTERVALS = 5000;
	
	private const float EPSILON = 0.001f;
	
	
	// the array of vertices for the mesh
	public Vector3[] vertices;
	public Vector2[] uvs;
	public int[] triangles;

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
	
	public void SetUVOffset(Vector2 uvOffset) {
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
				//Debug.Log("unmerged normal: " + PlaneProject(prevNormal, direction(tSteps[k])).ToString());
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
		} else {
			makeVertsAndFacesTube();
		}
	}
	
	// 
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
	
		vertices = new Vector3[numTSteps*numWidthSteps*4];
		uvs = new Vector2[numTSteps*numWidthSteps*4];
		int[,,] points = new int[numTSteps, numWidthSteps, 4];
		
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
			
			Vector3 offsetHeight = stepNormals[i];
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
				points[i,j,0] = k;
				if (evenStepLength) {
					float uvAdjustment = 1;
					if (i == numTSteps-1) {
						uvAdjustment = lastUVAdjustment;
					}
					uvs[k] = new Vector2(((i-1)*stepLength + stepLength*uvAdjustment)*uvScale[0] + uvOffset[0], widthPosition*uvScale[1] + uvOffset[1]);
				} else {
					uvs[k] = new Vector2(arcLengthSum*lengthAdjustment*uvScale[0] + uvOffset[0], widthPosition*uvScale[1] + uvOffset[1]);
				}
				vertices[k++] = finalPosition;
				
				points[i,j,1] = k;
				uvs[k] = uvs[k-1];
				vertices[k++] = finalPosition - offsetHeight*height;
				
				points[i,j,2] = k;
				uvs[k] = uvs[k-2] + new Vector2(0, height*uvScale[2]);
				vertices[k++] = finalPosition - offsetHeight*height;
				
				points[i,j,3] = k;
				uvs[k] = uvs[k-3] + new Vector2(height*uvScale[2], 0);
				vertices[k++] = finalPosition - offsetHeight*height;
				
			}
		}

		triangles = new int[numTSteps*numWidthSteps*4*3];
		for (int i = 0; i < numTSteps*numWidthSteps*4*3; i++) {
			triangles[i] = 0;
		}
		
		// faces
		k = 0;
		for (int i = 0; i < numTSteps; i++) {
			for (int j = 0; j < numWidthSteps; j++) {
				if (i == 0 && startFace) {
					if (j > 0) {
						//file_content += face(points[i][j][3], points[i][j][5], points[i][j][4], points[i][j][7], points[i][j-1][3], points[i][j-1][5])
						triangles[k++] = points[i,j,0];
						triangles[k++] = points[i,j-1,0];
						triangles[k++] = points[i,j,3];
					}
					if (j < numWidthSteps-1) {
						//file_content += face(points[i][j][3], points[i][j][5], points[i][j+1][4], points[i][j+1][7], points[i][j][4], points[i][j][7])
						triangles[k++] = points[i,j,0];
						triangles[k++] = points[i,j,3];
						triangles[k++] = points[i,j+1,3];
					}
				}
				if (i == numTSteps-1 && endFace) {
					if (j > 0) {
						//file_content += face(points[i][j][3], points[i][j][5], points[i][j-1][3], points[i][j-1][5], points[i][j][4], points[i][j][7])
						triangles[k++] = points[i,j,0];
						triangles[k++] = points[i,j,3];
						triangles[k++] = points[i,j-1,0];
					}
					if (j < numWidthSteps-1) {
						//file_content += face(points[i][j][3], points[i][j][5], points[i][j][4], points[i][j][7], points[i][j+1][4], points[i][j+1][7])
						triangles[k++] = points[i,j,0];
						triangles[k++] = points[i,j+1,3];
						triangles[k++] = points[i,j,3];
					}
				}
				if (j == 0 && rightFace) {
					if (i > 0) { 
						triangles[k++] = points[i,j,0];
						triangles[k++] = points[i,j,2];
						triangles[k++] = points[i-1,j,0];
					}
					if (i < numTSteps-1) {
						triangles[k++] = points[i,j,0];
						triangles[k++] = points[i+1,j,2];
						triangles[k++] = points[i,j,2];
					}
				}
				if (j == numWidthSteps-1 && leftFace) {
					if (i > 0) {
						//file_content += face(points[i][j][3], points[i][j][5], points[i][j][4], points[i][j][6], points[i-1][j][3], points[i-1][j][5])
						triangles[k++] = points[i,j,0];
						triangles[k++] = points[i-1,j,0];
						triangles[k++] = points[i,j,2];
					}
					if (i < numTSteps-1) {
						//file_content += face(points[i][j][3], points[i][j][5], points[i+1][j][4], points[i+1][j][6], points[i][j][4], points[i][j][6])
						triangles[k++] = points[i,j,0];
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
	
		vertices = new Vector3[numTSteps*(numWidthSteps+1)*4];
		uvs = new Vector2[numTSteps*(numWidthSteps+1)*4];
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
			
			Vector3 offsetY = stepNormals[i];
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
				Vector3 masterOffset = offsetX * masterWidthOffset  + offsetY * masterHeightOffset;
				Vector3 finalPosition = currPosition + innerRadius*offset + masterOffset;
				points[i,j,0] = k;
				if (evenStepLength) {
					float uvAdjustment = 1;
					if (i == numTSteps-1) {
						uvAdjustment = lastUVAdjustment;
					}
					uvs[k] = new Vector2(((i-1)*stepLength + stepLength*uvAdjustment)*uvScale[0] + uvOffset[0], widthPosition*circumference*uvScale[1] + uvOffset[1]);
				} else {
					uvs[k] = new Vector2(arcLengthSum*lengthAdjustment*uvScale[0] + uvOffset[0], widthPosition*circumference*uvScale[1] + uvOffset[1]);
				}
				vertices[k++] = finalPosition;
				Debug.Log(finalPosition);
				
				points[i,j,1] = k;
				uvs[k] = uvs[k-1];
				vertices[k++] = finalPosition + offset*(outerRadius - innerRadius);
				
				points[i,j,2] = k;
				uvs[k] = uvs[k-2] + new Vector2(0, (outerRadius - innerRadius)*uvScale[2]);
				vertices[k++] = finalPosition + offset*(outerRadius - innerRadius);
				
				points[i,j,3] = k;
				uvs[k] = uvs[k-3] + new Vector2((outerRadius - innerRadius)*uvScale[2], 0);
				vertices[k++] = finalPosition + offset*(outerRadius - innerRadius);
			}
		}
		
		triangles = new int[numTSteps*numWidthSteps*4*3];
		for (int i = 0; i < numTSteps*numWidthSteps*4*3; i++) {
			triangles[i] = 0;
		}
		
		// faces
		k = 0;
		for (int i = 0; i < numTSteps; i++) {
			for (int j = 0; j < numWidthSteps; j++) {	
				if (i == 0) {
					if (j > 0) {
						triangles[k++] = points[i,j,0];
						triangles[k++] = points[i,j-1,0];
						triangles[k++] = points[i,j,3];
					}
					triangles[k++] = points[i,j,0];
					triangles[k++] = points[i,j,3];
					triangles[k++] = points[i,j+1,3];
					if (j == numWidthSteps-1) {
						triangles[k++] = points[i,j+1,0];
						triangles[k++] = points[i,j,0];
						triangles[k++] = points[i,j+1,3];
					}
				}
				if (i == numTSteps-1) {
					if (j > 0) {
						triangles[k++] = points[i,j,0];
						triangles[k++] = points[i,j,3];
						triangles[k++] = points[i,j-1,0];
					}
					triangles[k++] = points[i,j,0];
					triangles[k++] = points[i,j+1,3];
					triangles[k++] = points[i,j,3];
					if (j == numWidthSteps-1) {
						triangles[k++] = points[i,j+1,0];
						triangles[k++] = points[i,j+1,3];
						triangles[k++] = points[i,j,0];
					}
				}
				if (i > 0) {
					if (j > 0)  {
						triangles[k++] = points[i,j,0];
						triangles[k++] = points[i,j-1,0];
						triangles[k++] = points[i-1,j,0];
						
						triangles[k++] = points[i,j,1];
						triangles[k++] = points[i-1,j,1];
						triangles[k++] = points[i,j-1,1];
					}
					if (j == numWidthSteps-1) {
						triangles[k++] = points[i,j+1,0];
						triangles[k++] = points[i,j,0];
						triangles[k++] = points[i-1,j+1,0];
						
						triangles[k++] = points[i,j+1,1];
						triangles[k++] = points[i-1,j+1,1];
						triangles[k++] = points[i,j,1];
					}
				}
				if (i < numTSteps-1) {	
					triangles[k++] = points[i,j,0];
					triangles[k++] = points[i,j+1,0];
					triangles[k++] = points[i+1,j,0];
					
					triangles[k++] = points[i,j,1];
					triangles[k++] = points[i+1,j,1];
					triangles[k++] = points[i,j+1,1];
				}
			}
		}
	}
}
