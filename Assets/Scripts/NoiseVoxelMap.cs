using UnityEngine;
using System.Collections.Generic;
using static BlockTypeScript;

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

    [Header("Map Settings")]
    public int width = 40;
    public int depth = 40;
    public int mapHeight = 64;
    public int waterLevel = 20;

    [Header("Surface Settings")]
    public int fixedSurfaceY = 40; //고정된 지표면 Y축 높이

    [Header("Noise Settings")]
    [SerializeField] float noiseScale = 30f;
    [SerializeField] float oreNoiseScale = 0.8f; // 광물 분산도 증가
    [SerializeField] float caveNoiseScale = 0.05f;
    [SerializeField] float caveThreshold = 0.7f;

    private Dictionary<BlockType, int> blockCounts = new Dictionary<BlockType, int>();

    void Start()
    {
        blockCounts.Clear();

        float offsetX = Random.Range(-9999f, 9999f);
        float offsetZ = Random.Range(-9999f, 9999f);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // 지형 높이를 고정값으로 설정하여 평평한 지표면 생성
                int surfaceHeight = fixedSurfaceY;

                for (int y = 0; y < mapHeight; y++)
                {
                    BlockType typeToPlace = BlockType.Air;

                    // 동굴 시스템 
                    float caveNoise = PerlinNoise3D(x * caveNoiseScale, y * caveNoiseScale, z * caveNoiseScale);
                    if (y < surfaceHeight && caveNoise > caveThreshold)
                    {
                        typeToPlace = BlockType.Air;
                    }
                    else if (y <= surfaceHeight)
                    {
                        // 지표면 및 지하 블록 결정
                        if (y == surfaceHeight)
                        {
                            typeToPlace = BlockType.Grass;
                        }
                        else
                        {
                            typeToPlace = GetUndergroundBlock(x, y, z, surfaceHeight);
                        }
                    }
                    else if (y <= waterLevel)
                    {
                        // 물 채우기
                        typeToPlace = BlockType.Water;
                    }

                    if (typeToPlace != BlockType.Air)
                    {
                        PlaceBlock(typeToPlace, x, y, z);
                    }
                }
            }
        }

        DisplayBlockCounts();
    }

    // 3D Perlin Noise (시뮬레이션)
    private float PerlinNoise3D(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float yz = Mathf.PerlinNoise(y, z);
        float xz = Mathf.PerlinNoise(x, z);
        return (xy + yz + xz) / 3f;
    }

    // 지하 블록 및 광물 결정 로직 (수직 깊이 기반)
    private BlockType GetUndergroundBlock(int x, int y, int z, int surfaceHeight)
    {
        // 지표면 아래 3칸까지는 흙 레이어
        if (surfaceHeight - y < 3)
        {
            return BlockType.Dirt;
        }

        // Y축 30 초과 구간은 Stone만 생성
        if (y > 30)
        {
            return BlockType.Stone;
        }

        // Y축 10 초과 ~ 30 이하 구간
        if (y > 10)
        {
            float oreNoise = PerlinNoise3D(x * oreNoiseScale, y * oreNoiseScale, z * oreNoiseScale);
            if (oreNoise > 0.65f) return BlockType.IronOre;

            return BlockType.Stone;
        }

        // Y축 10 이하 (매우 깊은 영역)
        {
            float oreNoise = PerlinNoise3D(x * oreNoiseScale, y * oreNoiseScale, z * oreNoiseScale);

            if (y < 5 && oreNoise > 0.85f) return BlockType.DiamondOre;
            if (y < 10 && oreNoise > 0.75f) return BlockType.GoldOre;
            if (oreNoise > 0.6f) return BlockType.IronOre;

            return BlockType.Stone;
        }
    }


    private void PlaceBlock(BlockType type, int x, int y, int z)
    {
        GameObject prefab = GetPrefab(type);
        if (prefab == null) return;

        var go = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"{type}_{x}_{y}_{z}";

        var B = go.GetComponent<Block>() ?? go.AddComponent<Block>();
        B.type = type;

        if (blockCounts.ContainsKey(type))
        {
            blockCounts[type]++;
        }
        else
        {
            blockCounts[type] = 1;
        }
    }

    private void DisplayBlockCounts()
    {
        Debug.Log("======================================");
        Debug.Log($"맵 생성 완료: {width}x{depth}x{mapHeight} 크기");

        int totalBlocks = 0;

        foreach (var kvp in blockCounts)
        {
            Debug.Log($"[{kvp.Key.ToString()}]: {kvp.Value}개");
            totalBlocks += kvp.Value;
        }

        Debug.Log($"총 생성된 블록: {totalBlocks}개");
        Debug.Log("======================================");
    }

    private GameObject GetPrefab(BlockType type)
    {
        return type switch
        {
            BlockType.Dirt => blockPrefabDirt,
            BlockType.Grass => blockPrefabGrass,
            BlockType.Water => blockPrefabWater,
            BlockType.Stone => blockPrefabStone,
            BlockType.IronOre => blockPrefabIron,
            BlockType.GoldOre => blockPrefabGold,
            BlockType.DiamondOre => blockPrefabDiamond,
            _ => null,
        };
    }
}