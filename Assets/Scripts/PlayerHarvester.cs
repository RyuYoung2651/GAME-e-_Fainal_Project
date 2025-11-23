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

    // 1. 도구 데미지 설정 (높을수록 빨리 캠)
    private float GetToolDamage()
    {
        GameData.ItemType tool = GameData.ItemType.None;
        if (invenUI != null) tool = invenUI.GetSelectedItemType();
        
        switch (tool)
        {
            case GameData.ItemType.StonePickaxe: return 2.0f;
            case GameData.ItemType.IronPickaxe: return 3.0f;
            case GameData.ItemType.GoldPickaxe: return 5.0f;
            case GameData.ItemType.Diamond: return 10.0f; // 다이아몬드 도구 (가정)
            default: return 1.0f; // 맨손 데미지
        }
    }
    
    // 도구 등급별 채집 가능 여부 확인
    private bool CanHarvest(GameData.BlockType blockType, GameData.ItemType toolType)
    {
        // 1. 도구의 등급(Level) 확인
        int toolLevel = 0; // 기본(맨손) = 0
        switch (toolType)
        {
            case GameData.ItemType.StonePickaxe: toolLevel = 1; break;
            case GameData.ItemType.IronPickaxe:  toolLevel = 2; break;
            case GameData.ItemType.GoldPickaxe:  toolLevel = 3; break;
            case GameData.ItemType.Diamond:      toolLevel = 4; break; // 다이아 도구
        }

        // 2. 블록의 요구 등급(Required Level) 확인
        int requiredLevel = 0; // 기본(흙, 풀, 돌) = 0
        switch (blockType)
        {
            // 0레벨 (맨손 가능)
            case GameData.BlockType.Dirt:
            case GameData.BlockType.Grass:
            case GameData.BlockType.Stone:
                requiredLevel = 0; 
                break;

            // 1레벨 (돌 곡괭이 이상 필요)
            case GameData.BlockType.CoalOre:
            case GameData.BlockType.IronOre:
                requiredLevel = 1;
                break;

            // 2레벨 (철 곡괭이 이상 필요)
            case GameData.BlockType.GoldOre:
                requiredLevel = 2;
                break;

            // 3레벨 (금/다이아 곡괭이 이상 필요)
            case GameData.BlockType.DiamondOre:
                requiredLevel = 3;
                break;
        }

        // 3. 도구 레벨이 요구 레벨보다 크거나 같아야 캘 수 있음
        if (toolLevel >= requiredLevel)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void Update()
    {
        if (invenUI == null) return; 

        // Q키: 아이템 버리기
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ThrowCurrentItem();
        }

        // 1. 수확 모드 (왼쪽 클릭)
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
                         
                         // 위에서 만든 등급 체크 함수 호출
                         if (CanHarvest(block.type, currentTool))
                         {
                            block.Hit(GetToolDamage());
                         }
                     }
                }
            }
        }
        
        // 2. 설치 모드 (오른쪽 클릭)
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
            else
            {
                if (selectedBlock != null) selectedBlock.transform.localScale = Vector3.zero;
            }
        }
    }
    
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

            if (drop.GetComponent<Collider>() == null)
            {
                drop.AddComponent<BoxCollider>();
            }

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