using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Assets
{
    [ExecuteInEditMode]
    public class GridMapBehavior : MonoBehaviour
    {
        public float cellSize = 1f;
        public float wallSize = -0.1f;
        public float wallHeight = 1f;
        public int chunkSize = 16;

        public bool reloadMeshes;

        public int xAmmount = 100;
        public int zAmmount = 100;
        public GridMaterialBehavior[,] floors;
        public GridMaterialBehavior[,] zWalls;
        public GridMaterialBehavior[,] xWalls;
        public GridChunkBehaviour chunkPrefab;
        public GridMaterialBehavior roofMaterial;

        // Use this for initialization
        void Start()
        {
            this.ReloadMeshes();
        }

        void OnValidate()
        {            
            this.reloadMeshes = true;
        }

        //// Update is called once per frame
        void Update()
        {
            if (this.reloadMeshes)
            {
                this.reloadMeshes = false;
                this.floors = null;
                this.ReloadMeshes();
            }
        }
        
        public void ReloadMeshes()
        {
            if (this.cellSize <= 0)
                return;

            GridBuildBehaviour[] builds = this.GetComponentsInChildren<GridBuildBehaviour>();
            GridBuildBehaviour build;
            switch (builds.Length)
            {
                case 0:
                    build = new GameObject("Build").AddComponent<GridBuildBehaviour>();
                    build.transform.SetParent(this.transform);
                    break;
                case 1:
                    build = builds[0];
                    break;
                default:
                    return;
            }

            if (this.floors == null)
                this.LoadGridArrays();

            List<GridChunkBehaviour> currentChunks = new List<GridChunkBehaviour>();
            build.GetComponentsInChildren<GridChunkBehaviour>(true, currentChunks);

            if (this.chunkPrefab == null)
            {
                foreach (GridChunkBehaviour chunk in currentChunks)
                    GameObject.DestroyImmediate(chunk.gameObject);
                return;
            }

            int xChunkCount = (int)Mathf.Ceil((float)this.xAmmount / (float)this.chunkSize);
            int zChunkCount = (int)Mathf.Ceil((float)this.zAmmount / (float)this.chunkSize);

            for (int x = 0; x < xChunkCount; x++)
            {
                int minX = x * this.chunkSize;
                int maxX = Mathf.Min(this.xAmmount - 1, minX + this.chunkSize - 1);

                for (int z = 0; z < zChunkCount; z++)
                {
                    if (currentChunks.Count > 0)
                    {
                        GridChunkBehaviour chunk = currentChunks[currentChunks.Count - 1];
                        currentChunks.RemoveAt(currentChunks.Count - 1);
                        chunk.minX = minX;
                        chunk.maxX = maxX;
                        chunk.minZ = z * this.chunkSize;
                        chunk.maxZ = Mathf.Min(this.zAmmount - 1, chunk.minZ + this.chunkSize - 1);
                        chunk.transform.localPosition = new Vector3(minX * this.cellSize, 0, chunk.minZ * this.cellSize);
                        chunk.ReloadMesh();
                    }
                    else
                    {
                        GridChunkBehaviour chunk = GameObject.Instantiate(this.chunkPrefab);
                        chunk.transform.SetParent(build.transform);
                        chunk.minX = minX;
                        chunk.maxX = maxX;
                        chunk.minZ = z * this.chunkSize;
                        chunk.maxZ = Mathf.Min(this.zAmmount - 1, chunk.minZ + this.chunkSize - 1);
                        chunk.transform.localPosition = new Vector3(minX * this.cellSize, 0, chunk.minZ * this.cellSize);
                    }
                }
            }

            foreach (GridChunkBehaviour chunk in currentChunks)
                GameObject.DestroyImmediate(chunk.gameObject);
        }

        private void LoadGridArrays()
        {
            this.floors = new GridMaterialBehavior[this.xAmmount, this.zAmmount];
            this.xWalls = new GridMaterialBehavior[this.xAmmount, this.zAmmount + 1];
            this.zWalls = new GridMaterialBehavior[this.xAmmount + 1, this.zAmmount];

            int childCount = this.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform t = this.transform.GetChild(i);

                GridFloorBehaviour floor = t.GetComponent<GridFloorBehaviour>();
                if (floor != null)
                {
                    int xEnd = floor.xStart + floor.xCount - 1;
                    int zEnd = floor.zStart + floor.zCount - 1;

                    for (int x = floor.xStart; x <= xEnd; x++)
                    {
                        if (x < 0)
                            continue;

                        if (x >= this.xAmmount)
                            break;

                        for (int z = floor.zStart; z <= zEnd; z++)
                        {
                            if (z < 0)
                                continue;

                            if (z >= this.zAmmount)
                                break;

                            this.floors[x, z] = floor.floorMaterial;
                        }
                    }
                }

                GridRoomBehaviour room = t.GetComponent<GridRoomBehaviour>();
                if (room != null)
                {
                    int xEnd = room.xStart + room.xCount - 1;
                    int zEnd = room.zStart + room.zCount - 1;

                    for (int x = room.xStart; x <= xEnd; x++)
                    {
                        if (x < 0)
                            continue;

                        if (x >= this.xAmmount)
                            break;

                        if ((room.zStart >= 0) &&
                            (room.zStart < this.zAmmount))
                            this.xWalls[x, room.zStart] = room.floorMaterial;

                        if ((zEnd + 1 >= 0) &&
                            (zEnd + 1 <= this.zAmmount))
                            this.xWalls[x, zEnd + 1] = room.floorMaterial;
                    }

                    for (int z = room.zStart; z <= zEnd; z++)
                    {
                        if (z < 0)
                            continue;

                        if (z >= this.zAmmount)
                            break;

                        if ((room.xStart >= 0) &&
                            (room.xStart < this.zAmmount))
                            this.zWalls[room.xStart, z] = room.floorMaterial;

                        if ((xEnd + 1 >= 0) &&
                            (xEnd + 1 <= this.zAmmount))
                            this.zWalls[xEnd + 1, z] = room.floorMaterial;
                    }
                }
            }
        }
  
        public GridMaterialBehavior Floor(int x, int z) { return this.floors[x, z]; }

        public GridMaterialBehavior LeftWall(int x, int z)
        {
            if (this.floors[x, z] == null)
            {
                if (x == 0)
                    return this.roofMaterial;
                else
                    return null;
            }
            else
            {
                GridMaterialBehavior ret = this.zWalls[x, z];
                if ((ret == null) &&
                    (x > 0) &&
                    (this.floors[x - 1, z] == null))
                    ret = this.floors[x, z];

                return ret;
            }
        }

        public GridMaterialBehavior RightWall(int x, int z)
        {
            if (this.floors[x, z] == null)
            {
                if (x == this.xAmmount - 1)
                    return this.roofMaterial;
                else
                    return null;
            }
            else
            {
                GridMaterialBehavior ret = this.zWalls[x + 1, z];
                if ((ret == null) &&
                    (x + 1 < this.xAmmount) &&
                    (this.floors[x + 1, z] == null))
                    ret = this.floors[x, z];

                return ret;
            }
        }

        public GridMaterialBehavior BackWall(int x, int z)
        {
            if (this.floors[x, z] == null)
            {
                if (z == 0)
                    return this.roofMaterial;
                else
                    return null;
            }
            else
            {
                GridMaterialBehavior ret = this.xWalls[x, z];
                if ((ret == null) &&
                    (z > 0) &&
                    (this.floors[x, z - 1] == null))
                    ret = this.floors[x, z];

                return ret;
            }
        }

        public GridMaterialBehavior ForwardWall(int x, int z)
        {
            if (this.floors[x, z] == null)
            {
                if (z == this.zAmmount - 1)
                    return this.roofMaterial;
                else
                    return null;
            }
            else
            {
                GridMaterialBehavior ret = this.xWalls[x, z + 1];
                if ((ret == null) &&
                    (z + 1 < this.zAmmount) &&
                    (this.floors[x, z + 1] == null))
                    ret = this.floors[x, z];

                return ret;
            }
        }
    }
}