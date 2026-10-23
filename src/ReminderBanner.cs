using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaleStarReminder
{
    // Large red headline across the top of the HUD with an optional smaller line under it. Shows either the Sale Star
    // reminder or, for a few seconds after a wasted star, the mocking message.
    internal sealed class ReminderBanner : MonoBehaviour
    {
        private const float FadeDuration = 0.2f;
        private const float SideMargin = 40f;
        private const float EmphasisFadeDuration = 0.045f;
        // Photosensitivity: a smooth opacity pulse at 2 Hz, under the 3 flashes per second that WCAG 2.3.1 treats as
        // the limit, and never a hard on/off flash. The alert also never fades out completely.
        private const float PulseFrequency = 2f;
        private const float PulseMinAlpha = 0.35f;
        private const string ReminderHeadline = "UNUSED SALE STAR";
        // The mocking line is a full sentence, so it reads better smaller than the alert. Relative to ReminderFontSize.
        private const float MockHeadlineScale = 0.5f;

        private static readonly Color HeadlineColor = new Color(1f, 0.17f, 0.17f);
        private static readonly Color SubheaderColor = new Color(1f, 0.92f, 0.92f);

        private RectTransform rectTransform;
        // The banner hangs below this box's bottom edge.
        private RectTransform topBox;
        private readonly Vector3[] corners = new Vector3[4];
        private CanvasGroup canvasGroup;
        private TextMeshProUGUI headline;
        private TextMeshProUGUI subheader;
        private bool visible;
        private bool emphasized;
        // Kept after Hide, so the text keeps its size while it fades out.
        private bool mocking;
        private float emphasis;
        private float pulseTime;
        private float shownHeadlineSize = -1f;
        private float shownSubheaderSize = -1f;

        public static ReminderBanner Create(HUD hud)
        {
            Transform container = hud.mainContainer.transform;
            GameObject root = new GameObject("SaleStarReminderBanner", typeof(RectTransform));
            root.layer = container.gameObject.layer;
            RectTransform rect = (RectTransform)root.transform;
            rect.SetParent(container, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(-2f * SideMargin, 0f);

            root.AddComponent<LayoutElement>().ignoreLayout = true;

            VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = root.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CanvasGroup group = root.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            ReminderBanner banner = root.AddComponent<ReminderBanner>();
            banner.rectTransform = rect;
            banner.canvasGroup = group;
            banner.headline = CreateText(rect, "Headline", HeadlineColor);
            banner.subheader = CreateText(rect, "Subheader", SubheaderColor);
            if (RiskUICompat.IsRiskUIHud(hud))
            {
                // RiskUI puts the items at the bottom by default, so the banner goes below its stack along the top
                // instead. The stack reserves room for the boss bar and spectator label even while they're hidden.
                banner.topBox = RiskUICompat.FindTopCenterCluster(hud);
                RiskUICompat.ApplyFont(hud, banner.headline);
                RiskUICompat.ApplyFont(hud, banner.subheader);
            }
            else if (hud.itemInventoryDisplay)
            {
                banner.topBox = (RectTransform)hud.itemInventoryDisplay.transform;
            }
            banner.subheader.gameObject.SetActive(false);
            return banner;
        }

        private static TextMeshProUGUI CreateText(RectTransform parent, string name, Color color)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform));
            textObject.layer = parent.gameObject.layer;
            textObject.transform.SetParent(parent, false);
            HGTextMeshProUGUI text = textObject.AddComponent<HGTextMeshProUGUI>();
            text.richText = true;
            text.enableWordWrapping = true;
            text.alignment = TextAlignmentOptions.Top;
            text.color = color;
            text.raycastTarget = false;
            // No outline or bold: the game font's material already draws a drop shadow, and either one on top of it
            // pushes each glyph past the font atlas's padding, so a sliver of the glyph above it in the atlas shows as
            // a thin line over the text. The font is heavy enough without bold.
            return text;
        }

        // nearChest: the game is circling a chest the star works on, so the alert grows and pulses.
        public void ShowReminder(string subline, bool nearChest)
        {
            Show(ReminderHeadline, subline);
            emphasized = nearChest;
            mocking = false;
        }

        public void ShowMock(string line, string subline)
        {
            Show(line, subline);
            emphasized = false;
            mocking = true;
        }

        public void Hide()
        {
            visible = false;
            emphasized = false;
        }

        private void Show(string line, string subline)
        {
            visible = true;
            if (headline.text != line)
            {
                headline.text = line;
            }
            bool hasSubline = !string.IsNullOrEmpty(subline);
            if (subheader.gameObject.activeSelf != hasSubline)
            {
                subheader.gameObject.SetActive(hasSubline);
            }
            if (hasSubline && subheader.text != subline)
            {
                subheader.text = subline;
            }
        }

        private void LateUpdate()
        {
            PluginConfig settings = SaleStarReminderPlugin.Settings;
            emphasis = Mathf.MoveTowards(emphasis, emphasized ? 1f : 0f, Time.unscaledDeltaTime / EmphasisFadeDuration);

            float headlineSize = settings.ReminderFontSize.Value
                * (mocking ? MockHeadlineScale : Mathf.Lerp(1f, settings.NearChestScale.Value, emphasis));
            if (headlineSize != shownHeadlineSize)
            {
                shownHeadlineSize = headlineSize;
                headline.fontSize = headlineSize;
            }
            UpdatePulse(settings.PulseNearChest.Value);
            float subheaderSize = settings.SubheaderFontSize.Value;
            if (subheaderSize != shownSubheaderSize)
            {
                shownSubheaderSize = subheaderSize;
                subheader.fontSize = subheaderSize;
            }
            float target = visible ? 1f : 0f;
            if (visible || canvasGroup.alpha > 0f)
            {
                float normalY = GetTopBoxBottom() - settings.ReminderOffsetY.Value;
                float centerY = normalY;
                if (rectTransform.parent is RectTransform parent)
                {
                    centerY = -(parent.rect.height * 0.5f) + rectTransform.rect.height * 0.5f;
                }
                float t = Mathf.SmoothStep(0f, 1f, emphasis);
                rectTransform.anchoredPosition = new Vector2(0f, Mathf.Lerp(normalY, centerY, t));
            }
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, Time.unscaledDeltaTime / FadeDuration);
        }

        // Starts at full opacity each time the player comes near a chest, so the alert never pops in half faded.
        private void UpdatePulse(bool enabled)
        {
            float alpha = 1f;
            if (enabled && emphasis > 0f)
            {
                pulseTime += Time.unscaledDeltaTime;
                float wave = 0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * PulseFrequency * pulseTime);
                alpha = Mathf.Lerp(1f, PulseMinAlpha, wave * emphasis);
            }
            else
            {
                pulseTime = 0f;
            }
            if (headline.alpha != alpha)
            {
                headline.alpha = alpha;
            }
        }

        // Bottom edge of the box along the top of the screen, relative to the top edge of the banner's parent. In the
        // vanilla HUD that is the box the item icons shrink to fit into (the display's own rect, drawn with a
        // translucent background). Either box has a fixed height, so the banner stays put however many items there are.
        private float GetTopBoxBottom()
        {
            if (!topBox || !(rectTransform.parent is RectTransform parent)) return 0f;

            topBox.GetWorldCorners(corners);
            return parent.InverseTransformPoint(corners[0]).y - parent.rect.yMax;
        }
    }
}
