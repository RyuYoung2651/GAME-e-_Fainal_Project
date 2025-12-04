using UnityEngine;
using System.Collections.Generic;

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

    // 아이템 드롭 연결 구조체
    [System.Serializable]
    public struct ItemDropLink
    {
        public GameData.ItemType itemType;
        public GameObject dropPrefab;
    }

    [Header("Drop System")]
    public List<ItemDropLink> dropItemsList;
    public float throwForce = 8f;

    void Awake()
    {
        _cam = Camera.main;
        if (inventory == null) inventory = gameObject.AddComponent<Inventory>();
        invenUI = FindObjectOfType<InventoryUI>();
    }

    // 도구 데미지 계산
    private float GetToolDamage()
    {
        GameData.ItemType tool = GameData.ItemType.None;
        if (invenUI != null) tool = invenUI.GetSelectedItemType();

        switch (tool)
        {
            case GameData.ItemType.StonePickaxe: return 2.0f;
            case GameData.ItemType.IronPickaxe: return 3.0f;
            case GameData.ItemType.GoldPickaxe: return 5.0f;
            case GameData.ItemType.Diamond: return 10.0f;
            default: return 1.0f;
        }
    }

    // 채집 가능 여부 확인
    private bool CanHarvest(GameData.BlockType blockType, GameData.ItemType toolType)
    {
        int toolLevel = 0;
        switch (toolType)
        {
            case GameData.ItemType.StonePickaxe: toolLevel = 1; break;
            case GameData.ItemType.IronPickaxe: toolLevel = 2; break;
            case GameData.ItemType.GoldPickaxe: toolLevel = 3; break;
            case GameData.ItemType.Diamond: toolLevel = 4; break;
        }

        int requiredLevel = 0;
        switch (blockType)
        {
            case GameData.BlockType.Dirt:
            case GameData.BlockType.Grass:
            case GameData.BlockType.Stone:
                requiredLevel = 0;
                break;
            case GameData.BlockType.CoalOre:
            case GameData.BlockType.IronOre:
                requiredLevel = 1;
                break;
            case GameData.BlockType.GoldOre:
                requiredLevel = 2;
                break;
            case GameData.BlockType.DiamondOre:
                requiredLevel = 3;
                break;
        }

        return toolLevel >= requiredLevel;
    }

    void Update()
    {
        if (invenUI == null) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ThrowCurrentItem();
        }

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out var hit, rayDistance, hitMask, QueryTriggerInteraction.Ignore))
            {
                // [우선순위 1] 리셋 버튼인지 확인 (맨손이어도 작동해야 함)
                ResetButton button = hit.collider.GetComponent<ResetButton>();
                if (button != null)
                {
                    button.Press();
                    return; // 버튼을 눌렀으면 여기서 끝! (설치 로직 실행 안 함)
                }

                // [우선순위 2] 블록 설치 (아이템을 들고 있을 때만)
                if (invenUI.selectedIndex >= 0)
                {
                    Vector3Int placePos = AdjacentCellOnHitFace(hit);
                    GameData.BlockType selectedBlockType = invenUI.GetInventorySlot();
                    GameData.ItemType itemToConsume = (GameData.ItemType)selectedBlockType;

                    // 도구는 설치 불가
                    if (itemToConsume == GameData.ItemType.StonePickaxe ||
                        itemToConsume == GameData.ItemType.IronPickaxe ||
                        itemToConsume == GameData.ItemType.GoldPickaxe) return;

                    if (inventory.Consume(itemToConsume, 1))
                    {
                        FindObjectOfType<NoiseVoxelMap>().PlaceTile(placePos, selectedBlockType);
                    }
                }
            }
        }

        if (Input.GetMouseButton(0) && Time.time >= _nextHitTime)
        {
            _nextHitTime = Time.time + hitCooldown;
            Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out var hit, rayDistance, hitMask, QueryTriggerInteraction.Ignore))
            {
                var block = hit.collider.GetComponent<Block>();
                if (block != null)
                {
                    GameData.ItemType currentTool = invenUI.GetSelectedItemType();
                    if (CanHarvest(block.type, currentTool))
                    {
                        block.Hit((int)GetToolDamage());
                    }
                }
            }
        }
        
        if (invenUI.selectedIndex >= 0)
        {
            Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out var hit, rayDistance, hitMask, QueryTriggerInteraction.Ignore))
            {
                if (selectedBlock != null)
                {
                    selectedBlock.transform.localScale = Vector3.one;
                    selectedBlock.transform.position = AdjacentCellOnHitFace(hit);
                    selectedBlock.transform.rotation = Quaternion.identity;
                }
            }
            else
            {
                if (selectedBlock != null) selectedBlock.transform.localScale = Vector3.zero;
            }
        }
        else
        {
            if (selectedBlock != null) selectedBlock.transform.localScale = Vector3.zero;
        }
    }

    // 아이템 버리기 함수
    void ThrowCurrentItem()
    {
        GameData.ItemType currentItem = invenUI.GetSelectedItemType();
        if (currentItem == GameData.ItemType.None) return;

        if (inventory.Consume(currentItem, 1))
        {
            GameObject prefabToSpawn = GetDropPrefab(currentItem);
            if (prefabToSpawn == null) return;

            Vector3 spawnPos = _cam.transform.position + _cam.transform.forward * 0.5f;
            GameObject drop = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

            var itemDrop = drop.GetComponent<ItemDrop>();
            if (itemDrop == null) itemDrop = drop.AddComponent<ItemDrop>();

            itemDrop.type = currentItem;
            itemDrop.count = 1;
            itemDrop.pickupDelay = 1.5f;

            Rigidbody rb = drop.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = drop.AddComponent<Rigidbody>();
                rb.useGravity = true;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }

            if (drop.GetComponent<Collider>() == null) drop.AddComponent<BoxCollider>();

            rb.velocity = Vector3.zero;
            Vector3 throwDirection = (_cam.transform.forward + Vector3.up * 0.2f).normalized;
            rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }
    }

    GameObject GetDropPrefab(GameData.ItemType type)
    {
        foreach (var link in dropItemsList)
        {
            if (link.itemType == type) return link.dropPrefab;
        }
        return null;
    }

    static Vector3Int AdjacentCellOnHitFace(in RaycastHit hit)
    {
        Vector3 baseCenter = hit.collider.transform.position;
        Vector3 adjCenter = baseCenter + hit.normal;
        return Vector3Int.RoundToInt(adjCenter);
    }
}