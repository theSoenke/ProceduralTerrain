## Create a simple static terrain
1. Add the script VoxelEngine to a GameObject
2. Set the chunk prefab on the VoxelEngine component. Examples can be found under Demo/Prefabs/Chunks
3. Click on "Generate" to create a terrain with a fixed size

## Performance optimizations
In case of performance problems you can try the following things:
- Reduce the view distance
- Lower the number of noise octaves
- Limit the max number of chunks generated per frame
- Use a lower terrain height. This will generate less chunks

## External assets and libraries
- Libnoise Unity: https://github.com/ricardojmendez/LibNoise.Unity
- NormalSolver: http://schemingdeveloper.com/2014/10/17/better-method-recalculate-normals-unity/