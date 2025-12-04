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

    [Header("UI")]
    public TextMeshProUGUI timerTextUI;
    public TextMeshPro mineTimerText3D;

    private float timer;
    private bool isMineOpen = true;

    void Start()
    {
        timer = openDuration;
        isMineOpen = true;
    }

    void Update()
    {
       

        timer -= Time.deltaTime;

        int timeLeft = Mathf.CeilToInt(timer);
        string message = "";
        Color textColor = Color.white;

        if (isMineOpen)
        {
            message = $"광산 리셋까지\n<size=150%>{timeLeft}</size>초";
            if (timeLeft <= 10) textColor = Color.red;
            else textColor = Color.green;
        }
        else
        {
            message = $"광산 정비 중...\n<size=150%>{timeLeft}</size>초";
            textColor = Color.yellow;
        }

        if (timerTextUI != null) { timerTextUI.text = message; timerTextUI.color = textColor; }
        if (mineTimerText3D != null) { mineTimerText3D.text = message; mineTimerText3D.color = textColor; }

        if (timer <= 0)
        {
            if (isMineOpen) CloseMine();
            else OpenMine();
        }
    }

    // [이 함수가 없어서 오류가 났던 것입니다!]
    public void ManualReset()
    {
        if (isMineOpen)
        {
            Debug.Log(" 수동 리셋 요청됨!");
            timer = 0; // 시간을 0으로 만들어 즉시 닫히게 함
            CloseMine();
        }
    }

    void CloseMine()
    {
        isMineOpen = false;
        timer = closedDuration;
        Debug.Log(" 광산이 닫혔습니다! 플레이어 귀환.");

        if (player != null && spawnPoint != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.position = spawnPoint.position;
            player.rotation = spawnPoint.rotation;
            if (cc != null) cc.enabled = true;
        }

        if (mapGenerator != null) mapGenerator.ResetMap();
    }

    void OpenMine()
    {
        isMineOpen = true;
        timer = openDuration;
        Debug.Log(" 광산이 다시 열렸습니다!");
    }
}