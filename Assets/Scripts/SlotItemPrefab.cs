// SlotItemPrefab.cs 파일

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static BlockTypeScript;

public class SlotItemPrefab : MonoBehaviour
{
    public Image itemImage;
    public TextMeshProUGUI itemText;

    // BlockType을 저장하는 필드
    public BlockType blockType;

    // ItemSetting 함수의 마지막 인수를 BlockType으로 설정
    public void ItemSetting(Sprite itemSprite, string txt, BlockType type)
    {
        itemImage.sprite = itemSprite;
        if (itemImage != null) itemImage.enabled = true;
        itemText.text = txt;

        // BlockType을 저장
        blockType = type;
    }
}