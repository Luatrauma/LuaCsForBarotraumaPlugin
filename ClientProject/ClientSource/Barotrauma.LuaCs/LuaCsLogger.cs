using Microsoft.Xna.Framework;

namespace Barotrauma
{
    partial class LuaCsLogger
    {
        private static GUIFrame overlayFrame;
        private static GUITextBlock textBlock;
        private static double showTimer = 0;

        private static void CreateOverlay(string message)
        {
            overlayFrame = new GUIFrame(new RectTransform(new Vector2(0.4f, 0.03f), null), null, new Color(50, 50, 50, 100))
            {
                CanBeFocused = false
            };

            GUILayoutGroup layout = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 0.8f), overlayFrame.RectTransform, Anchor.CenterLeft), false, Anchor.Center)
            {

            };

            textBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), layout.RectTransform), message);
            overlayFrame.RectTransform.MinSize = new Point((int)(textBlock.TextSize.X * 1.2), 0);

            layout.Recalculate();
        }

        public static void AddToGUIUpdateList()
        {
            if (overlayFrame != null && Timing.TotalTime <= showTimer)
            {
                overlayFrame.AddToGUIUpdateList();
            }
        }

        public static void ShowErrorOverlay(string message, float time = 5f, float duration = 1.5f)
        {
            if (Timing.TotalTime <= showTimer)
            {
                return;
            }

            CreateOverlay(message);

            overlayFrame.Flash(Color.Red, duration, true);
            showTimer = Timing.TotalTime + time;
        }
    }
}
