using System.Collections.Generic;
using UnityEngine;

namespace Assets.Grids
{
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class GridChunkWallBehaviour : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

        private Mesh mesh;
        private Material[] material;

        void Start()
        {
            this.meshFilter = this.GetComponent<MeshFilter>();
            this.meshRenderer = this.GetComponent<MeshRenderer>();
            this.meshCollider = this.GetComponent<MeshCollider>();

            if (this.mesh != null)
            {
                this.SetMesh(this.mesh, this.material);
                this.mesh = null;
                this.material = null;
            }
        }

        public void SetMesh(Mesh mesh, Material[] material)
        {
            if (this.meshFilter == null)
            {
                this.mesh = mesh;
                this.material = material;
            }
            else
            {
                this.meshFilter.sharedMesh = mesh;
                this.meshRenderer.sharedMaterials = material;
                this.meshCollider.sharedMesh = mesh;
            }
        }
    }
}