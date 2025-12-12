using Fusion;
using Meta.XR.MRUtilityKit;
using Meta.XR.Util;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static Meta.XR.MRUtilityKit.FindSpawnPositions;


namespace Assets.CryptoKartz.Scripts
{

    public class ERLPlatformSpawner : NetworkBehaviour
    {
        public float OverrideBounds = -1;
        public bool CheckOverlaps = true;
        public int MaxIterations = 1000;
        public float SurfaceClearanceDistance = 0.1f;
        public MRUKAnchor.SceneLabels Labels = ~(MRUKAnchor.SceneLabels)0;
        public LayerMask LayerMask = -1;
        [SerializeField] public GameObject SpawnObject;





        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void InitializePlatform()
        {
            var room = transform.GetComponent<MRUK>().GetCurrentRoom();
            if (room == null || Runner.IsServer) return;

            var prefabBounds = Utilities.GetPrefabBounds(SpawnObject);
            float minRadius = 0.0f;
            const float clearanceDistance = 0.01f;
            float baseOffset = -prefabBounds?.min.y ?? 0.0f;
            float centerOffset = prefabBounds?.center.y ?? 0.0f;
            Bounds adjustedBounds = new();

            if (prefabBounds.HasValue)
            {
                minRadius = Mathf.Min(-prefabBounds.Value.min.x, -prefabBounds.Value.min.z, prefabBounds.Value.max.x, prefabBounds.Value.max.z);
                if (minRadius < 0f)
                {
                    minRadius = 0f;
                }

                var min = prefabBounds.Value.min;
                var max = prefabBounds.Value.max;
                min.y += clearanceDistance;
                if (max.y < min.y)
                {
                    max.y = min.y;
                }

                adjustedBounds.SetMinMax(min, max);
                if (OverrideBounds > 0)
                {
                    Vector3 center = new Vector3(0f, clearanceDistance, 0f);
                    Vector3 size = new Vector3(OverrideBounds * 2f, clearanceDistance * 2f, OverrideBounds * 2f); // OverrideBounds represents the extents, not the size
                    adjustedBounds = new Bounds(center, size);
                }
            }

            bool foundValidSpawnPosition = false;
            for (int j = 0; j < MaxIterations; ++j)
            {
                Vector3 spawnPosition = Vector3.zero;
                Vector3 spawnNormal = Vector3.zero;
                MRUK.SurfaceType surfaceType = MRUK.SurfaceType.FACING_UP;


                if (room.GenerateRandomPositionOnSurface(surfaceType, minRadius, new LabelFilter(Labels), out var pos, out var normal))
                {
                    spawnPosition = pos + normal * baseOffset;
                    spawnNormal = normal;
                    var center = spawnPosition + normal * centerOffset;
                    // In some cases, surfaces may protrude through walls and end up outside the room
                    // check to make sure the center of the prefab will spawn inside the room
                    if (!room.IsPositionInRoom(center))
                    {
                        continue;
                    }

                    // Ensure the center of the prefab will not spawn inside a scene volume
                    if (room.IsPositionInSceneVolume(center))
                    {
                        continue;
                    }

                    // Also make sure there is nothing close to the surface that would obstruct it
                    if (room.Raycast(new Ray(pos, normal), SurfaceClearanceDistance, out _))
                    {
                        continue;
                    }
                }


                Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.up, spawnNormal);
                if (CheckOverlaps && prefabBounds.HasValue)
                {
                    if (Physics.CheckBox(spawnPosition + spawnRotation * adjustedBounds.center, adjustedBounds.extents, spawnRotation, LayerMask, QueryTriggerInteraction.Ignore))
                    {
                        continue;
                    }
                }

                foundValidSpawnPosition = true;
                //Instantiate(SpawnObject, spawnPosition, spawnRotation, transform);
                SpawnObject.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

                break;
            }

            if (!foundValidSpawnPosition)
            {
                Debug.LogWarning($"Failed to find valid spawn position after {MaxIterations} iterations.");
            }
        }

    }

}
