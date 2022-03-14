using UnityEngine;
using UnityEngine.UIElements;

namespace PublishersFork
{
    /// <summary>
    /// UIToolkit was launched in version 1.0.0 without the core "HelpBox" API call (which has been part of Unity's
    /// GUI library for about 15 years). They eventually added it post-launch, but that means most current versions
    /// of Unity are missing this class.
    ///
    /// This is a very simple temporary replacement that is API-compatible and can be deleted once you stop supporting
    /// Unity versions older than 2020.1.1
    /// 
    /// The methods here automatically provide a HelpBox API that's compatible with Unity's eventually-added one,
    /// but only in Unity versions where the official one is missing.
    /// </summary>
#if UNITY_2020_1_OR_NEWER
#else
    public enum HelpBoxMessageType
    {
        Error,
        Info,
        None,
        Warning
    }
    public class HelpBox : VisualElement
    {
        public HelpBox( string text, HelpBoxMessageType msgType )
        {
            style.flexDirection = UnityEngine.UIElements.FlexDirection.Row;
            style.alignItems = Align.Center;

            var icon = new Label()
            {
                text = "!", style =
                {
                    width = 40, height = 40, color = Color.black,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    fontSize = 32,
                    marginTop = 10,
                    marginBottom = 10,
                }
            };
            switch (msgType)
            {
                case HelpBoxMessageType.Warning:
                    icon.style.backgroundColor = new Color(1f, 0.93f, 0.44f);
                    icon.text = "!";
                    break;

                case HelpBoxMessageType.Error:
                    icon.style.backgroundColor = new Color(1f, 0.32f, 0.32f);
                    icon.text = "!!!";
                    break;

                case HelpBoxMessageType.Info:
                    icon.style.backgroundColor = new Color(0.81f, 0.76f, 0.77f);
                    icon.text = "?";
                    break;
            }

            Add(icon);

            Add(new Label()
            {
                text = text,
                style = 
                {
                    whiteSpace = WhiteSpace.Normal,

                    paddingBottom = 10,
                    paddingLeft = 10,
                    paddingRight = 10,
                    paddingTop = 10,
                }
            });
        }
        
    }
#endif
}