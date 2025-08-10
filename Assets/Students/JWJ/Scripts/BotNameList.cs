using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BotNameList
{
    public static List<string> botNames = new List<string>();

    private static List<string> initialNames = new List<string>
    {
        "이학권의작품","황천의AI","이름뿐인팀장장우진","피카소", "반고흐","먹다남은치킨",
        "봇이름추천좀", "점심뭐먹지", "zZ지존진영Zz", "민수민수최민수", "킹왕짱태우", "oO치우썬더Oo",
        "NullReferenceException", "김첨지", "플밍화이팅", "기능입니다", "봇아님", "마크하실분",
        "교회는영어로", "Null", "이거왜안되지", "이거왜되지", "살려주세요", "사이타마", "배아프네",
        "호드를위하여", "변절자", "비열한얼라이언스", "대대장님", "살빼야하는데", "봇입니다"
    };

    public static void Reset()
    {
        botNames = new List<string>(initialNames); //initialNames의 복사본으로 초기화
    }
}
