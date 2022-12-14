using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class SoundGenerator : MonoBehaviour
{
    public GameObject ground;
    public GameObject[] sounds;
    public GameObject generablePositions;
    public int generableSounds = 3;

    private Transform[] generablePositionsChildren;

    private float range = 5;

    // Start is called before the first frame update
    void Start()
    {
        generablePositionsChildren = generablePositions.GetComponentsInChildren<Transform>().Where(t => t != generablePositions.transform).ToArray();
        int[] indices = Enumerable.Range(0, generablePositionsChildren.Length).ToArray();
        indices = Shuffle<int>(indices);

        for (int i = 0; i < generableSounds; i++)
        {
            // Select position to generate
            //int posIndex = Random.Range(0, generablePositionsChildren.Length - 1);

            // Select sound to generate
            int soundIndex = Random.Range(0, sounds.Length - 1);

            // Random locate sound in range from position
            var x = Random.Range(-range, range);
            var z = Random.Range(-range, range);
            Vector3 position = new Vector3(x, 0, z);
            // If vector is bigger than range, normalize
            if (position.magnitude > range) position = position.normalized;

            GameObject sound = Instantiate(sounds[soundIndex], generablePositionsChildren[indices[i]]) as GameObject;
            sound.transform.localPosition = position;
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
