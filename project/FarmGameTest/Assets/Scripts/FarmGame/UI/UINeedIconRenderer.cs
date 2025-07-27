using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FarmGame.Managers;
using FarmGame.World.Crops;

namespace FarmGame.UI
{
    /// <summary>
    /// Handles rendering of "need water" and "ready to harvest" icons above crops.
    /// Uses GPU instancing for efficient rendering.
    /// </summary>
    public class UINeedIconRenderer : MonoBehaviour
    {
        // Managers
        private GridManager m_gridManager;

        [SerializeField] private Material iconMaterialWater;
        [SerializeField] private Material iconMaterialHarvest;
        [SerializeField] private float iconSize = 0.65f;
        [SerializeField] private float heightOffset = 0.75f;

        private Vector3 m_cameraForward;
        private Mesh m_quadMesh;

        // Separate lists for each state
        private readonly List<Matrix4x4> m_matricesWater = new List<Matrix4x4>();
        private readonly List<Matrix4x4> m_matricesHarvest = new List<Matrix4x4>();

        /// <summary>
        /// Initializes the renderer with grid data and a reference direction for facing icons.
        /// </summary>
        public void Initialize(GridManager gridManager, Camera mainCamera)
        {
            m_gridManager = gridManager;
            m_cameraForward = mainCamera.transform.forward;
            
            m_quadMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        }

        /// <summary>
        /// Rebuilds the list of transformation matrices for instanced rendering,
        /// filtering crops by the given state.
        /// </summary>
        public void RebuildMatrices(CropState state)
        {
            // Choose the right list to clear/populate
            List<Matrix4x4> targetList = state == CropState.WaterNeeded ? m_matricesWater : m_matricesHarvest;
            targetList.Clear();

            // Filter only those crops that match the requested state
            List<Transform> crops = m_gridManager.CropGrid.Values
                .Where(crop => crop && crop.State == state)
                .Select(crop => crop.transform)
                .ToList();

            foreach (Transform cropTransform in crops)
            {
                Vector3 pos = cropTransform.position + Vector3.up * heightOffset;
                Quaternion rot = Quaternion.LookRotation(m_cameraForward);
                Matrix4x4 matrix = Matrix4x4.TRS(pos, rot, Vector3.one * iconSize);
                targetList.Add(matrix);
            }
        }

        private void LateUpdate()
        {
            // Draw water icons
            if (m_matricesWater.Count > 0) Graphics.DrawMeshInstanced( m_quadMesh, 0, iconMaterialWater,
                m_matricesWater);

            // Draw harvest icons
            if (m_matricesHarvest.Count > 0) Graphics.DrawMeshInstanced(m_quadMesh, 0, iconMaterialHarvest,
                m_matricesHarvest);
        }
    }
}
