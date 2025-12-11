using UnityEngine;
using System.Collections.Generic;
using System.Linq; 

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
            case GameData.ItemType.DiamondPickaxe: return 10.0f;
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
            case GameData.ItemType.DiamondPickaxe: toolLevel = 4; break;
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
            case GameData.BlockType.Obsidian:
                requiredLevel = 4;
                break;
        }
        // 도구 레벨이 요구 레벨보다 크거나 같아야 함
        if (toolLevel >= requiredLevel)
        {
            return true;
        }
        else
        {
            // (선택) 화면에 메시지 띄우기: "이 블록은 더 강한 도구가 필요합니다!"
            return false;
        }
    }

    //  나를 제외하고 가장 가까운 물체를 찾는 함수
    RaycastHit? GetValidHit()
    {
        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // 1. 경로상의 모든 물체를 다 가져옴 (RaycastAll)
        RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance, hitMask, QueryTriggerInteraction.Ignore);

        // 2. 거리순으로 정렬 (가까운 게 먼저 오도록)
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // 3. 순서대로 검사하며 '나'는 무시
        foreach (var hit in hits)
        {
            // 내 몸통(Collider)이거나 Player 태그면 무시하고 다음 물체 검사
            if (hit.collider.gameObject == gameObject || hit.collider.CompareTag("Player"))
                continue;

            return hit; // 유효한 첫 번째 물체 반환
        }

        return null; // 아무것도 없으면 null
    }

    void Update()
    {
        if (invenUI == null) return;

        // Q키: 아이템 버리기
        if (Input.GetKeyDown(KeyCode.Q)) ThrowCurrentItem();

        // ================================================================
        // 2. 우클릭 상호작용 (버튼 누르기 & 블록 설치 & 포탈 생성)
        // ================================================================
        if (Input.GetMouseButtonDown(1))
        {
            // [수정] Raycast 대신 GetValidHit 사용
            RaycastHit? hitInfo = GetValidHit();

            if (hitInfo.HasValue) // 뭔가 맞았다면
            {
                RaycastHit hit = hitInfo.Value;

                // [우선순위 1] 리셋 버튼인지 확인
                ResetButton button = hit.collider.GetComponent<ResetButton>();
                if (button != null)
                {
                    button.Press();
                    return;
                }

                // [우선순위 2] 아이템 사용 / 블록 설치
                if (invenUI.selectedIndex >= 0)
                {
                    Vector3Int placePos = AdjacentCellOnHitFace(hit);
                    GameData.BlockType selectedBlockType = invenUI.GetInventorySlot();
                    GameData.ItemType currentItem = (GameData.ItemType)selectedBlockType;

                    // 라이터 + 흑요석 = 포탈 생성
                    if (currentItem == GameData.ItemType.Lighter)
                    {
                        Block hitBlock = hit.collider.GetComponent<Block>();
                        if (hitBlock != null && hitBlock.type == GameData.BlockType.Obsidian)
                        {
                            Debug.Log("포탈이 생성되었습니다.");
                            FindObjectOfType<NoiseVoxelMap>().PlaceTile(placePos, GameData.BlockType.Portal);
                            return;
                        }
                    }

                    // 도구 설치 방지
                    if (currentItem == GameData.ItemType.StonePickaxe ||
                        currentItem == GameData.ItemType.IronPickaxe ||
                        currentItem == GameData.ItemType.GoldPickaxe ||
                        currentItem == GameData.ItemType.Lighter) return;

                    // 블록 설치
                    if (inventory.Consume(currentItem, 1))
                    {
                        FindObjectOfType<NoiseVoxelMap>().PlaceTile(placePos, selectedBlockType);
                    }
                }
            }
        }

        // ================================================================
        // 3. 좌클릭 상호작용 (채집)
        // ================================================================
        if (Input.GetMouseButton(0) && Time.time >= _nextHitTime)
        {
            _nextHitTime = Time.time + hitCooldown;

            //  [수정] 채집할 때도 내 몸 뚫고 클릭되게 변경
            RaycastHit? hitInfo = GetValidHit();

            if (hitInfo.HasValue)
            {
                var block = hitInfo.Value.collider.GetComponent<Block>();
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

        // ================================================================
        // 4. 미리보기 업데이트
        // ================================================================
        if (invenUI.selectedIndex >= 0)
        {
            //  [수정] 미리보기에서도 적용
            RaycastHit? hitInfo = GetValidHit();

            if (hitInfo.HasValue)
            {
                if (selectedBlock != null)
                {
                    selectedBlock.transform.localScale = Vector3.one;
                    selectedBlock.transform.position = AdjacentCellOnHitFace(hitInfo.Value);
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