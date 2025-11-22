using UnityEngine;
using System.Collections.Generic;
using TMPro; 

public class Furnace : MonoBehaviour
{
    [Header("Settings")]
    public List<CraftingRecipe> recipes; 
    public Transform dropPoint; 
    public GameObject itemDropPrefab_Pickaxe; 

    [Header("UI")]
    public TextMeshPro statusText; 

    private Dictionary<GameData.ItemType, int> currentIngredients = new Dictionary<GameData.ItemType, int>();

    void Start()
    {
        UpdateUI();
        
        //  [진단 1] 시작하자마자 레시피가 제대로 등록됐는지 확인
        if (recipes == null || recipes.Count == 0)
        {
            Debug.LogError("[Furnace Error] 용광로에 등록된 레시피가 없습니다! Inspector에서 Recipes 리스트를 채워주세요.");
        }
        else
        {
            Debug.Log($"[Furnace] {recipes.Count}개의 레시피가 로드되었습니다.");
            foreach(var r in recipes)
                Debug.Log($"   - 레시피: {r.recipeName} (재료1: {r.ingredient1} {r.count1}개, 재료2: {r.ingredient2} {r.count2}개)");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ItemDrop item = other.GetComponent<ItemDrop>();
        
        if (item != null)
        {
            // 줍기 쿨타임 체크 (던지자마자 들어가는 것 방지)
            if (Time.time < item.GetComponent<ItemDrop>().pickupDelay + 0.5f) return;

            AddIngredient(item.type, item.count);
            Destroy(other.gameObject);
        }
    }

    void AddIngredient(GameData.ItemType type, int count)
    {
        if (currentIngredients.ContainsKey(type)) currentIngredients[type] += count;
        else currentIngredients[type] = count;

        Debug.Log($" [Furnace] 재료 투입됨: {type} (총 {currentIngredients[type]}개)");
        UpdateUI();
        CheckRecipes(); 
    }

    void CheckRecipes()
    {
        Debug.Log(" [Furnace] 레시피 매칭 시도 중...");

        foreach (var recipe in recipes)
        {
            int have1 = GetIngredientCount(recipe.ingredient1);
            int have2 = GetIngredientCount(recipe.ingredient2);

            Debug.Log($"  검사 중: {recipe.recipeName} | 필요: {recipe.ingredient1}({recipe.count1}), {recipe.ingredient2}({recipe.count2}) | 보유: {have1}, {have2}");

            if (have1 >= recipe.count1 && have2 >= recipe.count2)
            {
                Debug.Log(" 조건 충족! 제작 시작!");
                
                ConsumeIngredient(recipe.ingredient1, recipe.count1);
                ConsumeIngredient(recipe.ingredient2, recipe.count2);
                CraftItem(recipe.resultItem);
                return; 
            }
        }
        Debug.Log("  매칭되는 레시피가 없습니다. 재료가 부족합니다.");
    }

    void CraftItem(GameData.ItemType result)
    {
        if (itemDropPrefab_Pickaxe != null && dropPoint != null)
        {
            GameObject go = Instantiate(itemDropPrefab_Pickaxe, dropPoint.position, Quaternion.identity);
            ItemDrop drop = go.GetComponent<ItemDrop>();
            
            if (drop != null)
            {
                drop.type = result;
                drop.count = 1;
                drop.pickupDelay = 2.0f; 
                
                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb == null) { rb = go.AddComponent<Rigidbody>(); rb.useGravity = true; }
                
                rb.AddForce(Vector3.up * 5f + transform.forward * 2f, ForceMode.Impulse);
            }
            Debug.Log($" [Furnace] {result} 제작 완료 및 배출!");
        }
        else
        {
            Debug.LogError(" [Furnace Error] DropPoint 또는 ItemDropPrefab_Pickaxe가 연결되지 않았습니다!");
        }
        UpdateUI();
    }

    int GetIngredientCount(GameData.ItemType type)
    {
        if (currentIngredients.ContainsKey(type)) return currentIngredients[type];
        return 0;
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
        if (currentIngredients.Count == 0) txt += "비어있음";
        else
        {
            foreach (var item in currentIngredients)
                txt += $"{item.Key}: {item.Value}\n";
        }
        statusText.text = txt;
    }
}