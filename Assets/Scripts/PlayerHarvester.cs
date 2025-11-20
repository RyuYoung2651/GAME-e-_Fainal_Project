using UnityEngine;
using static BlockTypeScript;
using static ItemTypeScript;

public class PlayerHarvester : MonoBehaviour
{
    [Header("Harvesting")]
    public float rayDistance = 5f;
    public LayerMask hitMask = ~0;
    public ItemType currentTool = ItemType.None; // 초기 도구를 맨손(None)으로 설정

    private float hitCooldwon = 0.15f;
    private float _nextHitTime;
    private Camera _cam;
    public Inventory inventory;

    //블록 설치
    public InventoryUI invenUI; 
    public GameObject selectedBlock;

    void Awake()
    {
        _cam = Camera.main;
        if (inventory == null) inventory = gameObject.AddComponent<Inventory>();
        invenUI = FindObjectOfType<InventoryUI>();
    }

    private float GetToolDamage()
    {
        switch (currentTool)
        {
            case ItemType.StonePickaxe:
                return 5.0f;
            case ItemType.IronPickaxe:
                return 10.0f;
            case ItemType.GoldPickaxe:
                return 15.0f;
            case ItemType.None:
            default:
                return 1.0f; // 맨손 데미지
        }
    }

    // 맨손 채집 불가 로직
    private bool CanHarvest(BlockType blockType, ItemType toolType)
    {
        if (toolType == ItemType.None)
        {
            // Stone 이상의 블록은 맨손으로 채집 불가
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
        if (Input.GetMouseButton(0) && Time.time >= _nextHitTime)
        {
            _nextHitTime = Time.time + hitCooldwon;

            Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out var hit, rayDistance, hitMask))
            {
                var block = hit.collider.GetComponent<Block>();
                if (block != null)
                {
                    block.Hit(GetToolDamage());
                }
            }
        }
    }
}