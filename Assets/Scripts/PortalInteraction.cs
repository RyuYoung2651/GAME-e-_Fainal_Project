using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필수!

public class PortalInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float reachDistance = 5f; // 블록을 클릭할 수 있는 거리
    public string sceneToLoad = "GameScene2"; // 이동할 씬의 이름 (정확히 적어야 함)

    [Header("References")]
    public InventoryUI inventoryUI; // 현재 들고 있는 아이템 확인용

    // 레이캐스트를 위한 카메라
    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;

        // 인벤토리 UI가 연결 안 되어있으면 자동으로 찾기
        if (inventoryUI == null)
            inventoryUI = FindObjectOfType<InventoryUI>();
    }

    void Update()
    {
        // 마우스 우클릭 (1번 버튼) 감지
        if (Input.GetMouseButtonDown(1))
        {
            CheckPortalActivation();
        }
    }

    void CheckPortalActivation()
    {
        // 1. 현재 들고 있는 아이템이 '라이터'인지 확인
        if (inventoryUI.GetSelectedItemType() != GameData.ItemType.Lighter)
        {
            return; // 라이터가 아니면 아무 일도 안 함
        }

        // 2. 화면 중앙에서 레이저를 쏴서 블록 확인
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, reachDistance))
        {
            // 맞은 물체에서 Block 컴포넌트 가져오기
            Block targetBlock = hit.collider.GetComponent<Block>();

            // 3. 그 블록이 '흑요석(Obsidian)'인지 확인
            if (targetBlock != null && targetBlock.type == GameData.BlockType.Obsidian)
            {
                Debug.Log("포탈 점화! 다른 차원으로 이동합니다...");

                // 4. 씬 이동 실행
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}