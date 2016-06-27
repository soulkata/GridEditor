using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Grids
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class GridChunkRoofBehaviour : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        private Mesh mesh;
        private Material material;

        void Start()
        {
            this.meshFilter = this.GetComponent<MeshFilter>();
            this.meshRenderer = this.GetComponent<MeshRenderer>();

            if (this.mesh != null)
            {
                this.SetMesh(this.mesh, this.material);
                this.mesh = null;
                this.material = null;
            }
        }

        public void SetMesh(Mesh mesh, Material material)
        {
            if (this.meshFilter == null)
            {
                this.mesh = mesh;
                this.material = material;
            }
            else
            {
                this.meshFilter.sharedMesh = mesh;
                this.meshRenderer.sharedMaterial = material;
            }
        }
    }
}
