using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : MonoBehaviour
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
            Debug.Log("Object class : Dog");
            Debug.Log("Object position : "+transform.position);
            myAudio.Play();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            myAudio.Stop();
        }
    }
}
