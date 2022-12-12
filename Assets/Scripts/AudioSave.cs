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
	public AudioClip clip;
	public XRAPI api;
	public Canvas canvas;

	private float[] clip_data;

	private int channels;
	private int frequency;

	private int samples;
	private int rate;
	private float[] output_sample;
	private AudioClip new_clip;

	private int n = 0;

	private WaitForSeconds wait = new WaitForSeconds(1f);

    // Start is called before the first frame update
    void Start()
    {
		// var samples = clip.samples;
		rate = AudioSettings.outputSampleRate;
		samples = rate * 1;
		channels = 1;
		Debug.Log("Sample Rate: " + rate.ToString());
		Debug.Log("Sample Length: " + samples.ToString());
		Debug.Log("Channels: " + channels.ToString());

		source.Play();
		//samples = source.clip.samples;
		//channels = source.clip.channels;
		//frequency = source.clip.frequency;

		//Debug.Log(samples.ToString());
		//Debug.Log(channels.ToString());
		//Debug.Log(frequency.ToString());

		//var cd = Directory.GetCurrentDirectory();
		//Debug.Log(cd);

		//clip_data = new float[samples];
		//source.clip.GetData(clip_data, 0);
		//new_clip = AudioClip.Create("TestClip", samples, channels, rate, false);
		//new_clip.SetData(clip_data, 0);
		//SavWav.Save("test", new_clip);

		output_sample = new float[samples];
		//StartCoroutine(ListenToFile());
		StartCoroutine(api.GenerateRequest(canvas));
	}

    // Update is called once per frame
    void Update()
    {
		/*AudioListener.GetOutputData(output_sample, 0);
		Debug.Log(output_sample[10].ToString());*/
		//new_clip.SetData(output_sample, 0);
		//SavWav.Save("TestClip", new_clip);
		//float[] total = new float[8192 * 50];
  //      float[] spectrum = new float[8192];

  //      //AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
		//if (n < 50)
  //      {
		//	AudioListener.GetOutputData(spectrum, 0);
		//	spectrum.CopyTo(total, n * 8192);
		//	n++;
  //      }

		//if (n == 50)
  //      {
		//	AudioClip tmp = AudioClip.Create("tmp", 8192 * 50, channels, rate, false);
		//	tmp.SetData(total, 0);
		//	SavWav.Save("test" + n.ToString(), tmp);
		//}

		//Debug.Log(n.ToString());

        //for (int i = 1; i < spectrum.Length - 1; i++)
        //{
        //    Debug.DrawLine(new Vector3(i - 1, spectrum[i] + 10, 0), new Vector3(i, spectrum[i + 1] + 10, 0), Color.red);
        //    Debug.DrawLine(new Vector3(i - 1, Mathf.Log(spectrum[i - 1]) + 10, 2), new Vector3(i, Mathf.Log(spectrum[i]) + 10, 2), Color.cyan);
        //    Debug.DrawLine(new Vector3(Mathf.Log(i - 1), spectrum[i - 1] - 10, 1), new Vector3(Mathf.Log(i), spectrum[i] - 10, 1), Color.green);
        //    Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log(spectrum[i - 1]), 3), new Vector3(Mathf.Log(i), Mathf.Log(spectrum[i]), 3), Color.blue);
        //}
    }

	public IEnumerator ListenToFile()
    {
		Debug.Log("Start Listening");
		int n = 0;
		while (true)
        {
			AudioListener.GetOutputData(output_sample, 0);
			AudioClip tmp = AudioClip.Create("tmp", samples, channels, rate, false);
			tmp.SetData(output_sample, 0);
			SavWav.Save("test"+n.ToString(), tmp);
			n++;
			yield return wait;
		}
    }
}


public static class SavWav
{

	const int HEADER_SIZE = 44;

	public static bool Save(string filename, AudioClip clip)
	{
		if (!filename.ToLower().EndsWith(".wav"))
		{
			filename += ".wav";
		}

		var filepath = Path.Combine(Application.persistentDataPath, filename);

		Debug.Log(filepath);

		// Make sure directory exists if user is saving to sub dir.
		Directory.CreateDirectory(Path.GetDirectoryName(filepath));

		using (var fileStream = CreateEmpty(filepath))
		{

			ConvertAndWrite(fileStream, clip);

			WriteHeader(fileStream, clip);
		}

		Debug.Log("File Saved.");

		return true; // TODO: return false if there's a failure saving the file
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

	static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
	{

		var samples = new float[clip.samples];

		clip.GetData(samples, 0);

		Int16[] intData = new Int16[samples.Length];
		//converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

		Byte[] bytesData = new Byte[samples.Length * 2];
		//bytesData array is twice the size of
		//dataSource array because a float converted in Int16 is 2 bytes.

		int rescaleFactor = 32767; //to convert float to Int16

		for (int i = 0; i < samples.Length; i++)
		{
			intData[i] = (short)(samples[i] * rescaleFactor);
			Byte[] byteArr = new Byte[2];
			byteArr = BitConverter.GetBytes(intData[i]);
			byteArr.CopyTo(bytesData, i * 2);
		}

		fileStream.Write(bytesData, 0, bytesData.Length);
	}

	static void WriteHeader(FileStream fileStream, AudioClip clip)
	{

		var hz = clip.frequency;
		var channels = clip.channels;
		var samples = clip.samples;

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

		UInt16 two = 2;
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

		//		fileStream.Close();
	}
}