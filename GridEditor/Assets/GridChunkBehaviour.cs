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
                        GridMaterialBehavior leftWall = this.gridMap.LeftWall(x, y);
                        GridMaterialBehavior rightWall = this.gridMap.RightWall(x, y);
                        GridMaterialBehavior bottomWall = this.gridMap.BottomWall(x, y);
                        GridMaterialBehavior upperWall = this.gridMap.UpperWall(x, y);

                        float xFloorIni;
                        float xFloorEnd;
                        float yFloorIni;
                        float yFloorEnd;

                        if (floor == null)
                        {
                            xFloorIni = x * this.gridMap.cellSize;
                            xFloorEnd = (x + 1) * this.gridMap.cellSize;
                            yFloorIni = y * this.gridMap.cellSize;
                            yFloorEnd = (y + 1) * this.gridMap.cellSize;

                            this.WriteRoof(xFloorIni,
                                xFloorEnd,
                                yFloorIni,
                                yFloorEnd);

                            if (leftWall != null)
                                this.WriteRightWall(leftWall, xFloorIni, yFloorIni, yFloorEnd);

                            if (rightWall != null)
                                this.WriteLeftWall(rightWall, xFloorEnd, yFloorIni, yFloorEnd);

                            if (bottomWall != null)
                                this.WriteForwardWall(bottomWall, xFloorIni, xFloorEnd, yFloorIni);

                            if (upperWall != null)
                                this.WriteBackWall(upperWall, xFloorIni, xFloorEnd, yFloorEnd);
                        }
                        else
                        {
                            float xCellIni = x * this.gridMap.cellSize;
                            float xWallIni = xCellIni + this.gridMap.wallSize;
                            float xCellEnd = (x + 1) * this.gridMap.cellSize;
                            float xWallEnd = xCellEnd - this.gridMap.wallSize;
                            float yCellIni = y * this.gridMap.cellSize;
                            float yWallIni = yCellIni + this.gridMap.wallSize;
                            float yCellEnd = (y + 1) * this.gridMap.cellSize;
                            float yWallEnd = yCellEnd - this.gridMap.wallSize;

                            if (leftWall == null)
                                xFloorIni = xCellIni;
                            else
                            {
                                xFloorIni = xWallIni;
                                this.WriteRoof(xCellIni, xFloorIni, yCellIni, yCellEnd);
                            }

                            if (rightWall == null)
                                xFloorEnd = xCellEnd;
                            else
                            {
                                xFloorEnd = xWallEnd;
                                this.WriteRoof(xFloorEnd, xCellEnd, yCellIni, yCellEnd);
                            }

                            if (bottomWall == null)
                                yFloorIni = yCellIni;
                            else
                            {
                                yFloorIni = yWallIni;
                                this.WriteRoof(xFloorIni, xFloorEnd, yCellIni, yFloorIni);
                            }

                            if (upperWall == null)
                                yFloorEnd = yCellEnd;
                            else
                            {
                                yFloorEnd = yWallEnd;
                                this.WriteRoof(xFloorIni, xFloorEnd, yFloorEnd, yCellEnd);
                            }

                            GridMaterialBehavior leftIniWall = null;
                            GridMaterialBehavior leftEndWall = null;
                            GridMaterialBehavior rightIniWall = null;
                            GridMaterialBehavior rightEndWall = null;
                            GridMaterialBehavior backIniWall = null;
                            GridMaterialBehavior backEndWall = null;
                            GridMaterialBehavior forwardIniWall = null;
                            GridMaterialBehavior forwardEndWall = null;

                            if (leftWall == null)
                            {
                                if (y > 0)
                                {
                                    leftIniWall = this.gridMap.LeftWall(x, y - 1);
                                    if (leftIniWall != null)
                                    {
                                        if ((x > 0) &&
                                            (x < this.gridMap.xAmmount - 1))
                                        {
                                            this.WriteLeftWall(leftIniWall,
                                                xWallIni,
                                                yCellIni,
                                                yWallIni);

                                            this.WriteRoof(xCellIni,
                                                xWallIni,
                                                yCellIni,
                                                yWallIni);
                                        }
                                        else
                                        {
                                            this.WriteRightWall(leftIniWall,
                                                xCellIni,
                                                yCellIni,
                                                yWallIni);
                                        }
                                    }
                                }

                                if (y < this.gridMap.yAmmount - 1)
                                {
                                    leftEndWall = this.gridMap.LeftWall(x, y + 1);
                                    if (leftEndWall != null)
                                    {
                                        if ((x > 0) &&
                                            (x < this.gridMap.xAmmount - 1))
                                        {
                                            this.WriteLeftWall(leftEndWall,
                                                xWallIni,
                                                yWallEnd,
                                                yCellEnd);

                                            this.WriteRoof(xCellIni,
                                                xWallIni,
                                                yWallEnd,
                                                yCellEnd);
                                        }
                                        else
                                        {
                                            this.WriteRightWall(leftEndWall,
                                                xCellIni,
                                                yWallEnd,
                                                yCellEnd);
                                        }
                                    }
                                }
                            }
                            else
                                this.WriteLeftWall(leftWall, xFloorIni, yFloorIni, yFloorEnd);

                            if (rightWall == null)
                            {

                                if (y > 0)
                                {
                                    rightIniWall = this.gridMap.RightWall(x, y - 1);

                                    if (rightIniWall != null)
                                    {
                                        if ((x > 0) &&
                                            (x < this.gridMap.xAmmount - 1))
                                        {
                                            this.WriteRightWall(rightIniWall,
                                                xWallEnd,
                                                yCellIni,
                                                yWallIni);

                                            this.WriteRoof(xWallEnd,
                                                xCellEnd,
                                                yCellIni,
                                                yWallIni);
                                        }
                                        else
                                        {
                                            this.WriteLeftWall(rightIniWall,
                                                xCellEnd,
                                                yCellIni,
                                                yWallIni);
                                        }
                                    }
                                }

                                if (y < this.gridMap.yAmmount - 1)
                                {
                                    rightEndWall = this.gridMap.RightWall(x, y + 1);

                                    if (rightEndWall != null)
                                    {
                                        if ((x > 0) &&
                                            (x < this.gridMap.xAmmount - 1))
                                        {
                                            this.WriteRightWall(rightEndWall,
                                                xWallEnd,
                                                yWallEnd,
                                                yCellEnd);

                                            this.WriteRoof(xWallEnd,
                                                xCellEnd,
                                                yWallEnd,
                                                yCellEnd);
                                        }
                                        else
                                        {
                                            this.WriteLeftWall(rightEndWall,
                                                xCellEnd,
                                                yWallEnd,
                                                yCellEnd);
                                        }
                                    }
                                }
                            }
                            else
                                this.WriteRightWall(rightWall, xFloorEnd, yFloorIni, yFloorEnd);

                            if (bottomWall == null)
                            {
                                if (x > 0)
                                {
                                    backIniWall = this.gridMap.BottomWall(x - 1, y);

                                    if (backIniWall != null)
                                    {
                                        if ((y > 0) &&
                                            (y < this.gridMap.yAmmount - 1))
                                        {
                                            this.WriteBackWall(backIniWall,
                                                xCellIni,
                                                xWallIni,
                                                yWallIni);

                                            if (leftIniWall == null)
                                                this.WriteRoof(xCellIni,
                                                    xWallIni,
                                                    yCellIni,
                                                    yWallIni);
                                        }
                                        else
                                        {
                                            this.WriteForwardWall(backIniWall,
                                                xCellIni,
                                                xWallIni,
                                                yCellIni);
                                        }
                                    }
                                }

                                if (x < this.gridMap.xAmmount - 1)
                                {
                                    backEndWall = this.gridMap.BottomWall(x + 1, y);

                                    if (backEndWall != null)
                                    {
                                        if ((y > 0) &&
                                            (y < this.gridMap.yAmmount - 1))
                                        {
                                            this.WriteBackWall(backEndWall,
                                                xWallEnd,
                                                xCellEnd,
                                                yWallIni);

                                            if (rightIniWall == null)
                                                this.WriteRoof(xWallEnd,
                                                    xCellEnd,
                                                    yCellIni,
                                                    yWallIni);
                                        }
                                        else
                                        {
                                            this.WriteForwardWall(backEndWall,
                                                xWallEnd,
                                                xCellEnd,
                                                yCellIni);
                                        }
                                    }
                                }
                            }
                            else
                                this.WriteBackWall(bottomWall, xFloorIni, xFloorEnd, yFloorIni);

                            if (upperWall == null)
                            {
                                if (x > 0)
                                {
                                    forwardIniWall = this.gridMap.UpperWall(x - 1, y);

                                    if (forwardIniWall != null)
                                    {
                                        if ((y > 0) &&
                                            (y < this.gridMap.yAmmount - 1))
                                        {
                                            this.WriteForwardWall(forwardIniWall,
                                                xCellIni,
                                                xWallIni,
                                                yWallEnd);

                                            if (leftEndWall == null)
                                                this.WriteRoof(xCellIni,
                                                    xWallIni,
                                                    yWallEnd,
                                                    yCellEnd);
                                        }
                                        else
                                        {
                                            this.WriteBackWall(forwardIniWall,
                                                xCellIni,
                                                xWallIni,
                                                yCellEnd);
                                        }
                                    }
                                }

                                if (x < this.gridMap.xAmmount - 1)
                                {
                                    forwardEndWall = this.gridMap.UpperWall(x + 1, y);

                                    if (forwardEndWall != null)
                                    {
                                        if ((y > 0) &&
                                            (y < this.gridMap.yAmmount - 1))
                                        {
                                            this.WriteForwardWall(forwardEndWall,
                                                xWallEnd,
                                                xCellEnd,
                                                yWallEnd);

                                            if (rightEndWall == null)
                                                this.WriteRoof(xWallEnd,
                                                    xCellEnd,
                                                    yWallEnd,
                                                    yCellEnd);
                                        }
                                        else
                                        {
                                            this.WriteBackWall(forwardEndWall,
                                                xWallEnd,
                                                xCellEnd,
                                                yCellEnd);
                                        }
                                    }
                                }
                            }
                            else
                                this.WriteForwardWall(upperWall, xFloorIni, xFloorEnd, yFloorEnd);

                            this.WriteFloor(floor,
                                xFloorIni,
                                xFloorEnd,
                                yFloorIni,
                                yFloorEnd);
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

            void WriteBackWall(GridMaterialBehavior wall, float xVertIni, float xVertEnd, float yVert)
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

            void WriteForwardWall(GridMaterialBehavior wall, float xVertIni, float xVertEnd, float yVert)
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