using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour
{
    Vector3 initialPosition;

    float shakeTime;
    float shakeIntensity;

    void Start()
    {
        initialPosition = transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (shakeTime == -100)
        {
            Vector3 tmp = Random.insideUnitSphere * shakeIntensity;
            tmp.z = initialPosition.z;
            transform.position = tmp;
        }
        else if (shakeTime > 0)
        {
            Vector3 tmp = Random.insideUnitSphere * shakeIntensity;
            tmp.z = 0;
            transform.position = initialPosition + tmp;
            shakeTime -= Time.deltaTime;
        }
        else
        {
            transform.position = initialPosition;
        }
    }

    public void StartShake(float timer, float intensity)
    {
        shakeTime = timer;
        shakeIntensity = intensity;
    }
}
