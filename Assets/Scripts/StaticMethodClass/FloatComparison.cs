using UnityEngine;

namespace StaticMethodClass
{
    public static class FloatComparison
    {
        public enum CompareResult
        {
            Equal,
            Less,
            Greater
        }
        
        public const float Epsilon = 1e-6f;

        public static bool Equal(float lhs, float rhs)
        {
            return Mathf.Abs(lhs - rhs) <= Epsilon;
        }

        public static CompareResult Compare(float lhs, float rhs)
        {
            if (Equal(lhs, rhs))
            {
                return CompareResult.Equal;
            }
            return lhs < rhs ? CompareResult.Less : CompareResult.Greater;
        }
    }
}