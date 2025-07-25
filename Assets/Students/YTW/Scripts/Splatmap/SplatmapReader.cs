using System;
using UnityEngine;
using UnityEngine.Rendering;

public static class SplatmapReader
{
    // GPU 읽기 요청과 콜백을 함께 관리하기 위한 클래스
    private class ReadbackRequest
    {
        // GPU 읽기가 완료된 후 호출될 콜백 함수를 저장합니다.
        private Action<Color> onCallback;

        public ReadbackRequest(Action<Color> callback)
        {
            this.onCallback = callback;
        }

        // AsyncGPUReadback.Request가 비동기 작업을 완료하면 이 함수를 호출합니다.
        public void OnComplete(AsyncGPUReadbackRequest request)
        {
            if (request.hasError)
            {
                Debug.LogError("GPU가 이 텍스처 형식의 읽기를 지원하지 않거나 그래픽 드라이버에 문제가 있을 수 있습니다.");
                onCallback?.Invoke(Color.clear);
                return;
            }

            if (request.done && request.GetData<float>().Length > 0)
            {
                var tex = new Texture2D(1, 1, TextureFormat.RGBAFloat, false, true);
                tex.LoadRawTextureData(request.GetData<float>());
                tex.Apply();

                Color readColor = tex.GetPixel(0, 0);

                UnityEngine.Object.Destroy(tex);

                // 저장해두었던 콜백 함수를 호출하여 최종 결과를 전달합니다.
                onCallback?.Invoke(readColor);
            }
            else
            {
                onCallback?.Invoke(Color.clear);
            }
        }
    }

    /// <summary>
    /// RenderTexture의 특정 UV 좌표의 픽셀 색상을 비동기적으로 직접 읽어옵니다.
    /// </summary>
    public static void ReadPixel(RenderTexture target, Vector2 uv, Action<Color> callback)
    {
        if (target == null)
        {
            Debug.LogError("SplatmapReader: Target RenderTexture가 null입니다.");
            callback?.Invoke(Color.clear);
            return;
        }

        int x = Mathf.Clamp((int)(uv.x * target.width), 0, target.width - 1);
        int y = Mathf.Clamp((int)(uv.y * target.height), 0, target.height - 1);

        // 콜백을 관리할 ReadbackRequest 객체를 생성합니다.
        var readbackRequest = new ReadbackRequest(callback);

        // GPU에 비동기 읽기를 요청하고, 작업이 끝나면 readbackRequest 객체의 OnComplete 함수를 호출하도록 지정합니다.
        AsyncGPUReadback.Request(target, 0, x, 1, y, 1, 0, 1, TextureFormat.RGBAFloat, readbackRequest.OnComplete);
    }
}
