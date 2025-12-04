using UnityEngine;
using TMPro; // 텍스트 사용을 위해 필수

public class RecipeBoard : MonoBehaviour
{
    [Header("연결 설정")]
    public Furnace targetFurnace; // 레시피를 가져올 용광로
    public TextMeshPro boardText; // 글씨를 띄울 텍스트 오브젝트

    [Header("디자인 설정")]
    public string title = " 제작 레시피 ";
    public Color titleColor = Color.yellow;
    public Color itemColor = Color.white;

    void Start()
    {
        UpdateBoard();
    }

    //  레시피를 읽어서 텍스트로 변환하는 함수
    public void UpdateBoard()
    {
        if (targetFurnace == null || boardText == null) return;

        string finalString = $"<color=#{ColorUtility.ToHtmlStringRGB(titleColor)}><size=120%>{title}</size></color>\n\n";

        // 용광로에 등록된 모든 레시피를 하나씩 꺼냄
        foreach (var recipe in targetFurnace.recipes)
        {
            // 예: [ 돌 곡괭이 ]
            finalString += $"<b>[{recipe.recipeName}]</b>\n";

            // 예: = Stone x3 + Wood x2
            finalString += $"<color=#{ColorUtility.ToHtmlStringRGB(itemColor)}>";
            finalString += $"= {recipe.ingredient1} x{recipe.count1}";

            // 재료 2가 있다면 추가 표시
            if (recipe.ingredient2 != GameData.ItemType.None && recipe.count2 > 0)
            {
                finalString += $" + {recipe.ingredient2} x{recipe.count2}";
            }

            finalString += "</color>\n\n"; // 줄바꿈
        }

        boardText.text = finalString;
    }
}