using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralParts.Geometry
{
    public class ContourPoint
    {
        // Fields...
        private int _Index;
        private ContourProfile _Section;

        public int Index
        {
            get { return _Index; }
        }

        public ContourProfile Section
        {
            get { return _Section; }
        }

        public bool GeneratedSeam { get; set; }

        public Vector2 Position { get; set; }

        public Vector2 Normal { get; set; }

        /// <summary>
        /// UV value used for horizontal texturing 
        /// </summary>
        public float SideUV { get; set; }

        /// <summary>
        /// UV point used for caps/ends texturing
        /// </summary>
        public Vector2 TopUV { get; set; }

        /// <summary>
        /// The angle of the point around the center (used for sorting, will replace RadialAngle)
        /// </summary>
        public float RadialUV { get; set; }

        /// <summary>
        /// The angle of the point around the center adjusted by considering the normal.
        /// Used to correctly sort two points at the same position with different normals (aka hard edges).
        /// </summary>
        public float NormRadialUV { get; set; }

        public ContourPoint Next
        {
            get
            {
                if (Index < 0 || Section == null)
                    return null;
                return Section.Points[(Index + 1) % Section.PointCount];
            }
        }

        public ContourPoint Previous
        {
            get
            {
                if (Index < 0 || Section == null)
                    return null;
                int prevIndex = (Index == 0 ? Section.PointCount : Index) - 1;
                return Section.Points[prevIndex];
            }
        }

        public ContourPoint(Vector2 position, Vector2 normal)
            : this(position, normal, 0f) { }

        public ContourPoint(Vector2 position, Vector2 normal, float sideUV)
        {
            _Index = -1;
            _Section = null;
            Position = position;
            Normal = normal;
            SideUV = sideUV;
            CalculateAngles();
        }

        internal void Init(ContourProfile sec, int idx)
        {
            _Section = sec;
            _Index = idx;
        }

        public void CalculateAngles()
        {
            RadialUV = GetRadialAngle().Degrees / 360f;
            RadialUV = Mathf.Round(RadialUV * 1000f) / 1000f;
            NormRadialUV = GetRadialAngle(true).Degrees / 360f;
            NormRadialUV = Mathf.Round(NormRadialUV * 1000f) / 1000f;
        }

        private Angle GetRadialAngle(bool includeNormal = false)
        {
            var normPos = includeNormal? (Position.normalized * 10 + Normal) : Position.normalized;
            //y is used as z when bulding the mesh and in Unity, forward is -Z, so we flip y
            var radialAngle = Angle.FromRadians(Mathf.Atan2(-normPos.y, normPos.x));
            radialAngle.Normalize();
            return radialAngle;
        }

        public ContourPoint(float pX, float pZ, float nX, float nZ)
            : this(new Vector2(pX, pZ), new Vector2(nX, nZ)) { }

        public ContourPoint(float pX, float pZ, float nX, float nZ, float uv)
            : this(new Vector2(pX, pZ), new Vector2(nX, nZ), uv) { }
        

        internal static Vector2 GetPoint(float angle, float dist)
        {
            return new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle)) * dist;
        }

        internal static Vector2 GetPoint(double angle, double dist)
        {
            return new Vector2((float)Math.Cos(angle), -(float)Math.Sin(angle)) * (float)dist;
        }

        public ContourPoint Clone()
        {
            return new ContourPoint(Position, Normal);
        }

        public static ContourPoint Lerp(ContourPoint p1, ContourPoint p2, float t)
        {
            return new ContourPoint(Vector2.Lerp(p1.Position, p2.Position, t), Vector2.Lerp(p1.Normal, p2.Normal, t));
        }

        public static ContourPoint Slerp(ContourPoint p1, ContourPoint p2, float t)
        {
            if (Mathf.Approximately(t, 0f))
                return p1.Clone();
            if (Mathf.Approximately(t, 1f))
                return p2.Clone();
            return new ContourPoint(Vector2.Lerp(p1.Position, p2.Position, t), VectorUtils.SlerpNormal(p1.Normal, p2.Normal, t));
        }

        public bool IsCloseTo(ContourPoint other)
        {
            return Position.IsCloseTo(other.Position) && Normal.IsCloseTo(other.Normal);
        }
    }
}
