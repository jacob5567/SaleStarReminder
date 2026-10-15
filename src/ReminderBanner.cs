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
        private const float SubheaderScale = 0.5f;
        private const float OutlineWidth = 0.2f;
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
        private float shownFontSize = -1f;

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

        public void ShowReminder(string subline)
        {
            Show(ReminderHeadline, subline);
        }

        public void ShowMock(string line, string subline)
        {
            Show(line, subline);
        }

        public void Hide()
        {
            visible = false;
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
            float fontSize = settings.ReminderFontSize.Value;
            if (fontSize != shownFontSize)
            {
                shownFontSize = fontSize;
                headline.fontSize = fontSize;
                subheader.fontSize = fontSize * SubheaderScale;
            }
            float target = visible ? 1f : 0f;
            if (visible || canvasGroup.alpha > 0f)
            {
                rectTransform.anchoredPosition = new Vector2(0f, GetItemDisplayBottom() - settings.ReminderOffsetY.Value);
            }
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, Time.unscaledDeltaTime / FadeDuration);
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
