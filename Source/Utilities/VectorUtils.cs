using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralParts
{
    public static class VectorUtils
    {

        public static Vector3 xz(this Vector3 v)
        {
            return new Vector3(v.x, 0, v.z);
        }

        public static Vector4 toVec4(this Vector3 v, float w)
        {
            return new Vector4(v.x, v.y, v.z, w);
        }

        public static bool CloseTo(this Vector3 v1, Vector3 v2)
        {
            return (v2 - v1).magnitude < float.Epsilon;
        }

        public static bool CloseTo(this Vector2 v1, Vector2 v2)
        {
            return (v2 - v1).magnitude < float.Epsilon;
        }

        public static Vector2 SlerpNormal(Vector2 n1, Vector2 n2, float t)
        {
            if (n1 == n2)
                return n1;
            float omega = Mathf.Acos(Vector2.Dot(n1, n2));
            float sinOmega = Mathf.Sin(omega);
            return Mathf.Sin(omega * (1f - t)) / sinOmega * n1 + Mathf.Sin(omega * t) / sinOmega * n2;
        }
    }
}
