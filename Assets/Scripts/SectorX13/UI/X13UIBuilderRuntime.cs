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
            var img = go.GetComponent<Image>();
            img.color = bgColor;
            return rt;
        }

        public static RectTransform CreateCard(Transform parent, string name, Color bgColor, Color borderColor, float minHeight = 0f)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = bgColor;

            var outline = go.GetComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(2f, -2f);

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
            float height = 96f,
            int fontSize = 30)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            go.transform.SetParent(parent, false);

            var img = go.GetComponent<Image>();
            img.color = bgColor;

            var outline = go.GetComponent<Outline>();
            outline.effectColor = X13Theme.BorderSubtle;
            outline.effectDistance = new Vector2(2f, -2f);

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
            txtRt.offsetMin = new Vector2(16f, 8f);
            txtRt.offsetMax = new Vector2(-16f, -8f);

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
            var titleLe = titleCol.GetComponent<LayoutElement>();
            titleLe.flexibleWidth = 1f;
            var titleVlg = titleCol.GetComponent<VerticalLayoutGroup>();
            titleVlg.spacing = 4f;
            titleVlg.childControlWidth = true;
            titleVlg.childControlHeight = true;

            CreateText(titleCol.transform, "Title", title, 34, X13Theme.TextPrimary, FontStyle.Bold);
            CreateText(titleCol.transform, "Subtitle", subtitle, 22, X13Theme.TextSecondary, FontStyle.Normal);

            var status = CreateText(headerGo.transform, "Status", statusText, 22, X13Theme.CyanAccent, FontStyle.Bold, TextAnchor.MiddleRight);
            var statusLe = status.gameObject.AddComponent<LayoutElement>();
            statusLe.minWidth = 180f;

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
            vlg.padding = new RectOffset(32, 32, 24, 32);
            vlg.spacing = 24f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = content.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

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
            var navGo = new GameObject("BottomNav", typeof(RectTransform), typeof(Image), typeof(Outline));
            navGo.transform.SetParent(parent, false);
            var rt = navGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(0f, X13Theme.BottomNavHeight);
            rt.anchoredPosition = Vector2.zero;

            navGo.GetComponent<Image>().color = X13Theme.HeaderNavy;
            var outline = navGo.GetComponent<Outline>();
            outline.effectColor = X13Theme.BorderSubtle;
            outline.effectDistance = new Vector2(0f, 2f);

            var hlg = navGo.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 16, 16);
            hlg.spacing = 12f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            var tabs = new (string label, string scene)[]
            {
                ("HOME", X13SceneIds.Home),
                ("EXPERIMENT", X13SceneIds.ChemicalSelection),
                ("LIBRARY", X13SceneIds.ChemicalLibrary),
                ("SAFETY", X13SceneIds.SafetyAssessment)
            };

            for (int i = 0; i < tabs.Length; i++)
            {
                bool active = i == activeTabIndex;
                string target = tabs[i].scene;
                var btn = CreateButton(
                    navGo.transform,
                    $"Tab_{tabs[i].label}",
                    tabs[i].label,
                    active ? X13Theme.CardElevated : X13Theme.HeaderNavy,
                    active ? X13Theme.CyanAccent : X13Theme.TextSecondary,
                    () => onNavigate?.Invoke(target),
                    100f,
                    24);
                btn.GetComponent<Outline>().effectColor = active ? X13Theme.CyanAccent : X13Theme.BorderSubtle;
            }
        }
    }
}
