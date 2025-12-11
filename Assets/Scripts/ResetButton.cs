using UnityEngine;
using System.Collections;

public class ResetButton : MonoBehaviour
{
    private bool isPressed = false;
    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    public void Press()
    {
        if (isPressed) return;

        StartCoroutine(PressAnimation());

        MineManager manager = FindObjectOfType<MineManager>();
        if (manager != null)
        {
            //  [수정] 리셋 대신 타이머 토글(정지/재개) 호출
            manager.ToggleTimer();
            Debug.Log(" 버튼을 눌러 시간을 조작했습니다.");
        }
    }

    IEnumerator PressAnimation()
    {
        isPressed = true;
        transform.localPosition = originalPos - new Vector3(0, 0.1f, 0);
        yield return new WaitForSeconds(0.2f);
        transform.localPosition = originalPos;
        isPressed = false;
    }
}