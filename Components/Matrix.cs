using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matrix
{
    public static string String(float[,] A) {
		string output = "";
		for (int i = 0; i < A.GetLength(0); i++) {
			for (int j = 0; j < A.GetLength(1); j++) {
				output += A[i,j].ToString("0.00");
				if (j != A.GetLength(1)) output += " ";
			}
			output += "\n";
		}
		return output;
	}
	
	public static float[,] Identity(int n) {
		float[,] I = new float[n,n];
		for (int i = 0; i < n; i++) {
			for (int j = 0; j < n; j++) {
				if (i == j) {
					I[i,j] = 1;
				} else {
					I[i,j] = 0;
				}
			}
		}
		return I;
	}
	
	public static (float[,], float[,]) LUDecomposition(float[,] A) {
		int n = A.GetLength(0);
		if (n != A.GetLength(1)) return (null, null);
		float[,] L = Identity(n);
		
		for (int i = 0; i < n; i++) {
			if (i < n-1) {
				if (A[i,i] == 0) {
					return (null, null);
				}
				for (int j = i+1; j < n; j++) {
					L[j,i] = A[j,i]/A[i,i];
					A[j,i] = 0;
					for (int k = i+1; k < n; k++) {
						A[j,k] = A[j,k] - L[j,i]*A[i,k];
					}
				}
			}
		}
		
		return (L, A);
	}
	
	public static float[] ForwardSubstitution(float[,] L, float[] b) {
		int n = b.Length;
		float[] y = new float[n];
		for (int i = 0; i < n; i++) {
			float curr = 0;
			for (int j = 0; j < i; j++) {
				curr += y[j]*L[i,j];
			}
			y[i] = (b[i] - curr) / L[i,i];
		}
		return y;
	}
	
	public static float[] BackSubstitution(float[,] U, float[] y) {
		int n = y.Length;
		float[] x = new float[n];
		for (int i = n-1; i >= 0; i--) {
			float curr = 0;
			for (int j = i+1; j < n; j++) {
				curr += x[j]*U[i,j];
			}
			x[i] = (y[i] - curr) / U[i,i];
		}
		return x;
	}
	
	public static float[] Solve(float[,] A, float[] b) {
		(float[,], float[,]) result = LUDecomposition(A);
		float[,] L = result.Item1;
		float[,] U = result.Item2;
		if (L == null) return null;
		
		float[] y = ForwardSubstitution(L, b);
		float[] x = BackSubstitution(U, y);

		return x;
	}
}
