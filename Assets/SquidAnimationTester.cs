using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquidAnimationTester : MonoBehaviour
{
    private Animator[] animator;
    void Start()
    {
        animator = GetComponentsInChildren<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            transform.Rotate(0, 0, 90);
            foreach (Animator anim in animator)
            {
                anim.Play("Idle A");
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            transform.eulerAngles = Vector3.zero;
            foreach (Animator anim in animator)
            {
                anim.Play("Jump");
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            transform.Rotate(0, 0, 90);
            foreach (Animator anim in animator)
            {
                anim.Play("Swim/Fly");
            }
        }
    }
}
