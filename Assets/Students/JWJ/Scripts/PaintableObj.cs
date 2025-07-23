using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Burst.Intrinsics.X86.Avx;

public class PaintableObj : MonoBehaviour
{
    private Material blendMaterial; // 합칠때 알파등 계산할 메터리얼
    private Material splatMaterial; // 그려질 넓이, 강도 등 계산하는 메터리얼 (붓 같은 용도)
    private Material visualMeterial; // 플레이어에게 보여질 메터리얼

    private RenderTexture splatMap; //최종적으로 그려질 텍스쳐
    private RenderTexture tempMap; // 합치기전 임시로 그릴 텍스쳐

    private CommandBuffer buffer;  // 그래픽 명령 (명령을 모아서 한번에 작업함)

    private int textureSize = 1024;


    private void Start()
    {
        splatMap = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGBFloat);
        //잉크가 최종으로 저장될 텍스쳐 생성

        tempMap = new RenderTexture(splatMap.width, splatMap.height, 0, RenderTextureFormat.ARGBFloat);//
        //임시 텍스쳐 생성 splatMap과 같은 크기로 생성

        splatMaterial = new Material(Shader.Find("Unlit/SplatMask"));//
        // 쉐이더를 넣어서 splatMaterial 생성

        blendMaterial = new Material(Shader.Find("Unlit/Blend"));
        // 쉐이더를 넣어서 blendMaterial 생성

        visualMeterial = GetComponent<Renderer>().material;//
        // 오브젝트 표면에 잉크효과를 표시하기위해 메터리얼 정보 가져옴

        visualMeterial.SetTexture("_Splatmap", splatMap);//
        //오브젝트의 텍스쳐를 splatMap로 지정

        buffer = new CommandBuffer();//
        //그래픽 명령어를 담아둘 공간
    }

    public void DrawInk(Vector3 worldPos, float radius, float hardness, float strength, Team team) //그리기 함수
    {
        Color teamColor = FindObjectOfType<TeamColorInfo>().GetTeamColor(team);
        //팀정보를 토대로 팀컬러를 가져옴

        splatMaterial.SetFloat(Shader.PropertyToID("_Radius"), radius);  //반지름 
        splatMaterial.SetFloat(Shader.PropertyToID("_Hardness"), hardness); // 선명도
        splatMaterial.SetFloat(Shader.PropertyToID("_Strength"), strength); // 강도
        splatMaterial.SetVector(Shader.PropertyToID("_SplatPos"), worldPos); // 그릴 위치
        splatMaterial.SetVector(Shader.PropertyToID("_InkColor"), teamColor); // 팀 색깔


        buffer.SetRenderTarget(tempMap);
        // tempMap맵에 먼저 그리기위해 타겟으로 설정 

        buffer.DrawRenderer(GetComponent<Renderer>(), splatMaterial, 0);
        //splatMaterial 로 그림을 그려줌

        buffer.SetRenderTarget(splatMap);
        // 임시맵에 그리고 splatMap을 타겟으로 설정

        buffer.Blit(tempMap, splatMap, blendMaterial);
        // alphaCombiner을 이용해 temMap에 있는걸 splatMap에 합침

        Graphics.ExecuteCommandBuffer(buffer);
        //그래픽 작업 실행

        buffer.Clear();
        //작업 목록 비워줌


    }
}
