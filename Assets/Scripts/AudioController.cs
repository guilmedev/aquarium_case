using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour
{

    [SerializeField]
    private AudioMixer _audioMixer;

    private const string VOLUME_STRING = "Volume";

    [SerializeField]
    [Range(-80,0)]
    private float _minValueSound = -80f;
    [SerializeField]
    [Range(-80,0)]
    private float _maxValueSound = -10f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ToggleAudioMixer(bool isMuted)
    {
        _audioMixer.SetFloat(VOLUME_STRING, !isMuted ? _minValueSound : _maxValueSound);
    }
}
