using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    public GameData.ItemType type;
    public int count = 1;

    [Header("Minecraft Style Settings")]
    public float dropScale = 0.3f;      
    public float rotationSpeed = 50f;   
    
    [Header("Pickup Settings")]
    public float collectionRadius = 3.0f; 
    public float attractionSpeed = 15f; 
    public float destructionDistance = 1.5f; 
    public float pickupDelay = 1.5f;      

    private float spawnTime;
    private Transform playerTransform;
    private Inventory playerInventory;
    private Rigidbody rb;
    private Collider itemCollider;
    
    private bool isAttracted = false; 
    
    // 중복 획득 방지용 플래그
    private bool isCollected = false; 

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
        // 이미 주워진 상태라면 아무것도 하지 않음 (중복 방지)
        if (isCollected) return;

        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        if (playerTransform == null || playerInventory == null) return;

        if (Time.time < spawnTime + pickupDelay) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance < collectionRadius)
        {
            isAttracted = true;
        }

        if (isAttracted)
        {
            EnablePhysics(false); 
            Vector3 targetPos = playerTransform.position + Vector3.up * 1.0f;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, attractionSpeed * Time.deltaTime);

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

    private void OnTriggerEnter(Collider other)
    {
        // 이미 주워진 상태라면 무시
        if (isCollected) return;
        
        if (Time.time < spawnTime + pickupDelay) return;

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
        // 이중 잠금 장치
        if (isCollected) return; 
        isCollected = true; // "지금 줍는 중!"이라고 표시

        if (playerInventory != null)
        {
            if (playerInventory.Add(type, count))
            {
                Destroy(gameObject);
            }
            else
            {
                isCollected = false; 
                
                // 튕겨내기 로직
                pickupDelay = 2.0f; 
                spawnTime = Time.time; 
                
                if (rb != null)
                {
                    EnablePhysics(true); 
                    isAttracted = false; 
                    Vector3 pushDir = (transform.position - playerTransform.position).normalized;
                    rb.AddForce(pushDir * 5f + Vector3.up * 2f, ForceMode.Impulse);
                }
            }
        }
    }
}