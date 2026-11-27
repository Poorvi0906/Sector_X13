using System.Collections.Generic;
using SectorX13.Core;
using UnityEngine;

namespace SectorX13.UI
{
    /// <summary>
    /// Generates crisp procedural 9-sliced rounded rectangles, pills, gradients, and the cyan atom emblem
    /// so uGUI cards, buttons, and badges match the SECTOR X-13 visual specification without external sprite sheets.
    /// </summary>
    public static class X13Visuals
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite GetRoundedRect(int radius = 20, bool borderOnly = false, int borderWidth = 2)
        {
            string key = $"rr_{radius}_{borderOnly}_{borderWidth}";
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            int size = Mathf.Max(64, radius * 4);
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            float r = radius;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(r - x, x - (size - 1 - r), 0f);
                    float dy = Mathf.Max(r - y, y - (size - 1 - r), 0f);
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(r - dist + 0.5f);

                    if (borderOnly)
                    {
                        float innerR = Mathf.Max(1f, r - borderWidth);
                        float innerDx = Mathf.Max(r - x, x - (size - 1 - r), 0f);
                        float innerDy = Mathf.Max(r - y, y - (size - 1 - r), 0f);
                        bool inCorner = innerDx > 0f || innerDy > 0f;
                        float innerDist = inCorner
                            ? Mathf.Sqrt(innerDx * innerDx + innerDy * innerDy)
                            : Mathf.Max(borderWidth - Mathf.Min(x, size - 1 - x, y, size - 1 - y) > 0 ? r : 0f, 0f);
                        if (!inCorner)
                        {
                            int edge = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                            alpha = edge < borderWidth ? 1f : 0f;
                        }
                        else
                        {
                            float innerAlpha = Mathf.Clamp01(innerR - innerDist + 0.5f);
                            alpha = Mathf.Clamp01(alpha - innerAlpha);
                        }
                    }

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();

            var border = new Vector4(radius + 2, radius + 2, radius + 2, radius + 2);
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite GetGradientRoundedRect(Color leftColor, Color rightColor, int radius = 22)
        {
            string key = $"grad_{ColorUtility.ToHtmlStringRGBA(leftColor)}_{ColorUtility.ToHtmlStringRGBA(rightColor)}_{radius}";
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            int w = 256;
            int h = 96;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            float r = radius;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float t = (float)x / (w - 1);
                    Color c = Color.Lerp(leftColor, rightColor, t);
                    float dx = Mathf.Max(r - x, x - (w - 1 - r), 0f);
                    float dy = Mathf.Max(r - y, y - (h - 1 - r), 0f);
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    c.a *= Mathf.Clamp01(r - dist + 0.5f);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();

            var border = new Vector4(radius + 2, radius + 2, radius + 2, radius + 2);
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite GetAtomSprite(int size = 192)
        {
            string key = $"atom_{size}";
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Color clear = new Color(0f, 0f, 0f, 0f);
            Color cyan = X13Theme.CyanAccent;
            float cx = size * 0.5f;
            float cy = size * 0.5f;
            float a = size * 0.40f;
            float b = size * 0.16f;
            float[] angles = { 0f, 60f * Mathf.Deg2Rad, 120f * Mathf.Deg2Rad };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x - cx;
                    float py = y - cy;
                    float bestAlpha = 0f;

                    // Center nucleus dot
                    float nucDist = Mathf.Sqrt(px * px + py * py);
                    bestAlpha = Mathf.Max(bestAlpha, Mathf.Clamp01(size * 0.065f - nucDist + 0.5f));

                    // Three orbital ellipses
                    foreach (float ang in angles)
                    {
                        float cos = Mathf.Cos(ang);
                        float sin = Mathf.Sin(ang);
                        float rx = px * cos + py * sin;
                        float ry = -px * sin + py * cos;
                        float norm = Mathf.Sqrt((rx * rx) / (a * a) + (ry * ry) / (b * b));
                        float ringDist = Mathf.Abs(norm - 1f) * Mathf.Min(a, b);
                        bestAlpha = Mathf.Max(bestAlpha, Mathf.Clamp01(2.6f - ringDist));
                    }

                    Color col = cyan;
                    col.a = bestAlpha;
                    tex.SetPixel(x, y, bestAlpha > 0.01f ? col : clear);
                }
            }
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            Cache[key] = sprite;
            return sprite;
        }
    }
}
