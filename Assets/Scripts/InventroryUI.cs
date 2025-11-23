using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro; 

public class InventoryUI : MonoBehaviour
{
    #region // 스프라이트 목록
    [Header("Block Sprites")]
    public Sprite dirtSprite;
    public Sprite grassSprite;
    public Sprite stoneSprite;
    public Sprite coalSprite;
    public Sprite ironSprite;
    public Sprite goldSprite;
    public Sprite diamondSprite;

    //상점에서 팔 아이템 목록
    [Header("ShopItem Sprites")]
    public Sprite woodSprite;
    public Sprite dynamiteSprite;

    [Header("Tool Sprites")]
    public Sprite stonePickaxeSprite;
    public Sprite ironPickaxeSprite;
    public Sprite goldPickaxeSprite;
    #endregion

    [Header("UI Components")]
    public Slider bagGauge; 
    public TextMeshProUGUI fullMessageText;

    public List<Transform> slot = new List<Transform>(); 
    public GameObject SlotItem; 
    
    //  관리용 리스트 (생성된 UI 아이템들)
    List<GameObject> uiItems = new List<GameObject>(); 
    public int selectedIndex = -1;

    private void Update()
    {
        for (int i = 0; i < Mathf.Min(9, slot.Count); i++)
        {
            if(Input.GetKeyDown(KeyCode.Alpha1 + i)) SetSelectedIndex(i);
        }
    }

    public void ShowWarningMessage(string message)
    {
        if (fullMessageText != null)
        {
            fullMessageText.text = message;
            fullMessageText.gameObject.SetActive(true);
            StopAllCoroutines(); 
            StartCoroutine(HideWarningAfterDelay(2.0f));
        }
    }

    IEnumerator HideWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (fullMessageText != null) fullMessageText.gameObject.SetActive(false);
    }

    public void SetSelectedIndex(int idx)
    {
        ResetSelection();
        if(selectedIndex == idx) selectedIndex = -1; 
        else
        {
            Setselection(idx); 
            selectedIndex = idx;
        }
    }

    public void ResetSelection()
    {
        foreach (var slotTransform in slot) 
            if (slotTransform.GetComponent<Image>() != null) slotTransform.GetComponent<Image>().color = Color.white;
    }

    void Setselection(int _idx)
    {
        if (slot[_idx].GetComponent<Image>() != null) slot[_idx].GetComponent<Image>().color = Color.yellow;
    }

    //  데이터 가져오기 방식 변경 (리스트 인덱스 사용)
    public GameData.BlockType GetInventorySlot()
    {
        if (selectedIndex < 0 || selectedIndex >= uiItems.Count) return GameData.BlockType.Air;
        
        var itemComp = uiItems[selectedIndex].GetComponent<SlotItemPrefab>();
        // 활성화된 아이템일 때만 정보 반환
        if (itemComp != null && itemComp.gameObject.activeSelf) 
            return itemComp.blockType;
            
        return GameData.BlockType.Air;
    }

    public GameData.ItemType GetSelectedItemType()
    {
        if (selectedIndex < 0 || selectedIndex >= uiItems.Count) return GameData.ItemType.None;
        
        var itemComp = uiItems[selectedIndex].GetComponent<SlotItemPrefab>();
        if (itemComp != null && itemComp.gameObject.activeSelf)
            return itemComp.itemType;

        return GameData.ItemType.None;
    }
    
    //  UI 업데이트 로직 (전면 수정)
    public void UpdateInventory(Inventory myInven)
    {
        // 1. UI 아이템 오브젝트가 부족하면 미리 생성 (풀링 비슷하게)
        while (uiItems.Count < slot.Count)
        {
            var go = Instantiate(SlotItem, slot[uiItems.Count].transform);
            go.transform.localPosition = Vector3.zero;
            go.SetActive(false); // 기본은 꺼둠
            uiItems.Add(go);
        }

        int occupiedSlots = 0;

        // 2. 인벤토리 데이터(slots)를 순회하며 UI 갱신
        for (int i = 0; i < myInven.slots.Count; i++)
        {
            // UI 슬롯 개수보다 데이터가 많으면 중단 (혹시 모를 에러 방지)
            if (i >= uiItems.Count) break;

            var dataSlot = myInven.slots[i];
            var uiItem = uiItems[i];
            var prefab = uiItem.GetComponent<SlotItemPrefab>();

            if (!dataSlot.IsEmpty)
            {
                // 아이템 있음: 켜고 정보 입력
                uiItem.SetActive(true);
                GameData.BlockType typeToPass = (GameData.BlockType)dataSlot.itemType;
                Sprite itemSprite = GetSpriteForItemType(dataSlot.itemType);
                
                prefab.ItemSetting(itemSprite, "x" + dataSlot.count.ToString(), typeToPass, dataSlot.itemType);
                occupiedSlots++;
            }
            else
            {
                // 아이템 없음: 끔
                uiItem.SetActive(false);
                // 데이터 초기화 (선택 시 오류 방지)
                prefab.blockType = GameData.BlockType.Air;
                prefab.itemType = GameData.ItemType.None;
            }
        }

        // 3. 게이지 업데이트 (사용 중인 칸 수 / 전체 칸 수)
        if (bagGauge != null)
        {
            bagGauge.maxValue = myInven.maxSlots; 
            bagGauge.value = occupiedSlots; 
        }
    }
    
    private Sprite GetSpriteForItemType(GameData.ItemType type)
    {
        return type switch
        {
            GameData.ItemType.Dirt => dirtSprite,
            GameData.ItemType.Grass => grassSprite,
            GameData.ItemType.Stone => stoneSprite,
            GameData.ItemType.Coal => coalSprite,
            GameData.ItemType.Iron => ironSprite,
            GameData.ItemType.Gold => goldSprite,
            GameData.ItemType.Diamond => diamondSprite,

            GameData.ItemType.StonePickaxe => stonePickaxeSprite,
            GameData.ItemType.IronPickaxe => ironPickaxeSprite,
            GameData.ItemType.GoldPickaxe => goldPickaxeSprite,

            GameData.ItemType.Wood => woodSprite,
            GameData.ItemType.Dynamite => dynamiteSprite,
            _ => null
        };
    }
}