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
        public int minZ;
        public int maxZ;

        void Start()
        {
            this.ReloadMesh();
        }        

        public void ReloadMesh()
        {
            

            Mesh m = new Mesh();
            m.name = "chunk";



            //MaterialData materialItem;

            //float xVerticeOffset = 0;
            //for (int x = this.minX; x <= this.maxX; x++)
            //{
            //    float zVerticeOffset = 0;
            //    for (int z = this.minZ; z <= this.maxZ; z++)
            //    {
            //        GridMaterialBehavior floorMaterial;

            //        gridMap.CellData(x, z, out floorMaterial);

            //        if (floorMaterial != null)
            //        {
            //            float xVertIni;
            //            float xVertEnd;
            //            float zVertIni;
            //            float zVertEnd;
            //            float xUVIni;
            //            float xUVEnd;
            //            float zUVIni;
            //            float zUVEnd;

            //            float mod = ((float)(x % floorMaterial.floorTileXSize)) / floorMaterial.floorTileXSize;

            //            if (gridMap.LeftWall(x, z))
            //            {
            //                xVertIni = xVerticeOffset + gridMap.wallSize;
            //                xUVIni = mod + floorMaterial.wallXPercent;
            //            }
            //            else
            //            {
            //                xVertIni = xVerticeOffset;
            //                xUVIni = mod;
            //            }

            //            mod = ((float)(z % floorMaterial.floorTileZSize)) / floorMaterial.floorTileZSize;
            //            if (gridMap.BackWall(x, z))
            //            {
            //                zVertIni = zVerticeOffset + gridMap.wallSize;
            //                zUVIni = mod + floorMaterial.wallZPercent;
            //            }
            //            else
            //            {
            //                zVertIni = zVerticeOffset;
            //                zUVIni = mod;
            //            }

            //            if (gridMap.ForwardWall(x, z))
            //            {
            //                zVertEnd = zVerticeOffset + gridMap.cellSize - gridMap.wallSize;
            //                zUVEnd = mod + floorMaterial.floorZPercent - floorMaterial.wallZPercent;
            //            }
            //            else
            //            {
            //                zVertEnd = zVerticeOffset + gridMap.cellSize;
            //                zUVEnd = mod + floorMaterial.floorZPercent;
            //            }


            //        }
            //        else
            //        {
            //            floorMaterial = gridMap.wallTopBehaviour;
            //            if (!materialCache.TryGetValue(floorMaterial, out materialItem))
            //            {
            //                materialItem = new MaterialData();
            //                materialCache.Add(floorMaterial, materialItem);
            //            }

            //            float mod = ((float)(x % floorMaterial.floorTileXSize)) / floorMaterial.floorTileXSize;

            //            float xVertIni = xVerticeOffset;
            //            float xVertEnd = xVerticeOffset + gridMap.cellSize;
            //            float xUVIni = mod;
            //            float xUVEnd = mod + floorMaterial.floorXPercent;

            //            mod = ((float)(z % floorMaterial.floorTileZSize)) / floorMaterial.floorTileZSize;

            //            float zVertIni = zVerticeOffset;
            //            float zVertEnd = zVerticeOffset + gridMap.cellSize;
            //            float zUVIni = mod;
            //            float zUVEnd = mod + floorMaterial.floorZPercent;

            //            int index = vertices.Count;
            //            vertices.Add(new Vector3(xVertIni, gridMap.wallHeight, zVertIni));
            //            vertices.Add(new Vector3(xVertEnd, gridMap.wallHeight, zVertIni));
            //            vertices.Add(new Vector3(xVertIni, gridMap.wallHeight, zVertEnd));
            //            vertices.Add(new Vector3(xVertEnd, gridMap.wallHeight, zVertEnd));
            //            normals.Add(Vector3.up);
            //            normals.Add(Vector3.up);
            //            normals.Add(Vector3.up);
            //            normals.Add(Vector3.up);

            //            materialItem.floorTriangles.Add(index + 0);
            //            materialItem.floorTriangles.Add(index + 2);
            //            materialItem.floorTriangles.Add(index + 1);
            //            materialItem.floorTriangles.Add(index + 2);
            //            materialItem.floorTriangles.Add(index + 3);
            //            materialItem.floorTriangles.Add(index + 1);

            //            uvs.Add(new Vector2(xUVIni, zUVIni));
            //            uvs.Add(new Vector2(xUVEnd, zUVIni));
            //            uvs.Add(new Vector2(xUVIni, zUVEnd));
            //            uvs.Add(new Vector2(xUVEnd, zUVEnd));
            //        }

            //        zVerticeOffset += gridMap.cellSize;
            //    }

            //    xVerticeOffset += gridMap.cellSize;
            //}

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
                this.offset = new Vector3(this.owner.minX * this.gridMap.cellSize, 0f, this.owner.minZ * this.gridMap.cellSize);

                for (int x = this.owner.minX; x <= this.owner.maxX; x++)
                {
                    for (int z = this.owner.minZ; z <= this.owner.maxZ; z++)
                    {
                        GridMaterialBehavior floor = this.gridMap.Floor(x, z);
                        GridMaterialBehavior leftWall = this.gridMap.LeftWall(x, z);
                        GridMaterialBehavior rightWall = this.gridMap.RightWall(x, z);
                        GridMaterialBehavior backWall = this.gridMap.BackWall(x, z);
                        GridMaterialBehavior forwardWall = this.gridMap.ForwardWall(x, z);
                        GridMaterialBehavior aux;

                        float xVertIni;
                        float xVertEnd;
                        float zVertIni;
                        float zVertEnd;

                        if (floor == null)
                        {
                            xVertIni = x * this.gridMap.cellSize;
                            xVertEnd = (x + 1) * this.gridMap.cellSize;
                            zVertIni = z * this.gridMap.cellSize;
                            zVertEnd = (z + 1) * this.gridMap.cellSize;

                            this.WriteRoof(xVertIni,
                                xVertEnd,
                                zVertIni,
                                zVertEnd);

                            if (leftWall != null)
                                this.WriteRightWall(leftWall, xVertIni, zVertIni, zVertEnd);

                            if (rightWall != null)
                                this.WriteLeftWall(rightWall, xVertEnd, zVertIni, zVertEnd);

                            if (backWall != null)
                                this.WriteForwardWall(backWall, xVertIni, xVertEnd, zVertIni);

                            if (forwardWall != null)
                                this.WriteBackWall(forwardWall, xVertIni, xVertEnd, zVertEnd);
                        }
                        else
                        {
                            if (leftWall == null)
                                xVertIni = x * this.gridMap.cellSize;
                            else
                            {
                                xVertIni = x * this.gridMap.cellSize + this.gridMap.wallSize;
                                this.WriteRoof(x * this.gridMap.cellSize, xVertIni, z * this.gridMap.cellSize, (z + 1) * this.gridMap.cellSize);
                            }

                            if (rightWall == null)
                                xVertEnd = (x + 1) * this.gridMap.cellSize;
                            else
                            {
                                xVertEnd = (x + 1) * this.gridMap.cellSize - this.gridMap.wallSize;
                                this.WriteRoof(xVertEnd, (x + 1) * this.gridMap.cellSize, z * this.gridMap.cellSize, (z + 1) * this.gridMap.cellSize);
                            }

                            if (backWall == null)
                                zVertIni = z * this.gridMap.cellSize;
                            else
                            {
                                zVertIni = z * this.gridMap.cellSize + this.gridMap.wallSize;
                                this.WriteRoof(xVertIni, xVertEnd, z * this.gridMap.cellSize, zVertIni);
                            }

                            if (forwardWall == null)
                                zVertEnd = (z + 1) * this.gridMap.cellSize;
                            else
                            {
                                zVertEnd = (z + 1) * this.gridMap.cellSize - this.gridMap.wallSize;
                                this.WriteRoof(xVertIni, xVertEnd, zVertEnd, (z + 1) * this.gridMap.cellSize);
                            }

                            if (leftWall == null)
                            {
                                if (z > 0)
                                {
                                    aux = this.gridMap.LeftWall(x, z - 1);
                                    if (aux != null)
                                    {
                                        if ((x > 0) &&
                                            (x < this.gridMap.xAmmount - 1))
                                        {
                                            this.WriteLeftWall(aux,
                                                x * this.gridMap.cellSize + this.gridMap.wallSize,
                                                z * this.gridMap.cellSize,
                                                z * this.gridMap.cellSize + this.gridMap.wallSize);
                                            this.WriteRoof(x * this.gridMap.cellSize,
                                                x * this.gridMap.cellSize + this.gridMap.wallSize,
                                                z * this.gridMap.cellSize,
                                                z * this.gridMap.cellSize + this.gridMap.wallSize);
                                        }
                                        else
                                        {
                                            this.WriteRightWall(aux,
                                                x * this.gridMap.cellSize,
                                                z * this.gridMap.cellSize,
                                                z * this.gridMap.cellSize + this.gridMap.wallSize);
                                        }
                                    }
                                }

                                if (z < this.gridMap.zAmmount - 1)
                                {
                                    aux = this.gridMap.LeftWall(x, z + 1);
                                    if (aux != null)
                                    {
                                        if ((x > 0) &&
                                            (x < this.gridMap.xAmmount - 1))
                                        {
                                            this.WriteLeftWall(aux,
                                                x * this.gridMap.cellSize + this.gridMap.wallSize,
                                                (z + 1) * this.gridMap.cellSize - this.gridMap.wallSize,
                                                (z + 1) * this.gridMap.cellSize);

                                            this.WriteRoof(x * this.gridMap.cellSize,
                                                x * this.gridMap.cellSize + this.gridMap.wallSize,
                                                (z + 1) * this.gridMap.cellSize - this.gridMap.wallSize,
                                                (z + 1) * this.gridMap.cellSize);
                                        }
                                        else
                                        {
                                            this.WriteRightWall(aux,
                                                x * this.gridMap.cellSize,
                                                (z + 1) * this.gridMap.cellSize - this.gridMap.wallSize,
                                                (z + 1) * this.gridMap.cellSize);
                                        }
                                    }
                                }
                            }
                            else
                                this.WriteLeftWall(leftWall, xVertIni, zVertIni, zVertEnd);                            

                            if (rightWall == null)
                            {
                                
                                if (z > 0)
                                {
                                    aux = this.gridMap.RightWall(x, z - 1);

                                    if (aux != null)
                                    {
                                        if ((x > 0) &&
                                            (x < this.gridMap.xAmmount - 1))
                                        {
                                            this.WriteRightWall(aux,
                                                (x + 1) * this.gridMap.cellSize - this.gridMap.wallSize,
                                                z * this.gridMap.cellSize,
                                                z * this.gridMap.cellSize + this.gridMap.wallSize);

                                            this.WriteRoof((x + 1) * this.gridMap.cellSize - this.gridMap.wallSize,
                                                (x + 1) * this.gridMap.cellSize,
                                                z * this.gridMap.cellSize,
                                                z * this.gridMap.cellSize + this.gridMap.wallSize);
                                        }
                                        else
                                        {
                                            this.WriteLeftWall(aux,
                                                (x + 1) * this.gridMap.cellSize,
                                                z * this.gridMap.cellSize,
                                                z * this.gridMap.cellSize + this.gridMap.wallSize);
                                        }
                                    }
                                }

                                if (z < this.gridMap.zAmmount - 1)
                                {
                                    aux = this.gridMap.RightWall(x, z + 1);

                                    if (aux != null)
                                    {
                                        if ((x > 0) &&
                                            (x < this.gridMap.xAmmount - 1))
                                        {
                                            this.WriteRightWall(aux,
                                                (x + 1) * this.gridMap.cellSize - this.gridMap.wallSize,
                                                (z + 1) * this.gridMap.cellSize - this.gridMap.wallSize,
                                                (z + 1) * this.gridMap.cellSize);

                                            this.WriteRoof((x + 1) * this.gridMap.cellSize - this.gridMap.wallSize,
                                                (x + 1) * this.gridMap.cellSize,
                                                (z + 1) * this.gridMap.cellSize - this.gridMap.wallSize,
                                                (z + 1) * this.gridMap.cellSize);
                                        }
                                        else
                                        {
                                            this.WriteLeftWall(aux,
                                                (x + 1) * this.gridMap.cellSize,
                                                (z + 1) * this.gridMap.cellSize - this.gridMap.wallSize,
                                                (z + 1) * this.gridMap.cellSize);
                                        }
                                    }
                                }
                            }
                            else
                                this.WriteRightWall(rightWall, xVertEnd, zVertIni, zVertEnd);                                                       

                            if (backWall == null)
                            {
                                if (x > 0)
                                {
                                    aux = this.gridMap.BackWall(x - 1, z);

                                    if (aux != null)
                                    {
                                        if ((z > 0) &&
                                            (z < this.gridMap.zAmmount - 1))
                                        {
                                            this.WriteBackWall(aux,
                                                x * this.gridMap.cellSize,
                                                x * this.gridMap.cellSize + this.gridMap.wallSize,
                                                z * this.gridMap.cellSize + this.gridMap.wallSize);
                                        }
                                        else
                                        {
                                            this.WriteForwardWall(aux,
                                                x * this.gridMap.cellSize,
                                                x * this.gridMap.cellSize + this.gridMap.wallSize,
                                                z * this.gridMap.cellSize);
                                        }
                                    }
                                }

                                if (x < this.gridMap.xAmmount - 1)
                                {
                                    aux = this.gridMap.BackWall(x + 1, z);

                                    if (aux != null)
                                    {
                                        if ((z > 0) &&
                                            (z < this.gridMap.zAmmount - 1))
                                        {
                                            this.WriteBackWall(aux,
                                                (x + 1) * this.gridMap.cellSize - this.gridMap.wallSize,
                                                (x + 1) * this.gridMap.cellSize,
                                                z * this.gridMap.cellSize + this.gridMap.wallSize);
                                        }
                                        else
                                        {
                                            this.WriteForwardWall(aux,
                                                (x + 1) * this.gridMap.cellSize - this.gridMap.wallSize,
                                                (x + 1) * this.gridMap.cellSize,
                                                z * this.gridMap.cellSize);
                                        }
                                    }
                                }
                            }
                            else
                                this.WriteBackWall(backWall, xVertIni, xVertEnd, zVertIni);

                            if (forwardWall == null)
                            {
                                if (x > 0)
                                {
                                    aux = this.gridMap.ForwardWall(x - 1, z);

                                    if (aux != null)
                                    {
                                        if ((z > 0) &&
                                            (z < this.gridMap.zAmmount - 1))
                                        {
                                            this.WriteForwardWall(aux,
                                                x * this.gridMap.cellSize,
                                                x * this.gridMap.cellSize + this.gridMap.wallSize,
                                                (z + 1) * this.gridMap.cellSize - this.gridMap.wallSize);
                                        }
                                        else
                                        {
                                            this.WriteBackWall(aux,
                                                x * this.gridMap.cellSize,
                                                x * this.gridMap.cellSize + this.gridMap.wallSize,
                                                (z + 1) * this.gridMap.cellSize);
                                        }
                                    }
                                }

                                if (x < this.gridMap.xAmmount - 1)
                                {
                                    aux = this.gridMap.ForwardWall(x + 1, z);

                                    if (aux != null)
                                    {
                                        if ((z > 0) &&
                                            (z < this.gridMap.zAmmount - 1))
                                        {
                                            this.WriteForwardWall(aux,
                                                (x + 1) * this.gridMap.cellSize - this.gridMap.wallSize,
                                                (x + 1) * this.gridMap.cellSize,
                                                (z + 1) * this.gridMap.cellSize - this.gridMap.wallSize);
                                        }
                                        else
                                        {
                                            this.WriteBackWall(aux,
                                                (x + 1) * this.gridMap.cellSize - this.gridMap.wallSize,
                                                (x + 1) * this.gridMap.cellSize,
                                                (z + 1) * this.gridMap.cellSize);
                                        }
                                    }
                                }
                            }
                            else
                                this.WriteForwardWall(forwardWall, xVertIni, xVertEnd, zVertEnd);

                            this.WriteFloor(floor,
                                xVertIni,
                                xVertEnd,
                                zVertIni,
                                zVertEnd);                            
                        }                        
                    }
                }
            }

            void WriteRoof(float xVertIni, float xVertEnd, float zVertIni, float zVertEnd)
            {
                int index = this.vertices.Count;
                this.vertices.Add(new Vector3(xVertIni, this.gridMap.wallHeight, zVertIni) - offset);
                this.vertices.Add(new Vector3(xVertEnd, this.gridMap.wallHeight, zVertIni) - offset);
                this.vertices.Add(new Vector3(xVertIni, this.gridMap.wallHeight, zVertEnd) - offset);
                this.vertices.Add(new Vector3(xVertEnd, this.gridMap.wallHeight, zVertEnd) - offset);
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

            void WriteFloor(GridMaterialBehavior floor, float xVertIni, float xVertEnd, float zVertIni, float zVertEnd)
            {
                int index = this.vertices.Count;
                this.vertices.Add(new Vector3(xVertIni, 0f, zVertIni) - offset);
                this.vertices.Add(new Vector3(xVertEnd, 0f, zVertIni) - offset);
                this.vertices.Add(new Vector3(xVertIni, 0f, zVertEnd) - offset);
                this.vertices.Add(new Vector3(xVertEnd, 0f, zVertEnd) - offset);
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
                float zUVIni = Mathf.Repeat(zVertIni, floor.floorTileZSize) / floor.floorTileZSize;

                float xUVEnd = Mathf.Repeat(xVertEnd, floor.floorTileXSize);
                if (xUVEnd == 0)
                    xUVEnd = 1;
                else
                    xUVEnd /= floor.floorTileXSize;

                float zUVEnd = Mathf.Repeat(zVertEnd, floor.floorTileZSize);
                if (zUVEnd == 0)
                    zUVEnd = 1;
                else
                    zUVEnd /= floor.floorTileZSize;

                uvs.Add(new Vector2(xUVIni, zUVIni));
                uvs.Add(new Vector2(xUVEnd, zUVIni));
                uvs.Add(new Vector2(xUVIni, zUVEnd));
                uvs.Add(new Vector2(xUVEnd, zUVEnd));
            }

            void WriteLeftWall(GridMaterialBehavior wall, float xVert, float zVertIni, float zVertEnd)
            {
                int index = this.vertices.Count;
                this.vertices.Add(new Vector3(xVert, 0f, zVertIni) - offset);
                this.vertices.Add(new Vector3(xVert, this.gridMap.wallHeight, zVertIni) - offset);
                this.vertices.Add(new Vector3(xVert, 0f, zVertEnd) - offset);
                this.vertices.Add(new Vector3(xVert, this.gridMap.wallHeight, zVertEnd) - offset);
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

                float xUVIni = Mathf.Repeat(zVertIni, wall.wallSize) / wall.wallSize;

                float xUVEnd = Mathf.Repeat(zVertEnd, wall.wallSize);
                if (xUVEnd == 0)
                    xUVEnd = 1;
                else
                    xUVEnd /= wall.wallSize;

                uvs.Add(new Vector2(xUVIni, 0f));
                uvs.Add(new Vector2(xUVIni, 1f));
                uvs.Add(new Vector2(xUVEnd, 0f));
                uvs.Add(new Vector2(xUVEnd, 1f));
            }

            void WriteRightWall(GridMaterialBehavior wall, float xVert, float zVertIni, float zVertEnd)
            {
                int index = this.vertices.Count;
                this.vertices.Add(new Vector3(xVert, 0f, zVertIni) - offset);
                this.vertices.Add(new Vector3(xVert, this.gridMap.wallHeight, zVertIni) - offset);
                this.vertices.Add(new Vector3(xVert, 0f, zVertEnd) - offset);
                this.vertices.Add(new Vector3(xVert, this.gridMap.wallHeight, zVertEnd) - offset);
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

                float xUVIni = Mathf.Repeat(zVertIni, wall.wallSize) / wall.wallSize;

                float xUVEnd = Mathf.Repeat(zVertEnd, wall.wallSize);
                if (xUVEnd == 0)
                    xUVEnd = 1;
                else
                    xUVEnd /= wall.wallSize;

                uvs.Add(new Vector2(xUVIni, 0f));
                uvs.Add(new Vector2(xUVIni, 1f));
                uvs.Add(new Vector2(xUVEnd, 0f));
                uvs.Add(new Vector2(xUVEnd, 1f));
            }

            void WriteBackWall(GridMaterialBehavior wall, float xVertIni, float xVertEnd, float zVert)
            {
                int index = this.vertices.Count;

                this.vertices.Add(new Vector3(xVertIni, 0f, zVert) - offset);
                this.vertices.Add(new Vector3(xVertEnd, 0f, zVert) - offset);
                this.vertices.Add(new Vector3(xVertIni, this.gridMap.wallHeight, zVert) - offset);
                this.vertices.Add(new Vector3(xVertEnd, this.gridMap.wallHeight, zVert) - offset);                
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

            void WriteForwardWall(GridMaterialBehavior wall, float xVertIni, float xVertEnd, float zVert)
            {
                int index = this.vertices.Count;

                this.vertices.Add(new Vector3(xVertIni, 0f, zVert) - offset);
                this.vertices.Add(new Vector3(xVertEnd, 0f, zVert) - offset);
                this.vertices.Add(new Vector3(xVertIni, this.gridMap.wallHeight, zVert) - offset);
                this.vertices.Add(new Vector3(xVertEnd, this.gridMap.wallHeight, zVert) - offset);
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
