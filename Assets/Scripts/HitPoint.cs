using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPoint : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _particleSystem;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    internal void Init()
    {
        _particleSystem.gameObject.SetActive(true);
    }

    public void ReduceWatter(float endValue, float durartion)
    {
        Debug.Log("Piiiiu...");
        StartCoroutine(LerpEmissionValue( 0, .48f));
    }

    private IEnumerator LerpEmissionValue(float endValue, float durartion)
    {
        float timeElapsed = 0;
        float lerpValue = 0;
        float initalValue = _particleSystem.emission.rateOverTime.constant;

        ParticleSystem.EmissionModule emissionModule = _particleSystem.emission;

        while (timeElapsed < durartion)
        {
            lerpValue = Mathf.Lerp(initalValue, endValue, timeElapsed / durartion);

            emissionModule.rateOverTime = lerpValue;

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        emissionModule.rateOverTime = endValue;
    }

}
