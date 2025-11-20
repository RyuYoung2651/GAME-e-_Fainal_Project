using UnityEngine;
using static ItemTypeScript;

public class ItemDrop : MonoBehaviour
{
    // 어떤 타입의 아이템인지와 개수
    public ItemType type;
    public int count = 1;

    // 플레이어의 인벤토리에 아이템이 빨려 들어갈 거리
    public float collectionRadius = 1.5f;
    public float attractionSpeed = 10f; // 인벤토리로 이동하는 속도

    private Transform playerTransform;
    private Inventory playerInventory;

    //콜라이더 컴퍼넌트 참조
    private Collider itemCollider;
    //is Trigger 켜져있는지 확인
    private bool isAttracted = false;

    //아이템 파괴 범위
    public float destructionDistance = 5f;

    void Start()
    {
        // 플레이어 트랜스폼과 인벤토리 컴포넌트 찾기
        playerTransform = FindObjectOfType<PlayerController>()?.transform;
        playerInventory = FindObjectOfType<Inventory>();

        //컴퍼넌트 가져오기
        itemCollider = GetComponent<Collider>();

        // 드롭 시 약간의 물리적인 튕김
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.AddForce(Random.onUnitSphere * 2f, ForceMode.Impulse);
    }

    void Update()
    {
        if (playerTransform == null || playerInventory == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // 플레이어가 아이템 주변에 접근했을 때
        if (distance < collectionRadius)
        {
            // isAttracted가 false일 때만 isTrigger를 true로 설정
            if (!isAttracted && itemCollider != null)
            {
                itemCollider.isTrigger = true;
                isAttracted = true;
                Debug.Log($"[ItemDrop] {type} 아이템의 isTrigger가 활성화되었습니다.");
            }

            // 플레이어에게 끌어당기기 (Keep Digging 스타일)
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * attractionSpeed * Time.deltaTime;

            // 거의 도달했을 때 인벤토리에 추가
            if (distance < 0.5f)
            {
                Collect();
            }
        }
    }

    private void Collect()
    {
        playerInventory.Add(type, count);
        Destroy(gameObject); // 아이템 제거
    }
}