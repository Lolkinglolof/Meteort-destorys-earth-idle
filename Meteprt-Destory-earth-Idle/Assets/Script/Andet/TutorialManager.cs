using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Serializable]
    public class TutorialStep
    {
        public string title;

        [TextArea(2, 5)]
        public string description;

        public StepType stepType;

        [Header("Highlight")]
        public bool useDimOverlay = true;

        [Tooltip("UI thing to highlight, like a button image or icon.")]
        public Graphic uiHighlightGraphic;

        [Tooltip("World thing to highlight, like a sprite renderer.")]
        public SpriteRenderer worldHighlightRenderer;

        [Tooltip("What should pulse. Usually the button/object transform.")]
        public Transform pulseTarget;
        [Tooltip("Turn this on only if the highlighted UI graphic itself must stay clickable.")]


        public Color highlightColor = new Color(1f, 0.85f, 0.2f, 1f);
    }
    public bool ShouldUseUnscaledPlayerMovement()
    {
        return tutorialPausedGame &&
               tutorialRunning &&
               currentStepIndex < steps.Length &&
               steps[currentStepIndex].stepType == StepType.WaitForMove;
    }
    public bool ShouldFreezePassivePlayerDrift()
    {
        return tutorialPausedGame &&
               tutorialRunning &&
               currentStepIndex < steps.Length &&
               steps[currentStepIndex].stepType == StepType.WaitForMove;
    }
    public enum StepType
    {
        InfoOnly,
        WaitForMove,
        WaitForMeteorHit,
        WaitForUpgradeBought,
        WaitForAutopilotUsed
    }
    [Header("Highlight / Focus")]
    public Image dimOverlay;
    public Color dimOverlayColor = new Color(0f, 0f, 0f, 0.65f);
    public float pulseSpeed = 5f;
    public float pulseScaleAmount = 0.08f;
    public bool useUnscaledPulseTime = true;

    private Graphic currentUiHighlight;
    private Color currentUiOriginalColor;


    private SpriteRenderer currentWorldHighlight;
    private Color currentWorldOriginalColor;

    private Transform currentPulseTarget;
    private Vector3 currentPulseOriginalScale;
    private bool pulseActive;

    [Header("Tutorial Steps")]
    public TutorialStep[] steps;

    [Header("UI")]
    public GameObject tutorialPanel;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Button nextButton;
    public Button skipButton;

    [Header("Settings")]
    public bool showOnlyFirstTime = true;
    public string tutorialFinishedKey = "MeteorTutorialFinished";
    public string tutorialStepKey = "MeteorTutorialStep";
    public string autopilotUnlockedKey = "MeteorAutopilotUnlockedOnce";

    [Header("Meteor Hit Tags")]
    [Tooltip("Only these tags will count for WaitForMeteorHit steps. Leave empty to allow all tags.")]
    public string[] meteorHitTags;

    [Header("Movement Detection")]
    [Tooltip("Player meteor reference. If empty, it will try to find it automatically.")]
    public MeteorController playerController;

    [Tooltip("How far the player must move before WaitForMove is completed.")]
    public float moveDistanceThreshold = 0.35f;
    [Header("Tutorial Pause")]
    public bool pauseGameDuringUpgradeStep = true;
    public bool pauseGameDuringMoveStep = true;

    private bool tutorialPausedGame = false;
    private float tutorialPreviousTimeScale = 1f;
    [Tooltip("If true, holding left mouse button also counts as movement intent.")]
    public bool requireMouseHoldForMoveStep = true;

    private int currentStepIndex = 0;
    private bool tutorialRunning = false;
    private bool autopilotUnlockedOnce = false;

    private Vector3 moveStepStartPosition;
    private bool moveStepTrackingStarted = false;

    public string autopilotTutorialPendingKey = "MeteorAutopilotTutorialPending";
    public string autopilotTutorialCompletedKey = "MeteorAutopilotTutorialCompleted";

    private bool autopilotTutorialPending = false;
    private bool autopilotTutorialCompleted = false;
    private bool showingDeferredAutopilotStep = false;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        TryFindPlayer();
        autopilotTutorialPending = PlayerPrefs.GetInt(autopilotTutorialPendingKey, 0) == 1;
        autopilotTutorialCompleted = PlayerPrefs.GetInt(autopilotTutorialCompletedKey, 0) == 1;
        autopilotUnlockedOnce = PlayerPrefs.GetInt(autopilotUnlockedKey, 0) == 1;

        // Hvis autopilot allerede er købt fra savegame, så husk det også her
        if (!autopilotUnlockedOnce && UpgradeManager.Instance != null && UpgradeManager.Instance.autoPilotLevel > 0)
        {
            autopilotUnlockedOnce = true;
            PlayerPrefs.SetInt(autopilotUnlockedKey, 1);
            PlayerPrefs.Save();
        }

        if (showOnlyFirstTime && PlayerPrefs.GetInt(tutorialFinishedKey, 0) == 1)
        {
            EndTutorialInstant();
            return;
        }

        if (steps == null || steps.Length == 0)
        {
            EndTutorialInstant();
            return;
        }

        currentStepIndex = PlayerPrefs.GetInt(tutorialStepKey, 0);
        currentStepIndex = Mathf.Clamp(currentStepIndex, 0, steps.Length);

        StartTutorial();
    }

    private void Update()
    {
        if (playerController == null)
            TryFindPlayer();

        // Tjek altid unlock, også selv om hovedtutorialen ikke kører lige nu
        CheckAutopilotUnlockFromUpgradeManager();

        if (!tutorialRunning)
            return;

        CheckMoveStepProgress();
        UpdateHighlightPulse();
    }

    private void TryFindPlayer()
    {
        if (playerController == null)
            playerController = FindFirstObjectByType<MeteorController>();
    }

    private void CheckAutopilotUnlockFromUpgradeManager()
    {
        if (autopilotUnlockedOnce)
            return;

        if (UpgradeManager.Instance != null && UpgradeManager.Instance.autoPilotLevel > 0)
        {
            autopilotUnlockedOnce = true;
            PlayerPrefs.SetInt(autopilotUnlockedKey, 1);
            PlayerPrefs.Save();

            if (autopilotTutorialPending && !autopilotTutorialCompleted)
            {
                ShowDeferredAutopilotTutorial();
            }
        }
    }

    private void CheckMoveStepProgress()
    {
        if (!IsCurrentStep(StepType.WaitForMove))
            return;

        if (playerController == null)
            return;

        if (!moveStepTrackingStarted)
        {
            moveStepStartPosition = playerController.transform.position;
            moveStepTrackingStarted = true;
            return;
        }

        float movedDistance = Vector3.Distance(moveStepStartPosition, playerController.transform.position);

        bool mouseHeld = Input.GetMouseButton(0);
        bool movementAccepted = movedDistance >= moveDistanceThreshold;

        if (requireMouseHoldForMoveStep)
        {
            if (mouseHeld && movementAccepted)
                AdvanceFromGameplayAction();
        }
        else
        {
            if (movementAccepted)
                AdvanceFromGameplayAction();
        }
    }

    public void StartTutorial()
    {
        if (steps == null || steps.Length == 0)
        {
            EndTutorialInstant();
            return;
        }

        tutorialRunning = true;
        ShowStep();
    }

    private void ShowStep()
    {
        if (!tutorialRunning || currentStepIndex >= steps.Length)
        {
            FinishTutorial();

            return;
        }

        TutorialStep step = steps[currentStepIndex];
        UpdateTutorialPauseState(step);
        // Autopilot-step må først blive aktiv/synlig,
        // når autopilot er købt første gang.
        if (step.stepType == StepType.WaitForAutopilotUsed && !autopilotUnlockedOnce)
        {
            autopilotTutorialPending = true;
            PlayerPrefs.SetInt(autopilotTutorialPendingKey, 1);
            PlayerPrefs.Save();

            currentStepIndex++;
            SaveStepProgress();
            ShowStep();
            return;
        }

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        if (titleText != null)
            titleText.text = step.title;

        if (descriptionText != null)
            descriptionText.text = step.description;

        bool infoOnly = step.stepType == StepType.InfoOnly;

        if (nextButton != null)
            nextButton.gameObject.SetActive(infoOnly);
        ApplyStepHighlight(step);
        if (step.stepType == StepType.WaitForMove && playerController != null)
        {
            moveStepStartPosition = playerController.transform.position;
            moveStepTrackingStarted = true;
        }
        else
        {
            moveStepTrackingStarted = false;
        }

        SaveStepProgress();
    }

    public void NextStep()
    {
        if (!tutorialRunning)
            return;

        currentStepIndex++;
        ShowStep();
    }

    public void SkipTutorial()
    {
        if (showingDeferredAutopilotStep)
        {
            autopilotTutorialPending = false;
            autopilotTutorialCompleted = true;

            PlayerPrefs.SetInt(autopilotTutorialPendingKey, 0);
            PlayerPrefs.SetInt(autopilotTutorialCompletedKey, 1);
            PlayerPrefs.Save();

            showingDeferredAutopilotStep = false;
            tutorialRunning = false;
            EndTutorialInstant();
            return;
        }

        FinishTutorial();
    }

    private void FinishTutorial()
    {
        ResumeGameFromTutorialPause();

        tutorialRunning = false;
        moveStepTrackingStarted = false;
        pulseActive = false;
        PlayerPrefs.SetInt(tutorialFinishedKey, 1);
        PlayerPrefs.DeleteKey(tutorialStepKey);
        PlayerPrefs.Save();

        EndTutorialInstant();

        Debug.Log("Tutorial finished.");
    }

    private void EndTutorialInstant()
    {
        ResumeGameFromTutorialPause();
        ClearStepHighlight();

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    private void SaveStepProgress()
    {
        PlayerPrefs.SetInt(tutorialStepKey, currentStepIndex);
        PlayerPrefs.Save();
    }

    // ===== CALLED FROM OTHER SCRIPTS =====

    public void ReportPlayerMoved()
    {
        if (IsCurrentStep(StepType.WaitForMove))
            AdvanceFromGameplayAction();
    }

    public void ReportMeteorHit()
    {
        if (IsCurrentStep(StepType.WaitForMeteorHit))
            AdvanceFromGameplayAction();
    }

    public void ReportMeteorHit(GameObject hitObject)
    {
        if (!IsCurrentStep(StepType.WaitForMeteorHit))
            return;

        if (hitObject == null)
            return;

        string tagToCheck = hitObject.tag;

        if (!IsAllowedMeteorTag(tagToCheck))
        {
            Transform parent = hitObject.transform.parent;
            if (parent == null || !IsAllowedMeteorTag(parent.tag))
                return;
        }

        AdvanceFromGameplayAction();
    }

    public void ReportMeteorHit(string hitTag)
    {
        if (!IsCurrentStep(StepType.WaitForMeteorHit))
            return;

        if (!IsAllowedMeteorTag(hitTag))
            return;

        AdvanceFromGameplayAction();
    }

    public void ReportUpgradeBought()
    {
        if (IsCurrentStep(StepType.WaitForUpgradeBought))
            AdvanceFromGameplayAction();
    }

    public void ReportAutopilotBoughtFirstTime()
    {
        if (autopilotUnlockedOnce)
            return;

        autopilotUnlockedOnce = true;

        PlayerPrefs.SetInt(autopilotUnlockedKey, 1);
        PlayerPrefs.Save();

        if (autopilotTutorialPending && !autopilotTutorialCompleted)
        {
            ShowDeferredAutopilotTutorial();
        }
    }

    public void ReportAutopilotUsed()
    {
        if (!autopilotUnlockedOnce)
            return;

        if (!IsCurrentStep(StepType.WaitForAutopilotUsed))
            return;

        autopilotTutorialPending = false;
        autopilotTutorialCompleted = true;

        PlayerPrefs.SetInt(autopilotTutorialPendingKey, 0);
        PlayerPrefs.SetInt(autopilotTutorialCompletedKey, 1);
        PlayerPrefs.Save();

        if (showingDeferredAutopilotStep)
        {
            showingDeferredAutopilotStep = false;
            tutorialRunning = false;
            EndTutorialInstant();
            return;
        }

        AdvanceFromGameplayAction();
    }

    private bool IsCurrentStep(StepType type)
    {
        return tutorialRunning &&
               currentStepIndex < steps.Length &&
               steps[currentStepIndex].stepType == type;
    }

    private void AdvanceFromGameplayAction()
    {
        currentStepIndex++;
        ShowStep();
    }

    private bool IsAllowedMeteorTag(string hitTag)
    {
        if (string.IsNullOrWhiteSpace(hitTag))
            return false;

        if (meteorHitTags == null || meteorHitTags.Length == 0)
            return true;

        for (int i = 0; i < meteorHitTags.Length; i++)
        {
            string allowedTag = meteorHitTags[i];

            if (string.IsNullOrWhiteSpace(allowedTag))
                continue;

            if (hitTag == allowedTag)
                return true;
        }

        return false;
    }
    public void ResetTutorialProgress()
    {
        ResumeGameFromTutorialPause();

        PlayerPrefs.DeleteKey(tutorialFinishedKey);
        PlayerPrefs.DeleteKey(tutorialStepKey);
        PlayerPrefs.DeleteKey(autopilotUnlockedKey);
        PlayerPrefs.DeleteKey(autopilotTutorialPendingKey);
        PlayerPrefs.DeleteKey(autopilotTutorialCompletedKey);
        PlayerPrefs.Save();

        currentStepIndex = 0;
        tutorialRunning = false;
        autopilotUnlockedOnce = false;
        autopilotTutorialPending = false;
        autopilotTutorialCompleted = false;
        showingDeferredAutopilotStep = false;
        moveStepTrackingStarted = false;

        EndTutorialInstant();
        StartTutorial();

        Debug.Log("Tutorial progress has been reset and restarted.");
    }
    private void OnDisable()
    {
        ResumeGameFromTutorialPause();
    }

    private void OnDestroy()
    {
        ResumeGameFromTutorialPause();
    }
    private void ShowDeferredAutopilotTutorial()
    {
        if (autopilotTutorialCompleted)
            return;

        int autopilotStepIndex = FindStepIndex(StepType.WaitForAutopilotUsed);
        if (autopilotStepIndex == -1)
            return;

        showingDeferredAutopilotStep = true;
        tutorialRunning = true;
        currentStepIndex = autopilotStepIndex;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        ShowStep();
    }
    private int FindStepIndex(StepType type)
    {
        if (steps == null)
            return -1;

        for (int i = 0; i < steps.Length; i++)
        {
            if (steps[i].stepType == type)
                return i;
        }

        return -1;
    }
    private void ApplyStepHighlight(TutorialStep step)
    {
        ClearStepHighlight();

        if (dimOverlay != null)
        {
            dimOverlay.gameObject.SetActive(step.useDimOverlay);
            dimOverlay.color = dimOverlayColor;
            dimOverlay.raycastTarget = false;
        }

        if (step.uiHighlightGraphic != null)
        {
            currentUiHighlight = step.uiHighlightGraphic;
            currentUiOriginalColor = currentUiHighlight.color;

            // Only visual change. Do NOT change raycast/canvas/sorting.
            currentUiHighlight.color = step.highlightColor;

            if (step.pulseTarget == null)
                currentPulseTarget = currentUiHighlight.transform;
        }

        if (step.worldHighlightRenderer != null)
        {
            currentWorldHighlight = step.worldHighlightRenderer;
            currentWorldOriginalColor = currentWorldHighlight.color;
            currentWorldHighlight.color = step.highlightColor;

            if (step.pulseTarget == null && currentPulseTarget == null)
                currentPulseTarget = currentWorldHighlight.transform;
        }

        if (step.pulseTarget != null)
            currentPulseTarget = step.pulseTarget;

        if (currentPulseTarget != null)
        {
            currentPulseOriginalScale = currentPulseTarget.localScale;
            pulseActive = true;
        }
    }

    private void ClearStepHighlight()
    {
        if (currentUiHighlight != null)
            currentUiHighlight.color = currentUiOriginalColor;

        if (currentWorldHighlight != null)
            currentWorldHighlight.color = currentWorldOriginalColor;

        if (currentPulseTarget != null)
            currentPulseTarget.localScale = currentPulseOriginalScale;

        currentUiHighlight = null;
        currentWorldHighlight = null;
        currentPulseTarget = null;

        pulseActive = false;

        if (dimOverlay != null)
            dimOverlay.gameObject.SetActive(false);
    }

    private void UpdateHighlightPulse()
    {
        if (!pulseActive || currentPulseTarget == null)
            return;

        float t = useUnscaledPulseTime ? Time.unscaledTime : Time.time;
        float pulse = 1f + Mathf.Sin(t * pulseSpeed) * pulseScaleAmount;
        currentPulseTarget.localScale = currentPulseOriginalScale * pulse;
    }
    private void UpdateTutorialPauseState(TutorialStep step)
    {
        bool shouldPause =
            step != null &&
            (
                (pauseGameDuringUpgradeStep && step.stepType == StepType.WaitForUpgradeBought) ||
                (pauseGameDuringMoveStep && step.stepType == StepType.WaitForMove)
            );

        if (shouldPause)
        {
            PauseGameForTutorial();
        }
        else
        {
            ResumeGameFromTutorialPause();
        }
    }

    private void PauseGameForTutorial()
    {
        if (tutorialPausedGame)
            return;

        tutorialPreviousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        tutorialPausedGame = true;
    }

    private void ResumeGameFromTutorialPause()
    {
        if (!tutorialPausedGame)
            return;

        Time.timeScale = tutorialPreviousTimeScale;
        tutorialPausedGame = false;
    }

}