using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class RewardPopupText : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI amountText;
    public CanvasGroup canvasGroup;

    [Header("Animation")]
    public float moveUpDistance = 60f;
    public float duration = 0.8f;

    [Header("Debug")]
    public bool rewardPopupDebug = true;

    public System.Action<RewardPopupText> Finished;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private bool hasOriginalPosition;
    private bool hasFinished;

    void Awake()
    {
        FindReferences();
    }

    private void FindReferences()
    {
        if (rectTransform == null)
            rectTransform = transform as RectTransform;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (amountText == null)
            amountText = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    public void SetOriginalPosition(Vector2 newPosition)
    {
        FindReferences();

        originalPosition = newPosition;
        hasOriginalPosition = true;

        if (rectTransform != null)
            rectTransform.anchoredPosition = originalPosition;
    }

    public void Setup(string text)
    {
        FindReferences();

        if (rectTransform == null)
        {
            PopupDebug("SETUP FAILED", "RectTransform is NULL");
            return;
        }

        StopAllCoroutines();

        hasFinished = false;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (!gameObject.activeInHierarchy)
        {
            PopupDebug("SETUP FAILED", "Popup object is inactive in hierarchy");
            return;
        }

        if (!hasOriginalPosition)
        {
            originalPosition = rectTransform.anchoredPosition;
            hasOriginalPosition = true;
        }

        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = Vector3.one;
        rectTransform.SetAsLastSibling();

        if (amountText != null)
        {
            amountText.gameObject.SetActive(true);
            amountText.enabled = true;
            amountText.alpha = 1f;
            amountText.text = text;
            amountText.ForceMeshUpdate();
        }
        else
        {
            PopupDebug("SETUP WARNING", "AmountText is NULL");
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            PopupDebug("SETUP WARNING", "CanvasGroup is NULL");
        }

        PopupDebug(
            "POPUP SETUP",
            "popup=" + gameObject.name +
            " | text=" + text +
            " | amountTextExists=" + (amountText != null) +
            " | activeSelf=" + gameObject.activeSelf +
            " | activeInHierarchy=" + gameObject.activeInHierarchy +
            " | anchoredPosition=" + rectTransform.anchoredPosition
        );

        StartCoroutine(AnimatePopup());
    }

    private IEnumerator AnimatePopup()
    {
        float timer = 0f;

        Vector2 startPosition = originalPosition;
        Vector2 endPosition = originalPosition + new Vector2(0f, moveUpDistance);

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(timer / duration);

            rectTransform.anchoredPosition = Vector2.Lerp(
                startPosition,
                endPosition,
                progress
            );

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);

            yield return null;
        }

        Finish();
    }

    private void Finish()
    {
        if (hasFinished)
            return;

        hasFinished = true;

        PopupDebug("POPUP FINISH", "popup=" + gameObject.name);

        if (Finished != null)
            Finished.Invoke(this);

        Destroy(gameObject);
    }

    private void PopupDebug(string source, string message)
    {
        if (!rewardPopupDebug)
            return;

        Debug.Log(
            "<color=#00E5FF>[POPUP DEBUG]</color> <b>" +
            source +
            "</b> | " +
            message,
            this
        );
    }
}