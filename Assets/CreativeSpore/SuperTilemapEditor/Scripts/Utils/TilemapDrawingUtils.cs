using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CreativeSpore.SuperTilemapEditor
{

    public static class TilemapDrawingUtils
    {
        public const float k_timeToAbortFloodFill = 5f;

        struct Point
        {
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
            public int X;
            public int Y;
        }

        public static void FloodFill(Tilemap tilemap, Vector2 vLocalPos, uint tileData)
        {
            int gridX = BrushUtil.GetGridX(vLocalPos, tilemap.CellSize);
            int gridY = BrushUtil.GetGridY(vLocalPos, tilemap.CellSize);
            FloodFill(tilemap, gridX, gridY, tileData);
        }

        //https://social.msdn.microsoft.com/Forums/en-US/9d926a16-0051-4ca3-b77c-8095fb489ae2/flood-fill-c?forum=csharplanguage
        public static void FloodFill(Tilemap tilemap, int gridX, int gridY, uint tileData)
        {
            float timeStamp;
            timeStamp = Time.realtimeSinceStartup;
            //float callTimeStamp = timeStamp;

            LinkedList<Point> check = new LinkedList<Point>();
            uint floodFrom = tilemap.GetTileData(gridX, gridY);
            tilemap.SetTileData(gridX, gridY, tileData);
            bool isBrush = Tileset.GetBrushIdFromTileData(floodFrom) != 0;
            //Debug.Log(" Flood Fill Starts +++++++++++++++ ");
            if (
                isBrush? 
                Tileset.GetBrushIdFromTileData(floodFrom) != Tileset.GetBrushIdFromTileData(tileData)
                :
                floodFrom != tileData
            )
            {
                check.AddLast(new Point(gridX, gridY));
                while (check.Count > 0)
                {
                    Point cur = check.First.Value;
                    check.RemoveFirst();

                    foreach (Point off in new Point[] {
                        new Point(0, -1), new Point(0, 1), 
                        new Point(-1, 0), new Point(1, 0)})
                    {
                        Point next = new Point(cur.X + off.X, cur.Y + off.Y);
                        uint nextTileData = tilemap.GetTileData(next.X, next.Y);
                        if (
                            next.X >= tilemap.MinGridX && next.X <= tilemap.MaxGridX
                            && next.Y >= tilemap.MinGridY && next.Y <= tilemap.MaxGridY
                        )
                        {
                            if(
                                isBrush? 
                                Tileset.GetBrushIdFromTileData(floodFrom) == Tileset.GetBrushIdFromTileData(nextTileData)
                                :
                                floodFrom == nextTileData
                            )
                            {
                                check.AddLast(next);                                
                                tilemap.SetTileData(next.X, next.Y, tileData);
                            }
                        }
                    }

                    float timePast = Time.realtimeSinceStartup - timeStamp;
                    if (timePast > k_timeToAbortFloodFill)
                    {
#if UNITY_EDITOR
                        int result = UnityEditor.EditorUtility.DisplayDialogComplex("FloodFill is taking too much time", "Do you want to continue for another " + k_timeToAbortFloodFill + " seconds?", "Wait", "Cancel", "Wait and Don't ask again");
                        if (result == 0)
                        {
                            timeStamp = Time.realtimeSinceStartup;
                        }
                        else if (result == 1)
                        {
                            break;
                        }
                        else if (result == 2)
                        {
                            timeStamp = float.MaxValue;
                        }
#else
                    check.Clear();
#endif
                    }
                }
            }

            //Debug.Log("FloodFill Time " + (int)((Time.realtimeSinceStartup - callTimeStamp) * 1000) + "ms");
        }

        public static void FloodFillPreview(Tilemap tilemap, Vector2 vLocalPos, uint tileData, List<Vector2> outFilledPoints, uint maxPoints = uint.MaxValue)
        {
            int gridX = BrushUtil.GetGridX(vLocalPos, tilemap.CellSize);
            int gridY = BrushUtil.GetGridY(vLocalPos, tilemap.CellSize);
            FloodFillPreview(tilemap, gridX, gridY, tileData, outFilledPoints, maxPoints);
        }

        //Note: this is doing the same as FloodFill but not saving data in the tilemap, only saving the filled points and returning a list
        public static void FloodFillPreview(Tilemap tilemap, int gridX, int gridY, uint tileData, List<Vector2> outFilledPoints, uint maxPoints = uint.MaxValue)
        {
            if (
                gridX >= tilemap.MinGridX && gridX <= tilemap.MaxGridX
                && gridY >= tilemap.MinGridY && gridY <= tilemap.MaxGridY
            )
            {
                bool[] filledPoints = new bool[tilemap.GridWidth * tilemap.GridHeight];
                LinkedList<Point> check = new LinkedList<Point>();
                uint floodFrom = tilemap.GetTileData(gridX, gridY);
                outFilledPoints.Add(Vector2.Scale(new Vector2(gridX, gridY), tilemap.CellSize));
                filledPoints[(gridY - tilemap.MinGridY) * tilemap.GridWidth + gridX - tilemap.MinGridX] = true;
                bool isBrush = Tileset.GetBrushIdFromTileData(floodFrom) != 0;
                if (
                    isBrush ?
                    Tileset.GetBrushIdFromTileData(floodFrom) != Tileset.GetBrushIdFromTileData(tileData)
                    :
                    floodFrom != tileData
                )
                {
                    check.AddLast(new Point(gridX, gridY));
                    while (check.Count > 0)
                    {
                        Point cur = check.First.Value;
                        check.RemoveFirst();

                        foreach (Point off in new Point[] {
                        new Point(0, -1), new Point(0, 1), 
                        new Point(-1, 0), new Point(1, 0)})
                        {
                            Point next = new Point(cur.X + off.X, cur.Y + off.Y);

                            if (
                                next.X >= tilemap.MinGridX && next.X <= tilemap.MaxGridX
                                && next.Y >= tilemap.MinGridY && next.Y <= tilemap.MaxGridY
                            )
                            {
                                if (filledPoints[(next.Y - tilemap.MinGridY) * tilemap.GridWidth + next.X - tilemap.MinGridX]) continue; // skip already filled points
                                uint nextTileData = tilemap.GetTileData(next.X, next.Y);
                                if (
                                    isBrush ?
                                    Tileset.GetBrushIdFromTileData(floodFrom) == Tileset.GetBrushIdFromTileData(nextTileData)
                                    :
                                    floodFrom == nextTileData
                                )
                                {
                                    check.AddLast(next);
                                    filledPoints[(next.Y - tilemap.MinGridY) * tilemap.GridWidth + next.X - tilemap.MinGridX] = true;
                                    outFilledPoints.Add(Vector2.Scale(new Vector2(next.X, next.Y), tilemap.CellSize));
                                    if (outFilledPoints.Count >= maxPoints)
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}