using UnityEngine;

namespace NanaCiel
{
    public static class TextureExtension
    {
        /// <summary>
        /// 多角形・スクワークルの外周分割数（Squircleのみ使用）
        /// </summary>
        static int segments = 48;
        /// <summary>
        /// スクワークルの丸み p（2=楕円, 4前後=スクワークル）
        /// </summary>
        static float squircleP = 4f;
        /// <summary>
        /// アーチ形の幅スケール（0.5〜1.0 推奨）
        /// </summary>
        static float archWidthScale = 0.9f;
        /// <summary>
        /// アーチの下端Y（0〜1, テクスチャ高さに対する比率, 0=下,1=上）
        /// </summary>
        static float archBottomRatio = 0.05f;

        static float sizeScale = 0.48f;//0.5で元のスプライトギリギリ
        static float ppu = 100f;

        /// <summary>
        /// 六角形
        /// </summary>
        public static Sprite CreateSpriteWithHexagon(this Texture2D tex)
        {
            var sprite = CreateStandardSprite(tex);

            float w = tex.width;
            float h = tex.height;
            Vector2 center = new Vector2(w * 0.5f, h * 0.5f);
            float rx = w * 0.5f;// 横半径
            float ry = h * 0.5f;// 縦半径

            (Vector2[] verts, ushort[] tris) = BuildRegularPolygonWithFan(6, w, h, phaseRad: 0f);
            sprite.OverrideGeometry(verts, tris);
            return sprite;
        }

        /// <summary>
        /// 底が水平な八角形
        /// </summary>
        public static Sprite CreateSpriteWithOctagon(this Texture2D tex)
        {
            var sprite = CreateStandardSprite(tex);

            float w = tex.width;
            float h = tex.height;
            Vector2 center = new Vector2(w * 0.5f, h * 0.5f);
            float rx = w * 0.5f;// 横半径
            float ry = h * 0.5f;// 縦半径

            (Vector2[] verts, ushort[] tris) = BuildRegularPolygonWithFan(8, w, h, phaseRad: -Mathf.PI / 8f);
            sprite.OverrideGeometry(verts, tris);
            return sprite;
        }

        /// <summary>
        /// スクワークル型
        /// </summary>
        public static Sprite CreateSpriteWithSquircle(this Texture2D tex)
        {
            var sprite = CreateStandardSprite(tex);

            float w = tex.width;
            float h = tex.height;
            Vector2 center = new Vector2(w * 0.5f, h * 0.5f);
            float rx = w * 0.5f;// 横半径
            float ry = h * 0.5f;// 縦半径

            (Vector2[] verts, ushort[] tris) = BuildSquircleWithFan(Mathf.Max(segments, 24), w, h, squircleP);
            sprite.OverrideGeometry(verts, tris);
            return sprite;
        }

        /// <summary>
        /// アーチ形
        /// </summary>
        public static Sprite CreateSpriteWithArch(this Texture2D tex)
        {
            var sprite = CreateStandardSprite(tex);

            float w = tex.width;
            float h = tex.height;
            Vector2 center = new Vector2(w * 0.5f, h * 0.5f);
            float rx = w * 0.5f;// 横半径
            float ry = h * 0.5f;// 縦半径

            (Vector2[] verts, ushort[] tris) = BuildArchWithFan(w, h, archWidthScale, archBottomRatio, arcSegments: Mathf.Max(segments, 32));
            sprite.OverrideGeometry(verts, tris);
            return sprite;
        }

        static Sprite CreateStandardSprite(Texture2D tex)
        {
            var rect = new Rect(0, 0, tex.width, tex.height);
            var pivot = new Vector2(0.5f, 0.5f);
            return Sprite.Create(tex, rect, pivot, ppu, 0, SpriteMeshType.Tight);
        }

        // ========= 形状ビルダ =========

        static (Vector2[] verts, ushort[] tris) BuildRegularPolygonWithFan(int n, float w, float h, float phaseRad)
        {
            var cx = w * 0.5f; var cy = h * 0.5f;
            float r = Mathf.Min(w, h) * sizeScale;

            var verts = new Vector2[n + 1];
            verts[0] = new Vector2(cx, cy);
            for (int i = 0; i < n; i++)
            {
                float ang = phaseRad + (Mathf.PI * 2f) * i / n - Mathf.PI * 0.5f; // 上から開始寄り
                verts[i + 1] = new Vector2(cx + r * Mathf.Cos(ang), cy + r * Mathf.Sin(ang));
            }

            var tris = FanIndices(n);
            return (verts, tris);
        }

        static (Vector2[] verts, ushort[] tris) BuildSquircleWithFan(int seg, float w, float h, float p)
        {
            var cx = w * 0.5f; var cy = h * 0.5f;
            float a = w * 0.48f, b = h * sizeScale;

            var verts = new Vector2[seg + 1];
            verts[0] = new Vector2(cx, cy);
            for (int i = 0; i < seg; i++)
            {
                float t = (Mathf.PI * 2f) * i / seg;
                float cp = Mathf.Sign(Mathf.Cos(t)) * Mathf.Pow(Mathf.Abs(Mathf.Cos(t)), 2f / p);
                float sp = Mathf.Sign(Mathf.Sin(t)) * Mathf.Pow(Mathf.Abs(Mathf.Sin(t)), 2f / p);
                verts[i + 1] = new Vector2(cx + a * cp, cy + b * sp);
            }

            var tris = FanIndices(seg);
            return (verts, tris);
        }

        // 上半円 + 縦辺 + 下辺の「アーチ（上丸・下直線）」
        // widthScale: 横幅の割合（0.5〜1.0）、bottomRatio: 下端のy（0=下,1=上）
        static (Vector2[] verts, ushort[] tris) BuildArchWithFan(float w, float h, float widthScale, float bottomRatio, int arcSegments)
        {
            var cx = w * 0.5f; var cy = h * 0.5f;

            float halfW = (w * widthScale) * 0.5f;
            float yBottom = Mathf.Lerp(0f, h, Mathf.Clamp01(bottomRatio));         // 下端Y
            float radius = halfW;                                                  // 上半円の半径
            float yArcCenter = yBottom + radius;                                   // 半円中心
            float yTop = yArcCenter + radius;                                      // 極上（= hに近いほど良い）

            // 端がはみ出さないようにクランプ
            if (yTop > h) { float diff = yTop - h; yBottom -= diff; yArcCenter -= diff; }

            // 周囲点を左上→右上の半円、→右下、→左下、の順で回す（凸形状）
            int rimCount = arcSegments + 2; // 半円(arcSegments) + 両下角2点
            var rim = new Vector2[rimCount];

            // 半円（左端→右端）: θ=180°→0°
            for (int i = 0; i < arcSegments; i++)
            {
                float t = (float)i / (arcSegments - 1);
                float ang = Mathf.PI * (1f - t); // π→0
                float x = cx + radius * Mathf.Cos(ang);
                float y = yArcCenter + radius * Mathf.Sin(ang);
                rim[i] = new Vector2(x, y);
            }

            // 右下 -> 左下
            rim[arcSegments + 0] = new Vector2(cx + halfW, yBottom);
            rim[arcSegments + 1] = new Vector2(cx - halfW, yBottom);

            // 扇分割用に中心 + 周囲
            var verts = new Vector2[rimCount + 1];
            verts[0] = new Vector2(cx, (yArcCenter + yBottom) * 0.5f);
            for (int i = 0; i < rimCount; i++) verts[i + 1] = rim[i];

            var tris = FanIndices(rimCount);
            return (verts, tris);
        }

        static ushort[] FanIndices(int rimCount)
        {
            var tris = new ushort[rimCount * 3];
            for (int i = 0; i < rimCount; i++)
            {
                tris[i * 3 + 0] = 0;
                tris[i * 3 + 1] = (ushort)(1 + i);
                tris[i * 3 + 2] = (ushort)(1 + ((i + 1) % rimCount));
            }
            return tris;
        }
    }
}