using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class SoundGenerator : MonoBehaviour
{
    public GameObject skeleton;
    public AudioClip[] sounds;
    public GameObject generablePositions;
    public int generableSounds = 3;
    public float generableRange = 3f;

    private Transform[] generablePositionsChildren;
    private List<GameObject> generatedSounds = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //generablePositionsChildren = generablePositions.GetComponentsInChildren<Transform>();
        //generablePositionsChildren = generablePositionsChildren.Where(t => t != generablePositions.transform).ToArray();
        //Generate();
    }

    public void Setup()
    {
        generablePositionsChildren = generablePositions.GetComponentsInChildren<Transform>();
        generablePositionsChildren = generablePositionsChildren.Where(t => t != generablePositions.transform).ToArray();
    }

    public void Regenerate()
    {
        Degenerate();

        Generate();
    }

    public void Generate()
    {
        int[] indices = Enumerable.Range(0, generablePositionsChildren.Length).ToArray();
        indices = Shuffle<int>(indices);

        for (int i = 0; i < generableSounds; i++)
        {
            // Select position to generate
            //int posIndex = Random.Range(0, generablePositionsChildren.Length - 1);

            // Select sound to generate
            int soundIndex = Random.Range(0, sounds.Length - 1);

            // Random locate sound in range from position
            var x = Random.Range(-generableRange, generableRange);
            var z = Random.Range(-generableRange, generableRange);
            Vector3 position = new Vector3(x, 0, z);
            // If vector is bigger than range, normalize
            if (position.magnitude > generableRange) position = position.normalized;

            GameObject sound = Instantiate(skeleton, generablePositionsChildren[indices[i]]) as GameObject;
            sound.name = sounds[soundIndex].name;
            sound.GetComponent<AudioSource>().clip = sounds[soundIndex];
            sound.transform.localPosition = position;
            generatedSounds.Add(sound);
        }
    }

    public void Degenerate()
    {
        foreach (GameObject sound in generatedSounds)
        {
            Destroy(sound);
        }
    }

    T[] Shuffle<T>(T[] array)
    {
        int randomIndex;
        T temp;

        for (int i = 0; i < array.Length; ++i)
        {
            randomIndex = Random.Range(i, array.Length);

            temp = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = temp;
        }
        return array;
    }
}
