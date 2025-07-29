using UnityEngine;

public static class Util
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null)
        {
            comp = go.AddComponent<T>();
        }
        return comp;
    }
    
    public static bool ExtractTrailNumber(in string input, out int number)
    {
        number = -1;
        if(string.IsNullOrEmpty(input))
        {
            Debug.LogWarning("입력이 없습니다");
            return false;
        }
            
        int i = input.Length - 1;
        
        while (i >= 0 && char.IsDigit(input[i])) i--; // 숫자가 아닐때까지 string 맨 끝에서부터 체크
        
        if(i == input.Length - 1)
        {
            Debug.LogWarning("입력에 숫자가 없습니다");
            return false;
        }
        return int.TryParse(input[(i + 1)..], out number); // 여기서 .. 키워드는 input 끝까지를 나타낸다.
        
        // 아래와 같다.
        // string result = "";
        // for (; i < input.Length; i++)
        // {
        //     result += input[i];
        // }
        // return int.TryParse(result, out number); // 이거와 같다. 결과값이 무조건 0 이상이므로 true가 된다.
    }
}
