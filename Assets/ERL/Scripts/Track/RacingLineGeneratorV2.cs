using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.IO;
using System.Linq;
using System;
using Unity.Splines.Examples;
using Fusion;
using RestClient.Scripts.Clients;

namespace RacingSystem
{

    [System.Serializable]
    public class TrackPoint
    {
        public Vector2 position;
        public float distanceFromStart;

        public TrackPoint(Vector2 pos)
        {
            position = pos;
            distanceFromStart = 0f;
        }
    }
    public class RacingLineGeneratorV2 : NetworkBehaviour, ITrackAPI
    {
        [Header("Racing Line Settings")]
        [Range(0f, 1f)]
        public float racingLinePosition = 0.4f; // 0 = inside, 1 = outside
        public int smoothingIterations = 3;
        public float smoothingFactor = 0.5f;
        public bool optimizeForSpeed = true;

        [Header("Spline Settings")]
        public bool createSplineOnStart = true;
        public Material splineMaterial;
        public float splineWidth = 0.1f;

        [Header("Checkpoint Settings")]
        public float checkpointSpacing = 1.5f;
        public float checkpointWidth = 5f;
        public float checkpointHeight = 3f;
        public GameObject checkpointPrefab;

        [Header("Debug")]
        public bool showDebugGizmos = true;
        public Color insideColor = Color.red;
        public Color outsideColor = Color.blue;
        public Color racingLineColor = Color.green;
        public Color checkpointColor = Color.yellow;

        // Private variables
        private List<TrackPoint> insidePoints = new List<TrackPoint>();
        private List<TrackPoint> outsidePoints = new List<TrackPoint>();
        private List<TrackPoint> racingLinePoints = new List<TrackPoint>();
        private List<GameObject> checkpoints = new List<GameObject>();
        [SerializeField] public GameObject splineContainer;
        private GameObject trackParent;
        private AnimateCarAlongSplineV2 animateCar;

        public RestClientTrackAPI RestClientTrackGenerator;
        private TrackDefinition trackDefinition = new();
        private bool trackDefinitionLoaded = false;


        // Events
        public static event Action<int, GameObject> OnCheckpointTriggered;

        public override void Spawned()
        {
            Console.WriteLine("RacingLineGeneratorV2 Spawned");
            RestClientTrackGenerator.RegisterGetTrackDefinitionCompleteListener(this);

        }

        void Start()
        {
            

            Debug.Log($"Enable Animate Car along spline");
        }

        public void InitializeTrack()
        {
            // Create parent object for organization
            trackParent = new GameObject("Racing Track");
            trackParent.transform.SetParent(transform);

            // Load track data
            LoadTrackBoundaries();

            // Calculate racing line
            CalculateOptimalRacingLine();

            // Create spline
            if (createSplineOnStart)
            {
                CreateRacingSpline();
            }

            SplineContainer container = splineContainer.GetComponent<SplineContainer>();

            SplineUtility.ReverseFlow(container, 0);


            // Generate checkpoints
            //GenerateCheckpoints();
        }

        void LoadTrackBoundaries()
        {
            insidePoints = LoadPointsFromTrackDefinition(trackDefinition, 1);
            outsidePoints = LoadPointsFromTrackDefinition(trackDefinition, 0);

            if (insidePoints.Count == 0 || outsidePoints.Count == 0)
            {
                Debug.LogError("Failed to load track boundaries from CSV files!");
                return;
            }

            // Calculate distances from start for each boundary
            CalculateDistances(insidePoints);
            CalculateDistances(outsidePoints);

            Debug.Log($"Loaded {insidePoints.Count} inside points and {outsidePoints.Count} outside points");
        }

        List<TrackPoint> LoadPointsFromTrackDefinition(TrackDefinition _trackDefinition, int setIndex)
        {
            List<TrackPoint> points = new List<TrackPoint>();

            var count = trackDefinition.geometry.coordinates[setIndex].Length;

            for (int i = 0; i < count; i++)
            {
                float x = trackDefinition.geometry.coordinates[setIndex][i][0];
                float y = trackDefinition.geometry.coordinates[setIndex][i][1];
                points.Add(new TrackPoint(new Vector2(x, y)));
            }

            return points;
        }

        void CalculateDistances(List<TrackPoint> points)
        {
            if (points.Count == 0) return;

            points[0].distanceFromStart = 0f;

            for (int i = 1; i < points.Count; i++)
            {
                float distance = Vector2.Distance(points[i - 1].position, points[i].position);
                points[i].distanceFromStart = points[i - 1].distanceFromStart + distance;
            }
        }

        void CalculateOptimalRacingLine()
        {
            if (insidePoints.Count == 0 || outsidePoints.Count == 0) return;

            racingLinePoints.Clear();

            // Normalize both boundaries to have same number of points for interpolation
            int targetPointCount = Mathf.Max(insidePoints.Count, outsidePoints.Count);
            List<Vector2> normalizedInside = InterpolatePoints(insidePoints, targetPointCount);
            List<Vector2> normalizedOutside = InterpolatePoints(outsidePoints, targetPointCount);

            // Calculate initial racing line
            for (int i = 0; i < targetPointCount; i++)
            {
                Vector2 racingPoint;

                if (optimizeForSpeed)
                {
                    // Calculate optimal position based on track curvature
                    float optimalPosition = CalculateOptimalPosition(normalizedInside, normalizedOutside, i);
                    racingPoint = Vector2.Lerp(normalizedInside[i], normalizedOutside[i], optimalPosition);
                }
                else
                {
                    // Simple interpolation
                    racingPoint = Vector2.Lerp(normalizedInside[i], normalizedOutside[i], racingLinePosition);
                }

                racingLinePoints.Add(new TrackPoint(racingPoint));
            }

            // Smooth the racing line
            SmoothRacingLine();

            // Calculate distances
            CalculateDistances(racingLinePoints);

            Debug.Log($"Generated racing line with {racingLinePoints.Count} points");
        }

        List<Vector2> InterpolatePoints(List<TrackPoint> points, int targetCount)
        {
            List<Vector2> result = new List<Vector2>();

            if (points.Count == 0) return result;
            if (points.Count == 1)
            {
                for (int i = 0; i < targetCount; i++)
                    result.Add(points[0].position);
                return result;
            }

            float totalDistance = points[points.Count - 1].distanceFromStart;

            for (int i = 0; i < targetCount; i++)
            {
                float targetDistance = (float)i / (targetCount - 1) * totalDistance;
                Vector2 interpolatedPoint = GetPointAtDistance(points, targetDistance);
                result.Add(interpolatedPoint);
            }

            return result;
        }

        Vector2 GetPointAtDistance(List<TrackPoint> points, float distance)
        {
            if (points.Count == 0) return Vector2.zero;
            if (points.Count == 1) return points[0].position;

            // Find the segment containing this distance
            for (int i = 1; i < points.Count; i++)
            {
                if (distance <= points[i].distanceFromStart)
                {
                    float segmentLength = points[i].distanceFromStart - points[i - 1].distanceFromStart;
                    if (segmentLength == 0) return points[i - 1].position;

                    float t = (distance - points[i - 1].distanceFromStart) / segmentLength;
                    return Vector2.Lerp(points[i - 1].position, points[i].position, t);
                }
            }

            return points[points.Count - 1].position;
        }

        float CalculateOptimalPosition(List<Vector2> inside, List<Vector2> outside, int index)
        {
            // Calculate track curvature to determine optimal racing line position
            int prevIndex = (index - 1 + inside.Count) % inside.Count;
            int nextIndex = (index + 1) % inside.Count;

            // Calculate curvature using three points
            Vector2 p1 = Vector2.Lerp(inside[prevIndex], outside[prevIndex], 0.5f);
            Vector2 p2 = Vector2.Lerp(inside[index], outside[index], 0.5f);
            Vector2 p3 = Vector2.Lerp(inside[nextIndex], outside[nextIndex], 0.5f);

            float curvature = CalculateCurvature(p1, p2, p3);

            // Adjust racing line position based on curvature
            // For sharp turns, move towards the inside
            // For straight sections, stay in the middle
            float basePosition = racingLinePosition;
            float curvatureAdjustment = Mathf.Clamp(curvature * 2f, -0.3f, 0.3f);

            return Mathf.Clamp01(basePosition - curvatureAdjustment);
        }

        float CalculateCurvature(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            Vector2 v1 = (p2 - p1).normalized;
            Vector2 v2 = (p3 - p2).normalized;

            float cross = v1.x * v2.y - v1.y * v2.x;
            return cross;
        }

        void SmoothRacingLine()
        {
            for (int iteration = 0; iteration < smoothingIterations; iteration++)
            {
                List<Vector2> smoothedPoints = new List<Vector2>();

                for (int i = 0; i < racingLinePoints.Count; i++)
                {
                    int prevIndex = (i - 1 + racingLinePoints.Count) % racingLinePoints.Count;
                    int nextIndex = (i + 1) % racingLinePoints.Count;

                    Vector2 smoothed = Vector2.Lerp(
                        racingLinePoints[i].position,
                        (racingLinePoints[prevIndex].position + racingLinePoints[nextIndex].position) * 0.5f,
                        smoothingFactor
                    );

                    smoothedPoints.Add(smoothed);
                }

                for (int i = 0; i < racingLinePoints.Count; i++)
                {
                    racingLinePoints[i].position = smoothedPoints[i];
                }
            }
        }

        void CreateRacingSpline()
        {
            Spline spline = splineContainer.GetComponent<SplineContainer>().Spline;

            // Clear existing knots
            spline.Clear();

            // Add knots from racing line points
            for (int i = 0; i < racingLinePoints.Count; i++)
            {
                Vector3 position = new Vector3(racingLinePoints[i].position.x, 0f, racingLinePoints[i].position.y);

                BezierKnot knot = new BezierKnot(position);
                spline.Add(knot);
                spline.SetTangentMode(i, TangentMode.AutoSmooth);

            }

            // Make the spline closed (loop back to start)
            spline.Closed = true;

            // Add visual representation
            //AddSplineRenderer(splineObject);

            Debug.Log($"Created spline with {spline.Count} knots");



        }

        void AddSplineRenderer(GameObject splineObject)
        {
            SplineExtrude extruder = splineObject.AddComponent<SplineExtrude>();

            // Create a simple rectangular cross-section
            List<Vector2> shape = new List<Vector2>
            {
                new Vector2(-splineWidth/2, 0),
                new Vector2(splineWidth/2, 0),
                new Vector2(splineWidth/2, 0.01f),
                new Vector2(-splineWidth/2, 0.01f)
            };

            //extruder.CrossSection = shape.ToArray();
            extruder.SegmentsPerUnit = 10f;

            // Add material
            MeshRenderer renderer = splineObject.GetComponent<MeshRenderer>();
            if (renderer != null && splineMaterial != null)
            {
                renderer.material = splineMaterial;
            }
        }

        void GenerateCheckpoints()
        {
            if (insidePoints.Count == 0 || outsidePoints.Count == 0) return;

            // Clear existing checkpoints
            foreach (GameObject checkpoint in checkpoints)
            {
                if (checkpoint != null)
                    DestroyImmediate(checkpoint);
            }
            checkpoints.Clear();

            // Calculate total track length
            float totalLength = CalculateTrackLength();
            int checkpointCount = Mathf.RoundToInt(totalLength / checkpointSpacing);

            GameObject checkpointParent = new GameObject("Checkpoints");
            checkpointParent.transform.SetParent(trackParent.transform);

            for (int i = 0; i < checkpointCount; i++)
            {
                float distance = (float)i / checkpointCount * totalLength;
                CreateCheckpoint(i, distance, checkpointParent.transform);
            }

            Debug.Log($"Generated {checkpoints.Count} checkpoints");
        }

        float CalculateTrackLength()
        {
            float length = 0f;

            // Use the racing line to calculate track length
            for (int i = 0; i < racingLinePoints.Count; i++)
            {
                int nextIndex = (i + 1) % racingLinePoints.Count;
                length += Vector2.Distance(racingLinePoints[i].position, racingLinePoints[nextIndex].position);
            }

            return length;
        }

        void CreateCheckpoint(int index, float distance, Transform parent)
        {
            //// Get positions at this distance along the track
            //Vector2 insidePos = GetPointAtDistanceOnBoundary(insidePoints, distance);
            //Vector2 outsidePos = GetPointAtDistanceOnBoundary(outsidePoints, distance);

            //// Calculate checkpoint position and rotation
            //Vector2 checkpointCenter = (insidePos + outsidePos) * 0.5f;
            //Vector2 direction = (outsidePos - insidePos).normalized;
            //Vector2 forward = new Vector2(-direction.y, direction.x); // Perpendicular to the checkpoint line

            //// Create checkpoint GameObject
            //GameObject checkpoint = new GameObject($"Checkpoint_{index:D3}");
            //checkpoint.transform.SetParent(parent);
            //checkpoint.transform.position = new Vector3(checkpointCenter.x, checkpointHeight * 0.5f, checkpointCenter.y);

            //// Calculate width based on track width at this point
            //float width = Vector2.Distance(insidePos, outsidePos) + 1f; // Add 1m buffer

            //// Add BoxCollider as trigger
            //BoxCollider collider = checkpoint.AddComponent<BoxCollider>();
            //collider.size = new Vector3(width, checkpointHeight, 0.2f);
            //collider.isTrigger = true;

            //// Rotate to align with track
            //float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
            //checkpoint.transform.rotation = Quaternion.Euler(0, angle - 90f, 0);

            //// Add checkpoint trigger component
            //CheckpointTrigger trigger = checkpoint.AddComponent<CheckpointTrigger>();
            //trigger.checkpointIndex = index;

            //// Add to list
            //checkpoints.Add(checkpoint);
        }

        Vector2 GetPointAtDistanceOnBoundary(List<TrackPoint> boundary, float targetDistance)
        {
            if (boundary.Count == 0) return Vector2.zero;

            // Normalize distance to boundary length
            float boundaryLength = boundary[boundary.Count - 1].distanceFromStart;
            float normalizedDistance = (targetDistance % boundaryLength);

            return GetPointAtDistance(boundary, normalizedDistance);
        }

        void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            // Draw inside boundary
            Gizmos.color = insideColor;
            DrawBoundary(insidePoints);

            // Draw outside boundary  
            Gizmos.color = outsideColor;
            DrawBoundary(outsidePoints);

            // Draw racing line
            Gizmos.color = racingLineColor;
            DrawBoundary(racingLinePoints);

            // Draw checkpoints
            Gizmos.color = checkpointColor;
            foreach (GameObject checkpoint in checkpoints)
            {
                if (checkpoint != null)
                {
                    BoxCollider collider = checkpoint.GetComponent<BoxCollider>();
                    if (collider != null)
                    {
                        Gizmos.matrix = checkpoint.transform.localToWorldMatrix;
                        Gizmos.DrawWireCube(Vector3.zero, collider.size);
                    }
                }
            }
        }

        void DrawBoundary(List<TrackPoint> points)
        {
            if (points.Count < 2) return;

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 current = new Vector3(points[i].position.x, 0.1f, points[i].position.y);
                Vector3 next = new Vector3(points[(i + 1) % points.Count].position.x, 0.1f, points[(i + 1) % points.Count].position.y);

                Gizmos.DrawLine(current, next);
                Gizmos.DrawSphere(current, 0.05f);
            }
        }

        // Public methods for runtime adjustments
        public void RegenerateRacingLine()
        {
            CalculateOptimalRacingLine();
            if (splineContainer != null)
            {
                CreateRacingSpline();
            }
        }

        public void RegenerateCheckpoints()
        {
            GenerateCheckpoints();
        }

        public List<Vector3> GetRacingLinePoints3D()
        {
            List<Vector3> points3D = new List<Vector3>();
            foreach (TrackPoint point in racingLinePoints)
            {
                points3D.Add(new Vector3(point.position.x, 0f, point.position.y));
            }
            return points3D;
        }

        public void OnTrackDefinitionReceived(TrackDefinition _trackDefinition)
        {
            if (trackDefinitionLoaded) return;
            
            trackDefinition = _trackDefinition;
            trackDefinitionLoaded = true;

            InitializeTrack();
            animateCar = GetComponent<AnimateCarAlongSplineV2>();
            animateCar.enabled = true; // Enable after creation
        }
        //public void OnTrackListReceived(TrackList trackList) { }
    }

    // Checkpoint trigger component
    //public class CheckpointTrigger : MonoBehaviour
    //{
    //    public int checkpointIndex;

    //    void OnTriggerEnter(Collider other)
    //    {
    //        // Check if the object has a race car tag or component
    //        if (other.CompareTag("Player") || other.GetComponent<Rigidbody>() != null)
    //        {
    //            //RacingLineGenerator.OnCheckpointTriggered?.Invoke(checkpointIndex, other.gameObject);
    //            Debug.Log($"Checkpoint {checkpointIndex} triggered by {other.name}");
    //        }
    //    }
    //}
}