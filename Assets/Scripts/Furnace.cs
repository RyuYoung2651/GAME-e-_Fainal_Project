using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Furnace : MonoBehaviour
{
    [Header("Settings")]
    public List<CraftingRecipe> recipes; // 레시피 리스트
    public Transform dropPoint;          // 아이템 배출 위치

    [Header("UI")]
    public TextMeshPro statusText;       // 용광로 상태 텍스트

    // 용광로 내부 아이템 저장소
    private Dictionary<GameData.ItemType, int> currentIngredients = new Dictionary<GameData.ItemType, int>();

    void Start()
    {
        UpdateUI();
    }

    // 아이템 투입 감지
    private void OnTriggerEnter(Collider other)
    {
        ItemDrop item = other.GetComponent<ItemDrop>();

        if (item != null)
        {
            // 던지자마자 바로 들어가는 것 방지 (쿨타임 체크)
            if (Time.time < item.pickupDelay + 0.5f) return;

            AddIngredient(item.type, item.count);
            Destroy(other.gameObject); // 투입된 아이템 삭제
        }
    }

    // 재료 추가 로직
    void AddIngredient(GameData.ItemType type, int count)
    {
        if (currentIngredients.ContainsKey(type)) currentIngredients[type] += count;
        else currentIngredients[type] = count;

        UpdateUI();
        CheckRecipes(); // 재료가 들어올 때마다 레시피 검사
    }

    // 레시피 검사 로직
    void CheckRecipes()
    {
        foreach (var recipe in recipes)
        {
            int count1 = GetIngredientCount(recipe.ingredient1);
            int count2 = GetIngredientCount(recipe.ingredient2);

            // 재료 조건 충족 확인
            if (count1 >= recipe.count1 && count2 >= recipe.count2)
            {
                // 재료 소모
                ConsumeIngredient(recipe.ingredient1, recipe.count1);
                ConsumeIngredient(recipe.ingredient2, recipe.count2);

                //레시피에 설정된 프리팹으로 아이템 생성
                CraftItem(recipe.resultItem, recipe.resultPrefab);

                return; // 한 번에 하나만 제작
            }
        }
    }

    // 아이템 제작(배출) 로직
    void CraftItem(GameData.ItemType resultType, GameObject prefabToSpawn)
    {
        // 프리팹 연결 안 됨 방지
        if (prefabToSpawn == null)
        {
            Debug.LogError($"[Furnace] '{resultType}'을 만들려는데 레시피에 프리팹이 없습니다!");
            UpdateUI();
            return;
        }

        if (dropPoint != null)
        {
            // 아이템 생성
            GameObject go = Instantiate(prefabToSpawn, dropPoint.position, Quaternion.identity);
            ItemDrop drop = go.GetComponent<ItemDrop>();

            // 데이터 설정
            if (drop != null)
            {
                drop.type = resultType;
                drop.count = 1;
                drop.pickupDelay = 2.0f; // 바로 다시 먹히지 않게 넉넉한 쿨타임

                // 펑 튀어나오는 효과
                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb == null) { rb = go.AddComponent<Rigidbody>(); rb.useGravity = true; }

                // 위쪽 + 앞쪽으로 힘을 가함
                rb.AddForce(Vector3.up * 5f + transform.forward * 2f, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
            }
            Debug.Log($" [Furnace] {resultType} 제작 완료!");
        }

        UpdateUI();
    }

    int GetIngredientCount(GameData.ItemType type)
    {
        return currentIngredients.ContainsKey(type) ? currentIngredients[type] : 0;
    }

    void ConsumeIngredient(GameData.ItemType type, int amount)
    {
        if (currentIngredients.ContainsKey(type))
        {
            currentIngredients[type] -= amount;
            if (currentIngredients[type] <= 0) currentIngredients.Remove(type);
        }
    }

    void UpdateUI()
    {
        if (statusText == null) return;

        string txt = "<b><color=orange>[ 용광로 ]</color></b>\n";
        if (currentIngredients.Count == 0)
        {
            txt += "비어있음";
        }
        else
        {
            foreach (var item in currentIngredients)
            {
                txt += $"{item.Key}: {item.Value}\n";
            }
        }
        statusText.text = txt;
    }
}