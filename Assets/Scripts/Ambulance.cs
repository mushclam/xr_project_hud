using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ambulance : MonoBehaviour
{
    private AudioSource myAudio;
    private SphereCollider myCollider;
    private bool playing = false;
    // Start is called before the first frame update
    void Start()
    {
        myAudio = GetComponent<AudioSource>();
        myCollider = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        if (myAudio.isPlaying)
        {
            myCollider.enabled = false;
        }
        else
        {
            myCollider.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            myAudio.Play();
            Debug.Log("ambulance crash!!!");
        }
    }
}
