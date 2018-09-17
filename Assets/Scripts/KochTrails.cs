using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KochTrails : KochGenerator
{

    public class TrailObject
    {
        public GameObject GO;
        public TrailRenderer Trail;
        public int CurrentTargetNum;
        public Vector3 TargetPosition;
        public Color EmisssionColor;
    }

    [HideInInspector]
    public List<TrailObject> trails;

    [Header("Trail Properties")]
    public GameObject trailPrefab;
    public AnimationCurve trailWidthCurve;
    [Range(0, 8)]
    public int trailEndCapVertices;
    public Material trailMaterial;
    public Gradient trailColor;

    [Header("Audio")]
    public AudioPeer audioPeer;
    public int[] audioBand;
    public Vector2 speedMinMax, widthMinMax, trailTimeMinMax;
    public float colorMultiplier;

    private float _lerpPosSpeed;
    private float _distanceSnap;
    private Color _startColor, _endColor;

    private void Start()
    {
        trails = new List<TrailObject>();
        _startColor = new Color(0, 0, 0, 0);
        _endColor = new Color(0, 0, 0, 1);
        for (int i = 0; i < initiatorPointAmount; i++)
        {
            GameObject trailInstance = Instantiate(trailPrefab, transform.position, Quaternion.identity, transform);
            TrailObject trailObjectInstance = new TrailObject
            {
                GO = trailInstance,
                Trail = trailInstance.GetComponent<TrailRenderer>(),
                EmisssionColor = trailColor.Evaluate(i * (1.0f / initiatorPointAmount)),
            };
            trailObjectInstance.Trail.material = new Material(trailMaterial);
            trailObjectInstance.Trail.numCapVertices = trailEndCapVertices;
            trailObjectInstance.Trail.widthCurve = trailWidthCurve;
            Vector3 instantiatePosition;
            if (generationCount > 0)
            {
                int step;
                if (useBezierCurves)
                {
                    step = bezierPosition.Length / initiatorPointAmount;
                    instantiatePosition = bezierPosition[i * step];
                    trailObjectInstance.CurrentTargetNum = (i * step) + 1;
                    trailObjectInstance.TargetPosition = bezierPosition[trailObjectInstance.CurrentTargetNum];

                }
                else
                {
                    step = positions.Length / initiatorPointAmount;
                    instantiatePosition = positions[i * step];
                    trailObjectInstance.CurrentTargetNum = (i * step) + 1;
                    trailObjectInstance.TargetPosition = positions[trailObjectInstance.CurrentTargetNum];

                }
            }
            else
            {
                instantiatePosition = positions[i];
                trailObjectInstance.CurrentTargetNum = i + 1;
                trailObjectInstance.TargetPosition = positions[trailObjectInstance.CurrentTargetNum];
            }
            trailObjectInstance.GO.transform.localPosition = instantiatePosition;
            trails.Add(trailObjectInstance);
        }
    }

    private void Update()
    {
        Movement();
        AudioBehavior();
    }

    private void Movement()
    {
        _lerpPosSpeed = Mathf.Lerp(speedMinMax.x, speedMinMax.y, audioPeer.GetAmplitude());
        for (int i = 0; i < trails.Count; i++)
        {
            _distanceSnap = Vector3.Distance(trails[i].GO.transform.localPosition, trails[i].TargetPosition);
            if (_distanceSnap < 0.05f)
            {
                trails[i].GO.transform.localPosition = trails[i].TargetPosition;
                if (useBezierCurves && generationCount > 0)
                {
                    if (trails[i].CurrentTargetNum < bezierPosition.Length - 1)
                    {
                        trails[i].CurrentTargetNum++;
                    }
                    else
                    {
                        trails[i].CurrentTargetNum = 1;

                    }
                    trails[i].TargetPosition = bezierPosition[trails[i].CurrentTargetNum];
                }
                else
                {
                    if (trails[i].CurrentTargetNum < positions.Length - 1)
                    {
                        trails[i].CurrentTargetNum++;
                    }
                    else
                    {
                        trails[i].CurrentTargetNum = 1;
                    }
                    trails[i].TargetPosition = targetPosition[trails[i].CurrentTargetNum];
                }
            }
            trails[i].GO.transform.localPosition = Vector3.MoveTowards(trails[i].GO.transform.localPosition, trails[i].TargetPosition, Time.deltaTime * _lerpPosSpeed);
        }
    }

    private void AudioBehavior()
    {
        for (int i = 0; i < initiatorPointAmount; i++)
        {
            Color colorLerp = Color.Lerp(_startColor, trails[i].EmisssionColor * colorMultiplier, audioPeer.GetAudioBand(audioBand[i]));
            trails[i].Trail.material.SetColor("_EmissionColor", colorLerp);

            colorLerp = Color.Lerp(_startColor, _endColor, audioPeer.GetAudioBand(audioBand[i]));
            trails[i].Trail.material.SetColor("Color", colorLerp);

            float widthLerp = Mathf.Lerp(widthMinMax.x, widthMinMax.y, audioPeer.GetAudioBandBuffer(audioBand[i]));
            trails[i].Trail.widthMultiplier = widthLerp;

            float timeLerp = Mathf.Lerp(trailTimeMinMax.x, trailTimeMinMax.y, audioPeer.GetAudioBandBuffer(audioBand[i]));
            trails[i].Trail.time = timeLerp;

        }
    }
}
