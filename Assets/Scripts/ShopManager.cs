using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject bagShopPanel;
    public GameObject itemShopPanel;
    public GameObject sellPanel;

    public TextMeshProUGUI goldText;

    public Inventory inventory;
    private PlayerController playerController;

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

    // =================================================================
    //  1. 구매 가격 (살 때 가격) - 나무, 다이너마이트 포함
    // =================================================================
    int GetBuyPrice(GameData.ItemType type)
    {
        switch (type)
        {
            // 기본 블록
            case GameData.ItemType.Dirt: return 10;
            case GameData.ItemType.Grass: return 10;
            case GameData.ItemType.Stone: return 20;

            // 재료/도구 재료 (구매용)
            case GameData.ItemType.Wood: return 50;
            case GameData.ItemType.Flint: return 100;
            case GameData.ItemType.Obsidian: return 1000;

            // 특수 아이템
            case GameData.ItemType.Dynamite: return 300;

            // 가방
            case GameData.ItemType.Bag_Small: return 500;
            case GameData.ItemType.Bag_Medium: return 2000;
            case GameData.ItemType.Bag_Large: return 5000;
            case GameData.ItemType.Bag_Max: return 10000;

            default: return 0;
        }
    }

    // =================================================================
    //  2. 판매 가격 (팔 때 가격) - 광물만 가격 있음! 나머지는 0원
    // =================================================================
    int GetSellPrice(GameData.ItemType type)
    {
        switch (type)
        {
            // 돈이 되는 광물들
            case GameData.ItemType.Dirt: return 5;
            case GameData.ItemType.Grass: return 5;
            case GameData.ItemType.Stone: return 10;
            case GameData.ItemType.Coal: return 30;
            case GameData.ItemType.Iron: return 50;
            case GameData.ItemType.Gold: return 100;
            case GameData.ItemType.Diamond: return 500;

            //  나무, 다이너마이트, 도구 등은 여기에 없으므로 
            // default인 0원을 반환 -> 판매 불가!

            default: return 0;
        }
    }

    // --- 구매 로직 ---
    public void BuyItem(string itemTypeName)
    {
        if (System.Enum.TryParse(itemTypeName, out GameData.ItemType type))
        {
            if (IsBagItem(type))
            {
                BuyBag(type);
                return;
            }

            // 구매할 때는 GetBuyPrice 사용
            int price = GetBuyPrice(type);

            if (inventory.currentGold >= price)
            {
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
        int price = GetBuyPrice(bagType); // 구매가 사용
        int targetLevel = 0;

        if (bagType == GameData.ItemType.Bag_Small) targetLevel = 1;
        else if (bagType == GameData.ItemType.Bag_Medium) targetLevel = 2;
        else if (bagType == GameData.ItemType.Bag_Large) targetLevel = 3;
        else if (bagType == GameData.ItemType.Bag_Max) targetLevel = 4;

        if (inventory.bagLevel >= targetLevel) return;
        if (inventory.bagLevel < targetLevel - 1) return;

        if (inventory.currentGold >= price)
        {
            inventory.currentGold -= price;
            inventory.UpgradeBag(targetLevel);
            UpdateGoldUI();
        }
    }

    // --- 판매 로직 ---
    public void SellItem(string itemTypeName)
    {
        if (System.Enum.TryParse(itemTypeName, out GameData.ItemType type))
        {
            //  판매할 때는 GetSellPrice 사용
            int price = GetSellPrice(type);

            // 가격이 0원이면 판매 불가
            if (price <= 0) return;

            if (inventory.Consume(type, 1))
            {
                inventory.currentGold += price;
                UpdateGoldUI();
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
                //  판매할 때는 GetSellPrice 사용
                int price = GetSellPrice(slot.itemType);

                // 가격이 0원보다 커야만 팜 (나무, 다이너마이트는 여기서 걸러짐)
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
            Debug.Log($"[Shop]판매 완료! +{totalEarned} G");
        }
    }

    // --- 상점 UI 관리 ---
    public void EnterZone(ShopType type) { currentZone = type; OpenShop(type); }
    public void ExitZone() { currentZone = ShopType.None; CloseShop(); }

    private void OpenShop(ShopType type)
    {
        if (type == ShopType.BagShop)
        {
            if (bagShopPanel) bagShopPanel.SetActive(true);
            if (itemShopPanel) itemShopPanel.SetActive(false);
            if (sellPanel) sellPanel.SetActive(false);
        }
        else if (type == ShopType.ItemShop)
        {
            if (bagShopPanel) bagShopPanel.SetActive(false);
            if (itemShopPanel) itemShopPanel.SetActive(true);
            if (sellPanel) sellPanel.SetActive(false);
        }
        else if (type == ShopType.Sell)
        {
            if (bagShopPanel) bagShopPanel.SetActive(false);
            if (itemShopPanel) itemShopPanel.SetActive(false);
            if (sellPanel) sellPanel.SetActive(true);
        }
        UnlockCursor();
    }

    public void CloseShop()
    {
        if (bagShopPanel) bagShopPanel.SetActive(false);
        if (itemShopPanel) itemShopPanel.SetActive(false);
        if (sellPanel) sellPanel.SetActive(false);
        LockCursor();
    }

    void UnlockCursor() { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; if (playerController) playerController.canLook = false; }
    void LockCursor() { Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked; if (playerController) playerController.canLook = true; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) CloseShop();
    }
}