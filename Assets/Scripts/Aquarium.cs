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

    reference to stack-coroutines
    https://answers.unity.com/questions/1590871/how-to-stack-coroutines-and-call-each-one-till-all.html
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
    bool commandSent = false;


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

        //Maybe add as HitPoint ?
        hits.Add(currentHit);
        currentHit.transform.SetParent(this.transform);

        var hitPoint = currentHit.GetComponent<HitPoint>();
        if (hitPoint != null)
        {
            //Where hitted ?
            if (HittedInsideWatter(currentHit))
            {
                hitPoint.Init();
                //TODO Coroutine Queue 
                StartCoroutine(LerpTo(currentHit));
            }
        }
    }

    private bool HittedInsideWatter(GameObject currentHit)
    {
        return target.position.y > currentHit.transform.position.y;
    }

    IEnumerator LerpTo(GameObject hitPoint)
    {
        float timeElapsed = 0;
        float lerpValue = 0;
        float startValue = target.transform.position.y;
        float endValue = (hitPoint.transform.position.y) + HIT_POS_OFFSET;

        commandSent = false;

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

    private void ReduceWatter(GameObject hit)
    {
        if (commandSent) return;
        HitPoint hitPoint = hit.GetComponentInChildren<HitPoint>();
        hitPoint.ReduceWatter(0, .48f);
        commandSent = true;

    }
}
