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
    public int fixedSurfaceY = 40; // 평평한 지표면 Y축 높이

    [Header("Noise Settings")]
    [SerializeField] float noiseScale = 30f;
    [SerializeField] float oreNoiseScale = 0.4f; // 광물 분산도 증가
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
                        typeToPlace = BlockType.Water;
                    }

                    if (typeToPlace != BlockType.Air)
                    {
                        // 맵 생성 시에는 PlaceBlock 대신 Place* 함수를 직접 호출해야 함
                        PlaceBlockInitial(typeToPlace, x, y, z);
                    }
                }
            }
        }

        DisplayBlockCounts();
    }

    // 블록 설치 요청 처리 메서드 (PlayerHarvester에서 호출됨)
    public void PlaceTile(Vector3Int pos, BlockType type)
    {
        // 설치 위치에 이미 블록이 있는지 확인하는 로직은 여기에 추가할 수 있습니다.

        switch (type)
        {
            case BlockType.Dirt:
            case BlockType.Grass: // Grass 아이템으로도 Dirt 블록 설치 가능
                PlaceDirt(pos.x, pos.y, pos.z);
                break;
            case BlockType.Stone:
                PlaceStone(pos.x, pos.y, pos.z);
                break;
            case BlockType.IronOre: // Iron 아이템으로 IronOre 블록 설치한다고 가정
                PlaceIronOre(pos.x, pos.y, pos.z);
                break;
            case BlockType.Water:
                PlaceWater(pos.x, pos.y, pos.z);
                break;
            case BlockType.DiamondOre:
                PlaceDiamond(pos.x, pos.y, pos.z);
                break;
                // 다른 광물 타입 설치 로직 추가 필요
        }
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
        if (surfaceHeight - y < 3)
        {
            return BlockType.Dirt;
        }

        if (y > 30)
        {
            return BlockType.Stone;
        }

        if (y > 10)
        {
            float oreNoise = PerlinNoise3D(x * oreNoiseScale, y * oreNoiseScale, z * oreNoiseScale);
            if (oreNoise > 0.65f) return BlockType.IronOre;

            return BlockType.Stone;
        }

        {
            float oreNoise = PerlinNoise3D(x * oreNoiseScale, y * oreNoiseScale, z * oreNoiseScale);

            if (y < 5 && oreNoise > 0.85f) return BlockType.DiamondOre;
            if (y < 10 && oreNoise > 0.75f) return BlockType.GoldOre;
            if (oreNoise > 0.6f) return BlockType.IronOre;

            return BlockType.Stone;
        }
    }

    // 맵 생성 초기화 시 블록을 생성하는 헬퍼 함수
    private void PlaceBlockInitial(BlockType type, int x, int y, int z)
    {
        switch (type)
        {
            case BlockType.Dirt: PlaceDirt(x, y, z); break;
            case BlockType.Grass: PlaceGrass(x, y, z); break;
            case BlockType.Stone: PlaceStone(x, y, z); break;
            case BlockType.Water: PlaceWater(x, y, z); break;
            case BlockType.IronOre: PlaceIronOre(x, y, z); break;
            case BlockType.GoldOre: PlaceGoldOre(x, y, z); break;
            case BlockType.DiamondOre: PlaceDiamond(x, y, z); break;
        }
    }

    // 블록 생성 상세 로직 (PlaceTile에서 호출됨)

    private void CreateAndSetupBlock(GameObject prefab, BlockType type, int x, int y, int z)
    {
        var go = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"{type}_{x}_{y}_{z}";

        var B = go.GetComponent<Block>() ?? go.AddComponent<Block>();
        B.type = type;

        // 카운트 업데이트
        if (blockCounts.ContainsKey(B.type)) blockCounts[B.type]++; else blockCounts[B.type] = 1;
    }

    private void PlaceDirt(int x, int y, int z) => CreateAndSetupBlock(blockPrefabDirt, BlockType.Dirt, x, y, z);
    private void PlaceGrass(int x, int y, int z) => CreateAndSetupBlock(blockPrefabGrass, BlockType.Grass, x, y, z);
    private void PlaceStone(int x, int y, int z) => CreateAndSetupBlock(blockPrefabStone, BlockType.Stone, x, y, z);
    private void PlaceWater(int x, int y, int z) => CreateAndSetupBlock(blockPrefabWater, BlockType.Water, x, y, z);
    private void PlaceIronOre(int x, int y, int z) => CreateAndSetupBlock(blockPrefabIron, BlockType.IronOre, x, y, z);
    private void PlaceGoldOre(int x, int y, int z) => CreateAndSetupBlock(blockPrefabGold, BlockType.GoldOre, x, y, z);
    private void PlaceDiamond(int x, int y, int z) => CreateAndSetupBlock(blockPrefabDiamond, BlockType.DiamondOre, x, y, z);


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
}