using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class GridChunkBehaviour : MonoBehaviour
    {
        public int minX;
        public int maxX;
        public int minY;
        public int maxY;

        void Start()
        {
            this.ReloadMesh();
        }

        public void ReloadMesh()
        {
            Mesh m = new Mesh();
            m.name = "chunk";

            ReloadChunkHelper h = new ReloadChunkHelper(this);

            m.SetVertices(h.vertices);
            m.SetNormals(h.normals);
            m.SetUVs(0, h.uvs);

            List<List<int>> triangles = new List<List<int>>();
            List<Material> materials = new List<Material>();
            foreach (KeyValuePair<Material, MaterialData> materialItemValue in h.materialCache)
            {
                if (materialItemValue.Value.floorTriangles.Count > 0)
                {
                    triangles.Add(materialItemValue.Value.floorTriangles);
                    materials.Add(materialItemValue.Key);
                }
            }

            m.subMeshCount = materials.Count;

            for (int i = 0; i < materials.Count; i++)
                m.SetTriangles(triangles[i], i);

            this.GetComponent<MeshFilter>().sharedMesh = m;
            this.GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
        }

        class ReloadChunkHelper
        {
            public GridChunkBehaviour owner;
            public GridMapBehavior gridMap;
            public Dictionary<Material, MaterialData> materialCache = new Dictionary<Material, MaterialData>();
            public List<Vector3> vertices = new List<Vector3>();
            public List<Vector3> normals = new List<Vector3>();
            public List<Vector2> uvs = new List<Vector2>();
            public Vector3 offset;

            public ReloadChunkHelper(GridChunkBehaviour owner)
            {
                this.owner = owner;
                this.gridMap = this.owner.GetComponentInParent<GridMapBehavior>();
                this.offset = new Vector3(this.owner.minX * this.gridMap.cellSize, 0f, this.owner.minY * this.gridMap.cellSize);

                for (int x = this.owner.minX; x <= this.owner.maxX; x++)
                {
                    for (int y = this.owner.minY; y <= this.owner.maxY; y++)
                    {
                        GridMaterialBehavior floor = this.gridMap.Floor(x, y);

                        float xCellIni = x * this.gridMap.cellSize;
                        float xCellEnd = (x + 1) * this.gridMap.cellSize;
                        float yCellIni = y * this.gridMap.cellSize;
                        float yCellEnd = (y + 1) * this.gridMap.cellSize;

                        if (floor == null)
                        {
                            this.WriteRoof(xCellIni,
                                xCellEnd,
                                yCellIni,
                                yCellEnd);

                            if (x == 0)
                                this.WriteRightWall(this.gridMap.roofMaterial, xCellIni, yCellIni, yCellEnd);

                            if (x == this.gridMap.xAmmount - 1)
                                this.WriteLeftWall(this.gridMap.roofMaterial, xCellEnd, yCellIni, yCellEnd);

                            if (y == 0)
                                this.WriteUpperWall(this.gridMap.roofMaterial, xCellIni, xCellEnd, yCellIni);

                            if (y == this.gridMap.yAmmount - 1)
                                this.WriteBottomWall(this.gridMap.roofMaterial, xCellIni, xCellEnd, yCellEnd);
                        }
                        else
                        {
                            float xFloorIni;
                            float xFloorEnd;
                            float yFloorIni;
                            float yFloorEnd;
                            float xWallIni = xCellIni + this.gridMap.wallSize;
                            float xWallEnd = xCellEnd - this.gridMap.wallSize;
                            float yWallIni = yCellIni + this.gridMap.wallSize;
                            float yWallEnd = yCellEnd - this.gridMap.wallSize;
                            GridMaterialBehavior leftWall = this.gridMap.LeftWall(x, y);
                            GridMaterialBehavior rightWall = this.gridMap.RightWall(x, y);
                            GridMaterialBehavior bottomWall = this.gridMap.BottomWall(x, y);
                            GridMaterialBehavior upperWall = this.gridMap.UpperWall(x, y);
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
                                    leftIniWall = this.gridMap.LeftWall(x, y - 1);

                                if (y == this.gridMap.yAmmount - 1)
                                    leftEndWall = null;
                                else
                                    leftEndWall = this.gridMap.LeftWall(x, y + 1);
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
                                    this.WriteUpperWall(this.gridMap.roofMaterial, xCellIni, xWallIni, yCellIni);
                                if ((y == this.gridMap.yAmmount - 1) &&
                                    (upperWall == null))
                                    this.WriteBottomWall(this.gridMap.roofMaterial, xCellIni, xWallIni, yCellEnd);
                            }

                            if (rightWall == null)
                            {
                                xFloorEnd = xCellEnd;

                                if (y == 0)
                                    rightIniWall = null;
                                else
                                    rightIniWall = this.gridMap.RightWall(x, y - 1);

                                if (y == this.gridMap.yAmmount - 1)
                                    rightEndWall = null;
                                else
                                    rightEndWall = this.gridMap.RightWall(x, y + 1);
                            }
                            else
                            {
                                xFloorEnd = xWallEnd;
                                rightIniWall = null;
                                rightEndWall = null;

                                this.WriteRoof(xWallEnd, xCellEnd, yCellIni, yCellEnd);

                                this.WriteRightWall(rightWall, xWallEnd, yCellIni, yCellEnd);
                                if (x == this.gridMap.xAmmount - 1)
                                    this.WriteLeftWall(gridMap.roofMaterial, xCellEnd, yCellIni, yCellEnd);
                                if ((y == 0) &&
                                    (bottomWall == null))
                                    this.WriteUpperWall(this.gridMap.roofMaterial, xWallEnd, xCellEnd, yCellIni);
                                if ((y == this.gridMap.yAmmount - 1) &&
                                    (upperWall == null))
                                    this.WriteBottomWall(this.gridMap.roofMaterial, xWallEnd, xCellEnd, yCellEnd);
                            }

                            if (bottomWall == null)
                            {
                                yFloorIni = yCellIni;

                                if (x == 0)
                                    bottomIniWall = null;
                                else
                                    bottomIniWall = this.gridMap.BottomWall(x - 1, y);

                                if (x == this.gridMap.xAmmount - 1)
                                    bottomEndWall = null;
                                else
                                    bottomEndWall = this.gridMap.BottomWall(x + 1, y);
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
                                    this.WriteRightWall(this.gridMap.roofMaterial, xCellIni, yCellIni, yWallIni);
                                if ((x == this.gridMap.xAmmount - 1) &&
                                    (rightWall == null))
                                    this.WriteLeftWall(this.gridMap.roofMaterial, xCellEnd, yCellIni, yWallIni);
                            }

                            if (upperWall == null)
                            {
                                yFloorEnd = yCellEnd;

                                if (x == 0)
                                    upperIniWall = null;
                                else
                                    upperIniWall = this.gridMap.UpperWall(x - 1, y);

                                if (x == this.gridMap.xAmmount - 1)
                                    upperEndWall = null;
                                else
                                    upperEndWall = this.gridMap.UpperWall(x + 1, y);
                            }
                            else
                            {
                                yFloorEnd = yWallEnd;
                                upperIniWall = null;
                                upperEndWall = null;

                                this.WriteRoof(xFloorIni, xFloorEnd, yWallEnd, yCellEnd);
                                this.WriteUpperWall(upperWall, xFloorIni, xFloorEnd, yWallEnd);
                                if (y == this.gridMap.yAmmount - 1)
                                    this.WriteBottomWall(this.gridMap.roofMaterial, xCellIni, xCellEnd, yCellEnd);
                                if ((x == 0) &&
                                    (leftWall == null))
                                    this.WriteRightWall(this.gridMap.roofMaterial, xCellIni, yWallEnd, yCellEnd);
                                if ((x == this.gridMap.xAmmount - 1) &&
                                    (rightWall == null))
                                    this.WriteLeftWall(this.gridMap.roofMaterial, xCellEnd, yWallEnd, yCellEnd);
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
                                if (x == this.gridMap.xAmmount - 1)
                                    this.WriteLeftWall(gridMap.roofMaterial, xCellEnd, yWallEnd, yCellEnd);

                                if (upperEndWall != null)
                                    this.WriteUpperWall(upperEndWall, xWallEnd, xCellEnd, yWallEnd);
                                else
                                    this.WriteUpperWall(rightEndWall, xWallEnd, xCellEnd, yWallEnd);
                                if (y == this.gridMap.yAmmount - 1)
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
                                if (y == this.gridMap.yAmmount - 1)
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
                                if (x == this.gridMap.xAmmount - 1)
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




                            //if (leftWall == null)
                            //{
                            //    if (y > 0)
                            //    {
                            //        leftIniWall = this.gridMap.LeftWall(x, y - 1);
                            //        if (leftIniWall != null)
                            //        {
                            //            if (x > 0)
                            //            {
                            //                this.WriteLeftWall(leftIniWall,
                            //                    xWallIni,
                            //                    yCellIni,
                            //                    yWallIni);

                            //                this.WriteRoof(xCellIni,
                            //                    xWallIni,
                            //                    yCellIni,
                            //                    yWallIni);
                            //            }
                            //            else
                            //            {
                            //                this.WriteRightWall(leftIniWall,
                            //                    xCellIni,
                            //                    yCellIni,
                            //                    yWallIni);
                            //            }
                            //        }
                            //    }

                            //    if (y < this.gridMap.yAmmount - 1)
                            //    {
                            //        leftEndWall = this.gridMap.LeftWall(x, y + 1);
                            //        if (leftEndWall != null)
                            //        {
                            //            if ((x > 0) &&
                            //                (x < this.gridMap.xAmmount - 1))
                            //            {
                            //                this.WriteLeftWall(leftEndWall,
                            //                    xWallIni,
                            //                    yWallEnd,
                            //                    yCellEnd);

                            //                this.WriteRoof(xCellIni,
                            //                    xWallIni,
                            //                    yWallEnd,
                            //                    yCellEnd);
                            //            }
                            //            else
                            //            {
                            //                this.WriteRightWall(leftEndWall,
                            //                    xCellIni,
                            //                    yWallEnd,
                            //                    yCellEnd);
                            //            }
                            //        }
                            //    }
                            //}
                            //else
                            //    this.WriteLeftWall(leftWall, xFloorIni, yFloorIni, yFloorEnd);

                            //if (rightWall == null)
                            //{
                            //    if (y > 0)
                            //    {
                            //        rightIniWall = this.gridMap.RightWall(x, y - 1);

                            //        if (rightIniWall != null)
                            //        {
                            //            if (x > 0)
                            //            {
                            //                this.WriteRightWall(rightIniWall,
                            //                    xWallEnd,
                            //                    yCellIni,
                            //                    yWallIni);

                            //                this.WriteRoof(xWallEnd,
                            //                    xCellEnd,
                            //                    yCellIni,
                            //                    yWallIni);
                            //            }
                            //            else
                            //            {
                            //                this.WriteLeftWall(rightIniWall,
                            //                    xCellEnd,
                            //                    yCellIni,
                            //                    yWallIni);
                            //            }
                            //        }
                            //    }

                            //    if (y < this.gridMap.yAmmount - 1)
                            //    {
                            //        rightEndWall = this.gridMap.RightWall(x, y + 1);

                            //        if (rightEndWall != null)
                            //        {
                            //            if ((x > 0) &&
                            //                (x < this.gridMap.xAmmount - 1))
                            //            {
                            //                this.WriteRightWall(rightEndWall,
                            //                    xWallEnd,
                            //                    yWallEnd,
                            //                    yCellEnd);

                            //                this.WriteRoof(xWallEnd,
                            //                    xCellEnd,
                            //                    yWallEnd,
                            //                    yCellEnd);
                            //            }
                            //            else
                            //            {
                            //                this.WriteLeftWall(rightEndWall,
                            //                    xCellEnd,
                            //                    yWallEnd,
                            //                    yCellEnd);
                            //            }
                            //        }
                            //    }
                            //}
                            //else
                            //    this.WriteRightWall(rightWall, xFloorEnd, yFloorIni, yFloorEnd);

                            //if (bottomWall == null)
                            //{
                            //    if (x > 0)
                            //    {
                            //        bottomIniWall = this.gridMap.BottomWall(x - 1, y);

                            //        if (bottomIniWall != null)
                            //        {
                            //            if ((y > 0) &&
                            //                (y < this.gridMap.yAmmount - 1))
                            //            {
                            //                this.WriteBackWall(bottomIniWall,
                            //                    xCellIni,
                            //                    xWallIni,
                            //                    yWallIni);

                            //                if (leftIniWall == null)
                            //                    this.WriteRoof(xCellIni,
                            //                        xWallIni,
                            //                        yCellIni,
                            //                        yWallIni);
                            //            }
                            //            else
                            //            {
                            //                this.WriteForwardWall(bottomIniWall,
                            //                    xCellIni,
                            //                    xWallIni,
                            //                    yCellIni);
                            //            }
                            //        }
                            //    }

                            //    if (x < this.gridMap.xAmmount - 1)
                            //    {
                            //        bottomEndWall = this.gridMap.BottomWall(x + 1, y);

                            //        if (bottomEndWall != null)
                            //        {
                            //            if ((y > 0) &&
                            //                (y < this.gridMap.yAmmount - 1))
                            //            {
                            //                this.WriteBackWall(bottomEndWall,
                            //                    xWallEnd,
                            //                    xCellEnd,
                            //                    yWallIni);

                            //                if (rightIniWall == null)
                            //                    this.WriteRoof(xWallEnd,
                            //                        xCellEnd,
                            //                        yCellIni,
                            //                        yWallIni);
                            //            }
                            //            else
                            //            {
                            //                this.WriteForwardWall(bottomEndWall,
                            //                    xWallEnd,
                            //                    xCellEnd,
                            //                    yCellIni);
                            //            }
                            //        }
                            //    }
                            //}
                            //else
                            //    this.WriteBackWall(bottomWall, xFloorIni, xFloorEnd, yFloorIni);

                            //if (upperWall == null)
                            //{
                            //    if (x > 0)
                            //    {
                            //        upperIniWall = this.gridMap.UpperWall(x - 1, y);

                            //        if (upperIniWall != null)
                            //        {
                            //            if ((y > 0) &&
                            //                (y < this.gridMap.yAmmount - 1))
                            //            {
                            //                this.WriteForwardWall(upperIniWall,
                            //                    xCellIni,
                            //                    xWallIni,
                            //                    yWallEnd);

                            //                if (leftEndWall == null)
                            //                    this.WriteRoof(xCellIni,
                            //                        xWallIni,
                            //                        yWallEnd,
                            //                        yCellEnd);
                            //            }
                            //            else
                            //            {
                            //                this.WriteBackWall(upperIniWall,
                            //                    xCellIni,
                            //                    xWallIni,
                            //                    yCellEnd);
                            //            }
                            //        }
                            //    }

                            //    if (x < this.gridMap.xAmmount - 1)
                            //    {
                            //        upperEndWall = this.gridMap.UpperWall(x + 1, y);

                            //        if (upperEndWall != null)
                            //        {
                            //            if ((y > 0) &&
                            //                (y < this.gridMap.yAmmount - 1))
                            //            {
                            //                this.WriteForwardWall(upperEndWall,
                            //                    xWallEnd,
                            //                    xCellEnd,
                            //                    yWallEnd);

                            //                if (rightEndWall == null)
                            //                    this.WriteRoof(xWallEnd,
                            //                        xCellEnd,
                            //                        yWallEnd,
                            //                        yCellEnd);
                            //            }
                            //            else
                            //            {
                            //                this.WriteBackWall(upperEndWall,
                            //                    xWallEnd,
                            //                    xCellEnd,
                            //                    yCellEnd);
                            //            }
                            //        }
                            //    }
                            //}
                            //else
                            //    this.WriteForwardWall(upperWall, xFloorIni, xFloorEnd, yFloorEnd);                            
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

                MaterialData materialItem;
                if (!this.materialCache.TryGetValue(this.gridMap.roofMaterial.floorMaterial, out materialItem))
                {
                    materialItem = new MaterialData();
                    this.materialCache.Add(this.gridMap.roofMaterial.floorMaterial, materialItem);
                }

                materialItem.floorTriangles.Add(index + 0);
                materialItem.floorTriangles.Add(index + 2);
                materialItem.floorTriangles.Add(index + 1);
                materialItem.floorTriangles.Add(index + 2);
                materialItem.floorTriangles.Add(index + 3);
                materialItem.floorTriangles.Add(index + 1);

                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(0f, 1f));
                uvs.Add(new Vector2(1f, 1f));
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

                MaterialData materialItem;
                if (!this.materialCache.TryGetValue(floor.floorMaterial, out materialItem))
                {
                    materialItem = new MaterialData();
                    this.materialCache.Add(floor.floorMaterial, materialItem);
                }

                materialItem.floorTriangles.Add(index + 0);
                materialItem.floorTriangles.Add(index + 2);
                materialItem.floorTriangles.Add(index + 1);
                materialItem.floorTriangles.Add(index + 2);
                materialItem.floorTriangles.Add(index + 3);
                materialItem.floorTriangles.Add(index + 1);

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

                MaterialData materialItem;
                if (!this.materialCache.TryGetValue(wall.wallMaterial, out materialItem))
                {
                    materialItem = new MaterialData();
                    this.materialCache.Add(wall.wallMaterial, materialItem);
                }

                materialItem.floorTriangles.Add(index + 1);
                materialItem.floorTriangles.Add(index + 2);
                materialItem.floorTriangles.Add(index + 0);
                materialItem.floorTriangles.Add(index + 2);
                materialItem.floorTriangles.Add(index + 1);
                materialItem.floorTriangles.Add(index + 3);

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

                MaterialData materialItem;
                if (!this.materialCache.TryGetValue(wall.wallMaterial, out materialItem))
                {
                    materialItem = new MaterialData();
                    this.materialCache.Add(wall.wallMaterial, materialItem);
                }

                materialItem.floorTriangles.Add(index + 1);
                materialItem.floorTriangles.Add(index + 0);
                materialItem.floorTriangles.Add(index + 2);
                materialItem.floorTriangles.Add(index + 2);
                materialItem.floorTriangles.Add(index + 3);
                materialItem.floorTriangles.Add(index + 1);

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

                MaterialData materialItem;
                if (!this.materialCache.TryGetValue(wall.wallMaterial, out materialItem))
                {
                    materialItem = new MaterialData();
                    this.materialCache.Add(wall.wallMaterial, materialItem);
                }

                materialItem.floorTriangles.Add(index + 1);
                materialItem.floorTriangles.Add(index + 2);
                materialItem.floorTriangles.Add(index + 0);
                materialItem.floorTriangles.Add(index + 2);
                materialItem.floorTriangles.Add(index + 1);
                materialItem.floorTriangles.Add(index + 3);

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

                MaterialData materialItem;
                if (!this.materialCache.TryGetValue(wall.wallMaterial, out materialItem))
                {
                    materialItem = new MaterialData();
                    this.materialCache.Add(wall.wallMaterial, materialItem);
                }

                materialItem.floorTriangles.Add(index + 1);
                materialItem.floorTriangles.Add(index + 0);
                materialItem.floorTriangles.Add(index + 2);
                materialItem.floorTriangles.Add(index + 2);
                materialItem.floorTriangles.Add(index + 3);
                materialItem.floorTriangles.Add(index + 1);

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

        class MaterialData
        {
            public List<int> floorTriangles = new List<int>();
        }
    }
}