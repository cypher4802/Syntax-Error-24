using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FightingGameReplaySystem : MonoBehaviour
{
    [SerializeField] private float bufferDurationMinutes = 5f;
    private float bufferDurationSeconds;
    private List<InputEvent> inputBuffer = new List<InputEvent>();
    private bool isReplaying = false;
    private float replayStartTime;
    private float recordingStartTime;

    // Reference to the fighter's components
    [SerializeField] private Animator fighterAnimator;
    [SerializeField] private Guard_ani_Setting fighterController;
    [SerializeField] private Rigidbody fighterRigidbody;

    // Track attack states
    private bool isAttacking = false;
    private Dictionary<KeyCode, string> attackAnimations = new Dictionary<KeyCode, string>()
    {
        { KeyCode.I, "Jab_R" },
        { KeyCode.U, "Jab_L" },
        { KeyCode.K, "Kick_R" },
        { KeyCode.J, "Kick_L" }
    };

    private void Start()
    {
        bufferDurationSeconds = bufferDurationMinutes * 60f;
        recordingStartTime = Time.time;

        // Get required components
        if (fighterAnimator == null)
            fighterAnimator = GetComponent<Animator>();
        if (fighterController == null)
            fighterController = GetComponent<Guard_ani_Setting>();
        if (fighterRigidbody == null)
            fighterRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (GameManager.Gs != GameManager.Gamesetting.GameStart)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleReplay();
        }

        if (isReplaying)
        {
            PlayReplay();
        }
        else
        {
            RecordInput();
        }
    }

    private void RecordInput()
    {
        float currentTime = Time.time - recordingStartTime;

        // Remove old inputs
        while (inputBuffer.Count > 0 && currentTime - inputBuffer[0].time > bufferDurationSeconds)
        {
            inputBuffer.RemoveAt(0);
        }

        // Record movement inputs
        RecordKeyWithState(KeyCode.D, "walkfwd", currentTime);
        RecordKeyWithState(KeyCode.A, "walkbwd", currentTime);
        RecordKeyWithState(KeyCode.S, "sit", currentTime);
        RecordKeyWithState(KeyCode.W, "jump", currentTime);

        // Record attack inputs with their current animation states
        foreach (var attack in attackAnimations)
        {
            if (Input.GetKeyDown(attack.Key))
            {
                inputBuffer.Add(new InputEvent(currentTime, attack.Key, true, attack.Value));
                Debug.Log($"Recorded attack: {attack.Value} at time {currentTime}");
            }
        }
    }

    private void RecordKeyWithState(KeyCode key, string animParam, float currentTime)
    {
        if (Input.GetKeyDown(key))
        {
            inputBuffer.Add(new InputEvent(currentTime, key, true, animParam));
        }
        else if (Input.GetKeyUp(key))
        {
            inputBuffer.Add(new InputEvent(currentTime, key, false, animParam));
        }
    }

    private void ToggleReplay()
    {
        isReplaying = !isReplaying;
        if (isReplaying)
        {
            StartReplay();
        }
        else
        {
            StopReplay();
        }
    }

    private void StartReplay()
    {
        if (inputBuffer.Count == 0)
        {
            Debug.LogWarning("No input data available for replay.");
            isReplaying = false;
            return;
        }

        // Reset all states
        ResetAllAnimationStates();

        // Store a copy of the input buffer for replay
        List<InputEvent> replayBuffer = new List<InputEvent>(inputBuffer);
        inputBuffer = replayBuffer;

        replayStartTime = Time.time;
        Debug.Log($"Starting replay with {inputBuffer.Count} recorded inputs");
    }

    private void StopReplay()
    {
        Debug.Log("Stopping replay...");
        ResetAllAnimationStates();
        isAttacking = false;
    }

    private void PlayReplay()
    {
        float currentReplayTime = Time.time - replayStartTime;

        while (inputBuffer.Count > 0 && inputBuffer[0].time <= currentReplayTime)
        {
            var inputEvent = inputBuffer[0];
            inputBuffer.RemoveAt(0);
            SimulateInput(inputEvent);
        }

        if (inputBuffer.Count == 0)
        {
            isReplaying = false;
            Debug.Log("Replay finished.");
        }
    }

    private void SimulateInput(InputEvent inputEvent)
    {
        // Handle attacks
        if (attackAnimations.ContainsKey(inputEvent.keyCode) && inputEvent.isKeyDown)
        {
            string attackAnim = inputEvent.animationParam;
            Debug.Log($"Replaying attack: {attackAnim}");

            // Trigger the attack animation
            fighterAnimator.SetBool(attackAnim, true);

            // Update the fighter's state
            if (attackAnim.Contains("Jab"))
            {
                Guard_ani_Setting.G_A_T = Guard_ani_Setting.ani_state.H_attack;
            }
            else if (attackAnim.Contains("Kick"))
            {
                Guard_ani_Setting.G_A_T = Guard_ani_Setting.ani_state.K_attack;
            }

            StartCoroutine(ResetAttackAnimation(attackAnim));
            return;
        }

        // Handle movement
        if (inputEvent.isKeyDown)
        {
            fighterAnimator.SetBool(inputEvent.animationParam, true);

            // Update movement state
            switch (inputEvent.keyCode)
            {
                case KeyCode.D:
                    Guard_ani_Setting.G_A_T = Guard_ani_Setting.ani_state.forwardstep;
                    break;
                case KeyCode.A:
                    Guard_ani_Setting.G_A_T = Guard_ani_Setting.ani_state.backstep;
                    break;
                case KeyCode.S:
                    Guard_ani_Setting.G_A_T = Guard_ani_Setting.ani_state.sit;
                    break;
                case KeyCode.W:
                    Guard_ani_Setting.G_A_T = Guard_ani_Setting.ani_state.jump;
                    break;
            }
        }
        else
        {
            fighterAnimator.SetBool(inputEvent.animationParam, false);
        }
    }

    private IEnumerator ResetAttackAnimation(string animName)
    {
        yield return new WaitForSeconds(0.2f);
        fighterAnimator.SetBool(animName, false);
        Guard_ani_Setting.G_A_T = Guard_ani_Setting.ani_state.idle;
    }

    private void ResetAllAnimationStates()
    {
        // Reset movement animations
        fighterAnimator.SetBool("walkfwd", false);
        fighterAnimator.SetBool("walkbwd", false);
        fighterAnimator.SetBool("sit", false);
        fighterAnimator.SetBool("jump", false);

        // Reset attack animations
        foreach (var attack in attackAnimations.Values)
        {
            fighterAnimator.SetBool(attack, false);
        }

        // Reset state
        Guard_ani_Setting.G_A_T = Guard_ani_Setting.ani_state.idle;
    }
}

public struct InputEvent
{
    public float time;
    public KeyCode keyCode;
    public bool isKeyDown;
    public string animationParam;

    public InputEvent(float t, KeyCode key, bool isDown, string animParam)
    {
        time = t;
        keyCode = key;
        isKeyDown = isDown;
        animationParam = animParam;
    }
}