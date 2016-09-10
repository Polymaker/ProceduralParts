using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralParts.Geometry
{
    public class ProfilePoint
    {
        // Fields...
        private int _Index;
        private ProfileSection _Section;
        private float _NormRadialUV;
        private float _RadialUV;
        private Vector2 _Normal;
        private Vector2 _Position;

        public int Index
        {
            get { return _Index; }
        }

        public ProfileSection Section
        {
            get { return _Section; }
        }

        public Vector2 Position
        {
            get { return _Position; }
            set
            {
                _Position = value;
                CalculateAngles();
            }
        }

        public Vector2 Normal
        {
            get { return _Normal; }
            set
            {
                _Normal = value;
                CalculateAngles();
            }
        }

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
        public float RadialUV
        {
            get { return _RadialUV; }
            set
            {
                _RadialUV = value;
            }
        }

        /// <summary>
        /// The angle of the point around the center adjusted by considering the normal.
        /// Used to correctly sort two points at the same position with different normals (aka hard edges).
        /// </summary>
        public float NormRadialUV
        {
            get { return _NormRadialUV; }
            set
            {
                _NormRadialUV = value;
            }
        }

        public ProfilePoint Next
        {
            get
            {
                if (Index < 0 || Section == null)
                    return null;
                return Section.Points[(Index + 1) % Section.PointCount];
            }
        }

        public ProfilePoint Previous
        {
            get
            {
                if (Index < 0 || Section == null)
                    return null;
                int prevIndex = (Index == 0 ? Section.PointCount : Index) - 1;
                return Section.Points[prevIndex];
            }
        }

        public ProfilePoint(Vector2 position, Vector2 normal)
            : this(position, normal, 0f) { }

        public ProfilePoint(Vector2 position, Vector2 normal, float sideUV)
        {
            _Index = -1;
            _Section = null;
            _Position = position;
            _Normal = normal;
            SideUV = sideUV;
            CalculateAngles();
        }

        internal void Init(ProfileSection sec, int idx)
        {
            _Section = sec;
            _Index = idx;
        }

        public void CalculateAngles()
        {
            _RadialUV = GetRadialAngle().Degrees / 360f;
            _RadialUV = Mathf.Round(_RadialUV * 1000f) / 1000f;
            _NormRadialUV = GetRadialAngle(true).Degrees / 360f;
            _NormRadialUV = Mathf.Round(_NormRadialUV * 1000f) / 1000f;
        }

        private Angle GetRadialAngle(bool includeNormal = false)
        {
            var normPos = includeNormal? (Position.normalized * 10 + Normal) : Position.normalized;
            //y is used as z when bulding the mesh and in Unity, forward is -Z, so we flip y
            var radialAngle = Angle.FromRadians(Mathf.Atan2(-normPos.y, normPos.x));
            radialAngle.Normalize();
            return radialAngle;
        }

        public ProfilePoint(float pX, float pZ, float nX, float nZ)
            : this(new Vector2(pX, pZ), new Vector2(nX, nZ)) { }

        public ProfilePoint(float pX, float pZ, float nX, float nZ, float uv)
            : this(new Vector2(pX, pZ), new Vector2(nX, nZ), uv) { }
        

        internal static Vector2 GetPoint(float angle, float dist)
        {
            return new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle)) * dist;
        }

        public ProfilePoint Clone()
        {
            return new ProfilePoint(Position, Normal);
        }

        public static ProfilePoint Lerp(ProfilePoint p1, ProfilePoint p2, float t)
        {
            return new ProfilePoint(Vector2.Lerp(p1.Position, p2.Position, t), Vector2.Lerp(p1.Normal, p2.Normal, t));
        }

        public static ProfilePoint Slerp(ProfilePoint p1, ProfilePoint p2, float t)
        {
            if (Mathf.Approximately(t, 0f))
                return p1.Clone();
            if (Mathf.Approximately(t, 1f))
                return p2.Clone();
            return new ProfilePoint(Vector2.Lerp(p1.Position, p2.Position, t), VectorUtils.SlerpNormal(p1.Normal, p2.Normal, t));
        }
    }
}
