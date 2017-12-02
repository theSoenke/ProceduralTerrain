using UnityEngine;

namespace PCG.Voxel
{
    /// <summary>
    /// Visualizes chunk bounds
    /// </summary>
    public class ChunkBounds : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        private int chunkSize;
        private bool isBoundsGenerated;

        private void Start()
        {
            VoxelEngine voxelEngine = GetComponentInParent<VoxelEngine>();
            chunkSize = voxelEngine.terrainSettings.chunkSize;

            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.05f;
            lineRenderer.receiveShadows = false;
            lineRenderer.shadowCastingMode = 0;
            lineRenderer.reflectionProbeUsage = 0;
        }

        #region UnityFunctions
        private void Update()
        {
            if (!isBoundsGenerated)
            {
                DrawBounds();
                isBoundsGenerated = true;
                lineRenderer.enabled = true;
            }
        }
        #endregion

        private void DrawBounds()
        {
            lineRenderer.positionCount = 16;

            Vector3 frontBottomLeft = transform.position;
            Vector3 frontBottomRight = new Vector3(frontBottomLeft.x + chunkSize, frontBottomLeft.y, frontBottomLeft.z);
            Vector3 frontTopLeft = new Vector3(frontBottomLeft.x, frontBottomLeft.y + chunkSize, frontBottomLeft.z);
            Vector3 frontTopRight = new Vector3(frontBottomRight.x, frontBottomRight.y + chunkSize, frontBottomRight.z);
            Vector3 backBottomLeft = new Vector3(frontBottomLeft.x, frontBottomLeft.y, frontBottomLeft.z + chunkSize);
            Vector3 backBottomRight = new Vector3(frontBottomRight.x, frontBottomRight.y, frontBottomRight.z + chunkSize);
            Vector3 backTopLeft = new Vector3(frontTopLeft.x, frontTopLeft.y, frontTopLeft.z + chunkSize);
            Vector3 backTopRight = new Vector3(backTopLeft.x + chunkSize, backTopLeft.y, backTopLeft.z);

            lineRenderer.SetPosition(0, frontBottomLeft);
            lineRenderer.SetPosition(1, frontBottomRight);
            lineRenderer.SetPosition(2, backBottomRight);
            lineRenderer.SetPosition(3, backBottomLeft);
            lineRenderer.SetPosition(4, frontBottomLeft);
            lineRenderer.SetPosition(5, frontTopLeft);
            lineRenderer.SetPosition(6, frontTopRight);
            lineRenderer.SetPosition(7, frontBottomRight);
            lineRenderer.SetPosition(8, frontTopRight);
            lineRenderer.SetPosition(9, backTopRight);
            lineRenderer.SetPosition(10, backBottomRight);
            lineRenderer.SetPosition(11, backTopRight);
            lineRenderer.SetPosition(12, backTopLeft);
            lineRenderer.SetPosition(13, backBottomLeft);
            lineRenderer.SetPosition(14, backTopLeft);
            lineRenderer.SetPosition(15, frontTopLeft);
        }
    }
}
