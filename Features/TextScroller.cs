using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

namespace Timeline.Features
{
    public class TextScroller
    {
        private static Camera ScrollerCamera;
        private static GameObject Scroller;
        private static float StartTime;
        private static float ReadingTime;

        // translates along local Y axis
        public class MoveUp : MonoBehaviour
        {
            private TextMeshPro textMeshPro;
            private const float speed = 0.015f;

            void Start()
            {
                textMeshPro = GetComponent<TextMeshPro>();
            }

            void Update()
            {
                if (ReadingTime - Time.time + StartTime < 0)
                {
                    StartCoroutine(FadeText.FadeOutText(textMeshPro));
                }

                // allow skipping via whatever keys
                var abortKeys = new[] {KeyCode.Escape, KeyCode.Space, KeyCode.Mouse0};
                if (abortKeys.Any(Input.GetKeyDown))
                {
                    Destroy(GameObject.Find("Scroller Camera"));
                    Destroy(GameObject.Find("Scroller"));
                }
                else
                {
                    transform.Translate(0, speed, 0);
                }
            }
        }

        public class FadeText : MonoBehaviour
        {
            private static float speed = 0.3f;

            internal static IEnumerator FadeOutText(TextMeshPro text)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
                while (text.color.a > 0.0f)
                {
                    text.color = new Color(
                        text.color.r, text.color.g, text.color.b, text.color.a - Time.deltaTime * speed);
                    yield return null;
                }

                yield return new WaitForSeconds(1.25f);
                Destroy(GameObject.Find("Scroller Camera"));
                Destroy(GameObject.Find("Scroller"));
            }
        }

        internal static void CreateScroller(string text)
        {
            try
            {
                // approximate time to read instead of measuring distance
                // text fades after this time
                const float secondsPerCharacter = 0.12f;
                ReadingTime = text.Length * secondsPerCharacter;
                // found this .depth and .layer stuff online somewhere, maybe extreme values
                ScrollerCamera = new GameObject("Scroller Camera")
                    .AddComponent<Camera>();
                ScrollerCamera.depth = 100f;
                ScrollerCamera.transform.Translate(0, 20, 40);
                ScrollerCamera.backgroundColor = Color.black;
                ScrollerCamera.cullingMask = 1 << 30;

                Scroller = new GameObject("Scroller");
                Scroller.gameObject.AddComponent<MoveUp>();
                Scroller.gameObject.layer = 30;

                var scrollText = Scroller.AddComponent<TextMeshPro>();
                StartTime = Time.time;
                scrollText.font = UnityEngine.Resources
                    .FindObjectsOfTypeAll<TMP_FontAsset>()
                    .First(f => f.name == "Proxima Nova Semibold SDF");
                scrollText.alignment = TextAlignmentOptions.TopJustified;
                // tilt it and position it offscreen
                scrollText.transform.Rotate(Vector3.right, 80);
                scrollText.transform.Translate(0, 55, 0);
                scrollText.fontSize = 40f;

                var scrollRt = scrollText.GetComponentInChildren<RectTransform>();
                scrollText.SetText(text);
                scrollRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 45);
            }
            catch (Exception e)
            {
                Main.HBSLog?.LogException(e);
            }
        }
    }
}
