using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SectorX13.Lab
{
    /// <summary>
    /// Procedurally generates hollow 3D glass beaker meshes, soft bubble particle textures,
    /// and a glowing cyan holographic lab pad texture compatible with URP.
    /// </summary>
    public static class BeakerMeshGenerator
    {
        public static Mesh CreateHollowBeakerMesh(float outerRadius = 0.24f, float thickness = 0.018f, float height = 0.62f, int segments = 36)
        {
            var mesh = new Mesh { name = "X13_HollowBeaker" };
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var uvs = new List<Vector2>();

            float innerRadius = Mathf.Max(0.05f, outerRadius - thickness);
            float rimRadius = outerRadius + thickness * 0.7f;
            float floorY = 0.025f;

            // Profile ring points (radius, y) from bottom center -> outer wall -> rim -> inner wall -> inner floor
            var profile = new Vector2[]
            {
                new Vector2(0f, 0f),
                new Vector2(outerRadius, 0f),
                new Vector2(outerRadius, height - 0.03f),
                new Vector2(rimRadius, height),
                new Vector2(innerRadius, height),
                new Vector2(innerRadius, floorY),
                new Vector2(0f, floorY)
            };

            for (int s = 0; s <= segments; s++)
            {
                float u = (float)s / segments;
                float ang = u * Mathf.PI * 2f;
                float cos = Mathf.Cos(ang);
                float sin = Mathf.Sin(ang);

                for (int p = 0; p < profile.Length; p++)
                {
                    float r = profile[p].x;
                    float y = profile[p].y;
                    verts.Add(new Vector3(r * cos, y, r * sin));
                    uvs.Add(new Vector2(u, (float)p / (profile.Length - 1)));
                }
            }

            int stride = profile.Length;
            for (int s = 0; s < segments; s++)
            {
                int rowA = s * stride;
                int rowB = (s + 1) * stride;
                for (int p = 0; p < stride - 1; p++)
                {
                    int a0 = rowA + p;
                    int a1 = rowA + p + 1;
                    int b0 = rowB + p;
                    int b1 = rowB + p + 1;

                    tris.Add(a0); tris.Add(b0); tris.Add(a1);
                    tris.Add(a1); tris.Add(b0); tris.Add(b1);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Material CreateGlassMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Universal Render Pipeline/Simple Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader);
            mat.name = "X13_GlassMat";
            mat.SetFloat("_Surface", 1f); // Transparent
            mat.SetFloat("_Blend", 0f);   // Alpha
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_Smoothness", 0.92f);
            mat.SetFloat("_Metallic", 0.05f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3000;
            mat.color = new Color(0.55f, 0.88f, 0.98f, 0.24f);
            return mat;
        }

        public static Material CreateLiquidMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Simple Lit") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader);
            mat.name = "X13_LiquidMat";
            mat.SetFloat("_Smoothness", 0.75f);
            mat.color = color;
            return mat;
        }

        public static Material CreateHoloPadMaterial()
        {
            var tex = CreateHoloGridTexture(256);
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
            var mat = new Material(shader);
            mat.name = "X13_HoloPadMat";
            mat.mainTexture = tex;
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 2900;
            mat.color = Color.white;
            return mat;
        }

        public static Material CreateBubbleParticleMaterial()
        {
            var tex = CreateSoftCircleTexture(64);
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Universal Render Pipeline/Unlit");
            var mat = new Material(shader);
            mat.name = "X13_BubbleMat";
            mat.mainTexture = tex;
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.One);
            mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3100;
            mat.color = new Color(0.45f, 0.95f, 1.0f, 0.85f);
            return mat;
        }

        private static Texture2D CreateHoloGridTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Color bg = new Color(0.02f, 0.12f, 0.20f, 0.55f);
            Color cyan = new Color(0.05f, 0.88f, 0.98f, 0.95f);
            Color grid = new Color(0.05f, 0.65f, 0.85f, 0.35f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool border = x < 4 || x >= size - 4 || y < 4 || y >= size - 4;
                    bool line = (x % 32 < 2) || (y % 32 < 2);
                    Color c = border ? cyan : (line ? grid : bg);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateSoftCircleTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(1f - r);
                    a = a * a;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
