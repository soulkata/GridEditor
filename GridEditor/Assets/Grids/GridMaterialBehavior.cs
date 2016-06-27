using UnityEngine;

namespace Assets.Grids
{
    public class GridMaterialBehavior : MonoBehaviour
    {
        public Material floorMaterial;
        public float floorTileXSize;
        public float floorTileYSize;

        public Material wallMaterial;
        public float wallSize;
        public bool roofTop = false;

        void OnValidate()
        {
            GridMapBehavior parentMap = this.GetComponentInParent<GridMapBehavior>();
            if (parentMap != null)
                parentMap.reloadMeshes = true;
        }
    }
}
