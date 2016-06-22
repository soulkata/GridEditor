using UnityEngine;

namespace Assets
{
    public class GridMaterialBehavior : MonoBehaviour
    {
        public Material floorMaterial;
        public float floorTileXSize;
        public float floorTileZSize;

        public Material wallMaterial;
        public float wallSize;

        void OnValidate()
        {
            GridMapBehavior parentMap = this.GetComponentInParent<GridMapBehavior>();
            if (parentMap != null)
                parentMap.reloadMeshes = true;
        }
    }
}
