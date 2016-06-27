using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Assets.Grids
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
        public GridChunkWallBehaviour chunkWallPrefab;
        public GridChunkRoofBehaviour chunkRoofPrefab;
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

            this.LoadGridArrays();

            List<GridChunkWallBehaviour> currentChunkWalls = new List<GridChunkWallBehaviour>();
            build.GetComponentsInChildren<GridChunkWallBehaviour>(true, currentChunkWalls);

            List<GridChunkRoofBehaviour> currentChunkRoofs = new List<GridChunkRoofBehaviour>();
            build.GetComponentsInChildren<GridChunkRoofBehaviour>(true, currentChunkRoofs);

            if ((this.chunkWallPrefab == null) ||
                (this.chunkRoofPrefab == null))
            {
                foreach (GridChunkWallBehaviour chunk in currentChunkWalls)
                    GameObject.DestroyImmediate(chunk.gameObject);

                foreach (GridChunkRoofBehaviour chunk in currentChunkRoofs)
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
                    int minY = y * this.chunkSize;
                    int maxY = Mathf.Min(this.yAmmount - 1, minY + this.chunkSize - 1);

                    ReloadChunkHelper reload = new ReloadChunkHelper();
                    reload.ReloadFloor(this, minX, maxX, minY, maxY);

                    if (reload.vertices.Count > 0)
                    {
                        GridChunkWallBehaviour chunk;

                        if (currentChunkWalls.Count > 0)
                        {
                            chunk = currentChunkWalls[currentChunkWalls.Count - 1];
                            currentChunkWalls.RemoveAt(currentChunkWalls.Count - 1);
                        }
                        else
                        {
                            chunk = GameObject.Instantiate(this.chunkWallPrefab);
                            chunk.transform.SetParent(build.transform);
                        }

                        chunk.transform.localPosition = new Vector3(minX * this.cellSize, 0, minY * this.cellSize);

                        Mesh mesh = new Mesh();
                        mesh.name = "chunk";
                        mesh.SetVertices(reload.vertices);
                        mesh.SetUVs(0, reload.uvs);

                        List<List<int>> triangles = new List<List<int>>();
                        List<Material> materials = new List<Material>();
                        foreach (KeyValuePair<Material, List<int>> materialItemValue in reload.materialCache)
                        {
                            if (materialItemValue.Value.Count > 0)
                            {
                                triangles.Add(materialItemValue.Value);
                                materials.Add(materialItemValue.Key);
                            }
                        }

                        mesh.subMeshCount = materials.Count;

                        for (int i = 0; i < materials.Count; i++)
                            mesh.SetTriangles(triangles[i], i);

                        mesh.RecalculateNormals();

                        chunk.SetMesh(mesh, materials.ToArray());
                    }                    

                    if (reload.roofVertices.Count > 0)
                    {
                        GridChunkRoofBehaviour chunk;

                        if (currentChunkRoofs.Count > 0)
                        {
                            chunk = currentChunkRoofs[currentChunkRoofs.Count - 1];
                            currentChunkRoofs.RemoveAt(currentChunkRoofs.Count - 1);
                        }
                        else
                        {
                            chunk = GameObject.Instantiate(this.chunkRoofPrefab);
                            chunk.transform.SetParent(build.transform);
                        }

                        chunk.transform.localPosition = new Vector3(minX * this.cellSize, 0, minY * this.cellSize);

                        Mesh mesh = new Mesh();
                        mesh.name = "chunk";
                        mesh.SetVertices(reload.roofVertices);
                        mesh.SetUVs(0, reload.roofUvs);
                        mesh.triangles = reload.roofTriangles.ToArray();
                        mesh.RecalculateNormals();

                        chunk.SetMesh(mesh, this.roofMaterial.floorMaterial);
                    }
                }
            }

            foreach (GridChunkWallBehaviour chunk in currentChunkWalls)
                GameObject.DestroyImmediate(chunk.gameObject);

            foreach (GridChunkRoofBehaviour chunk in currentChunkRoofs)
                GameObject.DestroyImmediate(chunk.gameObject);
        }

        public void LoadGridArrays()
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
                    int xIni = Math.Max(floor.xStart, 0);
                    int yIni = Math.Max(floor.yStart, 0);
                    int xEnd = Math.Min(this.xAmmount, floor.xStart + floor.xCount) - 1;
                    int yEnd = Math.Min(this.yAmmount, floor.yStart + floor.yCount) - 1;

                    for (int x = xIni; x <= xEnd; x++)
                        for (int y = yIni; y <= yEnd; y++)
                            this.floors[x, y] = floor.floorMaterial;

                    yIni = Math.Max(floor.yStart + 1, 0);
                    yEnd = Math.Min(this.yAmmount, floor.yStart + floor.yCount - 1);

                    for (int x = xIni; x <= xEnd; x++)
                        for (int y = yIni; y <= yEnd; y++)
                            this.horizontalWalls[x, y] = null;

                    xIni = Math.Max(floor.xStart + 1, 0);
                    xEnd = Math.Min(this.xAmmount, floor.xStart + floor.xCount - 1);

                    yIni = Math.Max(floor.yStart, 0);
                    yEnd = Math.Min(this.yAmmount, floor.yStart + floor.yCount) - 1;

                    for (int x = xIni; x <= xEnd; x++)
                        for (int y = yIni; y <= yEnd; y++)
                            this.verticalWalls[x, y] = null;
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
                            (room.yStart <= this.yAmmount))
                            this.horizontalWalls[x, room.yStart] = room.floorMaterial;

                        if ((yEnd + 1 >= 0) &&
                            (yEnd + 1 <= this.yAmmount))
                            this.horizontalWalls[x, yEnd + 1] = room.floorMaterial;
                    }

                    for (int y = room.yStart; y <= yEnd; y++)
                    {
                        if (y < 0)
                            continue;

                        if (y >= this.yAmmount)
                            break;

                        if ((room.xStart >= 0) &&
                            (room.xStart <= this.yAmmount))
                            this.verticalWalls[room.xStart, y] = room.floorMaterial;

                        if ((xEnd + 1 >= 0) &&
                            (xEnd + 1 <= this.xAmmount))
                            this.verticalWalls[xEnd + 1, y] = room.floorMaterial;
                    }
                }
            }
        }
  
        public GridMaterialBehavior Floor(int x, int y) { return this.floors[x, y]; }

        public GridMaterialBehavior LeftWall(int x, int y)
        {
            if (this.floors[x, y] == null)
                return null;
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
                return null;
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
                return null;
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
                return null;
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

        class ReloadChunkHelper
        {
            public Dictionary<Material, List<int>> materialCache = new Dictionary<Material, List<int>>();
            public List<Vector3> vertices = new List<Vector3>();
            public List<Vector3> normals = new List<Vector3>();
            public List<Vector2> uvs = new List<Vector2>();
            private Vector3 offset;
            private GridMapBehavior gridMap;

            public List<int> roofTriangles = new List<int>();
            public List<Vector3> roofVertices = new List<Vector3>();
            public List<Vector3> roofNormals = new List<Vector3>();
            public List<Vector2> roofUvs = new List<Vector2>();

            public void ReloadFloor(GridMapBehavior gridMap, int minX, int maxX, int minY, int maxY)
            {
                this.gridMap = gridMap;

                this.offset = new Vector3(minX * gridMap.cellSize, 0f, minY * gridMap.cellSize);

                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        GridMaterialBehavior floor = gridMap.Floor(x, y);

                        float xCellIni = x * gridMap.cellSize;
                        float xCellEnd = (x + 1) * gridMap.cellSize;
                        float yCellIni = y * gridMap.cellSize;
                        float yCellEnd = (y + 1) * gridMap.cellSize;

                        if (floor == null)
                        {
                            this.WriteRoof(xCellIni,
                                xCellEnd,
                                yCellIni,
                                yCellEnd);

                            if (x == 0)
                                this.WriteRightWall(gridMap.roofMaterial, xCellIni, yCellIni, yCellEnd);

                            if (x == gridMap.xAmmount - 1)
                                this.WriteLeftWall(gridMap.roofMaterial, xCellEnd, yCellIni, yCellEnd);

                            if (y == 0)
                                this.WriteUpperWall(gridMap.roofMaterial, xCellIni, xCellEnd, yCellIni);

                            if (y == gridMap.yAmmount - 1)
                                this.WriteBottomWall(gridMap.roofMaterial, xCellIni, xCellEnd, yCellEnd);
                        }
                        else
                        {
                            float xFloorIni;
                            float xFloorEnd;
                            float yFloorIni;
                            float yFloorEnd;
                            float xWallIni = xCellIni + gridMap.wallSize;
                            float xWallEnd = xCellEnd - gridMap.wallSize;
                            float yWallIni = yCellIni + gridMap.wallSize;
                            float yWallEnd = yCellEnd - gridMap.wallSize;
                            GridMaterialBehavior leftWall = gridMap.LeftWall(x, y);
                            GridMaterialBehavior rightWall = gridMap.RightWall(x, y);
                            GridMaterialBehavior bottomWall = gridMap.BottomWall(x, y);
                            GridMaterialBehavior upperWall = gridMap.UpperWall(x, y);
                            GridMaterialBehavior leftIniWall;
                            GridMaterialBehavior leftEndWall;
                            GridMaterialBehavior rightIniWall;
                            GridMaterialBehavior rightEndWall;
                            GridMaterialBehavior bottomIniWall;
                            GridMaterialBehavior bottomEndWall;
                            GridMaterialBehavior upperIniWall;
                            GridMaterialBehavior upperEndWall;

                            if (leftWall == null)
                            {
                                xFloorIni = xCellIni;

                                if (y == 0)
                                    leftIniWall = null;
                                else
                                    leftIniWall = gridMap.LeftWall(x, y - 1);

                                if (y == gridMap.yAmmount - 1)
                                    leftEndWall = null;
                                else
                                    leftEndWall = gridMap.LeftWall(x, y + 1);
                            }
                            else
                            {
                                xFloorIni = xWallIni;
                                leftIniWall = null;
                                leftEndWall = null;

                                this.WriteRoof(xCellIni, xFloorIni, yCellIni, yCellEnd);

                                this.WriteLeftWall(leftWall, xWallIni, yCellIni, yCellEnd);
                                if (x == 0)
                                    this.WriteRightWall(gridMap.roofMaterial, xCellIni, yCellIni, yCellEnd);
                                if ((y == 0) &&
                                    (bottomWall == null))
                                    this.WriteUpperWall(gridMap.roofMaterial, xCellIni, xWallIni, yCellIni);
                                if ((y == gridMap.yAmmount - 1) &&
                                    (upperWall == null))
                                    this.WriteBottomWall(gridMap.roofMaterial, xCellIni, xWallIni, yCellEnd);
                            }

                            if (rightWall == null)
                            {
                                xFloorEnd = xCellEnd;

                                if (y == 0)
                                    rightIniWall = null;
                                else
                                    rightIniWall = gridMap.RightWall(x, y - 1);

                                if (y == gridMap.yAmmount - 1)
                                    rightEndWall = null;
                                else
                                    rightEndWall = gridMap.RightWall(x, y + 1);
                            }
                            else
                            {
                                xFloorEnd = xWallEnd;
                                rightIniWall = null;
                                rightEndWall = null;

                                this.WriteRoof(xWallEnd, xCellEnd, yCellIni, yCellEnd);

                                this.WriteRightWall(rightWall, xWallEnd, yCellIni, yCellEnd);
                                if (x == gridMap.xAmmount - 1)
                                    this.WriteLeftWall(gridMap.roofMaterial, xCellEnd, yCellIni, yCellEnd);
                                if ((y == 0) &&
                                    (bottomWall == null))
                                    this.WriteUpperWall(gridMap.roofMaterial, xWallEnd, xCellEnd, yCellIni);
                                if ((y == gridMap.yAmmount - 1) &&
                                    (upperWall == null))
                                    this.WriteBottomWall(gridMap.roofMaterial, xWallEnd, xCellEnd, yCellEnd);
                            }

                            if (bottomWall == null)
                            {
                                yFloorIni = yCellIni;

                                if (x == 0)
                                    bottomIniWall = null;
                                else
                                    bottomIniWall = gridMap.BottomWall(x - 1, y);

                                if (x == gridMap.xAmmount - 1)
                                    bottomEndWall = null;
                                else
                                    bottomEndWall = gridMap.BottomWall(x + 1, y);
                            }
                            else
                            {
                                yFloorIni = yWallIni;
                                bottomIniWall = null;
                                bottomEndWall = null;

                                this.WriteRoof(xFloorIni, xFloorEnd, yCellIni, yWallIni);

                                this.WriteBottomWall(bottomWall, xFloorIni, xFloorEnd, yWallIni);
                                if (y == 0)
                                    this.WriteUpperWall(gridMap.roofMaterial, xCellIni, xCellEnd, yCellIni);
                                if ((x == 0) &&
                                    (leftWall == null))
                                    this.WriteRightWall(gridMap.roofMaterial, xCellIni, yCellIni, yWallIni);
                                if ((x == gridMap.xAmmount - 1) &&
                                    (rightWall == null))
                                    this.WriteLeftWall(gridMap.roofMaterial, xCellEnd, yCellIni, yWallIni);
                            }

                            if (upperWall == null)
                            {
                                yFloorEnd = yCellEnd;

                                if (x == 0)
                                    upperIniWall = null;
                                else
                                    upperIniWall = gridMap.UpperWall(x - 1, y);

                                if (x == gridMap.xAmmount - 1)
                                    upperEndWall = null;
                                else
                                    upperEndWall = gridMap.UpperWall(x + 1, y);
                            }
                            else
                            {
                                yFloorEnd = yWallEnd;
                                upperIniWall = null;
                                upperEndWall = null;

                                this.WriteRoof(xFloorIni, xFloorEnd, yWallEnd, yCellEnd);
                                this.WriteUpperWall(upperWall, xFloorIni, xFloorEnd, yWallEnd);
                                if (y == gridMap.yAmmount - 1)
                                    this.WriteBottomWall(gridMap.roofMaterial, xCellIni, xCellEnd, yCellEnd);
                                if ((x == 0) &&
                                    (leftWall == null))
                                    this.WriteRightWall(gridMap.roofMaterial, xCellIni, yWallEnd, yCellEnd);
                                if ((x == gridMap.xAmmount - 1) &&
                                    (rightWall == null))
                                    this.WriteLeftWall(gridMap.roofMaterial, xCellEnd, yWallEnd, yCellEnd);
                            }

                            this.WriteFloor(floor,
                                xFloorIni,
                                xFloorEnd,
                                yFloorIni,
                                yFloorEnd);

                            if ((rightEndWall != null) ||
                                (upperEndWall != null))
                            {
                                this.WriteRoof(xWallEnd, xCellEnd, yWallEnd, yCellEnd);

                                if (rightEndWall != null)
                                    this.WriteRightWall(rightEndWall, xWallEnd, yWallEnd, yCellEnd);
                                else
                                    this.WriteRightWall(upperEndWall, xWallEnd, yWallEnd, yCellEnd);
                                if (x == gridMap.xAmmount - 1)
                                    this.WriteLeftWall(gridMap.roofMaterial, xCellEnd, yWallEnd, yCellEnd);

                                if (upperEndWall != null)
                                    this.WriteUpperWall(upperEndWall, xWallEnd, xCellEnd, yWallEnd);
                                else
                                    this.WriteUpperWall(rightEndWall, xWallEnd, xCellEnd, yWallEnd);
                                if (y == gridMap.yAmmount - 1)
                                    this.WriteBottomWall(gridMap.roofMaterial, xWallEnd, xCellEnd, yCellEnd);
                            }

                            if ((leftEndWall != null) ||
                                (upperIniWall != null))
                            {
                                this.WriteRoof(xCellIni, xWallIni, yWallEnd, yCellEnd);

                                if (leftEndWall != null)
                                    this.WriteLeftWall(leftEndWall, xWallIni, yWallEnd, yCellEnd);
                                else
                                    this.WriteLeftWall(upperIniWall, xWallIni, yWallEnd, yCellEnd);
                                if (x == 0)
                                    this.WriteRightWall(gridMap.roofMaterial, xCellIni, yWallEnd, yCellEnd);

                                if (upperIniWall != null)
                                    this.WriteUpperWall(upperIniWall, xCellIni, xWallIni, yWallEnd);
                                else
                                    this.WriteUpperWall(leftEndWall, xCellIni, xWallIni, yWallEnd);
                                if (y == gridMap.yAmmount - 1)
                                    this.WriteBottomWall(gridMap.roofMaterial, xCellIni, xWallIni, yCellEnd);
                            }

                            if ((rightIniWall != null) ||
                                (bottomEndWall != null))
                            {
                                this.WriteRoof(xWallEnd, xCellEnd, yCellIni, yWallIni);

                                if (rightIniWall != null)
                                    this.WriteRightWall(rightIniWall, xWallEnd, yCellIni, yWallIni);
                                else
                                    this.WriteRightWall(bottomEndWall, xWallEnd, yCellIni, yWallIni);
                                if (x == gridMap.xAmmount - 1)
                                    this.WriteLeftWall(gridMap.roofMaterial, xCellEnd, yCellIni, yWallIni);

                                if (bottomEndWall != null)
                                    this.WriteBottomWall(bottomEndWall, xWallEnd, xCellEnd, yWallIni);
                                else
                                    this.WriteBottomWall(rightIniWall, xWallEnd, xCellEnd, yWallIni);
                                if (y == 0)
                                    this.WriteUpperWall(gridMap.roofMaterial, xWallEnd, xCellEnd, yCellIni);
                            }

                            if ((leftIniWall != null) ||
                                (bottomIniWall != null))
                            {
                                this.WriteRoof(xCellIni, xWallIni, yCellIni, yWallIni);

                                if (leftIniWall != null)
                                    this.WriteLeftWall(leftIniWall, xWallIni, yCellIni, yWallIni);
                                else
                                    this.WriteLeftWall(bottomIniWall, xWallIni, yCellIni, yWallIni);
                                if (x == 0)
                                    this.WriteRightWall(gridMap.roofMaterial, xCellIni, yCellIni, yWallIni);

                                if (bottomIniWall != null)
                                    this.WriteBottomWall(bottomIniWall, xCellIni, xWallIni, yWallIni);
                                else
                                    this.WriteBottomWall(leftIniWall, xCellIni, xWallIni, yWallIni);
                                if (y == 0)
                                    this.WriteUpperWall(gridMap.roofMaterial, xCellIni, xWallIni, yCellIni);
                            }
                        }
                    }
                }
            }

            void WriteRoof(float xVertIni, float xVertEnd, float yVertIni, float yVertEnd)
            {
                int index = this.vertices.Count;
                this.vertices.Add(new Vector3(xVertIni, this.gridMap.wallHeight, yVertIni) - offset);
                this.vertices.Add(new Vector3(xVertEnd, this.gridMap.wallHeight, yVertIni) - offset);
                this.vertices.Add(new Vector3(xVertIni, this.gridMap.wallHeight, yVertEnd) - offset);
                this.vertices.Add(new Vector3(xVertEnd, this.gridMap.wallHeight, yVertEnd) - offset);
                this.normals.Add(Vector3.up);
                this.normals.Add(Vector3.up);
                this.normals.Add(Vector3.up);
                this.normals.Add(Vector3.up);

                List<int> materialItem;
                if (!this.materialCache.TryGetValue(this.gridMap.roofMaterial.floorMaterial, out materialItem))
                {
                    materialItem = new List<int>();
                    this.materialCache.Add(this.gridMap.roofMaterial.floorMaterial, materialItem);
                }

                materialItem.Add(index + 0);
                materialItem.Add(index + 2);
                materialItem.Add(index + 1);
                materialItem.Add(index + 2);
                materialItem.Add(index + 3);
                materialItem.Add(index + 1);

                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(0f, 1f));
                uvs.Add(new Vector2(1f, 1f));

                index = this.roofVertices.Count;
                this.roofVertices.Add(new Vector3(xVertIni, this.gridMap.wallHeight + 0.05f, yVertIni) - offset);
                this.roofVertices.Add(new Vector3(xVertEnd, this.gridMap.wallHeight + 0.05f, yVertIni) - offset);
                this.roofVertices.Add(new Vector3(xVertIni, this.gridMap.wallHeight + 0.05f, yVertEnd) - offset);
                this.roofVertices.Add(new Vector3(xVertEnd, this.gridMap.wallHeight + 0.05f, yVertEnd) - offset);
                this.roofNormals.Add(Vector3.up);
                this.roofNormals.Add(Vector3.up);
                this.roofNormals.Add(Vector3.up);
                this.roofNormals.Add(Vector3.up);                

                this.roofTriangles.Add(index + 0);
                this.roofTriangles.Add(index + 2);
                this.roofTriangles.Add(index + 1);
                this.roofTriangles.Add(index + 2);
                this.roofTriangles.Add(index + 3);
                this.roofTriangles.Add(index + 1);

                this.roofUvs.Add(new Vector2(0f, 0f));
                this.roofUvs.Add(new Vector2(1f, 0f));
                this.roofUvs.Add(new Vector2(0f, 1f));
                this.roofUvs.Add(new Vector2(1f, 1f));
            }

            void WriteFloor(GridMaterialBehavior floor, float xVertIni, float xVertEnd, float yVertIni, float yVertEnd)
            {
                int index = this.vertices.Count;
                this.vertices.Add(new Vector3(xVertIni, 0f, yVertIni) - offset);
                this.vertices.Add(new Vector3(xVertEnd, 0f, yVertIni) - offset);
                this.vertices.Add(new Vector3(xVertIni, 0f, yVertEnd) - offset);
                this.vertices.Add(new Vector3(xVertEnd, 0f, yVertEnd) - offset);
                this.normals.Add(Vector3.up);
                this.normals.Add(Vector3.up);
                this.normals.Add(Vector3.up);
                this.normals.Add(Vector3.up);

                List<int> materialItem;
                if (!this.materialCache.TryGetValue(floor.floorMaterial, out materialItem))
                {
                    materialItem = new List<int>();
                    this.materialCache.Add(floor.floorMaterial, materialItem);
                }

                materialItem.Add(index + 0);
                materialItem.Add(index + 2);
                materialItem.Add(index + 1);
                materialItem.Add(index + 2);
                materialItem.Add(index + 3);
                materialItem.Add(index + 1);

                float xUVIni = Mathf.Repeat(xVertIni, floor.floorTileXSize) / floor.floorTileXSize;
                float yUVIni = Mathf.Repeat(yVertIni, floor.floorTileYSize) / floor.floorTileYSize;

                float xUVEnd = Mathf.Repeat(xVertEnd, floor.floorTileXSize);
                if (xUVEnd == 0)
                    xUVEnd = 1;
                else
                    xUVEnd /= floor.floorTileXSize;

                float yUVEnd = Mathf.Repeat(yVertEnd, floor.floorTileYSize);
                if (yUVEnd == 0)
                    yUVEnd = 1;
                else
                    yUVEnd /= floor.floorTileYSize;

                uvs.Add(new Vector2(xUVIni, yUVIni));
                uvs.Add(new Vector2(xUVEnd, yUVIni));
                uvs.Add(new Vector2(xUVIni, yUVEnd));
                uvs.Add(new Vector2(xUVEnd, yUVEnd));

                if (floor.roofTop)
                {
                    index = this.roofVertices.Count;
                    this.roofVertices.Add(new Vector3(xVertIni, this.gridMap.wallHeight + 0.05f, yVertIni) - offset);
                    this.roofVertices.Add(new Vector3(xVertEnd, this.gridMap.wallHeight + 0.05f, yVertIni) - offset);
                    this.roofVertices.Add(new Vector3(xVertIni, this.gridMap.wallHeight + 0.05f, yVertEnd) - offset);
                    this.roofVertices.Add(new Vector3(xVertEnd, this.gridMap.wallHeight + 0.05f, yVertEnd) - offset);
                    this.roofNormals.Add(Vector3.up);
                    this.roofNormals.Add(Vector3.up);
                    this.roofNormals.Add(Vector3.up);
                    this.roofNormals.Add(Vector3.up);

                    this.roofTriangles.Add(index + 0);
                    this.roofTriangles.Add(index + 2);
                    this.roofTriangles.Add(index + 1);
                    this.roofTriangles.Add(index + 2);
                    this.roofTriangles.Add(index + 3);
                    this.roofTriangles.Add(index + 1);

                    this.roofUvs.Add(new Vector2(0.0f, 0.0f));
                    this.roofUvs.Add(new Vector2(1.0f, 0.0f));
                    this.roofUvs.Add(new Vector2(0.0f, 1.0f));
                    this.roofUvs.Add(new Vector2(1.0f, 1.0f));
                }
            }

            void WriteLeftWall(GridMaterialBehavior wall, float xVert, float yVertIni, float yVertEnd)
            {
                int index = this.vertices.Count;
                this.vertices.Add(new Vector3(xVert, 0f, yVertIni) - offset);
                this.vertices.Add(new Vector3(xVert, this.gridMap.wallHeight, yVertIni) - offset);
                this.vertices.Add(new Vector3(xVert, 0f, yVertEnd) - offset);
                this.vertices.Add(new Vector3(xVert, this.gridMap.wallHeight, yVertEnd) - offset);
                this.normals.Add(Vector3.right);
                this.normals.Add(Vector3.right);
                this.normals.Add(Vector3.right);
                this.normals.Add(Vector3.right);

                List<int> materialItem;
                if (!this.materialCache.TryGetValue(wall.wallMaterial, out materialItem))
                {
                    materialItem = new List<int>();
                    this.materialCache.Add(wall.wallMaterial, materialItem);
                }

                materialItem.Add(index + 1);
                materialItem.Add(index + 2);
                materialItem.Add(index + 0);
                materialItem.Add(index + 2);
                materialItem.Add(index + 1);
                materialItem.Add(index + 3);

                float xUVIni = Mathf.Repeat(yVertIni, wall.wallSize) / wall.wallSize;

                float xUVEnd = Mathf.Repeat(yVertEnd, wall.wallSize);
                if (xUVEnd == 0)
                    xUVEnd = 1;
                else
                    xUVEnd /= wall.wallSize;

                uvs.Add(new Vector2(xUVIni, 0f));
                uvs.Add(new Vector2(xUVIni, 1f));
                uvs.Add(new Vector2(xUVEnd, 0f));
                uvs.Add(new Vector2(xUVEnd, 1f));
            }

            void WriteRightWall(GridMaterialBehavior wall, float xVert, float yVertIni, float yVertEnd)
            {
                int index = this.vertices.Count;
                this.vertices.Add(new Vector3(xVert, 0f, yVertIni) - offset);
                this.vertices.Add(new Vector3(xVert, this.gridMap.wallHeight, yVertIni) - offset);
                this.vertices.Add(new Vector3(xVert, 0f, yVertEnd) - offset);
                this.vertices.Add(new Vector3(xVert, this.gridMap.wallHeight, yVertEnd) - offset);
                this.normals.Add(Vector3.left);
                this.normals.Add(Vector3.left);
                this.normals.Add(Vector3.left);
                this.normals.Add(Vector3.left);

                List<int> materialItem;
                if (!this.materialCache.TryGetValue(wall.wallMaterial, out materialItem))
                {
                    materialItem = new List<int>();
                    this.materialCache.Add(wall.wallMaterial, materialItem);
                }

                materialItem.Add(index + 1);
                materialItem.Add(index + 0);
                materialItem.Add(index + 2);
                materialItem.Add(index + 2);
                materialItem.Add(index + 3);
                materialItem.Add(index + 1);

                float xUVIni = Mathf.Repeat(yVertIni, wall.wallSize) / wall.wallSize;

                float xUVEnd = Mathf.Repeat(yVertEnd, wall.wallSize);
                if (xUVEnd == 0)
                    xUVEnd = 1;
                else
                    xUVEnd /= wall.wallSize;

                uvs.Add(new Vector2(xUVIni, 0f));
                uvs.Add(new Vector2(xUVIni, 1f));
                uvs.Add(new Vector2(xUVEnd, 0f));
                uvs.Add(new Vector2(xUVEnd, 1f));
            }

            void WriteBottomWall(GridMaterialBehavior wall, float xVertIni, float xVertEnd, float yVert)
            {
                int index = this.vertices.Count;

                this.vertices.Add(new Vector3(xVertIni, 0f, yVert) - offset);
                this.vertices.Add(new Vector3(xVertEnd, 0f, yVert) - offset);
                this.vertices.Add(new Vector3(xVertIni, this.gridMap.wallHeight, yVert) - offset);
                this.vertices.Add(new Vector3(xVertEnd, this.gridMap.wallHeight, yVert) - offset);
                this.normals.Add(Vector3.right);
                this.normals.Add(Vector3.right);
                this.normals.Add(Vector3.right);
                this.normals.Add(Vector3.right);

                List<int> materialItem;
                if (!this.materialCache.TryGetValue(wall.wallMaterial, out materialItem))
                {
                    materialItem = new List<int>();
                    this.materialCache.Add(wall.wallMaterial, materialItem);
                }

                materialItem.Add(index + 1);
                materialItem.Add(index + 2);
                materialItem.Add(index + 0);
                materialItem.Add(index + 2);
                materialItem.Add(index + 1);
                materialItem.Add(index + 3);

                float xUVIni = Mathf.Repeat(xVertIni, wall.wallSize) / wall.wallSize;

                float xUVEnd = Mathf.Repeat(xVertEnd, wall.wallSize);
                if (xUVEnd == 0)
                    xUVEnd = 1;
                else
                    xUVEnd /= wall.wallSize;

                uvs.Add(new Vector2(xUVIni, 0f));
                uvs.Add(new Vector2(xUVEnd, 0f));
                uvs.Add(new Vector2(xUVIni, 1f));
                uvs.Add(new Vector2(xUVEnd, 1f));
            }

            void WriteUpperWall(GridMaterialBehavior wall, float xVertIni, float xVertEnd, float yVert)
            {
                int index = this.vertices.Count;

                this.vertices.Add(new Vector3(xVertIni, 0f, yVert) - offset);
                this.vertices.Add(new Vector3(xVertEnd, 0f, yVert) - offset);
                this.vertices.Add(new Vector3(xVertIni, this.gridMap.wallHeight, yVert) - offset);
                this.vertices.Add(new Vector3(xVertEnd, this.gridMap.wallHeight, yVert) - offset);
                this.normals.Add(Vector3.right);
                this.normals.Add(Vector3.right);
                this.normals.Add(Vector3.right);
                this.normals.Add(Vector3.right);

                List<int> materialItem;
                if (!this.materialCache.TryGetValue(wall.wallMaterial, out materialItem))
                {
                    materialItem = new List<int>();
                    this.materialCache.Add(wall.wallMaterial, materialItem);
                }

                materialItem.Add(index + 1);
                materialItem.Add(index + 0);
                materialItem.Add(index + 2);
                materialItem.Add(index + 2);
                materialItem.Add(index + 3);
                materialItem.Add(index + 1);

                float xUVIni = Mathf.Repeat(xVertIni, wall.wallSize) / wall.wallSize;

                float xUVEnd = Mathf.Repeat(xVertEnd, wall.wallSize);
                if (xUVEnd == 0)
                    xUVEnd = 1;
                else
                    xUVEnd /= wall.wallSize;

                uvs.Add(new Vector2(xUVIni, 0f));
                uvs.Add(new Vector2(xUVEnd, 0f));
                uvs.Add(new Vector2(xUVIni, 1f));
                uvs.Add(new Vector2(xUVEnd, 1f));
            }
        }
    }
}