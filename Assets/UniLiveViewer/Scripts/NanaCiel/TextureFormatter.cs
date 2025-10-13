using UnityEngine;

namespace NanaCiel
{
    public static class TextureFormatter
    {
        public static Texture2D Resize(Texture2D baseTex, int width = 256, int height = 256)
        {
            var renderTexture = RenderTexture.GetTemporary(
                width,
                height,
                0,//0、16、24、32
                RenderTextureFormat.Default,
                //(RenderTextureFormat)texture2D.format,
                RenderTextureReadWrite.Default);

            //アクティブを設定
            RenderTexture.active = renderTexture;
            Graphics.Blit(baseTex, renderTexture);

            Texture2D result = new Texture2D(width, height, TextureFormat.RGB24, false);
            result.ReadPixels(new Rect(0, 0, width, height),0,0,false);
            result.Apply();

            //解放
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);

            return result;
        }
    }
}