// TODO: 
// [ ] Tube Generator (from 3D parametric curve)
//     [ ] UniformSampleRange
//     [ ] N-point regular polygon cross section subclass (still abstract)
//     [ ] 
// [ ] 2D Curve Mesh (from 2D parametric curve)
// [ ] Surface Generator (from basis splines and control point grid)
// [ ] Surface Generator (from parametric curve boundary)
// [ ] ???
// [ ] Parallelize
// [ ] Move to compute
// [ ] Non-destructive mesh simplification
// [ ] Intelligent optimizations (sampling based on 'curviness', etc.)
// [ ] Hopf Fibrations
// [ ] Lissajous curves

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
	// TODO: inclusivity // public bool bMinInclusive; // public bool bMaxInclusive; 

	public FUniformSampleRange(int NumSamples, float Min=0f, float Max=1f) {
		this.NumSamples = NumSamples;
		this.Min = Min;
		this.Max = Max;
	}

	public float GetSampleKeyAtIndexUnsafe(int SampleIndex) {
		float T = SampleIndex / (NumSamples - 1); // T in [0, 1]
		return T * (Max - Min) + Min;
	}

	public float GetSampleKeyAtIndexSafe(int SampleIndex) {
		SampleIndex = Mathf.Clamp(SampleIndex, 0, NumSamples - 1);
		float T = SampleIndex / (NumSamples - 1); // T in [0, 1]
		return T * (Max - Min) + Min;
	}

	public void GetSampleKeys(out float[] Keys) {
		Keys = new float[NumSamples];

		for (int SampleIndex = 0; SampleIndex < NumSamples; SampleIndex++) {
			float T = SampleIndex / (NumSamples - 1); // T in [0, 1]
			Keys[SampleIndex] = T * (Max - Min) + Min;
		}
	}
}

// TODO: make abstract base class with virtual overloads
public class ParametricCurveGenerator {
	public Vector3 GetPointAtKey(float Key) { // TODO: overwrite this function in children
		float X = Key;
		float Y = Key;
		float Z = Key;

		return new Vector3(X, Y, Z);
	}

	public int GetCrossSectionVertexCount() { // TODO: overwrite // TODO: GetCrossSectionVertexCountAtKey, with variable number of verts per key
		return 1;
	}

	public void GetCrossSectionAtKey(float Key, out Vector3[] ClockwisePoints) { // TODO: overwrite this function in children
		ClockwisePoints = new Vector3[1];
	}

	public void GetSamplePoints(out Vector3[] PointSamples, FUniformSampleRange SampleRange) { 
		PointSamples = new Vector3[SampleRange.NumSamples];
		SampleRange.GetSampleKeys(out float[] Keys);

		// TODO: compute LUT and get keys uniform

		for (int SampleIndex = 0; SampleIndex < SampleRange.NumSamples; SampleIndex++) {
			PointSamples[SampleIndex] = GetPointAtKey(Keys[SampleIndex]);
		}
	}

	// TODO: Closing method ParametricCurveClosingMethod ClosingMethod = ParametricCurveClosingMethod.NONE;
	public void GenerateMesh(in Vector3[] PointSamples, out Mesh mesh) {
		int NumSamples = PointSamples.Length;
		int CrossSectionVertCount = GetCrossSectionVertexCount();
		int NumVerts = NumSamples * CrossSectionVertCount;

		Vector3[] Vertices = new Vector3[NumVerts];
		int[] Triangles = new int[NumVerts * 6];
		int TriangleIndex = 0;

		for (int Sample = 0; Sample < NumSamples; Sample++) {
			int BaseVertexIndex = Sample * CrossSectionVertCount;

			int PreviousSampleIndex = Sample - 1;
			if (PreviousSampleIndex < 0) PreviousSampleIndex += NumSamples;

			Vector3 PreviousPoint = PointSamples[PreviousSampleIndex];
			Vector3 BasePoint = PointSamples[Sample];
			Vector3 NextPoint = PointSamples[(Sample + 1) % NumSamples];

			Vector3 LocalForward = Vector3.Normalize(((BasePoint - PreviousPoint) + (NextPoint - BasePoint)) * 0.5f);
			Vector3 LocalRight = Vector3.Cross(Vector3.up, LocalForward);
			Vector3 LocalUp = Vector3.Cross(LocalForward, LocalRight);

			// TODO: cross section function
			{
				int Resolution = 1;
				float theta_Increment = MathHelper.TWO_PI / Resolution;
				for (int i = 0; i < Resolution; i++) {
					float theta = i * theta_Increment; // [0, 360)

					// P0, P2, P3, P1 form a clockwise quad from this ring to the next ring
					int P0_Index = BaseVertexIndex + i;
					int P1_Index = BaseVertexIndex + (i + 1) % Resolution;
					int P2_Index = (P0_Index + Resolution) % NumVerts;
					int P3_Index = (P1_Index + Resolution) % NumVerts;

					Vector3 LocalPointUnscaled = LocalRight * Mathf.Cos(theta) + LocalUp * Mathf.Sin(theta);
					//Vertices[P0_Index] = PointSamples[Sample] + LocalPointUnscaled * TubeRadius;

					Triangles[TriangleIndex++] = P0_Index;
					Triangles[TriangleIndex++] = P2_Index;
					Triangles[TriangleIndex++] = P1_Index;

					Triangles[TriangleIndex++] = P1_Index;
					Triangles[TriangleIndex++] = P2_Index;
					Triangles[TriangleIndex++] = P3_Index;
				}
			}
		}

		mesh = new Mesh();
		mesh.vertices = Vertices;
		mesh.triangles = Triangles;
		mesh.RecalculateNormals();

		// TODO: normals, UVs
	}
}
