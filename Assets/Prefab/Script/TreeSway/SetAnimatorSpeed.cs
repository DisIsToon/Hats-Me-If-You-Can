using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SetAnimatorSpeed : MonoBehaviour
{
    private Animator animator;
    [SerializeField] float low = .3f;
    [SerializeField] float high = 1.5f;
    [SerializeField] bool shouldStartWithOffset = false;
    [SerializeField] Vector2 startOffset; // value 1 will send to end of timeline

    void Start() {
        animator = GetComponent<Animator>() as Animator;
        animator.speed = Random.Range(low, high);
        if (shouldStartWithOffset) animator.Play(0, -1, Random.Range(startOffset.x, startOffset.y)); 
    }
}
