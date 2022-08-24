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


[RequireComponent(typeof(HitsHandler))]
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

    [Header("References")]
    [SerializeField]
    private GameObject _myhitPrefab;
    [SerializeField]
    private GameObject _glassPiecesPrefab;
    private GameObject _glassPiecesContainer;

    [Space]
    [SerializeField]
    private GameObject _glass;

    [SerializeField]
    private GameObject _watter;

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

    [Header("Physics Value")]
    public float radius = 5.0F;
    public float power = 10.0F;
    public float upwardsModifier = 10.0F;

    [Header("Sound")]
    [SerializeField]
    private AudioSource _brokenAudioSource;
    [SerializeField]
    private AudioClip _clip;


    [Header("hitPoint")]
    [SerializeField]
    private GameObject particleCollision;
    [Header("Effects")]
    [SerializeField]
    private GameObject _watterSplashParcticle;

    private HitsHandler _hitsHanlder;

    private void Awake()
    {
        _hitsHanlder = GetComponent<HitsHandler>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Calculate the journey length.
        _watterLength = Vector3.Distance(topLimit.position, bottomlimit.position);

        isBroken = false;
        _currentLife = _life;
        _hitsHanlder.currentTimeToBeEmpty = _timeToBeEmpty;

        _brokenAudioSource.clip = _clip;

        _hitsHanlder.Init(target.gameObject, HIT_POS_OFFSET);
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
            BreakAquarium();
            return;
        }

        GameObject currentHit = Instantiate(_myhitPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        currentHit.transform.SetParent(this.transform);

        _hitsHanlder.AddNewPoint(currentHit);


        var hitPoint = currentHit.GetComponent<HitPoint>();
        if (hitPoint != null)
        {

            //Where hitted ?
            if (HittedInsideWatter(currentHit))
            {
                hitPoint.Init(particleCollision);
                //TODO Coroutine Queue 
                _hitsHanlder.LerpToPoint(currentHit);
                _hitsHanlder.currentTimeToBeEmpty -= _watterGravityModifier;
            }
        }
        else
        {
            Destroy(currentHit);
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

        _glassPiecesContainer = Instantiate(_glassPiecesPrefab, _glass.gameObject.transform.position, _glass.gameObject.transform.rotation);
        _watterSplashParcticle.SetActive(true);
        ApplyPhysycs();

        isBroken = true;

        _brokenAudioSource.PlayOneShot(_clip);
    }

    private void ApplyPhysycs()
    {
        Vector3 explosionPos = target.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
                rb.AddExplosionForce(power, explosionPos, radius, upwardsModifier);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(target.position, radius);
        Gizmos.color = Color.red;
    }

    private void RemoveWater()
    {
        _hitsHanlder.currentTimeToBeEmpty = _removeWatterTime;
        _hitsHanlder.RemoveWater(bottomlimit.gameObject);
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
        _hitsHanlder.currentTimeToBeEmpty = _timeToBeEmpty;

        // disable all hits
        DisableAllHits();
        // disable particles
        _glass.gameObject.SetActive(true);
        _watterSplashParcticle.SetActive(false);


        isBroken = false;
    }

    private void DisableAllHits()
    {
        _hitsHanlder.DisableAllHits();

        Destroy(_glassPiecesContainer);
    }
}
