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
    private int _currentLife;

    private int _totalHits = 0;
    [Tooltip("Drecease amount for every new hit")]
    [SerializeField]
    private float _watterGravityModifier = .01f;
    [Tooltip("Time to remove all water when aquarium is broken")]
    [SerializeField]
    private float _removeWatterTime = .45f;


    [Tooltip("Defatult value to water reaches the hit point")]
    [SerializeField]
    private float _timeToBeEmpty = 5f;
    private float _currentTimeToBeEmpty;

    [Header("References")]

    [SerializeField]
    private GameObject _myhitPrefab;

    [Space]
    [SerializeField]
    private GameObject _glass;

    [SerializeField]
    private GameObject _watter;

    private List<GameObject> hits = new List<GameObject>();
    [SerializeField]
    private float HIT_POS_OFFSET = .03f; // to help lerp stops in the middle of the role

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


    private bool isBroken;


    // Start is called before the first frame update
    void Start()
    {
        // Calculate the journey length.
        _watterLength = Vector3.Distance(topLimit.position, bottomlimit.position);

        isBroken = false;
        _currentLife = _life;
        _currentTimeToBeEmpty = _timeToBeEmpty;
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
        if (_totalHits > _currentLife)
        {
            Debug.Log("BROKEN!");
            BreakAquarium();
            return;
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
                _currentTimeToBeEmpty -= _watterGravityModifier;
                StartCoroutine(LerpTo(currentHit));
            }
        }
    }

    private void BreakAquarium()
    {
        StopAllCoroutines();
        // Disable all hits
        DisableAllHits();
        // Broken particles
        _glass.gameObject.SetActive(false);
        // Empty watter
        RemoveWater();
        isBroken = true;
    }

    private void RemoveWater()
    {
        _currentTimeToBeEmpty = _removeWatterTime;
        StartCoroutine(LerpTo(bottomlimit.gameObject));
        //target.transform.position = new Vector3(target.transform.position.x, bottomlimit.position.y, target.transform.position.z);

    }

    private void RestoreWater()
    {
        target.transform.position = new Vector3(target.transform.position.x, topLimit.position.y, target.transform.position.z);
    }

    private bool HittedInsideWatter(GameObject currentHit)
    {
        return target.position.y > currentHit.transform.position.y;
    }

    public void ResetAquarium()
    {
        //restore watter
        RestoreWater();

        // reset time ?
        _totalHits = 0;
        _currentLife = _life;
        _currentTimeToBeEmpty = _timeToBeEmpty;

        // disable all hits
        DisableAllHits();
        // disable particles
        _glass.gameObject.SetActive(true);

        isBroken = false;
    }

    private void DisableAllHits()
    {
        foreach (GameObject gameObject in hits)
        {
            Destroy(gameObject);
        }

        hits.Clear();
    }

    IEnumerator LerpTo(GameObject hitPoint)
    {
        float timeElapsed = 0;
        float lerpValue = 0;
        float startValue = target.transform.position.y;
        float endValue = (hitPoint.transform.position.y) + HIT_POS_OFFSET;

        commandSent = false;

        while (timeElapsed < _currentTimeToBeEmpty)
        {
            lerpValue = Mathf.Lerp(startValue, endValue, timeElapsed / _currentTimeToBeEmpty);

            target.transform.position = new Vector3(target.transform.position.x, lerpValue, target.transform.position.z);

            timeElapsed += Time.deltaTime;

            lerping = true;

            if (_currentTimeToBeEmpty - timeElapsed < .8)
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
        if (commandSent || isBroken) return;

        HitPoint hitPoint = hit.GetComponentInChildren<HitPoint>();
        if (hitPoint != null)
        {
            hitPoint.ReduceWatter(0, .48f);
        }
        commandSent = true;

    }
}
