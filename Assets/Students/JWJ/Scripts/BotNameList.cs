using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BotNameList
{
    public static List<string> botNames = new List<string>();

    private static List<string> initialNames = new List<string>
    {
        "이학권의 작품","황천의 AI","이름뿐인 팀장 장우진","피카소", "반 고흐","먹다남은 치킨",
        "봇이름 추천좀", "점심 뭐먹지", "zZ지존진영Zz", "민수민수최민수", "킹왕짱태우", "oO치우썬더Oo",
        "NullReferenceException", "김첨지", "플밍화이팅", "기능입니다", "봇아님", "마크 하실분"
    };

    public static void Reset()
    {
        botNames = new List<string>(initialNames); //initialNames의 복사본으로 초기화
    }
}
