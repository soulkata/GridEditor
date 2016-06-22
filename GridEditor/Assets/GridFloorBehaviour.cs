using UnityEngine;

namespace Assets
{
    public class GridFloorBehaviour : MonoBehaviour
    {
        public GridMaterialBehavior floorMaterial;
        public int xStart;
        public int zStart;
        public int xCount;
        public int zCount;

        void OnValidate()
        {
            GridMapBehavior parentMap = this.GetComponentInParent<GridMapBehavior>();
            if (parentMap != null)
                parentMap.reloadMeshes = true;
        }
    }
}
