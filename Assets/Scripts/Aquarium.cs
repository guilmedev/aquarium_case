using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
TODO

    "watter target" lerps into hit positions
    Increase watter fall velocity based on how many hits was taken
    Keep watter fall until the lowest hit
    Stops when achive the heighest hit


    reference
    https://gamedevbeginner.com/the-right-way-to-lerp-in-unity-with-examples/#lerp_vector3
*/



public class Aquarium : MonoBehaviour
{

    [SerializeField]
    private int _life = 2;

    private int _totalHits = 0;


    [SerializeField]
    private float _timeToBeEmpty = 5f;

    [Header("References")]

    [SerializeField]
    private GameObject _myhitPrefab;

    [Space]
    [SerializeField]
    private GameObject _glass;

    [SerializeField]
    private GameObject _watter;

    private List<GameObject> hits = new List<GameObject>();
    private const float HIT_POS_OFFSET = .03f; // to help lerp stops in the middle of the role

    [Space]
    [Header("Limits")]
    //limits to calculate acquarium heights
    [SerializeField]
    private Transform topLimit;
    [SerializeField]
    private Transform bottomlimit;
    [SerializeField]
    private Transform target; // watter scale will follow this target

    // Fraction of journey completed equals current distance divided by total distance.
    private float filledDistance; // Fraction of 
    private float fractionOfDistance;


    private float _watterLength; // 0 to 1 ( scale 0 to 1)

    // Learping values
    float currentHitPosition;
    bool lerping;

    // Start is called before the first frame update
    void Start()
    {
        // Calculate the journey length.
        _watterLength = Vector3.Distance(topLimit.position, bottomlimit.position);
    }

    // Update is called once per frame
    void Update()
    {
        // Fraction of journey completed equals current distance divided by total distance.
        filledDistance = Vector3.Distance(target.position, bottomlimit.position);

        //TODO: Try clamp target y position to not pass acquaruim cage
        fractionOfDistance = filledDistance / _watterLength;

        // Scale watter based on target fractionDistance
        _watter.transform.localScale = new Vector3(_watter.transform.localScale.x, fractionOfDistance, _watter.transform.localScale.z);
    }

    public void SetHit(RaycastHit hit)
    {
        _totalHits++;
        if (_totalHits > _life)
        {
            Debug.Log("BROKEN!");
            // return;
        }

        GameObject currentHit = Instantiate(_myhitPrefab, hit.point, Quaternion.LookRotation(hit.normal));

        hits.Add(currentHit);
        currentHit.transform.SetParent(this.transform);

        //Where hitted ?

        //TODO Coroutine Queue
        StartCoroutine(LerpTo(currentHit));

    }

    IEnumerator LerpTo(GameObject hitPoint)
    {
        float timeElapsed = 0;
        float lerpValue = 0;
        float startValue = target.transform.position.y;
        float endValue = (hitPoint.transform.position.y) + HIT_POS_OFFSET;

        commandSend = false;

        while (timeElapsed < _timeToBeEmpty)
        {
            lerpValue = Mathf.Lerp(startValue, endValue, timeElapsed / _timeToBeEmpty);

            target.transform.position = new Vector3(target.transform.position.x, lerpValue, target.transform.position.z);

            timeElapsed += Time.deltaTime;

            lerping = true;

            if (_timeToBeEmpty - timeElapsed < .8)
            {
                ReduceWatter(hitPoint);
            }

            yield return null;
        }
        lerping = false;
        target.transform.position = new Vector3(target.transform.position.x, endValue, target.transform.position.z);
    }

    IEnumerator LerpEmissionValue(ParticleSystem particle, float endValue, float durartion)
    {
        float timeElapsed = 0;
        float lerpValue = 0;
        float initalValue = particle.emission.rateOverTime.constant;

        ParticleSystem.EmissionModule emissionModule = particle.emission;

        while (timeElapsed < durartion)
        {
            lerpValue = Mathf.Lerp(initalValue, endValue, timeElapsed / durartion);

            emissionModule.rateOverTime = lerpValue;

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        emissionModule.rateOverTime = endValue;

    }

    bool commandSend = false;
    private void ReduceWatter(GameObject hit)
    {
        if (commandSend) return;
        Debug.Log("Piiiiu...");
        var particle = hit.GetComponentInChildren<ParticleSystem>();

        StartCoroutine(LerpEmissionValue(particle, 0, .48f));
        commandSend = true;

    }
}
