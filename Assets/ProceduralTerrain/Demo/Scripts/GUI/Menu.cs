using System;
using UnityEngine;
using UnityEngine.UI;

namespace PCG.Voxel
{
    public class Menu : MonoBehaviour
    {
        public VoxelEngine voxelEngine;
        public Dropdown algorithmDropdown;
        public Dropdown terrainTypeDropdown;
        public InputField viewDistanceInput;
        public Button generateButton;
        public GameObject generateMessage;
        public GameObject terrainOptions;
        public GameObject controlsText;

        private TerrainSettings terrainSettings;
        private bool cursorIsLocked;
        private bool showMenu;
        private bool isTerrainGenerated;


        #region UnityFunctions
        public void Start()
        {
            terrainSettings = voxelEngine.terrainSettings;
            generateButton.onClick.AddListener(OnGenerateClick);

            terrainOptions.SetActive(true);
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isTerrainGenerated)
                {
                    showMenu = !showMenu;
                    SetCursorLock(!showMenu);
                    terrainOptions.SetActive(showMenu);
                    controlsText.SetActive(showMenu);
                }
            }
        }
        #endregion

        private void SetCursorLock(bool lockCursor)
        {
            cursorIsLocked = lockCursor;

            if (cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void OnGenerateClick()
        {
            isTerrainGenerated = false;
            showMenu = false;
            terrainOptions.SetActive(false);
            generateMessage.SetActive(true);
            SetCursorLock(true);
            SetTerrainValues();

            voxelEngine.OnTerrainReady += OnPlayerSpawn;
            voxelEngine.GenerateTerrain();
        }

        private void SetTerrainValues()
        {
            terrainSettings.infiniteTerrain = true;
            terrainSettings.isosurfaceAlgorithm = (IsosurfaceAlgorithm)algorithmDropdown.value;
            terrainSettings.terrainDensityType = (TerrainDensityType)terrainTypeDropdown.value;
            terrainSettings.viewDistance = int.Parse(viewDistanceInput.text);

            if (terrainSettings.viewDistance < terrainSettings.chunkSize)
            {
                terrainSettings.viewDistance = terrainSettings.chunkSize;
            }
        }

        private void OnPlayerSpawn()
        {
            isTerrainGenerated = true;
            showMenu = false;
            voxelEngine.OnTerrainReady -= OnPlayerSpawn;

            terrainOptions.SetActive(false);
            generateMessage.SetActive(false);
            controlsText.SetActive(false);
            SpawnPlayer();
        }

        private void SpawnPlayer()
        {
            Vector3 spawnPos = voxelEngine.transform.position;

            // Spawn player in center for static terrain
            if (!voxelEngine.terrainSettings.infiniteTerrain)
            {
                Vector2 worldSize = voxelEngine.terrainSettings.worldSize;
                spawnPos = new Vector3(worldSize.x / 2, voxelEngine.terrainSettings.maxHeight, worldSize.y / 2);
            }

            const int WaterMask = 4;
            const int ChunkMask = 8;
            const int LayerMask = 1 << WaterMask | 1 << ChunkMask; // ignore all collider but the chunk and water collider
            Vector3 rayOrigin = spawnPos;
            rayOrigin.y = voxelEngine.terrainSettings.maxHeight + 1;
            RaycastHit hit;
            bool hitGround = Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, LayerMask);

            if (hitGround)
            {
                float height = rayOrigin.y - hit.distance;
                height += 2;
                spawnPos.y = height;

                voxelEngine.player.transform.position = spawnPos;
            }
            else
            {
                throw new Exception("Terrain below player not ready");
            }
        }
    }
}
