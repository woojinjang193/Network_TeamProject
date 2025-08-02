using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AnimationRiggingWeight : MonoBehaviour
{
    private Rig rigging;
    private float targetWeight;
    void Start()
    {
        rigging = GetComponent<Rig>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            targetWeight = 1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            targetWeight = 0;
        }
        if (rigging.weight == targetWeight) return;
        rigging.weight = Mathf.Lerp(rigging.weight, targetWeight, Time.deltaTime* 10f);
    }
}
