using Cinemachine;
using UnityEngine;

public class Camera_zoom : MonoBehaviour
{
    public Transform object1; // 첫 번째 오브젝트
    public Transform object2; // 두 번째 오브젝트

    public float zoomSpeed = 5f; // 줌 속도
    public float minDistance = 5f; // 최소 거리
    public float maxDistance = 10f; // 최대 거리
    CinemachineBrain CB;


    private Vector3 initialOffset; // 초기 카메라 위치와 두 오브젝트의 중심점의 오프셋

    private void Start()
    {
        // 초기 카메라 위치 설정
        CB = gameObject.GetComponent<CinemachineBrain>();
    }

    private void Update()
    {
        if (GameManager.Gs == GameManager.Gamesetting.GameStart)
            CB.enabled = false;
        // 두 오브젝트의 중심점 계산
        Vector3 centerPoint = GetCenterPoint();

        // 거리에 따른 줌 인과 줌 아웃
        float distance = Vector3.Distance(object1.position, object2.position);
        float zoomLevel = Mathf.InverseLerp(minDistance, maxDistance, distance);
        // Field of view View 바꿈
        float targetFOV = Mathf.Lerp(28f, 70f, zoomLevel);
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }

    private Vector3 GetCenterPoint()
    {
        // 두 오브젝트의 중심점 계산
        Vector3 centerPoint = (object1.position + object2.position) / 2f;

        return centerPoint;
    }
}
