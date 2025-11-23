using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject bagShopPanel;  // 가방 상점 UI
    public GameObject itemShopPanel; // 아이템 상점 UI
    public GameObject sellPanel;     // 판매 상점 UI
    
    public TextMeshProUGUI goldText; 

    public Inventory inventory; 
    private PlayerController playerController; 

    // 상점 타입 분리 (BagShop, ItemShop)
    public enum ShopType { None, BagShop, ItemShop, Sell }
    private ShopType currentZone = ShopType.None;

    void Start()
    {
        inventory = FindObjectOfType<Inventory>();
        playerController = FindObjectOfType<PlayerController>();
        
        CloseShop();
        UpdateGoldUI(); 
    }

    void UpdateGoldUI()
    {
        if (goldText != null && inventory != null)
        {
            goldText.text = $"{inventory.currentGold:N0} G"; 
        }
    }

    // --- 가격표 ---
    int GetItemPrice(GameData.ItemType type)
    {
        switch (type)
        {
            case GameData.ItemType.Dirt: return 3;
            case GameData.ItemType.Grass: return 3;
            case GameData.ItemType.Stone: return 5;
            case GameData.ItemType.Coal: return 10;
            case GameData.ItemType.Iron: return 22;
            case GameData.ItemType.Gold: return 50;
            case GameData.ItemType.Diamond: return 400;
            
            case GameData.ItemType.Wood: return 50; //나무 구매 
            case GameData.ItemType.Dynamite: return 300;
            
            case GameData.ItemType.Bag_Small: return 200;
            case GameData.ItemType.Bag_Medium: return 500;
            case GameData.ItemType.Bag_Large: return 1000;
            case GameData.ItemType.Bag_Max: return 40000;
            
            default: return 0; 
        }
    }

    // --- 통합 구매 함수 (가방 & 아이템 모두 처리) ---
    public void BuyItem(string itemTypeName) 
    {
        if (System.Enum.TryParse(itemTypeName, out GameData.ItemType type))
        {
            // 1. 가방인지 확인 -> 가방 구매 로직으로 이동
            if (IsBagItem(type))
            {
                BuyBag(type); 
                return;
            }

            // 2. 일반 아이템(나무 등) 구매 로직
            int price = GetItemPrice(type); // (구매가는 판매가와 같거나 다르게 설정 가능)
            // int price = GetItemPrice(type) * 2; 

            if (inventory.currentGold >= price)
            {
                // 인벤토리 공간 확인
                if (inventory.Add(type, 1)) 
                {
                    inventory.currentGold -= price;
                    UpdateGoldUI();
                    Debug.Log($"[Shop] {type} 구매 완료!");
                }
            }
            else
            {
                Debug.LogWarning("[Shop] 골드가 부족합니다!");
            }
        }
    }

    bool IsBagItem(GameData.ItemType type)
    {
        return type == GameData.ItemType.Bag_Small || 
               type == GameData.ItemType.Bag_Medium || 
               type == GameData.ItemType.Bag_Large ||
               type == GameData.ItemType.Bag_Max;
    }

    void BuyBag(GameData.ItemType bagType)
    {
        int price = GetItemPrice(bagType);
        int targetLevel = 0;

        if (bagType == GameData.ItemType.Bag_Small) targetLevel = 1;
        else if (bagType == GameData.ItemType.Bag_Medium) targetLevel = 2;
        else if (bagType == GameData.ItemType.Bag_Large) targetLevel = 3;
        else if (bagType == GameData.ItemType.Bag_Max) targetLevel = 4;

        if (inventory.bagLevel >= targetLevel)
        {
            Debug.LogWarning("[Shop] 이미 보유중이거나 더 좋은 가방이 있습니다.");
            return;
        }

        if (inventory.bagLevel < targetLevel - 1)
        {
            Debug.LogWarning("[Shop] 이전 단계의 가방이 필요합니다.");
            return;
        }

        if (inventory.currentGold >= price)
        {
            inventory.currentGold -= price;
            inventory.UpgradeBag(targetLevel);
            UpdateGoldUI();
        }
        else
        {
            Debug.LogWarning("[Shop] 골드가 부족합니다!");
        }
    }

    // --- 판매 로직 ---
    public void SellItem(string itemTypeName)
    {
        if (System.Enum.TryParse(itemTypeName, out GameData.ItemType type))
        {
            int price = GetItemPrice(type);
            if (price <= 0) return;

            if (inventory.Consume(type, 1))
            {
                inventory.currentGold += price; 
                UpdateGoldUI();                 
                Debug.Log($"[Shop] 판매 완료. 현재 골드: {inventory.currentGold}");
            }
        }
    }

    public void SellAllItems()
    {
        int totalEarned = 0;
        bool soldAny = false;

        foreach (var slot in inventory.slots)
        {
            if (!slot.IsEmpty)
            {
                int price = GetItemPrice(slot.itemType);
                if (price > 0)
                {
                    totalEarned += price * slot.count;
                    slot.Clear(); 
                    soldAny = true;
                }
            }
        }

        if (soldAny)
        {
            inventory.currentGold += totalEarned; 
            UpdateGoldUI();
            FindObjectOfType<InventoryUI>().UpdateInventory(inventory);
            Debug.Log($"[Shop] 싹쓸이 판매 완료! +{totalEarned} G");
        }
        else
        {
            Debug.Log("[Shop] 팔 수 있는 자원이 없습니다.");
        }
    }

    // --- 상점 UI 관리 ---
    public void EnterZone(ShopType type) { currentZone = type; OpenShop(type); }
    public void ExitZone() { currentZone = ShopType.None; CloseShop(); }
    
    private void OpenShop(ShopType type)
    {
        // 3가지 상점 타입에 맞춰 UI 켜기/끄기
        if (type == ShopType.BagShop) 
        { 
            if(bagShopPanel) bagShopPanel.SetActive(true); 
            if(itemShopPanel) itemShopPanel.SetActive(false);
            if(sellPanel) sellPanel.SetActive(false); 
        }
        else if (type == ShopType.ItemShop) 
        { 
            if(bagShopPanel) bagShopPanel.SetActive(false); 
            if(itemShopPanel) itemShopPanel.SetActive(true);
            if(sellPanel) sellPanel.SetActive(false); 
        }
        else if (type == ShopType.Sell) 
        { 
            if(bagShopPanel) bagShopPanel.SetActive(false); 
            if(itemShopPanel) itemShopPanel.SetActive(false);
            if(sellPanel) sellPanel.SetActive(true); 
        }
        UnlockCursor();
    }
    
    public void CloseShop()
    {
        if(bagShopPanel) bagShopPanel.SetActive(false); 
        if(itemShopPanel) itemShopPanel.SetActive(false); 
        if(sellPanel) sellPanel.SetActive(false);
        LockCursor();
    }
    
    void UnlockCursor() { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; if(playerController) playerController.canLook = false; }
    void LockCursor() { Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked; if(playerController) playerController.canLook = true; }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) CloseShop();
    }
}