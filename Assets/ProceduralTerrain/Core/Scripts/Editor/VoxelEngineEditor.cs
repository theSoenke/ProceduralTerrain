using PCG.Noise;
using System;
using UnityEditor;
using UnityEngine;


namespace PCG.Voxel
{
    [CustomEditor(typeof(VoxelEngine))]
    public class
        VoxelEngineEditor : Editor
    {
        private VoxelEngine voxelEngine;
        private TerrainSettings terrainSettings;
        private MenuState menuState;
        private PlacementMenuState placementMenuState;
        private int selectedTreeIndex = -1;
        private int selectedObjectIndex = -1;
        private bool showAdvancedSettings;

        private SerializedProperty terrainSettingsProperty;
        private SerializedProperty treeSpawnSettingsProperty;
        private SerializedProperty objectSpawnSettingsProperty;
        private SerializedProperty maxChunksPerFrameProperty;
        private SerializedProperty visualizeChunksProperty;
        private SerializedProperty chunkPrefabProperty;
        private SerializedProperty waterPrefabProperty;
        private SerializedProperty addTreeProperty;
        private SerializedProperty addObjectProperty;


        private enum MenuState
        {
            General,
            Placements,
        }

        private enum PlacementMenuState
        {
            Objects,
            Trees
        }

        #region UnityFunctions
        private void OnEnable()
        {
            terrainSettingsProperty = serializedObject.FindProperty("terrainSettings");
            treeSpawnSettingsProperty = serializedObject.FindProperty("treeSpawnSettings");
            objectSpawnSettingsProperty = serializedObject.FindProperty("objectSpawnSettings");
            maxChunksPerFrameProperty = serializedObject.FindProperty("maxChunksPerFrame");
            visualizeChunksProperty = serializedObject.FindProperty("visualizeChunkBounds");
            chunkPrefabProperty = serializedObject.FindProperty("chunkPrefab");
            waterPrefabProperty = serializedObject.FindProperty("waterPrefab");
            addTreeProperty = serializedObject.FindProperty("addTree");
            addObjectProperty = serializedObject.FindProperty("addObject");
        }

        public override void OnInspectorGUI()
        {
            voxelEngine = target as VoxelEngine;
            terrainSettings = voxelEngine.terrainSettings;

            serializedObject.Update();
            ShowMainTabs();
            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(10);
            if (!terrainSettings.infiniteTerrain)
            {
                if (GUILayout.Button("Generate", GUILayout.Height(30)))
                {
                    voxelEngine.GenerateTerrain();
                }
            }
        }

        #endregion

        /// <summary>
        /// Draws main options tabs
        /// </summary>
        private void ShowMainTabs()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            var toolbarOptions = new GUIContent[2];
            toolbarOptions[0] = new GUIContent("General");
            toolbarOptions[1] = new GUIContent("Placements");

            menuState = (MenuState)GUILayout.Toolbar((int)menuState, toolbarOptions, GUILayout.Height(30));
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            switch (menuState)
            {
                case MenuState.General:
                    ShowGenerateView();
                    break;
                case MenuState.Placements:
                    ShowPlacementView();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ShowGenerateView()
        {
            var content = new GUIContent("Random Seed", "Use a random seed for terrain generation");
            SerializedProperty randomSeedProperty = terrainSettingsProperty.FindPropertyRelative("randomSeed");
            EditorGUILayout.PropertyField(randomSeedProperty, content);

            if (!terrainSettings.randomSeed)
            {
                NoiseSettings noiseSettings = terrainSettings.noiseSettings;
                noiseSettings.seed = EditorGUILayout.IntField("Seed", noiseSettings.seed);
            }

            GUILayout.Space(10);

            ShowTerrainOptions();
            ShowTerrainNoiseOptions();
            ShowTextureOptions();
            ShowAdvancedSettings();
        }

        private void ShowTerrainOptions()
        {
            EditorGUILayout.LabelField("Terrain", EditorStyles.boldLabel);

            var content = new GUIContent("Infinite Terrain", "Generates an infinite terrain");
            SerializedProperty infiniteTerrainProperty = terrainSettingsProperty.FindPropertyRelative("infiniteTerrain");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(infiniteTerrainProperty, content);

            if (EditorGUI.EndChangeCheck())
            {
                // check whether infinite terrain settings have been turned active to then cleanup old chunks
                voxelEngine.DestroyPools();
            }

            if (terrainSettings.infiniteTerrain)
            {
                content = new GUIContent("View Distance", "Distance around player in which terrain is generated");
                SerializedProperty viewDistanceProperty = terrainSettingsProperty.FindPropertyRelative("viewDistance");
                EditorGUILayout.PropertyField(viewDistanceProperty, content);

                if (terrainSettings.viewDistance > 128)
                {
                    EditorGUILayout.HelpBox("The terrain will take quite long to generate with this terrain size", MessageType.Warning);
                }
            }
            else
            {
                content = new GUIContent("World Size", "Length in x and z direction of a static terrain");
                SerializedProperty worldSizeProperty = terrainSettingsProperty.FindPropertyRelative("worldSize");
                EditorGUILayout.PropertyField(worldSizeProperty, content);
            }

            content = new GUIContent("Height", "Maximum height of terrain");
            SerializedProperty maxHeightProperty = terrainSettingsProperty.FindPropertyRelative("maxHeight");
            EditorGUILayout.PropertyField(maxHeightProperty, content);

            content = new GUIContent("Sea Level", "Water height");
            SerializedProperty seaLevelProperty = terrainSettingsProperty.FindPropertyRelative("seaLevel");
            EditorGUILayout.PropertyField(seaLevelProperty, content);

            if (terrainSettings.maxHeight < terrainSettings.chunkSize)
            {
                EditorGUILayout.HelpBox("Terrain height needs to be at least as high as a chunk", MessageType.Warning);
            }

            content = new GUIContent("Terrain Type", "Classical heightmap or terrain width overhangs");
            SerializedProperty terrainDensityProperty = terrainSettingsProperty.FindPropertyRelative("terrainDensityType");
            EditorGUILayout.PropertyField(terrainDensityProperty, content);

            content = new GUIContent("Algorithm", "Algorithm used to generate mesh");
            SerializedProperty isosurfaceProperty = terrainSettingsProperty.FindPropertyRelative("isosurfaceAlgorithm");
            EditorGUILayout.PropertyField(isosurfaceProperty, content);
        }

        private void ShowTerrainNoiseOptions()
        {
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Noise", EditorStyles.boldLabel);
            SerializedProperty noiseSettingsProperty = terrainSettingsProperty.FindPropertyRelative("noiseSettings");
            ShowNoiseOptions(noiseSettingsProperty);
        }

        private void ShowNoiseOptions(SerializedProperty noiseProperty)
        {
            var content = new GUIContent("Type", "Type of noise terrain");
            SerializedProperty noiseTypeProperty = noiseProperty.FindPropertyRelative("noiseType");
            EditorGUILayout.PropertyField(noiseTypeProperty, content);

            // TODO Better tooltip descriptions
            content = new GUIContent("Octaves", "Number of noise iterations. More iterations create more details but also slowdown performance");
            SerializedProperty octavesProperty = noiseProperty.FindPropertyRelative("octaves");
            EditorGUILayout.IntSlider(octavesProperty, 1, 100, content);

            content = new GUIContent("Frequency", "Zoom effect for the noise");
            SerializedProperty frequenzyProperty = noiseProperty.FindPropertyRelative("frequency");
            EditorGUILayout.PropertyField(frequenzyProperty, content);

            content = new GUIContent("Lacunarity");
            SerializedProperty lacunarityProperty = noiseProperty.FindPropertyRelative("lacunarity");
            EditorGUILayout.PropertyField(lacunarityProperty, content);

            content = new GUIContent("Persistence", "Creates rougher or smoother terrains");
            SerializedProperty persistenceProperty = noiseProperty.FindPropertyRelative("persistence");
            EditorGUILayout.PropertyField(persistenceProperty, content);

            content = new GUIContent("Amplitude", "Multiplier for the terrain height");
            SerializedProperty amplitudeProperty = noiseProperty.FindPropertyRelative("amplitude");
            EditorGUILayout.PropertyField(amplitudeProperty, content);
        }

        private void ShowTextureOptions()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Chunk", EditorStyles.boldLabel);

            var content = new GUIContent("Chunk Prefab");
            EditorGUILayout.PropertyField(chunkPrefabProperty, content);

            if (voxelEngine.chunkPrefab != null)
            {
                if (GUILayout.Button("Edit Chunk Shader"))
                {
                    Selection.activeObject = voxelEngine.chunkPrefab;
                }
            }
        }

        private void ShowPlacementView()
        {
            EditorGUILayout.BeginHorizontal();
            var toolbarOptions = new GUIContent[2];
            toolbarOptions[0] = new GUIContent("Objects");
            toolbarOptions[1] = new GUIContent("Trees");

            placementMenuState = (PlacementMenuState)GUILayout.Toolbar((int)placementMenuState, toolbarOptions, GUILayout.Height(25));
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            switch (placementMenuState)
            {
                case PlacementMenuState.Objects:
                    ShowObjectPlacementView();
                    break;
                case PlacementMenuState.Trees:
                    ShowTreesView();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ShowObjectPlacementView()
        {
            EditorGUILayout.LabelField("General Spawn Options", EditorStyles.boldLabel);
            ShowSpawnSettings(objectSpawnSettingsProperty, voxelEngine.objectSpawnSettings);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Spawn Noise", EditorStyles.boldLabel);
            SerializedProperty noiseProperty = objectSpawnSettingsProperty.FindPropertyRelative("noiseSettings");
            ShowNoiseOptions(noiseProperty);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Spawn Objects", EditorStyles.boldLabel);

            var content = new GUIContent("Add object", "Drag and drop gameobject to add it to the list");
            EditorGUILayout.PropertyField(addObjectProperty, content);

            DrawObjectList();
            ShowSelectedObject();
        }

        private void DrawObjectList()
        {
            GUILayout.Space(10);

            GUIStyle toogleStyle = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 25
            };

            ObjectSpawnSettings objectSpawnSettings = voxelEngine.objectSpawnSettings;
            EditorGUILayout.BeginVertical();
            {
                for (int i = 0; i < objectSpawnSettings.resources.Count; i++)
                {
                    ObjectResource spawnRessource = objectSpawnSettings.resources[i];

                    if (spawnRessource.prefab == null)
                    {
                        continue;
                    }

                    if (i == selectedObjectIndex)
                    {
                        GUILayout.Toggle(true, spawnRessource.prefab.name, toogleStyle);
                    }
                    else
                    {
                        if (GUILayout.Toggle(false, spawnRessource.prefab.name, toogleStyle))
                        {
                            selectedObjectIndex = i;
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            {
                if (selectedObjectIndex >= 0)
                {
                    if (GUILayout.Button("Remove"))
                    {
                        SerializedProperty objectResourcesProperty = objectSpawnSettingsProperty.FindPropertyRelative("resources");
                        objectResourcesProperty.DeleteArrayElementAtIndex(selectedObjectIndex);
                        selectedObjectIndex = -1;
                    }
                }

                if (objectSpawnSettings.resources.Count > 0)
                {
                    if (GUILayout.Button("Remove All"))
                    {
                        SerializedProperty objectResourcesProperty = objectSpawnSettingsProperty.FindPropertyRelative("resources");
                        objectResourcesProperty.ClearArray();
                        selectedObjectIndex = -1;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void ShowSelectedObject()
        {
            if (selectedObjectIndex < 0)
            {
                return;
            }

            GUILayout.Space(15);
            EditorGUILayout.LabelField("Spawn Rules", EditorStyles.boldLabel);

            SerializedProperty objectResourcesProperty = objectSpawnSettingsProperty.FindPropertyRelative("resources");
            SerializedProperty selectedObjectProperty = objectResourcesProperty.GetArrayElementAtIndex(selectedObjectIndex);
            SerializedProperty spawnHeightProperty = selectedObjectProperty.FindPropertyRelative("spawnHeightRange");
            SerializedProperty spawnSlopeProperty = selectedObjectProperty.FindPropertyRelative("spawnSlopeRange");

            var content = new GUIContent("Terrain Height", "Max relative height in which the object will be spawned");
            EditorGUILayout.PropertyField(spawnHeightProperty, content);
            content = new GUIContent("Terrain Slope", "Max relative slope in which in the object will be spawned");
            EditorGUILayout.PropertyField(spawnSlopeProperty, content);
        }

        private void ShowTreesView()
        {
            EditorGUILayout.LabelField("General Spawn Options", EditorStyles.boldLabel);
            ShowSpawnSettings(treeSpawnSettingsProperty, voxelEngine.treeSpawnSettings);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Spawn Noise", EditorStyles.boldLabel);
            SerializedProperty noiseProperty = treeSpawnSettingsProperty.FindPropertyRelative("noiseSettings");
            ShowNoiseOptions(noiseProperty);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Spawn Objects", EditorStyles.boldLabel);
            var content = new GUIContent("Add tree", "Drag and drop tree gameobject to add it to the list");
            EditorGUILayout.PropertyField(addTreeProperty, content);

            DrawTreeList();
            ShowSelectedTree();
        }

        private void DrawTreeList()
        {
            GUILayout.Space(10);

            GUIStyle toogleStyle = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 25
            };

            ObjectSpawnSettings treeSpawnSettings = voxelEngine.treeSpawnSettings;
            EditorGUILayout.BeginVertical();
            {
                for (int i = 0; i < treeSpawnSettings.resources.Count; i++)
                {
                    ObjectResource spawnResource = treeSpawnSettings.resources[i];

                    if (spawnResource.prefab == null)
                    {
                        continue;
                    }

                    if (i == selectedTreeIndex)
                    {
                        GUILayout.Toggle(true, spawnResource.prefab.name, toogleStyle);
                    }
                    else
                    {
                        if (GUILayout.Toggle(false, spawnResource.prefab.name, toogleStyle))
                        {
                            selectedTreeIndex = i;
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            {
                if (selectedTreeIndex >= 0)
                {
                    if (GUILayout.Button("Remove"))
                    {
                        SerializedProperty treeResourcesProperty = treeSpawnSettingsProperty.FindPropertyRelative("resources");
                        treeResourcesProperty.DeleteArrayElementAtIndex(selectedTreeIndex);
                        selectedTreeIndex = -1;
                    }
                }

                if (treeSpawnSettings.resources.Count > 0)
                {
                    if (GUILayout.Button("Remove All"))
                    {
                        SerializedProperty treeResourcesProperty = treeSpawnSettingsProperty.FindPropertyRelative("resources");
                        treeResourcesProperty.ClearArray();
                        selectedTreeIndex = -1;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void ShowSelectedTree()
        {
            if (selectedTreeIndex < 0)
            {
                return;
            }

            GUILayout.Space(15);
            EditorGUILayout.LabelField("Spawn Rules", EditorStyles.boldLabel);

            SerializedProperty treeResourcesProperty = treeSpawnSettingsProperty.FindPropertyRelative("resources");
            SerializedProperty selectedTreeProperty = treeResourcesProperty.GetArrayElementAtIndex(selectedTreeIndex);
            SerializedProperty spawnHeightProperty = selectedTreeProperty.FindPropertyRelative("spawnHeightRange");
            SerializedProperty spawnSlopeProperty = selectedTreeProperty.FindPropertyRelative("spawnSlopeRange");

            var content = new GUIContent("Terrain Height", "Max relative height in which the tree will be used");
            EditorGUILayout.PropertyField(spawnHeightProperty, content);
            content = new GUIContent("Terrain Slope", "Max relative slope in which in the tree will be used");
            EditorGUILayout.PropertyField(spawnSlopeProperty, content);
        }

        private void ShowSpawnSettings(SerializedProperty spawnProperty, ObjectSpawnSettings spawnSettings)
        {
            var content = new GUIContent("Min Distance", "Minimum distance between spawned object");
            SerializedProperty minDistanceProperty = spawnProperty.FindPropertyRelative("minDistance");
            EditorGUILayout.PropertyField(minDistanceProperty, content);

            content = new GUIContent("Density", "Spawn probability for an object to spawn at a given location");
            spawnSettings.spawnProbability = EditorGUILayout.Slider(content, spawnSettings.spawnProbability, 0, 1);
        }

        private void ShowAdvancedSettings()
        {
            showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Options");

            if (!showAdvancedSettings)
            {
                return;
            }

            EditorGUI.indentLevel++;

            var content = new GUIContent("Player Prefab");
            voxelEngine.playerPrefab = (GameObject)EditorGUILayout.ObjectField(content, voxelEngine.playerPrefab, typeof(GameObject), true);

            content = new GUIContent("Water Prefab");
            EditorGUILayout.PropertyField(waterPrefabProperty, content);

            content = new GUIContent("Chunk Size", "Diameter of a chunk");
            SerializedProperty chunkSizeProperty = terrainSettingsProperty.FindPropertyRelative("chunkSize");
            EditorGUILayout.IntSlider(chunkSizeProperty, 1, 32, content);

            content = new GUIContent("Chunks per Frame", "Maximum number of chunks generated per frame. Decrease this for a better framerate or increase it for faster chunk generation");
            EditorGUILayout.IntSlider(maxChunksPerFrameProperty, 1, 20, content);

            content = new GUIContent("Visualize Chunks", "Draws bounds of chunks");
            EditorGUILayout.PropertyField(visualizeChunksProperty, content);

            EditorGUI.indentLevel--;
        }
    }
}