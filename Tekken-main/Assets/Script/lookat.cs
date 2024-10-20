using UnityEngine;

public class lookat : MonoBehaviour
{
    public GameObject obj;
    // Update is called once per frame

    private void Start()
    {

    }
    void Update()
    {

        transform.LookAt(obj.transform.position);
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

    }
}
