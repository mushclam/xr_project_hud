using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundObject : MonoBehaviour
{
    private AudioSource myAudio;
    private SphereCollider myCollider;
    private string AudioName;
    private Vector3 position;
    private bool startTrigger = false;
    // Start is called before the first frame update
    void Start()
    {
        myAudio = GetComponent<AudioSource>();
        myCollider = GetComponent<SphereCollider>();
        AudioName = myAudio.name;
        position = transform.position;
        startTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!startTrigger) return;
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Object class : " + AudioName);
            Debug.Log("Object position : " + position);
            myAudio.Play();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!startTrigger) return;
        if (other.gameObject.tag == "Player")
        {
            myAudio.Stop();
        }
    }
}
