using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class EquippableItem : MonoBehaviour
{
    public Animator animator;
    public CharacterController characterController; // optional for run anims

    // internal state
    bool isEquipped = false;
    Rigidbody rb;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (characterController == null)
        {
            characterController = FindObjectOfType<CharacterController>();
        }

        // When spawned in hand, you usually want it kinematic until thrown
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        // Optional: sync movement anims with player
        if (characterController != null)
        {
            bool isMoving = characterController.velocity.magnitude > 0.1f;
            // animator.SetBool("runWithPlayer", isMoving);
        }

        // 🚫 NO input or throw logic here anymore
    }

    public void OnEquip()
    {
        isEquipped = true;
        gameObject.SetActive(true);

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;   // held in hand, no physics
        }

        // animator?.SetTrigger("equip");
    }

    public void OnUnequip()
    {
        isEquipped = false;

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // animator?.SetTrigger("unequip");
        gameObject.SetActive(false);
    }

    public void OnThrow()
    {
        // just animation hook
        // animator?.SetTrigger("throw");
    }

    public void GetHit()
    {
        // handle damage/hit here if needed
    }

    IEnumerator SwingSoundDelay()
    {
        yield return new WaitForSeconds(0.1f);
        SoundManager.Instance.PlaySound(SoundManager.Instance.axeSwingSound);
    }
}
