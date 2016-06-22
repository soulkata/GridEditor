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

                GridMaterialBehavior mat = t.GetComponent<GridMaterialBehavior>();
                if (mat != null)
                {
                    if (mat.floorTileXSize <= 0)
                        return;

                    if (mat.floorTileZSize <= 0)
                        return;
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

        public void CellData(int x, int z, out GridMaterialBehavior floorMaterial, out GridMaterialBehavior leftWall, out GridMaterialBehavior rightWall, out GridMaterialBehavior backWall, out GridMaterialBehavior forwardWall, out GridMaterialBehavior leftIniWall, out GridMaterialBehavior leftEndWall, out GridMaterialBehavior rightIniWall, out GridMaterialBehavior rightEndWall, out GridMaterialBehavior backIniWall, out GridMaterialBehavior backEndWall, out GridMaterialBehavior forwardIniWall, out GridMaterialBehavior forwardEndWall)
        {
            if (this.floors == null)
                this.LoadGridArrays();

            floorMaterial = this.floors[x, z];
            leftIniWall = null;
            leftEndWall = null;
            rightIniWall = null;
            rightEndWall = null;
            backIniWall = null;
            backEndWall = null;
            forwardIniWall = null;
            forwardEndWall = null;

            if (floorMaterial == null)
            {
                if (x == 0)
                    leftWall = this.roofMaterial;
                else
                    leftWall = null;

                if (x == this.xAmmount - 1)
                    rightWall = this.roofMaterial;
                else
                    rightWall = null;

                if (z == 0)
                    backWall = this.roofMaterial;
                else
                    backWall = null;

                if (z == this.zAmmount - 1)
                    forwardWall = this.roofMaterial;
                else
                    forwardWall = null;                
            }
            else
            {
                leftWall = this.zWalls[x, z];
                if ((leftWall == null) &&
                    (x > 0) &&
                    (this.floors[x - 1, z] == null))
                    leftWall = this.floors[x, z];

                if (leftWall == null)
                {
                    if ((x > 0) &&
                        (x < this.xAmmount - 1))
                    {
                        if (z > 0)
                            leftIniWall = this.LeftWall(x, z - 1);

                        if (z < this.zAmmount - 1)
                            leftEndWall = this.LeftWall(x, z + 1);
                    }
                }

                rightWall = this.zWalls[x + 1, z];
                if ((rightWall == null) &&
                    (x + 1 < this.xAmmount) && 
                    (this.floors[x + 1, z] == null))
                    rightWall = this.floors[x, z];

                if (rightWall == null)
                {
                    if ((x > 0) &&
                        (x < this.xAmmount - 1))
                    {
                        if (z > 0)
                            rightIniWall = this.RightWall(x, z - 1);

                        if (z < this.zAmmount - 1)
                            rightEndWall = this.RightWall(x, z + 1);
                    }
                }

                backWall = this.xWalls[x, z];
                if ((backWall == null) &&
                    (z > 0) &&
                    (this.floors[x, z - 1] == null))
                    backWall = this.floors[x, z];

                if (backWall == null)
                {
                    if ((z > 0) &&
                        (z < this.zAmmount - 1))
                    {
                        if (x > 0)
                            backIniWall = this.BackWall(x - 1, z);

                        if (x < this.xAmmount - 1)
                            backEndWall = this.BackWall(x + 1, z);
                    }
                }

                forwardWall = this.xWalls[x, z + 1];
                if ((forwardWall == null) &&
                    (z + 1 < this.zAmmount) &&
                    (this.floors[x, z + 1] == null))
                    forwardWall = this.floors[x, z];

                if (forwardWall == null)
                {
                    if ((z > 0) &&
                        (z < this.zAmmount - 1))
                    {
                        if (x > 0)
                            forwardIniWall = this.ForwardWall(x - 1, z);

                        if (x < this.xAmmount - 1)
                            forwardEndWall = this.ForwardWall(x + 1, z);
                    }
                }                
            }
        }
    }
}