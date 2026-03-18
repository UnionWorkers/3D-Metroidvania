using UnityEngine;

namespace Utils.Math
{
    public enum EasingFunctionType
    {
        EaseOutSine,
        EaseInSine,
        EaseInOutSine
    }

    public static class EasingFunctions
    {
        public static float EasingFunction(EasingFunctionType easingFunctionType, float x)
        {
            switch (easingFunctionType)
            {
                case EasingFunctionType.EaseOutSine:
                    return EaseOutSine(x);
                case EasingFunctionType.EaseInSine:
                    return EaseInSine(x);
                case EasingFunctionType.EaseInOutSine:
                    return EaseInOutSine(x);
            }
            return 0;
        }

        private static float EaseOutSine(float x)
        {
            return Mathf.Sin((x * Mathf.PI) / 2);
        }

        private static float EaseInSine(float x)
        {
            return 1 - Mathf.Cos((x * Mathf.PI) / 2);
        }

        private static float EaseInOutSine(float x)
        {
            return -(Mathf.Cos(Mathf.PI * x) - 1) / 2;
        }
    }

}