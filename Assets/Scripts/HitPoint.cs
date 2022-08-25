using System.Collections;
using UnityEngine;

public class HitPoint : MonoBehaviour
{
    [Header("Grapichs")]
    [SerializeField]
    private GameObject _glassTexture;
    [SerializeField]
    [Range(.1f, 2f)]
    private float _maxSize;
    [SerializeField]
    [Range(.1f, 2f)]
    private float _minSize;
    [Header("Particle")]
    [SerializeField]
    private ParticleSystem _particleSystem;
    [Space]
    [Header("Sounds")]
    [SerializeField]
    private AudioSource _watterClip;
    [SerializeField]
    private AudioSource _impactClip;

    private Transform waterPosition = null;
    private float hitOffset;
    private float timeToBeEmptyMultiplayer;
    private bool reducedWater;

    // Start is called before the first frame update
    void Start()
    {

    }
    private void Awake()
    {
        _impactClip.volume = Random.Range(.5f, .8f);
    }

    internal void Init(GameObject collisinObject, Transform waterPos, float hIT_POS_OFFSET)
    {
        this.waterPosition = waterPos;
        this.hitOffset = hIT_POS_OFFSET;
        _particleSystem.gameObject.SetActive(true);
        _particleSystem.collision.AddPlane(collisinObject.transform);

        RandomizeValues();

        _watterClip.volume = _watterClip.volume;
        _watterClip.Play();
    }

    private void Update()
    {
        //Chegar pela distance se estou acima da água
        if (waterPosition == null || reducedWater) return;

        if ((waterPosition.transform.position.y - this.transform.position.y) < hitOffset)
        {
            reducedWater = true;
            ReduceWatter();
        }
        else
        {
            _particleSystem.gameObject.SetActive(true);
        }
    }

    private bool IsParticleSystemActive()
    {
        return _particleSystem.gameObject.activeSelf;
    }

    private void RandomizeValues()
    {
        _glassTexture.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-90f, 90f));

        _glassTexture.transform.localScale = new Vector3
            (
                Random.Range(_minSize, _maxSize),
                Random.Range(_minSize, _maxSize),
                Random.Range(_minSize, _maxSize)
            );
    }

    private void ReduceWatter()
    {
        if (!IsParticleSystemActive()) return;

        StartCoroutine(LerpValuesToZero(.48f * timeToBeEmptyMultiplayer));
    }

    private IEnumerator LerpValuesToZero(float durartion)
    {
        float endValue = 0;

        float timeElapsed = 0;
        //Particle stuff
        float lerpRateOverTimeValue = 0;
        float initalValue = _particleSystem.emission.rateOverTime.constant;

        //Audio stuff
        float initalAuidioValue = _watterClip.volume;
        float lerpAudioVolume = 0;

        ParticleSystem.EmissionModule emissionModule = _particleSystem.emission;

        while (timeElapsed < durartion)
        {
            lerpRateOverTimeValue = Mathf.Lerp(initalValue, endValue, timeElapsed / durartion);
            lerpAudioVolume = Mathf.Lerp(initalAuidioValue, endValue, timeElapsed / durartion);

            emissionModule.rateOverTime = lerpRateOverTimeValue;
            _watterClip.volume = lerpAudioVolume;

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        emissionModule.rateOverTime = endValue;
        _watterClip.volume = endValue;
    }

}
