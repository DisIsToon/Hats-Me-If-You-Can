using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateController : MonoBehaviour
{
    [SerializeField] private Animator gateAnimator;

    private void OnTriggerEnter(Collider other)
    {
        if (EquipSystem.Instance.IsHoldingGatePass())
        {
            gateAnimator.SetBool("isOpen", true);
            gateAnimator.SetBool("isClose", false);
        }
         


    }

    private void OnTriggerExit(Collider other)
    {
        if (EquipSystem.Instance.IsHoldingGatePass())
        {
            gateAnimator.SetBool("isOpen", false);
            gateAnimator.SetBool("isClose", true);
        }
    }
}
