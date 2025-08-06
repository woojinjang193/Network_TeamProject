using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.Rendering.Universal;

public class HumanFace : MonoBehaviour
{
    [Header("0:Idle 1:Upset 2:Hit")] 
    [SerializeField] public List<GameObject> faceModel = new(3);

    public GameObject GetFace(FaceType type)
    {
        switch (type)
        {
            case FaceType.Idle:
                return faceModel[0];
            case FaceType.Upset:
                return faceModel[1];
            case FaceType.Hit:
                return faceModel[2];
            default :
                return null;
        }
    }
}
public enum FaceType { Idle,Upset, Hit}
