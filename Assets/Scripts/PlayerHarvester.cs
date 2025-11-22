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

    // (기존 함수 유지)
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
    
    private bool CanHarvest(GameData.BlockType blockType, GameData.ItemType toolType)
    {
        if (toolType == GameData.ItemType.None)
        {
            if (blockType == GameData.BlockType.Stone || 
                blockType == GameData.BlockType.IronOre || 
                blockType == GameData.BlockType.GoldOre || 
                blockType == GameData.BlockType.DiamondOre) return false; 
        }
        return true;
    }

    void Update()
    {
        if (invenUI == null) return; 

        // 1. 아이템 버리기 진단
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("[DEBUG] Q키 입력 감지됨. 버리기 시도...");
            ThrowCurrentItem();
        }

        // 2. 수확 모드
        if (invenUI.selectedIndex < 0 || Input.GetMouseButton(0))
        {
            if (invenUI.selectedIndex < 0 && selectedBlock != null) 
                selectedBlock.transform.localScale = Vector3.zero;

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
        }
        
        // 3. 설치 모드 진단
        if (invenUI.selectedIndex >= 0)
        {
            Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            
            if (Physics.Raycast(ray, out var hit, rayDistance, hitMask, QueryTriggerInteraction.Ignore))
            {
                Vector3Int placePos = AdjacentCellOnHitFace(hit);
                
                if (selectedBlock != null)
                {
                    selectedBlock.transform.localScale = Vector3.one;
                    selectedBlock.transform.position = placePos;
                    selectedBlock.transform.rotation = Quaternion.identity;
                }

                if (Input.GetMouseButtonDown(1)) 
                {
                    // 설치 실패 원인 추적
                    GameData.BlockType selectedBlockType = invenUI.GetInventorySlot();
                    GameData.ItemType itemToConsume = (GameData.ItemType)selectedBlockType; 
                    
                    int count = inventory.GetItemCount(itemToConsume);

                    Debug.Log($"[DEBUG] 설치 시도! 선택된 블록: {selectedBlockType} -> 변환된 아이템: {itemToConsume}");
                    Debug.Log($"[DEBUG] 인벤토리 보유량: {count}개");

                    if (itemToConsume == GameData.ItemType.StonePickaxe || 
                        itemToConsume == GameData.ItemType.IronPickaxe || 
                        itemToConsume == GameData.ItemType.GoldPickaxe) 
                    {
                        Debug.LogWarning("[DEBUG] 도구는 설치할 수 없습니다.");
                        return; 
                    }

                    if (inventory.Consume(itemToConsume, 1))
                    {
                        FindObjectOfType<NoiseVoxelMap>().PlaceTile(placePos, selectedBlockType);
                        Debug.Log("[DEBUG] 설치 성공!");
                    }
                    else
                    {
                        Debug.LogError("[DEBUG] 설치 실패! 인벤토리 소모 실패 (아이템 부족 또는 불일치)");
                    }
                }
            }
            else
            {
                if (selectedBlock != null) selectedBlock.transform.localScale = Vector3.zero;
            }
        }
    }
    
    void ThrowCurrentItem()
    {
        GameData.ItemType currentItem = invenUI.GetSelectedItemType();
        Debug.Log($"[DEBUG] 버리기 시도 아이템: {currentItem}");

        if (currentItem == GameData.ItemType.None) 
        {
            Debug.LogWarning("[DEBUG] 손에 든 아이템이 없습니다 (None). 슬롯 선택 상태를 확인하세요.");
            return;
        }

        if (inventory.Consume(currentItem, 1))
        {
            GameObject prefabToSpawn = GetDropPrefab(currentItem);
            if (prefabToSpawn == null)
            {
                Debug.LogError($"[DEBUG] {currentItem}의 드롭 프리팹이 리스트에 없습니다! Inspector를 확인하세요.");
                return;
            }

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

            if (drop.GetComponent<Collider>() == null)
            {
                drop.AddComponent<BoxCollider>();
            }

            rb.velocity = Vector3.zero; 
            Vector3 throwDirection = (_cam.transform.forward + Vector3.up * 0.2f).normalized;
            rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
            
            Debug.Log("[DEBUG] 버리기 성공!");
        }
        else
        {
            Debug.LogError("[DEBUG] 버리기 실패! 인벤토리에 아이템이 부족합니다.");
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