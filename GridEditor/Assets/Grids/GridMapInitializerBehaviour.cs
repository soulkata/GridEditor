using UnityEngine;

namespace Assets.Grids
{
    public abstract class GridMapInitializerBehaviour : MonoBehaviour
    {
        public abstract void InitializeMap(out int xAmmount, out int yAmmount, out GridMaterialBehavior[,] floors, out GridMaterialBehavior[,] verticalWalls, out GridMaterialBehavior[,] horizontalWalls);
    }
}
