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
        private const float OutlineWidth = 0.2f;
        private const float EmphasisFadeDuration = 0.15f;
        // Photosensitivity: a smooth opacity pulse at 2 Hz, under the 3 flashes per second that WCAG 2.3.1 treats as
        // the limit, and never a hard on/off flash. The alert also never fades out completely.
        private const float PulseFrequency = 2f;
        private const float PulseMinAlpha = 0.35f;
        private const string ReminderHeadline = "UNUSED SALE STAR";

        private static readonly Color HeadlineColor = new Color(1f, 0.17f, 0.17f);
        private static readonly Color SubheaderColor = new Color(1f, 0.92f, 0.92f);

        private RectTransform rectTransform;
        private ItemInventoryDisplay itemDisplay;
        private readonly Vector3[] corners = new Vector3[4];
        private CanvasGroup canvasGroup;
        private TextMeshProUGUI headline;
        private TextMeshProUGUI subheader;
        private bool visible;
        private bool emphasized;
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
            banner.itemDisplay = hud.itemInventoryDisplay;
            banner.canvasGroup = group;
            banner.headline = CreateText(rect, "Headline", HeadlineColor, FontStyles.Bold);
            banner.subheader = CreateText(rect, "Subheader", SubheaderColor, FontStyles.Normal);
            banner.subheader.gameObject.SetActive(false);
            return banner;
        }

        private static TextMeshProUGUI CreateText(RectTransform parent, string name, Color color, FontStyles style)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform));
            textObject.layer = parent.gameObject.layer;
            textObject.transform.SetParent(parent, false);
            HGTextMeshProUGUI text = textObject.AddComponent<HGTextMeshProUGUI>();
            text.richText = true;
            text.enableWordWrapping = true;
            text.alignment = TextAlignmentOptions.Top;
            text.color = color;
            text.fontStyle = style;
            text.raycastTarget = false;
            // A dark outline keeps the text readable over bright skies and snow.
            text.outlineWidth = OutlineWidth;
            text.outlineColor = Color.black;
            return text;
        }

        // nearChest: the game is circling a chest the star works on, so the alert grows and pulses.
        public void ShowReminder(string subline, bool nearChest)
        {
            Show(ReminderHeadline, subline);
            emphasized = nearChest;
        }

        public void ShowMock(string line, string subline)
        {
            Show(line, subline);
            emphasized = false;
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

            float headlineSize = settings.ReminderFontSize.Value * Mathf.Lerp(1f, settings.NearChestScale.Value, emphasis);
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
                rectTransform.anchoredPosition = new Vector2(0f, GetItemDisplayBottom() - settings.ReminderOffsetY.Value);
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

        // Bottom edge of the box along the top of the screen that the item icons shrink to fit into (the display's own
        // rect, drawn with a translucent background), relative to the top edge of the banner's parent. The box has a
        // fixed height, so the banner stays put however many items there are.
        private float GetItemDisplayBottom()
        {
            if (!itemDisplay || !(rectTransform.parent is RectTransform parent)) return 0f;

            ((RectTransform)itemDisplay.transform).GetWorldCorners(corners);
            return parent.InverseTransformPoint(corners[0]).y - parent.rect.yMax;
        }
    }
}
