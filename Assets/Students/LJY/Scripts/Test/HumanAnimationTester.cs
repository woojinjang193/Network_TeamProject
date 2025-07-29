using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAnimationTester : MonoBehaviour
{
    [SerializeField] private Animator humanAnimator;
    void Start()
    {
        humanAnimator =  GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            humanAnimator.Play("Idle");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            humanAnimator.Play("Walk");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            humanAnimator.Play("Run");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            humanAnimator.Play("SideRun");
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            humanAnimator.Play("Jump");
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            humanAnimator.Play("Hit");
        }
    }
}
