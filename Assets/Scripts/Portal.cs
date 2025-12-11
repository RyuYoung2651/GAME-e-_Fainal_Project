using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Settings")]
    public string nextSceneName = "EndScene"; // 이동할 씬 이름
    public float delay = 1.0f; // 닿고 나서 이동까지 걸리는 시간

    private bool isTriggered = false;

    // 플레이어가 포탈 안으로 들어오면 발동
    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        // 플레이어인지 확인 (태그나 컴포넌트로 확인)
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            isTriggered = true;
            Debug.Log(" 포탈");

            // 웅~ 하는 효과음이나 파티클을 여기서 재생하면 좋습니다.

            // 일정 시간 후 씬 이동
            Invoke("LoadScene", delay);
        }
    }

    void LoadScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}