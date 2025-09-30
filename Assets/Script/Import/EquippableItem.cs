using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EquippableItem : MonoBehaviour
{
    public Animator animator;
    public CharacterController characterController; // ✅ Replaced FirstPersonController

    public static EquippableItem Instance { get; set; }

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (characterController == null)
        {
            characterController = FindObjectOfType<CharacterController>();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) //&& // Left Mouse Button
            //InventorySystem.Instance.isOpen == false &&
            //CraftingSystem.Instance.isOpen == false &&
            //QuestManager.Instance.isQuestMenuOpen == false &&
            //DialogSystem.Instance.dialogUIActive == false &&
            )
        {
            //animator.SetTrigger("hit");
        }

        // ✅ Check movement using CharacterController
        if (characterController != null && characterController.velocity.magnitude > 0.1f)
        {
            //animator.SetBool("runWithPlayer", true);
        }
        else
        {
            //animator.SetBool("runWithPlayer", false);
        }
    }

    // THIS IS RAYCAST
    // NEED TO MAKE ONE FOR COLLIDER INSTEAD 
    public void GetHit()
    {
        GameObject selectedStone = SelectionManager.Instance.selectedStone;

        if (selectedStone != null)
        {
            //selectedStone.GetComponent<MinableStone>().GetHit();
        }
    }

    IEnumerator SwingSoundDelay()
    {
        yield return new WaitForSeconds(0.1f);

        SoundManager.Instance.PlaySound(SoundManager.Instance.axeSwingSound);
    }
}
