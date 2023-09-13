using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public enum Mode { Basic, Advanced };

    [Header("Generator Mode")]
    public Mode mode;

    [Header("Map Constaints")]
    public int width;
    public int height;
    public float seed;
    [Range(1, 100)]
    public float surfaceSmoothness;

    [Header("Cave Constraints")]
    [Range(0, 1)]
    public float caveDensity;

    [Header("Tile Data")]
    public TileBase groundTile;
    public Tilemap groundTileMap;
    public bool autoUpdate;


    [Header("Advanced ONLY")]
    [Range(0, 60)]
    public int cellularAutomatonRepetitions;
    [Range(1, 100)]
    public int cellularDensity;
    [Range(1, 20)]
    public int passagewayRadius;

    int[,] map;

    public int[,] GetMap()
    {
        return map;
    }

    // generates a basic map using perlin height and caves
    public void GenerateBasic()
    {
        groundTileMap.ClearAllTiles();
        map = TerrainGeneration(InitializeArray(width, height, true));
        RenderMap(map, groundTileMap, groundTile);
    }

    // generates an advanced map using cellular automata on a noise map
    public void GenerateAdvanced()
    {
        // generate tilemap using cellular automata
        groundTileMap.ClearAllTiles();
        map = GeneratePerlinNoiseMap(width, height);
        map = ApplyCellularAutomaton(map, cellularAutomatonRepetitions);

        // fill in small holes and remove small walls in tilemap
        ProcessMapRegions();

        // create a mesh using marching squares
        //MeshGenerator meshGen = GetComponent<MeshGenerator>();
        //meshGen.GenerateMesh(map, 1);

        // render tiles with value 1 on map with rule tile groundTile
        RenderMap(map, groundTileMap, groundTile);

    }

    void ProcessMapRegions()
    {
        List<List<Coord>> wallRegions = GetRegions(1);

        int wallThresholdSize = 50;

        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0);

        int roomThresholdSize = 50;

        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }

        survivingRooms.Sort();

        survivingRooms[0].isMainRoom = true;
        survivingRooms[0].isAccessibleFromMainRoom = true;
        
        ConnectClosestRooms(survivingRooms);
    }

    void ConnectClosestRooms(List <Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach(Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }
            

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2)) + (int)(Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);

        List<Coord> line = GetLine(tileA, tileB);
        foreach (Coord c in line)
        {
            DrawCircle(c, passagewayRadius);
        }
    }

    void DrawCircle(Coord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x*x + y*y <= r*r)
                {
                    int realX = c.tileX + x;
                    int realY = c.tileY + y;

                    if (IsWithinBounds(realX, realY))
                    {
                        map[realX, realY] = 0;
                    }
                }
            }
        }
    }

    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }

                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(tile.tileX, tile.tileY, 0);
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for(int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if(IsWithinBounds(x, y) && (x == tile.tileX || y == tile.tileY))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x,y] == 0 && map[x,y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    // set all the values in the map to default value 0 if empty is true, else set it all to 1
    public int[,] InitializeArray(int width, int height, bool empty)
    {
        int[,] map = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = (empty) ? 0 : 1;
            }
        }

        return map;
    }

    public int[,] GeneratePerlinNoiseMap(int width, int height)
    {
        int random;

        int[,] map = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                random = UnityEngine.Random.Range(1, 100);

                if (random > cellularDensity)
                {
                    map[x, y] = 0;
                }
                else
                {
                    map[x, y] = 1;
                }
            }
        }
        
        return map;
    }

    public int[,] TerrainGeneration(int[,] map)
    {
        int perlinHeight, caveValue;

        for (int x = 0; x < width; x++)
        {
            perlinHeight = (surfaceSmoothness == 1) ? height : Mathf.RoundToInt(Mathf.PerlinNoise(x / surfaceSmoothness, seed) * height / 2) + (height / 2);

            for (int y = 0; y < perlinHeight; y++)
            {
                caveValue = Mathf.RoundToInt(Mathf.PerlinNoise((x * caveDensity) + seed, (y * caveDensity) + seed));

                map[x, y] = (caveValue == 1) ? 0 : 1;
            }
        }

        return map;
    }

    public int[,] ApplyCellularAutomaton(int[,] map, int count)
    {
        int neighbourWallCount;
        int[,] tempMap = new int[width, height];

        // iterate cellular automaton algorithm specified number of times
        for (int i = 1; i <= count; i++)
        {
            // create a copy of the original map
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tempMap[x, y] = map[x, y];
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // reset count before counting
                    neighbourWallCount = 0;

                    // check all neighbours for tiles. for each tile, add a count to neighbourWallCount
                    for (int j = (x - 1); j <= (x + 1); j++)
                    {
                        for (int k = (y - 1); k <= (y + 1); k++)
                        {
                            if (IsWithinBounds(j,k))
                            {
                                if (j != x || k != y)
                                {
                                    if (tempMap[j, k] == 1)
                                    {
                                        neighbourWallCount++;
                                    }
                                }
                            }
                            else
                            {
                                neighbourWallCount++;
                            }
                        }
                    }

                    // update the original map with the calculated neighbours
                    if (neighbourWallCount > 4)
                    {
                        map[x, y] = 1;
                    }
                    else if (neighbourWallCount < 4)
                    {
                        map[x, y] = 0;
                    }
                }
            }
        }

        return map;
    }

    private bool IsWithinBounds(int x, int y)
    {
        if (x < 0 || x >= width)
        {
            return false;
        }

        if (y < 0 || y >= height)
        {
            return false;
        }

        return true;
    }

    // for all values 1 in the map, fill it in with a tile
    public void RenderMap(int[,] map, Tilemap groundTileMap, TileBase groundTileBase)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x,y] == 1)
                {
                    groundTileMap.SetTile(new Vector3Int(x, y, 0), groundTileBase);
                }
            }
        }
    }

    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        public Room() {}

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edgeTiles = new List<Coord>();
            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1))
                            {
                                if (map[x, y] == 1)
                                {
                                    edgeTiles.Add(tile);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMainRoom()
        {
            if (!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.isAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }

            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }
}
