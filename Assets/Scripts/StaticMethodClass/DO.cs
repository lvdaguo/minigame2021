using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utilities.DataStructures;
using Utilities.StaticMethodClass;

namespace StaticMethodClass
{
    /// <summary>
    /// DOTween方法封装
    /// </summary>
    public static class DO
    {
        public const Ease DefaultCurve = Ease.InOutSine;

        static DO()
        {
            DOTween.defaultEaseType = DefaultCurve;
        }
        
        public static void EnsureColorAlpha(Graphic origin, float target)
        {
            if (FloatComparison.Equal(origin.color.a, target) == false)
            {
                origin.color = new Color
                {
                    r = origin.color.r,
                    g = origin.color.g,
                    b = origin.color.b,
                    a = target
                };
            }
        }
        
        public static IEnumerator FadeInCo(Graphic graphic, float fadeTime, Ease curve = DefaultCurve)
        {
            EnsureColorAlpha(graphic, 0f);
            yield return graphic.DOFade(1f, fadeTime).SetEase(curve).WaitForCompletion();
        }

        public static IEnumerator FadeOutCo(Graphic graphic, float fadeTime, Ease curve = DefaultCurve)
        {
            EnsureColorAlpha(graphic, 1f);
            yield return graphic.DOFade(0f, fadeTime).SetEase(curve).WaitForCompletion();
        }
        
        public static IEnumerator FadeInOutCo(Graphic graphic, float fadeInTime, float fadeOutTime, float stayTime, Ease curve = DefaultCurve)
        {
            yield return FadeInCo(graphic, fadeInTime, curve);
            yield return Wait.Seconds(stayTime);
            yield return FadeOutCo(graphic, fadeOutTime, curve);
        }

        public static IEnumerator FadeInOutCo(Graphic graphic, float fadeTime, float stayTime, Ease curve = DefaultCurve)
        {
            yield return FadeInOutCo(graphic, fadeTime, fadeTime, stayTime, curve);
        }

        public static IEnumerator ScaleXCo(Transform transform, float target, float scaleTime, Ease curve = DefaultCurve)
        {
            yield return transform.DOScaleX(target, scaleTime).SetEase(curve).WaitForCompletion();
        }

        public static IEnumerator ScaleYCo(Transform transform, float target, float scaleTime, Ease curve = DefaultCurve)
        {
            yield return transform.DOScaleY(target, scaleTime).SetEase(curve).WaitForCompletion();
        }

        public static IEnumerator MoveYCo(Transform transform, float target, float moveTime, Ease curve = DefaultCurve)
        {
            yield return transform.DOMoveY(target, moveTime).SetEase(curve);
        }
    }
}