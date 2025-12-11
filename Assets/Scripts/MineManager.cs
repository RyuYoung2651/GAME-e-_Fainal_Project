using UnityEngine;
using TMPro;

public class MineManager : MonoBehaviour
{
    [Header("Time Settings")]
    public float openDuration = 60f;
    public float closedDuration = 5f;

    [Header("References")]
    public NoiseVoxelMap mapGenerator;
    public Transform player;
    public Transform spawnPoint;
    public GameObject mineBarrier;

    [Header("UI")]
    public TextMeshProUGUI timerTextUI;
    public TextMeshPro mineTimerText3D;

    private float timer;
    private bool isMineOpen = true;

    //  [추가] 타이머 정지 여부 체크
    public bool isTimerPaused = false;

    void Start()
    {
        timer = openDuration;
        isMineOpen = true;
        if (mineBarrier != null) mineBarrier.SetActive(false);
    }

    void Update()
    {
        // F5키는 여전히 강제 리셋 (개발자용)
        if (Input.GetKeyDown(KeyCode.F5)) ManualReset();

        //  [수정] 일시정지 상태가 아닐 때만 시간 흐름
        if (!isTimerPaused)
        {
            timer -= Time.deltaTime;
        }

        // --- UI 업데이트 ---
        int timeLeft = Mathf.CeilToInt(timer);
        string message = "";
        Color textColor = Color.white;

        if (isMineOpen)
        {
            if (isTimerPaused)
            {
                //  멈췄을 때 표시
                message = "시간 정지됨\n<size=150%>PAUSED</size>";
                textColor = Color.cyan; // 하늘색
            }
            else
            {
                message = $"광산 리셋까지\n<size=150%>{timeLeft}</size>초";
                if (timeLeft <= 10) textColor = Color.red;
                else textColor = Color.green;
            }
        }
        else
        {
            message = $"광산 정비 중...\n<size=150%>{timeLeft}</size>초";
            textColor = Color.yellow;
        }

        if (timerTextUI != null) { timerTextUI.text = message; timerTextUI.color = textColor; }
        if (mineTimerText3D != null) { mineTimerText3D.text = message; mineTimerText3D.color = textColor; }

        // 시간 종료 체크
        if (timer <= 0)
        {
            if (isMineOpen) CloseMine();
            else OpenMine();
        }
    }

    //  [신규] 버튼이 호출할 함수 (토글 방식: 멈춤 <-> 재개)
    public void ToggleTimer()
    {
        if (isMineOpen) // 광산이 열려있을 때만 가능
        {
            isTimerPaused = !isTimerPaused; // 상태 반전 (True <-> False)
            Debug.Log($"타이머 상태 변경: {(isTimerPaused ? "정지됨" : "재개됨")}");
        }
    }

    public void ManualReset()
    {
        if (isMineOpen)
        {
            isTimerPaused = false; // 리셋할 때는 정지 풀기
            timer = 0;
            CloseMine();
        }
    }

    void CloseMine()
    {
        isMineOpen = false;
        isTimerPaused = false; // 닫혀있을 때는 시간 흘러야 함
        timer = closedDuration;

        if (player != null && spawnPoint != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.position = spawnPoint.position;
            player.rotation = spawnPoint.rotation;
            if (cc != null) cc.enabled = true;
        }

        if (mineBarrier != null) mineBarrier.SetActive(true);
        if (mapGenerator != null) mapGenerator.ResetMap();
    }

    void OpenMine()
    {
        isMineOpen = true;
        timer = openDuration;
        if (mineBarrier != null) mineBarrier.SetActive(false);
    }
}