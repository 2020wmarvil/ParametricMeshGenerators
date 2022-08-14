using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Test : MonoBehaviour
{
    int NumSamples = 10;

    void Start()
    {
        ParametricCurveGenerator Generator = new ParametricCurveGenerator();

        Generator.GetSamplePoints(out Vector3[] PointSamples, new FUniformSampleRange(NumSamples));
        Generator.GenerateMesh(PointSamples, out Mesh mesh);

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
