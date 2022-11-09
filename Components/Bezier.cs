using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Bezier
{
	
	public static float[] DivDiff(float tStart, float tEnd, float nodeStartPosition, float nodeEndPosition, float nodeStartDirection, float nodeEndDirection) {
		float[] result = new float[4];
		result[0] = nodeStartPosition;
		result[1] = nodeStartPosition;
		result[2] = nodeEndPosition;
		result[3] = nodeEndPosition;
		
		result[1] = nodeStartDirection;
		result[2] = (nodeEndPosition - nodeStartPosition) / (tEnd - tStart);
		result[3] = nodeEndDirection;
		
		result[3] = (result[3] - result[2]) / (tEnd - tStart);
		result[2] = (result[2] - result[1]) / (tEnd - tStart);
		
		result[3] = (result[3] - result[2]) / (tEnd - tStart);
		
		return result;
	}
	
	public static float EvalNewt(float[] coefficients, float tStart, float tEnd, float t) {
		float result = coefficients[3] * (t - tEnd);
		result = (result + coefficients[2]) * (t - tStart);
		result = (result + coefficients[1]) * (t - tStart);
		result += coefficients[0];
		return result;
	}
	
	public static float[] NewtToStand(float[] newtCoefficients, float tStart, float tEnd) {
		float[] result = new float[4];
		result[0] = newtCoefficients[0] - newtCoefficients[1]*tStart + newtCoefficients[2]*tStart*tStart - newtCoefficients[3]*tStart*tStart*tEnd;
		result[1] = newtCoefficients[1] - 2*newtCoefficients[2]*tStart + 2*newtCoefficients[3]*tStart*tEnd + newtCoefficients[3]*tStart*tStart;
		result[2] = newtCoefficients[2] - newtCoefficients[3]*tEnd - 2*newtCoefficients[3]*tStart;
		result[3] = newtCoefficients[3];
		
		return result;
	}
	
	public static float EvalStand(float[] coefficients, float t) {
		float result = 0;
		float tPower = 1;
		for (int i = 0; i < coefficients.Length; i++) {
			result += coefficients[i]*tPower;
			tPower *= t;
		}
		return result;
	}
	
	public static float[] StandDerivative(float[] coefficients) {
		float[] result = new float[coefficients.Length - 1];
		for (int i = 0; i < coefficients.Length - 1; i++) {
			result[i] = coefficients[i+1]*(i+1);
		}
		return result;
	}
	
	public static Func<float, float> LambdaFromStand(float[] coefficients) {
		return t => EvalStand(coefficients, t);
	}
	
	public static float EvalPiecewise(float[,] polynomials, float t) {
		int n = polynomials.GetLength(0);
		int tprime = (int) t;
		if (tprime < 0) tprime = 0;
		if (tprime >= n) tprime = n-1;
		float[] c = new float[polynomials.GetLength(1)];
		for (int i = 0; i < polynomials.GetLength(1); i++) {
			c[i] = polynomials[tprime, i];
		}
		return EvalStand(c, t);
	}
	
	public static (Func<float, float>, Func<float, float>) LambdasFromNodes(float[] positions, float[] directions) {
		int n = positions.Length;
		float[,] polynomials = new float[n-1, 4];
		float[,] derivatives = new float[n-1, 3];
		for (int i = 0; i < n-1; i++) {
			float[] c = NewtToStand(DivDiff(i, i+1, positions[i], positions[i+1], directions[i], directions[i+1]), i, i+1);
			float[] d = StandDerivative(c);
			for (int j = 0; j < 3; j++) {
				polynomials[i, j] = c[j];
				derivatives[i, j] = d[j];
			}
			polynomials[i, 3] = c[3];
		}
		
		return (t => EvalPiecewise(polynomials, t), t => EvalPiecewise(derivatives, t));
	}
	
	public static float[] Spline(float[] y, float sStart, float sEnd) {
		int n = y.Length;
		
		float[] x = new float[n];
		for (int i = 0; i < n; i++) {
			x[i] = i;
		}
		
		float[] h = new float[n-1];
		float[] r = new float[n-1];
		for (int i = 0; i < n-1; i++) {
			h[i] = x[i+1] - x[i];
			r[i] = (y[i+1] - y[i]) / h[i];
		}
		
		float[,] A = new float[n-2,n-2];
		float[] b = new float[n-2];
		for (int i = 0; i < n-2; i++) {
			A[i,i] = (2 / h[i]) + (2 / h[i+1]);
			if (i > 0) {
				A[i,i-1] = 1 / h[i];
			}
			if (i < n-3) {
				A[i,i+1] = 1 / h[i+1];
			}
			
			b[i] = 3 * ((r[i] / h[i]) + (r[i+1] / h[i+1]));
		}
		b[0] = b[0] - (sStart / h[0]);
		b[n-3] = b[n-3] - (sEnd / h[n-2]);
		
		float[] s = new float[n];
		float[] middleS = Matrix.Solve(A, b);
		for (int i = 1; i < n-1; i++) {
			s[i] = middleS[i-1];
		}
		s[0] = sStart;
		s[n-1] = sEnd;
		
		return s;
	}
}
