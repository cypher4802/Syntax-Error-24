using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    static public Knockback instance;
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.5f;
    public AnimationCurve knockbackCurve;

    private Coroutine knockbackCoroutine;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
       
    }
    public void ApplyKnockback(Vector3 direction, float Force)
    {
       
        // 넉백 코루틴이 실행 중이라면 중단합니다.
        if (knockbackCoroutine != null)
            StopCoroutine(knockbackCoroutine);

        // 넉백 코루틴을 실행합니다.
        knockbackCoroutine = StartCoroutine(DoKnockback(direction,Force));
    }

    private IEnumerator DoKnockback(Vector3 direction, float Force)
    {
        // 넉백을 적용하기 위해 지나야 할 거리를 계산합니다.
        float distance = Force * knockbackDuration;

        // 시작 위치를 저장합니다.
        Vector3 startPosition = transform.position;

        // 넉백 시간이 흐를수록 점점 감속하도록 보간값을 계산합니다.
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / knockbackDuration;
            float progress = knockbackCurve.Evaluate(t);

            // 보간값을 사용하여 위치를 이동합니다.
            Vector3 targetPosition = startPosition + (direction * distance * progress);
            transform.position = targetPosition;

            yield return null;
        }

        // 넉백이 완료되면 넉백 코루틴을 종료합니다.
        knockbackCoroutine = null;
    }
}
