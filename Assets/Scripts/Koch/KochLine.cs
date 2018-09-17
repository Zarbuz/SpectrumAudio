using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class KochLine : KochGenerator
{
    public float generateMultiplier;

    [Header("Audio")]
    public AudioPeer audioPeer;
    public int[] audioBand;
    public int audioBandMaterial;
    public float emissionMultiplier;
    public bool colorOnAudio;
    public bool useBufferOnColor;

    public Material material;
    public Color color;

    private Vector3[] _lerpPosition;
    private LineRenderer _lineRenderer;
    private float[] _lerpAudio;
    private Material _matInstance;

    // Use this for initialization
    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.enabled = true;
        _lineRenderer.useWorldSpace = false;
        _lineRenderer.loop = true;
        _lineRenderer.positionCount = positions.Length;
        _lineRenderer.SetPositions(positions);
        _lerpPosition = new Vector3[positions.Length];

        _lerpAudio = new float[initiatorPointAmount];

        _matInstance = new Material(material);
        _matInstance.SetColor("_TintColor", color);
        _matInstance.SetColor("_EmissionColor", color * emissionMultiplier);
        _lineRenderer.material = _matInstance;
    }

    private void Update()
    {
        if (colorOnAudio)
        {
            if (useBufferOnColor)
            {
                _matInstance.SetColor("_TintColor", color * audioPeer.GetAudioBandBuffer(audioBandMaterial) * emissionMultiplier);
                _matInstance.SetColor("_EmissionColor", color * audioPeer.GetAudioBandBuffer(audioBandMaterial) * emissionMultiplier);
            }
            else
            {
                _matInstance.SetColor("_TintColor", color * audioPeer.GetAudioBand(audioBandMaterial) * emissionMultiplier);
                _matInstance.SetColor("_EmissionColor", color * audioPeer.GetAudioBand(audioBandMaterial) * emissionMultiplier);
            }
        }

        if (generationCount != 0)
        {
            int count = 0;
            for (int i = 0; i < initiatorPointAmount; i++)
            {
                _lerpAudio[i] = audioPeer.GetAudioBandBuffer(audioBand[i]);
                for (int j = 0; j < (positions.Length - 1) / initiatorPointAmount; j++)
                {
                    _lerpPosition[count] = Vector3.Lerp(positions[count], targetPosition[count], _lerpAudio[i]);
                    count++;
                }
            }
            _lerpPosition[count] = Vector3.Lerp(positions[count], targetPosition[count], _lerpAudio[initiatorPointAmount - 1]);


            if (useBezierCurves)
            {
                bezierPosition = BezierCurve(_lerpPosition, bezierVertexCount);
                _lineRenderer.positionCount = bezierPosition.Length;
                _lineRenderer.SetPositions(bezierPosition);
            }
            else
            {
                _lineRenderer.positionCount = _lerpPosition.Length;
                _lineRenderer.SetPositions(_lerpPosition);
            }
        }
    }
}
