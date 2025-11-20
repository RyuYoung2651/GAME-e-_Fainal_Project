using static BlockTypeScript;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    #region // 각 큐브 별 스프라이트
    public Sprite dirtSprite;
    public Sprite grassSprite;
    public Sprite stoneSprite;
    public Sprite coalSprite;
    public Sprite ironSprite;
    public Sprite goldSprite;
    public Sprite diamondSprite;
    #endregion

    public List<Transform> slot = new List<Transform>(); // UI의 슬롯들의 리스트
    // 슬롯 내부에 들어가는 아이템
    public GameObject SlotItem; //슬롯 내부에 들어가는 아이템

    List<GameObject> items = new List<GameObject>(); // 아이템 삭제 전체 리스트

    // 인벤토리 업데이트 시 호출
    public int selectedIndex = -1;
    // 참조 값

    public void UpdateInventory(Inventory myInven)
    {
        // 1. 기존 슬롯 초기화
        foreach (var slotItems in items)
        {
            Destroy(slotItems); // 시작할때 슬롯 아이템들의 GameObject 삭제
        }
        items.Clear(); // 초기화할때 아이템 리스트 클리어
        // 2. 새 인벤토리 데이터를 화면에 적용
        int idx = 0; // 슬롯별 슬롯의 인덱스
        foreach (var item in myInven.items)
        {
           // #region 슬롯아이템 생성 로직 (게임오브젝트 인스턴스 생성, 위치 조정, SlotItemPrefab 컴포넌트 가져오기, 그 후 아이템 세팅)
            var go = Instantiate(SlotItem,slot[idx].transform);

            go.transform.localPosition = Vector3.zero;

            SlotItemPrefab slotItemPrefab = go.GetComponent<SlotItemPrefab>();

            items.Add(go); // 아이템 리스트에 하나 추가

            switch (item.Key) // 각 케이스별로 아이템 추가
            {
                case ItemTypeScript.ItemType.Dirt:
                    slotItemPrefab.ItemSetting(dirtSprite, "x" + item.Value.ToString(), item.Key);
                    break;
                case ItemTypeScript.ItemType.Grass:
                    slotItemPrefab.ItemSetting(grassSprite, "x" + item.Value.ToString(), item.Key);
                    break;
                case ItemTypeScript.ItemType.Coal:
                    slotItemPrefab.ItemSetting(coalSprite, "x" + item.Value.ToString(), item.Key);
                    break;
                case ItemTypeScript.ItemType.Stone:
                    slotItemPrefab.ItemSetting(stoneSprite, "x" + item.Value.ToString(), item.Key);
                    break;
                case ItemTypeScript.ItemType.Iron:
                    slotItemPrefab.ItemSetting(ironSprite, "x" + item.Value.ToString(), item.Key);
                    break;
                case ItemTypeScript.ItemType.Gold:
                    slotItemPrefab.ItemSetting(goldSprite, "x" + item.Value.ToString(), item.Key);
                    break;
                case ItemTypeScript.ItemType.Diamond:
                    slotItemPrefab.ItemSetting(diamondSprite, "x" + item.Value.ToString(), item.Key);
                    break;
            }

            idx++; // 인덱스 1씩 증가
        }
    }
}