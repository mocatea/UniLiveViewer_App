using UnityEngine;

namespace NanaCiel
{
    public static class TextExtension
    {
        /// <summary>
        /// bounds基準でフォントサイズを調整してテキスト設定
        /// 視認性も加味して縮小下限はBaseの50%
        /// </summary>
        public static void SetAutoSizedText(this TextMesh textMesh, string text, float maxWidth, int baseFontSize = 40)
        {
            if (textMesh == null || string.IsNullOrEmpty(text) || maxWidth <= 0f)
            {
                return;
            }

            textMesh.text = text;
            textMesh.fontSize = baseFontSize;

            // 一時的に強制更新する(boundsに反映)
            textMesh.gameObject.SetActive(false);
            textMesh.gameObject.SetActive(true);

            var bounds = textMesh.GetComponent<Renderer>().bounds;
            float actualWidth = bounds.size.x;

            float resizeFactor = maxWidth / actualWidth;
            textMesh.fontSize = Mathf.Clamp((int)(baseFontSize * resizeFactor), baseFontSize / 2, baseFontSize);
        }

        /// <summary>
        /// n文字毎に改行を入れる（末尾は除く）
        /// </summary>
        public static string InsertNewline(this string input, int lineBreakPosition = 20)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var builder = new System.Text.StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                builder.Append(input[i]);
                if ((i + 1) % lineBreakPosition == 0 && (i + 1) < input.Length)
                {
                    builder.Append('\n');
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// 文字幅か文字数がオーバーなら、切り捨てて省略文字を末尾に追記
        /// </summary>
        public static string TruncateWithEllipsis(this string input, int maxFontWidth = 40, int maxFontLength = 20, string abbreviation = "...")
        {
            if (string.IsNullOrEmpty(input)) return null;

            // 表示幅判定(ex: 半角なら40文字以下はセーフ)
            if (GetDisplayWidth(input) <= maxFontWidth) return input;

            return $"{input.Substring(0, Mathf.Min(input.Length, maxFontLength))}{abbreviation}";
        }

        static int GetDisplayWidth(string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;

            int width = 0;
            foreach (char c in input)
            {
                width += IsFullWidth(c) ? 2 : 1;
            }
            return width;
        }

        static bool IsFullWidth(char c)
        {
            // 全角文字の範囲（ひらがな、カタカナ、漢字、全角記号など）
            return c >= '\u3000' && c <= '\uFF60' || c >= '\uFFE0' && c <= '\uFFEF';
        }
    }
}