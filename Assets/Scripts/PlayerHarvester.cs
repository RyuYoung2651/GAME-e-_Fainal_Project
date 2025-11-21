using UnityEngine;
using static BlockTypeScript;
using static ItemTypeScript;

public class PlayerHarvester : MonoBehaviour
{
    [Header("Harvesting")]
    public float rayDistance = 5f;
    public LayerMask hitMask = ~0;
    public float hitCooldown = 0.15f;

    private float _nextHitTime;
    private Camera _cam;
    public Inventory inventory;

    InventoryUI invenUI;
    public GameObject selectedBlock;

    void Awake()
    {
        _cam = Camera.main;
        if (inventory == null) inventory = gameObject.AddComponent<Inventory>();
        invenUI = FindObjectOfType<InventoryUI>();
    }

    private float GetToolDamage()
    {
        ItemType tool = ItemType.None;

        switch (tool)
        {
            case ItemType.StonePickaxe: return 5.0f;
            case ItemType.IronPickaxe: return 10.0f;
            case ItemType.GoldPickaxe: return 15.0f;
            default: return 1.0f;
        }
    }

    private bool CanHarvest(BlockType blockType, ItemType toolType)
    {
        if (toolType == ItemType.None)
        {
            if (blockType == BlockType.Stone ||
                blockType == BlockType.IronOre ||
                blockType == BlockType.GoldOre ||
                blockType == BlockType.DiamondOre)
            {
                return false;
            }
        }
        return true;
    }

    void Update()
    {
        // 선택된 인덱스에 따라 모드 전환
        if (invenUI != null && invenUI.selectedIndex < 0)
        {
            // 수확 모드 (왼쪽 클릭)
            if (Input.GetMouseButton(0) && Time.time >= _nextHitTime)
            {
                _nextHitTime = Time.time + hitCooldown;
                Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

                if (Physics.Raycast(ray, out var hit, rayDistance, hitMask, QueryTriggerInteraction.Ignore))
                {
                    var block = hit.collider.GetComponent<Block>();
                    if (block != null)
                    {
                        ItemType currentTool = ItemType.None;
                        if (CanHarvest(block.type, currentTool))
                        {
                            block.Hit(GetToolDamage());
                        }
                    }
                }
            }
        }
        else // 설치 모드
        {
            if (Input.GetMouseButtonDown(1)) // 마우스 오른쪽 버튼
            {
                Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

                if (Physics.Raycast(ray, out var hit, rayDistance, hitMask, QueryTriggerInteraction.Ignore))
                {
                    Vector3Int placePos = AdjacentCellOnHitFace(hit);

                    // 1. BlockType을 가져옴
                    BlockType selectedBlockType = invenUI.GetInventorySlot();

                    // 2. BlockType을 ItemType으로 명시적 변환 (CS1503 해결)
                    ItemType itemToConsume = (ItemType)selectedBlockType;

                    // 3. ItemType을 Inventory.Consume에 전달 (인벤토리 소모)
                    if (inventory.Consume(itemToConsume, 1))
                    {
                        // 4. BlockType을 NoiseVoxelMap.PlaceTile에 전달 (블록 설치)
                        FindObjectOfType<NoiseVoxelMap>().PlaceTile(placePos, selectedBlockType);
                    }
                }
            }
        }
    }

    static Vector3Int AdjacentCellOnHitFace(in RaycastHit hit)
    {
        Vector3 baseCenter = hit.collider.transform.position;
        Vector3 adjCenter = baseCenter + hit.normal;
        return Vector3Int.RoundToInt(adjCenter);
    }
}