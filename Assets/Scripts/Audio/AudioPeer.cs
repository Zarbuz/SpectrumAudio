using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPeer : MonoBehaviour
{
    [SerializeField] private float audioProfile = 0.0f;
    [SerializeField] private int samples = 512;
    [SerializeField] private int frequencyBands = 8;
    [SerializeField] private FFTWindow window = FFTWindow.Blackman;

    // cached
    private AudioSource cached_AudioSource;
    private float[] cached_Samples;
    private float[] cached_FreqBands;
    private float[] cached_FreqBandBuffer;
    private float[] cached_FreqBufferDecrease;
    private float[] cached_FreqBandHighest;
    private float[] cached_AudioBand;
    private float[] cached_AudioBandBuffer;

    private float cached_Amplitude;
    private float cached_AmplitudeBuffer;
    private float cached_AmplitudeHighest;

    public float[] Samples
    {
        get { return cached_Samples; }
    }

    void Awake()
    {
        cached_AudioSource = GetComponent<AudioSource>();
        cached_Samples = new float[samples];
        cached_FreqBands = new float[frequencyBands];
        cached_FreqBandBuffer = new float[frequencyBands];
        cached_FreqBufferDecrease = new float[frequencyBands];
        cached_FreqBandHighest = new float[frequencyBands];
        cached_AudioBand = new float[frequencyBands];
        cached_AudioBandBuffer = new float[frequencyBands];
    }

    void Start()
    {
        InitialiseAudioProfile(audioProfile);
    }

    void Update()
    {
        GenerateFrequencyBands();
        GenerateAudioBands();
        GenerateAmplitude();
    }

    public float GetAmplitude()
    {
        return cached_Amplitude;
    }

    public float GetAmplitudeBuffer()
    {
        return cached_AmplitudeBuffer;
    }

    public float GetFrequencyBand(int i)
    {
        return cached_FreqBands[i];
    }

    public float GetFrequencyBandBuffer(int i)
    {
        return cached_FreqBandBuffer[i];
    }

    public float GetAudioBand(int i)
    {
        return cached_AudioBand[i];
    }

    public float GetAudioBandBuffer(int i)
    {
        return cached_AudioBandBuffer[i];
    }

    private void InitialiseAudioProfile(float value)
    {
        for (int i = 0; i < cached_FreqBandHighest.Length; ++i)
        {
            cached_FreqBandHighest[i] = value;
        }
    }

    private void GenerateAmplitude()
    {
        cached_Amplitude = 0.0f;
        cached_AmplitudeBuffer = 0.0f;

        int length = Mathf.Min(cached_AudioBand.Length, cached_AudioBandBuffer.Length);
        for (int i = 0; i < length; ++i)
        {
            cached_Amplitude += cached_AudioBand[i];
            cached_AmplitudeBuffer += cached_AudioBandBuffer[i];
        }

        if (cached_Amplitude > cached_AmplitudeHighest)
        {
            cached_AmplitudeHighest = cached_Amplitude;
        }
        cached_Amplitude /= cached_AmplitudeHighest;
        cached_AmplitudeBuffer /= cached_AmplitudeHighest;
    }

    private void GenerateAudioBands()
    {
        for (int i = 0; i < cached_FreqBands.Length; ++i)
        {
            if (cached_FreqBands[i] > cached_FreqBandHighest[i])
            {
                cached_FreqBandHighest[i] = cached_FreqBands[i];
            }
            cached_AudioBand[i] = cached_FreqBands[i] / cached_FreqBandHighest[i];
            cached_AudioBandBuffer[i] = cached_FreqBandBuffer[i] / cached_FreqBandHighest[i];
        }
    }

    private void GenerateFrequencyBands()
    {
        // 22050 / 512 = 43Hz per sample
        // 20 - 60
        // 60 - 250
        // 250 - 500
        // 500 - 2000
        // 2000 - 4000
        // 4000 - 6000
        // 6000 - 20000

        cached_AudioSource.GetSpectrumData(cached_Samples, 0, FFTWindow.Blackman);

        int count = 0;
        for (int i = 0; i < frequencyBands; ++i)
        {
            int sampleCount = (int)Mathf.Pow(2, i) * 2;
            if (i == 7) sampleCount += 2;

            float average = 0.0f;
            for (int j = 0; j < sampleCount; ++j)
            {
                average += cached_Samples[count] * (count + 1);
                ++count;
            }
            average /= count;
            cached_FreqBands[i] = average;
        }

        ModulateFrequencyBands();
    }

    private void ModulateFrequencyBands()
    {
        for (int i = 0; i < cached_FreqBandBuffer.Length; ++i)
        {
            if (cached_FreqBands[i] > cached_FreqBandBuffer[i])
            {
                cached_FreqBandBuffer[i] = cached_FreqBands[i];
                cached_FreqBufferDecrease[i] = 0.005f;
            }

            if (cached_FreqBands[i] < cached_FreqBandBuffer[i])
            {
                cached_FreqBandBuffer[i] -= cached_FreqBufferDecrease[i];
                cached_FreqBufferDecrease[i] *= 1.2f;
            }
        }
    }


}
