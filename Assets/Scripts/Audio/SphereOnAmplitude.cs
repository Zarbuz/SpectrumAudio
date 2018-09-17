using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereOnAmplitude : MonoBehaviour
{
    public AudioPeer audioPeer;

    public enum State
    {
        Off,
        OnAmplitude
    };

    public State state = State.Off;
    public Material material;
    public Color colorOff;
    public Gradient colorGradient;
    public float emissionMultiplier;
    public float threshold;

    private Transform[] _spheres;
    private Material[] _spheresMaterials;
    private Color[] _colorOn;
    private int _sphereCount;
    private bool[] _sphereState;
    private List<int> _sphereList;


    private void Start()
    {
        _sphereCount = transform.childCount;
        _spheres = new Transform[_sphereCount];
        _sphereState = new bool[_sphereCount];
        _spheresMaterials = new Material[_sphereCount];
        _sphereList = new List<int>();
        _colorOn = new Color[_sphereCount];


        for (int i = 0; i < _sphereCount; i++)
        {
            Material mat = new Material(material);
            transform.GetChild(i).GetComponent<MeshRenderer>().material = mat;
            _colorOn[i] = colorGradient.Evaluate(i * (1.0f / (_sphereCount - 1)));
            _spheresMaterials[i] = mat;
            _spheres[i] = transform.GetChild(i);
        }
    }

    private void Update()
    {
        StateSelect();
    }

   
    private void StateSelect()
    {
        switch (state)
        {
            case State.Off:
                for (int i = 0; i < _sphereCount; i++)
                {
                    _spheresMaterials[i].SetColor("_EmissionColor", colorOff);
                }
                break;
            case State.OnAmplitude:
                for (int i = 0; i < _sphereCount; i++)
                {
                    if (audioPeer.GetAudioBandBuffer(i) > threshold)
                    {
                        _spheresMaterials[i].SetColor("_EmissionColor", _colorOn[i] * emissionMultiplier * audioPeer.GetAudioBandBuffer(i));
                    }
                    else
                    {
                        _spheresMaterials[i].SetColor("_EmissionColor", colorOff);
                    }
                }
                break;
        }
    }
}
