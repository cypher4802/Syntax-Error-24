using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReplaySystem : MonoBehaviour
{
    [SerializeField] private float bufferDurationMinutes = 5f;
    private float bufferDurationSeconds;
    private List<ReplaySystem.InputEvent> inputBuffer = new List<ReplaySystem.InputEvent>();
    private bool isReplaying = false;
    private float replayStartTime;
    private float recordingStartTime;

    private void Start()
    {
        bufferDurationSeconds = bufferDurationMinutes * 60f;
        recordingStartTime = Time.time;
    }

    private void Update()
    {
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

        // Record new inputs
        CheckAndRecordKey(KeyCode.D, currentTime);
        CheckAndRecordKey(KeyCode.A, currentTime);
        CheckAndRecordKey(KeyCode.S, currentTime);
        CheckAndRecordKey(KeyCode.W, currentTime);
        CheckAndRecordKey(KeyCode.I, currentTime);
        CheckAndRecordKey(KeyCode.U, currentTime);
        CheckAndRecordKey(KeyCode.K, currentTime);
        CheckAndRecordKey(KeyCode.J, currentTime);

        // Record axis inputs
        RecordAxisInput("Horizontal", currentTime);
        RecordAxisInput("Vertical", currentTime);
    }

    private void CheckAndRecordKey(KeyCode key, float currentTime)
    {
        if (Input.GetKeyDown(key))
        {
            inputBuffer.Add(new ReplaySystem.InputEvent(currentTime, key, true));
        }
        else if (Input.GetKeyUp(key))
        {
            inputBuffer.Add(new ReplaySystem.InputEvent(currentTime, key, false));
        }
    }

    private void RecordAxisInput(string axisName, float currentTime)
    {
        float axisValue = Input.GetAxis(axisName);
        if (Mathf.Abs(axisValue) > 0.01f)
        {
            inputBuffer.Add(new ReplaySystem.InputEvent(currentTime, axisName, axisValue));
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
        replayStartTime = Time.time;
        Debug.Log("Starting replay...");
    }

    private void StopReplay()
    {
        Debug.Log("Stopping replay...");
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

    private void SimulateInput(ReplaySystem.InputEvent inputEvent)
    {
        if (inputEvent.isAxisInput)
        {
            Debug.Log($"Simulating axis input: {inputEvent.axisName} = {inputEvent.axisValue}");
            // You would need to implement a way to simulate axis input in your game
        }
        else
        {
            if (inputEvent.isKeyDown)
            {
                Debug.Log($"Simulating key down: {inputEvent.keyCode}");
                StartCoroutine(SimulateKeyPress(inputEvent.keyCode));
            }
            else
            {
                Debug.Log($"Simulating key up: {inputEvent.keyCode}");
            }
        }
    }

    private IEnumerator SimulateKeyPress(KeyCode key)
    {
        // Simulate a quick press and release
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
    }

    public struct InputEvent
    {
        public float time;
        public KeyCode keyCode;
        public bool isKeyDown;
        public string axisName;
        public float axisValue;
        public bool isAxisInput;

        public InputEvent(float t, KeyCode key, bool isDown)
        {
            time = t;
            keyCode = key;
            isKeyDown = isDown;
            axisName = "";
            axisValue = 0f;
            isAxisInput = false;
        }

        public InputEvent(float t, string axis, float value)
        {
            time = t;
            axisName = axis;
            axisValue = value;
            keyCode = KeyCode.None;
            isKeyDown = false;
            isAxisInput = true;
        }
    }
}