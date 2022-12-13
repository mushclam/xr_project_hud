using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;



public class AudioSave : MonoBehaviour
{
	public AudioSource source;
	public XRAPI api;
	public Canvas canvas;

	[SerializeField]
    private int interval = 5;
    private int sampleRate;

	private float[] outputSample;
	private static int maxAudioLength;
	private int currentAudioLength = 0;

	private bool audioReadRunning;

	private string savePath;
	private WaitForSeconds wait = new WaitForSeconds(1f);
	private DateTime originTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

	void Awake()
    {
		// Path to save temporary audio files
		savePath = Application.persistentDataPath;
		// Output Sample Rate
		sampleRate = AudioSettings.outputSampleRate;
		// initialize time interval to avoid error
		if (interval <= 0) interval = 5;
		// Length for audio file
		maxAudioLength = sampleRate * interval;
		outputSample = new float[maxAudioLength];
		Debug.Log("Sample Rate: " + sampleRate.ToString());
		Debug.Log("Sample Length: " + maxAudioLength.ToString());
	}

	// Start is called before the first frame update
	void Start()
    {	
		// Deprecated when add audio play trigger
		source.Play();
        // Set running
        audioReadRunning = true;
        // Start inference displaying UI
        StartCoroutine(api.InferenceAudioAndDraw(canvas));
    }

	void OnAudioFilterRead(float[] data, int channels)
    {
		if (!audioReadRunning) return;

		if (currentAudioLength + data.Length > maxAudioLength)
        {
			data = data[..(maxAudioLength - currentAudioLength)];
        }
        try
        {
			data.CopyTo(outputSample, currentAudioLength);
			currentAudioLength += data.Length;

			if (currentAudioLength >= maxAudioLength)
			{
				currentAudioLength = 0;
				var timeStamp = (long)(DateTime.UtcNow - originTime).TotalSeconds;
				// Save audio to temp wav file
				SaveFloatToWav.Save(savePath, timeStamp.ToString(), outputSample, sampleRate, channels);
				var filepath = Path.Combine(savePath, timeStamp.ToString() + ".wav");
				// Send wav file to inference api
				api.audioQueue.Enqueue(filepath);
            }
        }
        catch (Exception e)
        {
			Debug.Log(e.ToString());
			Debug.Log("Error.");
        }
    }
}

public static class SaveFloatToWav
{
	const int HEADER_SIZE = 44;

	public static bool Save(string dir, string filename, float[] audioArr, int hz, int channels)
	{
		if (!filename.ToLower().EndsWith(".wav"))
		{
			filename += ".wav";
		}

		var filepath = Path.Combine(dir, filename);

		// Make sure directory exists if user is saving to sub dir.
		Directory.CreateDirectory(Path.GetDirectoryName(filepath));

        try
        {
			using (var fileStream = CreateEmpty(filepath))
			{
				ConvertAndWrite(fileStream, audioArr);
				WriteHeader(fileStream, audioArr.Length, hz, channels);
			}

			Debug.Log("File Saved.");
			return true;
        }
        catch
        {
			Debug.Log("File Do Not Saved.");
			return false;
        }
	}

	public static AudioClip TrimSilence(AudioClip clip, float min)
	{
		var samples = new float[clip.samples];
		clip.GetData(samples, 0);
		return TrimSilence(new List<float>(samples), min, clip.channels, clip.frequency);
	}

	public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz)
	{
		return TrimSilence(samples, min, channels, hz, false);
	}

	public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz, bool stream)
	{
		int i;

		for (i = 0; i < samples.Count; i++)
		{
			if (Mathf.Abs(samples[i]) > min)
			{
				break;
			}
		}

		samples.RemoveRange(0, i);

		for (i = samples.Count - 1; i > 0; i--)
		{
			if (Mathf.Abs(samples[i]) > min)
			{
				break;
			}
		}

		samples.RemoveRange(i, samples.Count - i);
		var clip = AudioClip.Create("TempClip", samples.Count, channels, hz, stream);
		clip.SetData(samples.ToArray(), 0);

		return clip;
	}

	static FileStream CreateEmpty(string filepath)
	{
		var fileStream = new FileStream(filepath, FileMode.Create);
		byte emptyByte = new byte();

		for (int i = 0; i < HEADER_SIZE; i++) //preparing the header
		{
			fileStream.WriteByte(emptyByte);
		}

		return fileStream;
	}

	static void ConvertAndWrite(FileStream fileStream, float[] samples)
	{
		Int16[] intData = new Int16[samples.Length];
		//converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

		Byte[] bytesData = new Byte[samples.Length * 2];
		//bytesData array is twice the size of
		//dataSource array because a float converted in Int16 is 2 bytes.

		int rescaleFactor = 32767; //to convert float to Int16

		for (int i = 0; i < samples.Length; i++)
		{
			intData[i] = (short)(samples[i] * rescaleFactor);
			Byte[] byteArr = BitConverter.GetBytes(intData[i]);
			byteArr.CopyTo(bytesData, i * 2);
		}

		fileStream.Write(bytesData, 0, bytesData.Length);
	}

	static void WriteHeader(FileStream fileStream, int samples, int hz, int channels)
	{
		fileStream.Seek(0, SeekOrigin.Begin);

		Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
		fileStream.Write(riff, 0, 4);

		Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
		fileStream.Write(chunkSize, 0, 4);

		Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
		fileStream.Write(wave, 0, 4);

		Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
		fileStream.Write(fmt, 0, 4);

		Byte[] subChunk1 = BitConverter.GetBytes(16);
		fileStream.Write(subChunk1, 0, 4);

		//UInt16 two = 2;
		UInt16 one = 1;

		Byte[] audioFormat = BitConverter.GetBytes(one);
		fileStream.Write(audioFormat, 0, 2);

		Byte[] numChannels = BitConverter.GetBytes(channels);
		fileStream.Write(numChannels, 0, 2);

		Byte[] sampleRate = BitConverter.GetBytes(hz);
		fileStream.Write(sampleRate, 0, 4);

		Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
		fileStream.Write(byteRate, 0, 4);

		UInt16 blockAlign = (ushort)(channels * 2);
		fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

		UInt16 bps = 16;
		Byte[] bitsPerSample = BitConverter.GetBytes(bps);
		fileStream.Write(bitsPerSample, 0, 2);

		Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
		fileStream.Write(datastring, 0, 4);

		Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
		fileStream.Write(subChunk2, 0, 4);
	}
}