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
        _lineRenderer.positionCount = _positions.Length;
        _lineRenderer.SetPositions(_positions);
        _lerpPosition = new Vector3[_positions.Length];

        _lerpAudio = new float[_initiatorPointAmount];

        _matInstance = new Material(material);
        _lineRenderer.material = _matInstance;
    }

    private void Update()
    {
        _matInstance.SetColor("_EmissionColor", color * audioPeer.GetAudioBandBuffer(audioBandMaterial) * emissionMultiplier);

        if (_generationCount != 0)
        {
            int count = 0;
            for (int i = 0; i < _initiatorPointAmount; i++)
            {
                _lerpAudio[i] = audioPeer.GetAudioBandBuffer(audioBand[i]);
                for (int j = 0; j < (_positions.Length - 1) / _initiatorPointAmount; j++)
                {
                    _lerpPosition[count] = Vector3.Lerp(_positions[count], _targetPosition[count], _lerpAudio[i]);
                    count++;
                }
            }
            _lerpPosition[count] = Vector3.Lerp(_positions[count], _targetPosition[count], _lerpAudio[_initiatorPointAmount - 1]);


            if (useBezierCurves)
            {
                _bezierPosition = BezierCurve(_lerpPosition, bezierVertexCount);
                _lineRenderer.positionCount = _bezierPosition.Length;
                _lineRenderer.SetPositions(_bezierPosition);
            }
            else
            {
                _lineRenderer.positionCount = _lerpPosition.Length;
                _lineRenderer.SetPositions(_lerpPosition);
            }
        }
    }
}
