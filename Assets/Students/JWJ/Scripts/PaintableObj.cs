using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PaintableObj : MonoBehaviour
{
    private Material blendMaterial; // 합칠때 알파등 계산할 메터리얼
    private Material splatMaterial; // 그려질 넓이, 강도 등 계산하는 메터리얼 (붓 같은 용도)
    private Material visualMeterial; // 플레이어에게 보여질 메터리얼

    public RenderTexture splatMap; //최종적으로 그려질 텍스쳐
    private RenderTexture tempMap; // 합치기전 임시로 그릴 텍스쳐

    private CommandBuffer buffer;  // 그래픽 명령 (명령을 모아서 한번에 작업함)

    [SerializeField] private int textureSize = 1024;

    private TeamColorInfo teamColorInfo;

    //쉐이더 프로퍼티들을 캐싱 한번만 생성하면 되니까 static으로 
    private static int RadiusID = Shader.PropertyToID("_Radius");  //반지름 
    private static int HardnessID = Shader.PropertyToID("_Hardness"); // 선명도
    private static int StrengthID = Shader.PropertyToID("_Strength"); // 강도
    private static int SplatPosID = Shader.PropertyToID("_SplatPos"); // 그릴 위치
    private static int InkColorID = Shader.PropertyToID("_InkColor"); // 팀 인풋 색깔

    private void Awake()
    {
        //포톤뷰 넣었는지 체크
        if (GetComponent<PhotonView>() == null)
        {
            Debug.LogError($"PhotonView 없음: {gameObject.name}");
        }
    }


    private void Start()
    {
        teamColorInfo = FindObjectOfType<TeamColorInfo>();

        splatMap = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGBFloat);
        //잉크가 최종으로 저장될 텍스쳐 생성

        //RenderTexture.active = splatMap;
        splatMap.Create();
        //GPU에 할당해줌

        tempMap = new RenderTexture(splatMap.width, splatMap.height, 0, RenderTextureFormat.ARGBFloat);
        //임시 텍스쳐 생성 splatMap과 같은 크기로 생성

        //splatMaterial = new Material(Shader.Find("Unlit/SplatMask"));
        splatMaterial = new Material(Resources.Load<Shader>("Shaders/SplatMask"));
        // 쉐이더를 넣어서 splatMaterial 생성

        //blendMaterial = new Material(Shader.Find("Unlit/Blend"));
        blendMaterial = new Material(Resources.Load<Shader>("Shaders/Blend"));
        // 쉐이더를 넣어서 blendMaterial 생성

        visualMeterial = GetComponent<Renderer>().material;
        // 오브젝트 표면에 잉크효과를 표시하기위해 메터리얼 정보 가져옴

        visualMeterial.SetTexture("_Splatmap", splatMap);
        //오브젝트의 텍스쳐를 splatMap로 지정

        buffer = new CommandBuffer();
        //그래픽 명령어를 담아둘 공간
    }

    public void DrawInk(Vector3 worldPos, float radius, float hardness, float strength, Team team) //그리기 함수
    {
        //팀정보를 토대로 팀 인풋컬러를 가져옴
        Color teamInputColor = teamColorInfo.GetTeamInputColor(team);

        //visualMetarial에 넣어줄 팀컬러
        Color team1Color = teamColorInfo.Team1Color;
        Color team2Color = teamColorInfo.Team2Color;
        
        //visualMeterial 컬러 세팅
        visualMeterial.SetColor("Color1", team1Color);
        visualMeterial.SetColor("Color2", team2Color);

        //받아온 값들을 넣어줌
        splatMaterial.SetFloat(RadiusID, radius);
        splatMaterial.SetFloat(HardnessID, hardness);
        splatMaterial.SetFloat(StrengthID, strength);
        splatMaterial.SetVector(SplatPosID, worldPos);
        splatMaterial.SetVector(InkColorID, teamInputColor);

        buffer.SetRenderTarget(tempMap);
        // tempMap맵에 먼저 그리기위해 타겟으로 설정 

        buffer.DrawRenderer(GetComponent<Renderer>(), splatMaterial, 0);
        //splatMaterial 로 그림을 그려줌

        buffer.SetRenderTarget(splatMap);
        // 임시맵에 그리고 splatMap을 타겟으로 설정

        buffer.Blit(tempMap, splatMap, blendMaterial);
        // blendMaterial을 이용해 temMap에 있는걸 splatMap에 합침

        Graphics.ExecuteCommandBuffer(buffer);
        //그래픽 작업 실행

        buffer.Clear();
        //작업 목록 비워줌
    }

}
