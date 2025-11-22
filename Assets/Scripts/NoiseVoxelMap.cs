

using UnityEngine;
using System.Collections.Generic;

public class NoiseVoxelMap : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject blockPrefabDirt;
    public GameObject blockPrefabGrass;
    public GameObject blockPrefabWater;
    public GameObject blockPrefabStone; 
    public GameObject blockPrefabIron;  
    public GameObject blockPrefabGold;  
    public GameObject blockPrefabDiamond;
    public GameObject blockPrefabCoal; 

    [Header("Map Settings")]
    public int width = 40;
    public int depth = 40;
    public int mapHeight = 64; 
    public int waterLevel = 20;
    public int fixedSurfaceY = 40; 
    
    [Header("Noise Settings")]
    [SerializeField] float noiseScale = 30f;
    [SerializeField] float oreNoiseScale = 0.95f; 
    [SerializeField] float caveNoiseScale = 0.05f; 
    [SerializeField] float caveThreshold = 0.7f; 

    private Dictionary<GameData.BlockType, int> blockCounts = new Dictionary<GameData.BlockType, int>(); 

    void Start()
    {
        blockCounts.Clear(); 
        
        float offsetX = Random.Range(-9999f, 9999f);
        float offsetZ = Random.Range(-9999f, 9999f);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                int surfaceHeight = fixedSurfaceY; 

                for (int y = 0; y < mapHeight; y++)
                {
                    GameData.BlockType typeToPlace = GameData.BlockType.Air; 
                    float caveNoise = PerlinNoise3D(x * caveNoiseScale, y * caveNoiseScale, z * caveNoiseScale);
                    
                    if (y < surfaceHeight && caveNoise > caveThreshold)
                    {
                        typeToPlace = GameData.BlockType.Air; 
                    }
                    else if (y <= surfaceHeight)
                    {
                        if (y == surfaceHeight)
                        {
                            typeToPlace = GameData.BlockType.Grass; 
                        }
                        else
                        {
                            typeToPlace = GetUndergroundBlock(x, y, z, surfaceHeight);
                        }
                    }
                    else if (y <= waterLevel)
                    {
                        typeToPlace = GameData.BlockType.Water; 
                    }

                    if (typeToPlace != GameData.BlockType.Air)
                    {
                        PlaceBlockInitial(typeToPlace, x, y, z); 
                    }
                }
            }
        }
        
        DisplayBlockCounts();
    }
    
    public void PlaceTile(Vector3Int pos, GameData.BlockType type)
    {
        switch (type)
        {
            case GameData.BlockType.Dirt: 
            case GameData.BlockType.Grass: PlaceDirt(pos.x, pos.y, pos.z); break;
            case GameData.BlockType.Stone: PlaceStone(pos.x, pos.y, pos.z); break;
            case GameData.BlockType.IronOre: PlaceIronOre(pos.x, pos.y, pos.z); break;
            case GameData.BlockType.GoldOre: PlaceGoldOre(pos.x, pos.y, pos.z); break;
            case GameData.BlockType.DiamondOre: PlaceDiamond(pos.x, pos.y, pos.z); break;
            case GameData.BlockType.Water: PlaceWater(pos.x, pos.y, pos.z); break;
            case GameData.BlockType.CoalOre: PlaceCoalOre(pos.x, pos.y, pos.z); break;
        }
    }
    
    private float PerlinNoise3D(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float yz = Mathf.PerlinNoise(y, z);
        float xz = Mathf.PerlinNoise(x, z);
        return (xy + yz + xz) / 3f;
    }

    private GameData.BlockType GetUndergroundBlock(int x, int y, int z, int surfaceHeight)
    {
        if (surfaceHeight - y < 3) return GameData.BlockType.Dirt;

        float oreNoise = PerlinNoise3D(x * oreNoiseScale, y * oreNoiseScale, z * oreNoiseScale);

        if (y < 12 && oreNoise > 0.80f) return GameData.BlockType.DiamondOre; 
        if (y < 30 && oreNoise > 0.75f) return GameData.BlockType.GoldOre;
        if (y < 50 && oreNoise > 0.65f) return GameData.BlockType.IronOre;
        if (y < 60 && oreNoise > 0.60f) return GameData.BlockType.CoalOre;

        return GameData.BlockType.Stone; 
    }

    private void PlaceBlockInitial(GameData.BlockType type, int x, int y, int z)
    {
        switch (type)
        {
            case GameData.BlockType.Dirt: PlaceDirt(x, y, z); break;
            case GameData.BlockType.Grass: PlaceGrass(x, y, z); break;
            case GameData.BlockType.Stone: PlaceStone(x, y, z); break;
            case GameData.BlockType.Water: PlaceWater(x, y, z); break;
            case GameData.BlockType.IronOre: PlaceIronOre(x, y, z); break;
            case GameData.BlockType.GoldOre: PlaceGoldOre(x, y, z); break;
            case GameData.BlockType.DiamondOre: PlaceDiamond(x, y, z); break;
            case GameData.BlockType.CoalOre: PlaceCoalOre(x, y, z); break;
        }
    }
    
    private void CreateAndSetupBlock(GameObject prefab, GameData.BlockType type, int x, int y, int z)
    {
        var go = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"{type}_{x}_{y}_{z}";
        var B = go.GetComponent<Block>() ?? go.AddComponent<Block>();
        B.type = type;

        if (blockCounts.ContainsKey(B.type)) blockCounts[B.type]++; else blockCounts[B.type] = 1;
    }

    private void PlaceDirt(int x, int y, int z) => CreateAndSetupBlock(blockPrefabDirt, GameData.BlockType.Dirt, x, y, z);
    private void PlaceGrass(int x, int y, int z) => CreateAndSetupBlock(blockPrefabGrass, GameData.BlockType.Grass, x, y, z);
    private void PlaceStone(int x, int y, int z) => CreateAndSetupBlock(blockPrefabStone, GameData.BlockType.Stone, x, y, z);
    private void PlaceWater(int x, int y, int z) => CreateAndSetupBlock(blockPrefabWater, GameData.BlockType.Water, x, y, z);
    private void PlaceIronOre(int x, int y, int z) => CreateAndSetupBlock(blockPrefabIron, GameData.BlockType.IronOre, x, y, z);
    private void PlaceGoldOre(int x, int y, int z) => CreateAndSetupBlock(blockPrefabGold, GameData.BlockType.GoldOre, x, y, z);
    private void PlaceDiamond(int x, int y, int z) => CreateAndSetupBlock(blockPrefabDiamond, GameData.BlockType.DiamondOre, x, y, z);
    private void PlaceCoalOre(int x, int y, int z) => CreateAndSetupBlock(blockPrefabCoal, GameData.BlockType.CoalOre, x, y, z);

    private void DisplayBlockCounts()
    {
        Debug.Log("======================================");
        Debug.Log($" 맵 생성 완료: {width}x{depth}x{mapHeight} 크기");
        int totalBlocks = 0;
        foreach (var kvp in blockCounts)
        {
            Debug.Log($"[{kvp.Key}]: {kvp.Value}개");
            totalBlocks += kvp.Value;
        }
        Debug.Log($"총 생성된 블록: {totalBlocks}개");
        Debug.Log("======================================");
    }
}