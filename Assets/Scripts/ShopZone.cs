using UnityEngine;

public class ShopZone : MonoBehaviour
{
    public ShopManager.ShopType zoneType; 
    private ShopManager shopManager;

    void Start()
    {
        shopManager = FindObjectOfType<ShopManager>();
    }

    // OnTriggerEnter 함수 시작
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[ShopZone 감지] 닿은 오브젝트: {other.name} | 태그: {other.tag}");

        if (other.CompareTag("Player"))
        {
            shopManager.EnterZone(zoneType);
        }
    }

    //  OnTriggerExit 함수 시작 (Enter 함수 바깥에 있어야 함)
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shopManager.ExitZone();
        }
    }
}