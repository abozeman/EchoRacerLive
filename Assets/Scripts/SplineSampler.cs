using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Splines;
#endif
using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode()]
public class SplineSampler : MonoBehaviour
{
    [SerializeField]
    private SplineContainer m_splineContainer;

    [SerializeField]
    private int m_splineIndex;
    [SerializeField]
    [Range(0f, 1f)]
    private float m_time;

    [SerializeField]
    private float m_width;

    public SplineContainer Container => m_splineContainer;
    public int NumSplines => m_splineContainer.Splines.Count;

    float3 m_position;
    float3 m_foward;
    float3 m_upVector;

    Vector3 p1;
    Vector3 p2;

    private void Update()
    {
        m_splineContainer.Evaluate(m_splineIndex, m_time, out m_position, out m_foward, out m_upVector);

        //Tangent is the forward direction
        //Find the *right* direction based on the forward;
        float3 right = Vector3.Cross(m_foward, m_upVector).normalized;
        p1 = m_position + (right * m_width);
        p2 = m_position + (-right * m_width);

    }

    public void SampleSplineWidth(int splineIndex, float t, float width, out Vector3 p1, out Vector3 p2)
    {
        m_splineContainer.Evaluate(splineIndex, t, out float3 position, out float3 forward, out float3 upVector);

        float3 right = Vector3.Cross(forward, upVector).normalized;
        p1 = position + (right * width);
        p2 = position + (-right * width);
    }
    public void SampleSplineWidth(float t, float width, out Vector3 p1, out Vector3 p2)
    {
        SampleSplineWidth(m_splineIndex, t, width, out p1, out p2);
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Handles.matrix = transform.localToWorldMatrix;
        Handles.SphereHandleCap(0, p1, Quaternion.identity, .5f, EventType.Repaint);
        Handles.DrawDottedLine(p1, p2, .5f);
        Handles.SphereHandleCap(0, p2, Quaternion.identity, .5f, EventType.Repaint);


        //Debug
        SplineToolUtility.GetSelection();
        
    }
#endif
}
