using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace PCG.Voxel.Generators
{
    public class PerfTestMenu : MonoBehaviour
    {
        public VoxelEngine voxelEngine;
        public Button generateButton;
        public GameObject generateMessage;
        public InputField viewDistanceInput;
        public Toggle useOvrCamera;
        public Toggle useGrass;
        public Dropdown algorithmDropdown;
        public GameObject testCamera;
        public GameObject testOvrCamera;
        public Chunk grassChunk;
        public Chunk simpleChunk;

        [Header("Performance Test")]
        public int testLengthInSec = 60;

        private float deltaTime;
        private Stopwatch watch;


        #region UnityFunctions
        public void Start()
        {
            generateButton.onClick.AddListener(OnGenerateClick);
            generateMessage.SetActive(false);
        }

        private void Update()
        {
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        }
        #endregion

        private void OnGenerateClick()
        {
            HideMenu();
            SetTerrainSettings();

            watch = new Stopwatch();
            watch.Start();
            voxelEngine.GenerateTerrain();
            voxelEngine.OnTerrainReady += OnPlayerSpawn;
        }

        private void SetTerrainSettings()
        {
            voxelEngine.terrainSettings.infiniteTerrain = true;
            voxelEngine.terrainSettings.viewDistance = int.Parse(viewDistanceInput.text);
            voxelEngine.terrainSettings.isosurfaceAlgorithm = (IsosurfaceAlgorithm)algorithmDropdown.value;

            voxelEngine.playerPrefab = useOvrCamera.isOn ? testOvrCamera : testCamera;
            voxelEngine.chunkPrefab = useGrass.isOn ? grassChunk : simpleChunk;
        }

        private void HideMenu()
        {
            generateButton.gameObject.SetActive(false);
            useOvrCamera.gameObject.SetActive(false);
            useGrass.gameObject.SetActive(false);
            viewDistanceInput.gameObject.SetActive(false);
            algorithmDropdown.gameObject.SetActive(false);
            generateMessage.SetActive(true);
        }

        private void OnPlayerSpawn()
        {
            long duration = watch.ElapsedMilliseconds;
            watch.Stop();
            watch.Reset();
            Debug.Log("Base terrain generation took: " + duration + "ms");

            voxelEngine.OnTerrainReady -= OnPlayerSpawn;
            generateMessage.SetActive(false);
            StartCoroutine(TrackFps(duration));
        }

        private IEnumerator TrackFps(long generationTime)
        {
            var fpsList = new float[testLengthInSec];
            string perfData = "Time to generate intitial terrain: " + generationTime + "ms\n";

            for (int i = 0; i < testLengthInSec; i++)
            {
                yield return new WaitForSeconds(1);
                float fps = 1.0f / deltaTime;
                fpsList[i] = fps;
                perfData += fps + "\n";
            }

            string testSettings = useGrass.isOn ? "grass-" : "";
            testSettings += voxelEngine.terrainSettings.viewDistance;
            testSettings += useOvrCamera.isOn ? "-VR-" : "";
            testSettings += algorithmDropdown.value == 0 ? "-mc-" : "-dc-";
            string path = Application.dataPath + "/fps-" + testSettings + DateTime.Now.ToString("h-mm-ss") + ".txt";
            System.IO.File.WriteAllText(path, perfData);
            Debug.Log(perfData);
        }
    }
}
