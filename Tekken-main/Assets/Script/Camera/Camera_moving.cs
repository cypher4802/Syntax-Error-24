using UnityEngine;

public class Camera_moving : MonoBehaviour
{
    public Transform object1; // 첫 번째 오브젝트
    public Transform object2; // 두 번째 오브젝트

    public float cameraSpeed = 5f; // 카메라 이동 속도

    private void Update()
    {

        Vector3 middlepoint = new Vector3(((object1.position.x + object2.position.x) / 2),
            1.5f, ((object1.position.z + object2.position.z) / 2));

        Vector3 targetposition = new Vector3(((object1.position.x + object2.position.x) / 2),
            1.5f, ((object1.position.z + object2.position.z) / 2) + 5); ;

        transform.position = Vector3.Lerp(transform.position, targetposition, cameraSpeed);

        Vector3 cameraPo = middlepoint;

        transform.LookAt(cameraPo);



    }
}


