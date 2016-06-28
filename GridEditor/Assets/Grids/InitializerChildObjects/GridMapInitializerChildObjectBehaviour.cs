using System;
using UnityEngine;

namespace Assets.Grids.InitializerChildObjects
{
    public class GridMapInitializerChildObjectBehaviour : GridMapInitializerBehaviour
    {
        public int xAmmount = 100;
        public int yAmmount = 100;

        public override void InitializeMap(out int xAmmount, out int yAmmount, out GridMaterialBehavior[,] floors, out GridMaterialBehavior[,] verticalWalls, out GridMaterialBehavior[,] horizontalWalls)
        {
            xAmmount = this.xAmmount;
            yAmmount = this.yAmmount;

            floors = new GridMaterialBehavior[this.xAmmount, this.yAmmount];
            horizontalWalls = new GridMaterialBehavior[this.xAmmount, this.yAmmount + 1];
            verticalWalls = new GridMaterialBehavior[this.xAmmount + 1, this.yAmmount];

            int childCount = this.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform t = this.transform.GetChild(i);

                if (!t.gameObject.activeInHierarchy)
                    continue;

                GridFloorBehaviour floor = t.GetComponent<GridFloorBehaviour>();
                if (floor != null)
                {
                    int xIni = Math.Max(floor.xStart, 0);
                    int yIni = Math.Max(floor.yStart, 0);
                    int xEnd = Math.Min(this.xAmmount, floor.xStart + floor.xCount) - 1;
                    int yEnd = Math.Min(this.yAmmount, floor.yStart + floor.yCount) - 1;

                    for (int x = xIni; x <= xEnd; x++)
                        for (int y = yIni; y <= yEnd; y++)
                            floors[x, y] = floor.floorMaterial;

                    yIni = Math.Max(floor.yStart + 1, 0);
                    yEnd = Math.Min(this.yAmmount, floor.yStart + floor.yCount - 1);

                    for (int x = xIni; x <= xEnd; x++)
                        for (int y = yIni; y <= yEnd; y++)
                            horizontalWalls[x, y] = null;

                    xIni = Math.Max(floor.xStart + 1, 0);
                    xEnd = Math.Min(this.xAmmount, floor.xStart + floor.xCount - 1);

                    yIni = Math.Max(floor.yStart, 0);
                    yEnd = Math.Min(this.yAmmount, floor.yStart + floor.yCount) - 1;

                    for (int x = xIni; x <= xEnd; x++)
                        for (int y = yIni; y <= yEnd; y++)
                            verticalWalls[x, y] = null;
                }

                GridRoomBehaviour room = t.GetComponent<GridRoomBehaviour>();
                if (room != null)
                {
                    int xEnd = room.xStart + room.xCount - 1;
                    int yEnd = room.yStart + room.yCount - 1;

                    for (int x = room.xStart; x <= xEnd; x++)
                    {
                        if (x < 0)
                            continue;

                        if (x >= this.xAmmount)
                            break;

                        if ((room.yStart >= 0) &&
                            (room.yStart <= this.yAmmount))
                            horizontalWalls[x, room.yStart] = room.floorMaterial;

                        if ((yEnd + 1 >= 0) &&
                            (yEnd + 1 <= this.yAmmount))
                            horizontalWalls[x, yEnd + 1] = room.floorMaterial;
                    }

                    for (int y = room.yStart; y <= yEnd; y++)
                    {
                        if (y < 0)
                            continue;

                        if (y >= this.yAmmount)
                            break;

                        if ((room.xStart >= 0) &&
                            (room.xStart <= this.yAmmount))
                            verticalWalls[room.xStart, y] = room.floorMaterial;

                        if ((xEnd + 1 >= 0) &&
                            (xEnd + 1 <= this.xAmmount))
                            verticalWalls[xEnd + 1, y] = room.floorMaterial;
                    }
                }
            }
        }

        void OnValidate()
        {
            GridMapBehavior parentMap = this.GetComponent<GridMapBehavior>();
            if (parentMap != null)
                parentMap.reloadMeshes = true;
        }
    }
}
