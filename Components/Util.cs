using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util
{
    public static void LogVector3(Vector3 v) {
		Debug.Log("(" + string.Format("{0:N3}", v.x) + ", " + string.Format("{0:N3}", v.y) + ", " + string.Format("{0:N3}", v.z) + ")");
	}
	
	public static void LogFloatArray(float[] xs) {
		string str = "{";
		for (int i = 0; i < xs.Length; i++) {
			str += string.Format("{0:N3}", xs[i]);
			if (i != xs.Length - 1) str += ", ";
		}
		str += "}";
		Debug.Log(str);
	}
	
	public static void Log2DFloatArray(float[,] xs, int position) {
		string str = "{";
		for (int i = 0; i < xs.GetLength(1); i++) {
			str += string.Format("{0:N3}", xs[position, i]);
			if (i != xs.GetLength(1) - 1) str += ", ";
		}
		str += "}";
		Debug.Log(str);
	}
}
