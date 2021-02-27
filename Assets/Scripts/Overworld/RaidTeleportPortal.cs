using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaidTeleportPortal : MonoBehaviour
{
    public GameObject text;
    bool canTeleport = false;

    private void OnTriggerEnter(Collider other)
    {
        text.SetActive(true);
        canTeleport = true;
    }

    private void OnTriggerExit(Collider other)
    {
        text.SetActive(false);
        canTeleport = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (canTeleport)
            {
                SceneManager.LoadScene("RaidSelect", LoadSceneMode.Single);
            }
        }
    }
}
