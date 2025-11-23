using UnityEngine;
using System.Collections; 
using TMPro; 

public class Dynamite : MonoBehaviour
{
    [Header("Settings")]
    public float fuseTime = 3.0f;      
    public float explosionRadius = 4.0f; 
    public int explosionDamage = 100;  
    public float explosionForce = 500f; 

    [Header("Effects")]
    public GameObject explosionEffect; 
    public AudioClip explosionSound;   
    
    [Header("UI")]
    public TextMeshPro countdownText; 
    public float textHeight = 1.0f; // 텍스트가 떠있을 높이

    private bool hasExploded = false;
    private Transform cameraTransform; 

    void Start()
    {
        if (Camera.main != null) cameraTransform = Camera.main.transform;

        StartCoroutine(Countdown());
        
        // 던져진 아이템 기능 끄기 (다시 줍기 방지)
        var itemDrop = GetComponent<ItemDrop>();
        if (itemDrop != null) Destroy(itemDrop); 
        
        // 텍스트 초기 설정 (잘 보이게)
        if (countdownText != null)
        {
            countdownText.alignment = TextAlignmentOptions.Center; 
            countdownText.fontSize = 12; 
            countdownText.color = Color.yellow; 
            countdownText.outlineWidth = 0.2f; 
            countdownText.outlineColor = Color.black;
        }
    }

    // LateUpdate를 써서 물리가 다 계산된 후에 텍스트 위치를 강제로 잡음
    void LateUpdate()
    {
        if (countdownText != null)
        {
            // 1. 위치 고정: 다이너마이트(transform)의 위치에서 '월드 좌표 위쪽'으로 띄움
            // (transform.up이 아니라 Vector3.up을 써야 다이너마이트가 굴러도 텍스트는 하늘로 솟음)
            countdownText.transform.position = transform.position + Vector3.up * textHeight;

            // 2. 빌보드: 항상 카메라 정면을 바라봄
            if (cameraTransform != null)
            {
                // 텍스트가 카메라를 등지고 서는 걸 방지하기 위해 방향 계산
                countdownText.transform.rotation = Quaternion.LookRotation(countdownText.transform.position - cameraTransform.position);
            }
        }
    }

    IEnumerator Countdown()
    {
        float timer = fuseTime;

        while (timer > 0)
        {
            if (countdownText != null)
            {
                // 소수점 버리고 정수만 표시 (3, 2, 1)
                countdownText.text = Mathf.Ceil(timer).ToString(); 
                
                // 1초 이하일 때 빨간색으로 경고
                if (timer <= 1.0f) countdownText.color = Color.red;
            }

            yield return null;
            timer -= Time.deltaTime;
        }

        if (countdownText != null) countdownText.text = "!!!";
        Explode();
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (explosionEffect != null) Instantiate(explosionEffect, transform.position, Quaternion.identity);
        if (explosionSound != null) AudioSource.PlayClipAtPoint(explosionSound, transform.position);

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Block block = nearbyObject.GetComponent<Block>();
            if (block != null) block.Hit(explosionDamage); 

            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null) rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
        }

        // 다이너마이트가 삭제되면 자식인 텍스트도 같이 사라집니다.
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}