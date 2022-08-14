using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ParametricCurveClosingMethod
{
	NONE,
	CAPS,
	LOOP
}

public struct FUniformSampleRange {
	public int NumSamples;
	public float Min;
	public float Max;
	public bool bMinInclusive;
	public bool bMaxInclusive;

	public FUniformSampleRange(int NumSamples, float Min=0f, float Max=1f, bool bMinInclusive=true, bool bMaxInclusive=true) {
		this.NumSamples = NumSamples;
		this.Min = Min;
		this.Max = Max;
		this.bMinInclusive = bMinInclusive;
		this.bMaxInclusive = bMaxInclusive;
	}

	public Vector3 GetSample(int SampleIndex) {
		if (SampleIndex < 0 || SampleIndex >= NumSamples) {
			// error
		}



		return Vector3.zero;
	}
}

public class ParametricCurveGenerator {
	Mesh mesh;

	ParametricCurveGenerator() {
		mesh = new Mesh();
	}

	public void GenerateUniformPointSamples(out Vector3[] PointSamples, FUniformSampleRange SampleRange) { 
		PointSamples = new Vector3[SampleRange.NumSamples];

		// MinT to MaxT inclusive in N steps

	}

	public void GenerateMesh() {
		int NumSamples = 1;
		float MinT = 0f;
		float MaxT = 1f;
		// PointSampling Method. provide a default that is a simple N-point polygon extrusion

		Vector3[] PointSamples = new Vector3[NumSamples];

		bool bUniformSamples = false;
		ParametricCurveClosingMethod ClosingMethod = ParametricCurveClosingMethod.NONE;

		int Resolution = 1;


		float phi_Increment = MathHelper.TWO_PI / NumSamples;
		float theta_Increment = MathHelper.TWO_PI / Resolution;

		for (int Sample = 0; Sample < NumSamples; Sample++) {
			// TODO: precompute LUT and sample uniformly?
			float T = Sample * phi_Increment; // TODO: arbitrary range [Min, Max]
			PointSamples[Sample] = new Vector3(); // TODO: get point as function of T
		}



		int NumVerts = NumSamples * Resolution;

		Vector3[] Vertices = new Vector3[NumVerts];
		int[] Triangles = new int[NumVerts * 6];
		int TriangleIndex = 0;

		for (int Sample = 0; Sample < NumSamples; Sample++) {
			int BaseVertexIndex = Sample * Resolution;

			int PreviousSampleIndex = Sample - 1;
			if (PreviousSampleIndex < 0) PreviousSampleIndex += NumSamples;

			Vector3 PreviousPoint = PointSamples[PreviousSampleIndex];
			Vector3 BasePoint = PointSamples[Sample];
			Vector3 NextPoint = PointSamples[(Sample + 1) % NumSamples];

			Vector3 LocalForward = Vector3.Normalize(((BasePoint - PreviousPoint) + (NextPoint - BasePoint)) * 0.5f);
			Vector3 LocalRight = Vector3.Cross(Vector3.up, LocalForward);
			Vector3 LocalUp = Vector3.Cross(LocalForward, LocalRight);

			for (int i = 0; i < Resolution; i++) {
				float theta = i * theta_Increment; // [0, 360)

				// P0, P2, P3, P1 form a clockwise quad from this ring to the next ring
				int P0_Index = BaseVertexIndex + i;
				int P1_Index = BaseVertexIndex + (i + 1) % Resolution;
				int P2_Index = (P0_Index + Resolution) % NumVerts;
				int P3_Index = (P1_Index + Resolution) % NumVerts;

				Vector3 LocalPointUnscaled = LocalRight * Mathf.Cos(theta) + LocalUp * Mathf.Sin(theta);
				Vertices[P0_Index] = PointSamples[Sample] + LocalPointUnscaled * TubeRadius;

				Triangles[TriangleIndex++] = P0_Index;
				Triangles[TriangleIndex++] = P2_Index;
				Triangles[TriangleIndex++] = P1_Index;

				Triangles[TriangleIndex++] = P1_Index;
				Triangles[TriangleIndex++] = P2_Index;
				Triangles[TriangleIndex++] = P3_Index;
			}
		}

		mesh.Clear();
		mesh.vertices = Vertices;
		mesh.triangles = Triangles;
		mesh.RecalculateNormals();
	}
}
