using System;
using SectorX13.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SectorX13.UI
{
    /// <summary>
    /// Runtime uGUI factory producing responsive 9:16 portrait components styled to the SECTOR X-13 specification.
    /// </summary>
    public static class X13UIBuilderRuntime
    {
        public static Canvas CreatePortraitCanvas(string name, int sortingOrder = 10)
        {
            EnsureEventSystem();

            var canvasGo = new GameObject(name);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(X13Theme.ReferenceWidth, X13Theme.ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            var existing = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
            if (existing != null)
            {
                if (existing.GetComponent<InputSystemUIInputModule>() == null)
                {
                    var legacy = existing.GetComponent<StandaloneInputModule>();
                    if (legacy != null) UnityEngine.Object.Destroy(legacy);
                    existing.gameObject.AddComponent<InputSystemUIInputModule>();
                }
                return;
            }

            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<InputSystemUIInputModule>();
        }

        public static RectTransform CreatePanel(Transform parent, string name, Color bgColor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = bgColor;
            return rt;
        }

        public static RectTransform CreateCard(Transform parent, string name, Color bgColor, Color borderColor, float minHeight = 0f, int radius = 24)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = X13Visuals.GetRoundedRect(radius);
            img.type = Image.Type.Sliced;
            img.color = bgColor;

            // Crisp rounded border overlay
            var borderGo = new GameObject("Border", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            borderGo.transform.SetParent(go.transform, false);
            borderGo.GetComponent<LayoutElement>().ignoreLayout = true;
            var brt = borderGo.GetComponent<RectTransform>();
            brt.anchorMin = Vector2.zero;
            brt.anchorMax = Vector2.one;
            brt.offsetMin = Vector2.zero;
            brt.offsetMax = Vector2.zero;
            var bimg = borderGo.GetComponent<Image>();
            bimg.sprite = X13Visuals.GetRoundedRect(radius, borderOnly: true, borderWidth: 2);
            bimg.type = Image.Type.Sliced;
            bimg.color = borderColor;
            bimg.raycastTarget = false;

            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(32, 32, 26, 26);
            vlg.spacing = 12f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            if (minHeight > 0f)
            {
                var le = go.AddComponent<LayoutElement>();
                le.minHeight = minHeight;
            }

            return go.GetComponent<RectTransform>();
        }

        public static RectTransform CreateRow(Transform parent, string name, float spacing = 16f)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            go.transform.SetParent(parent, false);
            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = spacing;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            return go.GetComponent<RectTransform>();
        }

        public static Image CreateAtomIcon(Transform parent, string name, float size = 140f)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var le = go.GetComponent<LayoutElement>();
            le.minWidth = size;
            le.preferredWidth = size;
            le.minHeight = size;
            le.preferredHeight = size;

            var img = go.GetComponent<Image>();
            img.sprite = X13Visuals.GetAtomSprite();
            img.preserveAspect = true;
            img.raycastTarget = false;
            return img;
        }

        public static RectTransform CreatePillBadge(Transform parent, string name, string text, Color borderAndTextColor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var le = go.GetComponent<LayoutElement>();
            le.minHeight = 46f;
            le.preferredHeight = 46f;

            var img = go.GetComponent<Image>();
            img.sprite = X13Visuals.GetRoundedRect(22, borderOnly: true, borderWidth: 2);
            img.type = Image.Type.Sliced;
            img.color = borderAndTextColor;

            var txt = CreateText(go.transform, "Label", text, 21, borderAndTextColor, FontStyle.Bold, TextAnchor.MiddleCenter);
            var trt = txt.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(18f, 4f);
            trt.offsetMax = new Vector2(-18f, -4f);
            return go.GetComponent<RectTransform>();
        }

        public static Text CreateText(
            Transform parent,
            string name,
            string content,
            int fontSize,
            Color color,
            FontStyle style = FontStyle.Normal,
            TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var txt = go.GetComponent<Text>();
            txt.font = X13Theme.DefaultFont;
            txt.text = content;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.fontStyle = style;
            txt.alignment = alignment;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.supportRichText = true;
            return txt;
        }

        public static Button CreateButton(
            Transform parent,
            string name,
            string label,
            Color bgColor,
            Color textColor,
            Action onClick,
            float height = 92f,
            int fontSize = 28,
            bool primaryGradient = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var img = go.GetComponent<Image>();
            if (primaryGradient)
            {
                img.sprite = X13Visuals.GetGradientRoundedRect(new Color(0.02f, 0.90f, 0.98f, 1f), new Color(0.08f, 0.58f, 0.98f, 1f), 22);
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.sprite = X13Visuals.GetRoundedRect(20);
                img.type = Image.Type.Sliced;
                img.color = bgColor;

                var borderGo = new GameObject("Border", typeof(RectTransform), typeof(Image));
                borderGo.transform.SetParent(go.transform, false);
                var brt = borderGo.GetComponent<RectTransform>();
                brt.anchorMin = Vector2.zero;
                brt.anchorMax = Vector2.one;
                brt.offsetMin = Vector2.zero;
                brt.offsetMax = Vector2.zero;
                var bimg = borderGo.GetComponent<Image>();
                bimg.sprite = X13Visuals.GetRoundedRect(20, borderOnly: true, borderWidth: 2);
                bimg.type = Image.Type.Sliced;
                bimg.color = X13Theme.BorderSubtle;
                bimg.raycastTarget = false;
            }

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null)
            {
                btn.onClick.AddListener(() => onClick());
            }

            var le = go.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;

            var txt = CreateText(go.transform, "Label", label, fontSize, textColor, FontStyle.Bold, TextAnchor.MiddleCenter);
            var txtRt = txt.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(18f, 8f);
            txtRt.offsetMax = new Vector2(-18f, -8f);

            return btn;
        }

        public static RectTransform CreateHeader(
            Transform parent,
            string title,
            string subtitle,
            string statusText,
            bool showBack,
            Action onBack)
        {
            var headerGo = new GameObject("Header", typeof(RectTransform), typeof(Image));
            headerGo.transform.SetParent(parent, false);
            var rt = headerGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, X13Theme.HeaderHeight);
            rt.anchoredPosition = Vector2.zero;

            headerGo.GetComponent<Image>().color = X13Theme.HeaderNavy;

            var hlg = headerGo.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(28, 28, 24, 20);
            hlg.spacing = 20f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            if (showBack)
            {
                var backBtn = CreateButton(headerGo.transform, "BackButton", "<", X13Theme.CardSurface, X13Theme.CyanAccent, onBack, 76f, 34);
                var backLe = backBtn.GetComponent<LayoutElement>();
                backLe.minWidth = 76f;
                backLe.preferredWidth = 76f;
            }

            var titleCol = new GameObject("TitleGroup", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            titleCol.transform.SetParent(headerGo.transform, false);
            titleCol.GetComponent<LayoutElement>().flexibleWidth = 1f;
            var titleVlg = titleCol.GetComponent<VerticalLayoutGroup>();
            titleVlg.spacing = 4f;
            titleVlg.childControlWidth = true;
            titleVlg.childControlHeight = true;

            CreateText(titleCol.transform, "Title", title, 34, X13Theme.TextPrimary, FontStyle.Bold);
            CreateText(titleCol.transform, "Subtitle", subtitle, 22, X13Theme.TextSecondary, FontStyle.Normal);

            var status = CreateText(headerGo.transform, "Status", statusText, 22, X13Theme.CyanAccent, FontStyle.Bold, TextAnchor.MiddleRight);
            status.gameObject.AddComponent<LayoutElement>().minWidth = 180f;

            return rt;
        }

        public static RectTransform CreateScrollBody(Transform parent, float topOffset, float bottomOffset)
        {
            var scrollGo = new GameObject("ScrollBody", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollGo.transform.SetParent(parent, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = new Vector2(0f, bottomOffset);
            scrollRt.offsetMax = new Vector2(0f, -topOffset);
            scrollGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(scrollGo.transform, false);
            var vpRt = viewport.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero;
            vpRt.offsetMax = Vector2.zero;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = Vector2.zero;

            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(32, 32, 22, 32);
            vlg.spacing = 22f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sr = scrollGo.GetComponent<ScrollRect>();
            sr.viewport = vpRt;
            sr.content = contentRt;
            sr.horizontal = false;
            sr.vertical = true;
            sr.scrollSensitivity = 35f;

            return contentRt;
        }

        public static void CreateBottomNav(Transform parent, int activeTabIndex, Action<string> onNavigate)
        {
            var navGo = new GameObject("BottomNav", typeof(RectTransform), typeof(Image));
            navGo.transform.SetParent(parent, false);
            var rt = navGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(0f, X13Theme.BottomNavHeight);
            rt.anchoredPosition = Vector2.zero;

            navGo.GetComponent<Image>().color = X13Theme.HeaderNavy;

            var hlg = navGo.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 16, 16);
            hlg.spacing = 12f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            var tabs = new (string label, string scene)[]
            {
                ("Home", X13SceneIds.Home),
                ("Experiment", X13SceneIds.ChemicalSelection),
                ("Library", X13SceneIds.ChemicalLibrary),
                ("Safety", X13SceneIds.SafetyAssessment)
            };

            for (int i = 0; i < tabs.Length; i++)
            {
                bool active = i == activeTabIndex;
                string target = tabs[i].scene;
                CreateButton(
                    navGo.transform,
                    $"Tab_{tabs[i].label}",
                    tabs[i].label,
                    active ? X13Theme.CardElevated : X13Theme.HeaderNavy,
                    active ? X13Theme.CyanAccent : X13Theme.TextSecondary,
                    () => onNavigate?.Invoke(target),
                    100f,
                    24);
            }
        }
    }
}
