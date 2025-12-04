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
            // 이제 MineManager에 ManualReset이 있으니 오류가 안 날 겁니다.
            manager.ManualReset();
            Debug.Log(" 3D 버튼을 눌러 광산을 리셋했습니다!");
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