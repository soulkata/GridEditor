using UnityEngine;

namespace Assets.Grids.InitializerChildObjects
{
    public class GridFloorBehaviour : MonoBehaviour
    {
        public GridMaterialBehavior floorMaterial;
        public int xStart;
        public int yStart;
        public int xCount;
        public int yCount;

        void OnValidate()
        {
            GridMapBehavior parentMap = this.GetComponentInParent<GridMapBehavior>();
            if (parentMap != null)
                parentMap.reloadMeshes = true;
        }
    }
}
