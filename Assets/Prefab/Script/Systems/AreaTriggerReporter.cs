using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AreaTriggerReporter : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameTracker.Instance.CheckPlayerInTrigger(this.gameObject);
        }
    }

}
