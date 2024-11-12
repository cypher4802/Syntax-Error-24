using UnityEngine;
using System.Collections.Generic;

public class InputCapture : MonoBehaviour
{
    public float bufferDuration = 5f; // Duration in minutes
    private float bufferTime; // Time to buffer in seconds
    private List<InputData> inputBuffer = new List<InputData>();
    private float startTime;

    [System.Serializable]
    public class InputData
    {
        public float time;
        public Vector2 direction; // Store movement direction or other inputs
    }

    void Start()
    {
        bufferTime = bufferDuration * 60f; // Convert minutes to seconds
        startTime = Time.time;
    }

    void Update()
    {
        // Capture inputs every frame
        CaptureInput();
    }

    void CaptureInput()
    {
        if (Time.time - startTime < bufferTime)
        {
            // Get input
            Vector2 inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (inputDirection != Vector2.zero)
            {
                InputData inputData = new InputData
                {
                    time = Time.time - startTime,
                    direction = inputDirection
                };
                inputBuffer.Add(inputData);
            }
        }
        else
        {
            // Remove oldest input if buffer is full
            if (inputBuffer.Count > 0)
            {
                inputBuffer.RemoveAt(0);
            }
        }
    }

    public List<InputData> GetInputBuffer()
    {
        return inputBuffer;
    }
}
