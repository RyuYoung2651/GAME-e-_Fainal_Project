using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    public GameData.ItemType type;
    public int count = 1;

    [Header("Minecraft Style Settings")]
    public float dropScale = 0.3f;      
    public float rotationSpeed = 50f;   
    
    [Header("Pickup Settings")]
    public float collectionRadius = 2.5f; 
    public float attractionSpeed = 15f; // 속도를 좀 더 높임 
    public float destructionDistance = 0.5f; 
    public float pickupDelay = 0.1f;      

    private float spawnTime;
    private Transform playerTransform;
    private Inventory playerInventory;
    private Rigidbody rb;
    private Collider itemCollider;
    
    private bool isAttracted = false; 

    void Start()
    {
        spawnTime = Time.time;
        playerTransform = FindObjectOfType<PlayerController>()?.transform;
        playerInventory = FindObjectOfType<Inventory>();
        
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();

        transform.localScale = Vector3.one * dropScale;
        EnablePhysics(true);
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        if (playerTransform == null || playerInventory == null) return;

        // 쿨타임 체크
        if (Time.time < spawnTime + pickupDelay) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance < collectionRadius)
        {
            isAttracted = true;
        }

        if (isAttracted)
        {
            EnablePhysics(false); 

            // 플레이어의 '가슴' 높이 정도를 향해 날아오게 함 (바닥에 끌리지 않게)
            Vector3 targetPos = playerTransform.position + Vector3.up * 1.0f;
            
            transform.position = Vector3.MoveTowards(transform.position, targetPos, attractionSpeed * Time.deltaTime);

            // 거리 체크: 거리가 충분히 가까우면 수집
            if (distance < destructionDistance)
            {
                Collect();
            }
        }
        else
        {
            EnablePhysics(true);
        }
    }

    // 안전장치: 플레이어와 '충돌(Trigger)'하면 즉시 수집
    private void OnTriggerEnter(Collider other)
    {
        // 쿨타임이 안 지났으면 무시
        if (Time.time < spawnTime + pickupDelay) return;

        // 플레이어와 닿았는지 확인
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            Collect();
        }
    }
    
    void EnablePhysics(bool enable)
    {
        if (rb != null)
        {
            rb.useGravity = enable;       
            rb.isKinematic = !enable;     
        }
        if (itemCollider != null)
        {
            itemCollider.isTrigger = !enable; 
        }
    }

    private void Collect()
    {
        if (playerInventory != null)
        {
            // [수정] Add가 true(성공)를 반환했을 때만 파괴!
            if (playerInventory.Add(type, count))
            {
                Destroy(gameObject);
            }
            else
            {
                //  [추가] 가방이 꽉 차서 못 먹었을 때
                // 바로 다시 줍기를 시도하면 무한루프에 빠지므로, 잠시 쿨타임을 줍니다.
                pickupDelay = 2.0f; // 2초 뒤에 다시 시도
                spawnTime = Time.time; // 타이머 리셋
                
                if (rb != null)
                {
                    EnablePhysics(true); // 다시 물리 켜기
                    isAttracted = false; // 자석 끄기
                    // 반대 방향으로 살짝 튕김
                    Vector3 pushDir = (transform.position - playerTransform.position).normalized;
                    rb.AddForce(pushDir * 5f + Vector3.up * 2f, ForceMode.Impulse);
                }
            }
        }
    }
}