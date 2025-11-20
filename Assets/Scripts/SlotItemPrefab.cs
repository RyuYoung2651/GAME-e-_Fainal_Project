using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Unity 스크립트 (자산 참조 1개) | 참조 0개
public class SlotItemPrefab : MonoBehaviour
{
    public Image itemImage;
    public TextMeshProUGUI itemText;

    // 참조 0개
    public void ItemSetting(Sprite itemSprite, string txt,ItemTypeScript.ItemType itemType)
    {
        itemImage.sprite = itemSprite;
        itemText.text = txt;
    }
}