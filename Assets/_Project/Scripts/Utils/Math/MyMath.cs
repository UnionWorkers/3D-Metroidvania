using UnityEngine;
using UnityEngine.UIElements;

namespace Utils.Math
{
    public static class MyMath
    {
        public enum ClampMode
        {
            ZeroToOne,
            MinusOneToOne
        }
        public static float ClampMinusOneToOne(float value)
        {
            return Mathf.Clamp(value, -1, 1);
        }

        public static Vector3 ClampVector(Vector3 vector, float minValue, float maxValue)
        {
            return new(Mathf.Clamp(vector.x, minValue, maxValue), Mathf.Clamp(vector.y, minValue, maxValue), Mathf.Clamp(vector.z, minValue, maxValue));
        }

        public static Vector3 ClampVector(Vector3 vector, ClampMode clampMode)
        {
            switch (clampMode)
            {
                case ClampMode.ZeroToOne:
                    vector = ClampVector(vector, 0, 1);
                    break;

                case ClampMode.MinusOneToOne:
                    vector = ClampVector(vector, -1, 1);
                    break;

            }
            
            return vector;
        }

    }

}