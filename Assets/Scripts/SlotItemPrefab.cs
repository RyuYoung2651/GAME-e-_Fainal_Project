using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotItemPrefab : MonoBehaviour
{
    public Image itemImage;
    public TextMeshProUGUI itemText;
    
    // 설치를 위한 블록 타입
    public GameData.BlockType blockType; 
    
    // [추가] 도구 사용 및 아이템 구분을 위한 아이템 타입
    public GameData.ItemType itemType; 

    // [수정] ItemType 정보도 함께 받아서 저장합니다.
    public void ItemSetting(Sprite itemSprite, string txt, GameData.BlockType bType, GameData.ItemType iType)
    {
        itemImage.sprite = itemSprite;
        if (itemImage != null) itemImage.enabled = true; 
        itemText.text = txt;
        
        blockType = bType;
        itemType = iType; // 아이템 타입 저장
    }
}