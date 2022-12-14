using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using TMPro;

public class EvaluationManager : MonoBehaviour
{
    public int maxTest = 5;

    public GameObject player;
    private PlayerNavigation navigator;
    private SoundGenerator generator;

    public TextMeshProUGUI testNumber;

    private WaitUntil waitUntilArrival;
    private WaitForSeconds wait = new WaitForSeconds(3f);
    // Start is called before the first frame update
    void Start()
    {
        generator = GetComponent<SoundGenerator>();
        navigator = player.GetComponent<PlayerNavigation>();
        StartCoroutine(Evaluation());
    }

    IEnumerator Evaluation()
    {
        yield return new WaitUntil(() => navigator.agent != null);
        generator.Setup();

        waitUntilArrival = new WaitUntil(() => (player.transform.position - navigator.destination.position).magnitude < 0.1);
        for (int i = 0; i < maxTest; i++)
        {
            testNumber.text = "Test: " + i;

            generator.Generate();
            navigator.agent.enabled = true;
            navigator.agent.destination = navigator.destination.position;
            yield return waitUntilArrival;

            generator.Degenerate();
            navigator.agent.enabled = false;
            player.transform.position = Vector3.zero;
            testNumber.text = "Test: " + (i+1) + "(Ready)";
            yield return wait;
        }
    }
}
