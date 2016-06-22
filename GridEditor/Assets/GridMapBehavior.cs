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
        public int yAmmount = 100;
        public GridMaterialBehavior[,] floors;
        public GridMaterialBehavior[,] verticalWalls;
        public GridMaterialBehavior[,] horizontalWalls;
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
            int yChunkCount = (int)Mathf.Ceil((float)this.yAmmount / (float)this.chunkSize);

            for (int x = 0; x < xChunkCount; x++)
            {
                int minX = x * this.chunkSize;
                int maxX = Mathf.Min(this.xAmmount - 1, minX + this.chunkSize - 1);

                for (int y = 0; y < yChunkCount; y++)
                {
                    if (currentChunks.Count > 0)
                    {
                        GridChunkBehaviour chunk = currentChunks[currentChunks.Count - 1];
                        currentChunks.RemoveAt(currentChunks.Count - 1);
                        chunk.minX = minX;
                        chunk.maxX = maxX;
                        chunk.minY = y * this.chunkSize;
                        chunk.maxY = Mathf.Min(this.yAmmount - 1, chunk.minY + this.chunkSize - 1);
                        chunk.transform.localPosition = new Vector3(minX * this.cellSize, 0, chunk.minY * this.cellSize);
                        chunk.ReloadMesh();
                    }
                    else
                    {
                        GridChunkBehaviour chunk = GameObject.Instantiate(this.chunkPrefab);
                        chunk.transform.SetParent(build.transform);
                        chunk.minX = minX;
                        chunk.maxX = maxX;
                        chunk.minY = y * this.chunkSize;
                        chunk.maxY = Mathf.Min(this.yAmmount - 1, chunk.minY + this.chunkSize - 1);
                        chunk.transform.localPosition = new Vector3(minX * this.cellSize, 0, chunk.minY * this.cellSize);
                    }
                }
            }

            foreach (GridChunkBehaviour chunk in currentChunks)
                GameObject.DestroyImmediate(chunk.gameObject);
        }

        private void LoadGridArrays()
        {
            this.floors = new GridMaterialBehavior[this.xAmmount, this.yAmmount];
            this.horizontalWalls = new GridMaterialBehavior[this.xAmmount, this.yAmmount + 1];
            this.verticalWalls = new GridMaterialBehavior[this.xAmmount + 1, this.yAmmount];

            int childCount = this.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform t = this.transform.GetChild(i);

                if (!t.gameObject.activeInHierarchy)
                    continue;

                GridFloorBehaviour floor = t.GetComponent<GridFloorBehaviour>();
                if (floor != null)
                {
                    int xEnd = floor.xStart + floor.xCount - 1;
                    int yEnd = floor.yStart + floor.yCount - 1;

                    for (int x = floor.xStart; x <= xEnd; x++)
                    {
                        if (x < 0)
                            continue;

                        if (x >= this.xAmmount)
                            break;

                        for (int y = floor.yStart; y <= yEnd; y++)
                        {
                            if (y < 0)
                                continue;

                            if (y >= this.yAmmount)
                                break;

                            this.floors[x, y] = floor.floorMaterial;
                        }
                    }
                }

                GridRoomBehaviour room = t.GetComponent<GridRoomBehaviour>();
                if (room != null)
                {
                    int xEnd = room.xStart + room.xCount - 1;
                    int yEnd = room.yStart + room.yCount - 1;

                    for (int x = room.xStart; x <= xEnd; x++)
                    {
                        if (x < 0)
                            continue;

                        if (x >= this.xAmmount)
                            break;

                        if ((room.yStart >= 0) &&
                            (room.yStart < this.yAmmount))
                            this.horizontalWalls[x, room.yStart] = room.floorMaterial;

                        if ((yEnd + 1 >= 0) &&
                            (yEnd < this.yAmmount))
                            this.horizontalWalls[x, yEnd + 1] = room.floorMaterial;
                    }

                    for (int y = room.yStart; y <= yEnd; y++)
                    {
                        if (y < 0)
                            continue;

                        if (y >= this.yAmmount)
                            break;

                        if ((room.xStart >= 0) &&
                            (room.xStart < this.yAmmount))
                            this.verticalWalls[room.xStart, y] = room.floorMaterial;

                        if ((xEnd + 1 >= 0) &&
                            (xEnd < this.yAmmount))
                            this.verticalWalls[xEnd + 1, y] = room.floorMaterial;
                    }
                }
            }
        }
  
        public GridMaterialBehavior Floor(int x, int y) { return this.floors[x, y]; }

        public GridMaterialBehavior LeftWall(int x, int y)
        {
            if (this.floors[x, y] == null)
            {
                if (x == 0)
                    return this.roofMaterial;
                else
                    return null;
            }
            else
            {
                GridMaterialBehavior ret = this.verticalWalls[x, y];
                if ((ret == null) &&
                    (x > 0) &&
                    (this.floors[x - 1, y] == null))
                    ret = this.floors[x, y];

                return ret;
            }
        }

        public GridMaterialBehavior RightWall(int x, int y)
        {
            if (this.floors[x, y] == null)
            {
                if (x == this.xAmmount - 1)
                    return this.roofMaterial;
                else
                    return null;
            }
            else
            {
                GridMaterialBehavior ret = this.verticalWalls[x + 1, y];
                if ((ret == null) &&
                    (x + 1 < this.xAmmount) &&
                    (this.floors[x + 1, y] == null))
                    ret = this.floors[x, y];

                return ret;
            }
        }

        public GridMaterialBehavior BottomWall(int x, int y)
        {
            if (this.floors[x, y] == null)
            {
                if (y == 0)
                    return this.roofMaterial;
                else
                    return null;
            }
            else
            {
                GridMaterialBehavior ret = this.horizontalWalls[x, y];
                if ((ret == null) &&
                    (y > 0) &&
                    (this.floors[x, y - 1] == null))
                    ret = this.floors[x, y];

                return ret;
            }
        }

        public GridMaterialBehavior UpperWall(int x, int y)
        {
            if (this.floors[x, y] == null)
            {
                if (y == this.yAmmount - 1)
                    return this.roofMaterial;
                else
                    return null;
            }
            else
            {
                GridMaterialBehavior ret = this.horizontalWalls[x, y + 1];
                if ((ret == null) &&
                    (y + 1 < this.yAmmount) &&
                    (this.floors[x, y + 1] == null))
                    ret = this.floors[x, y];

                return ret;
            }
        }
    }
}