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
        private Angle _RadialAngle;
        private Vector2 _Normal;
        private Vector2 _Position;
        private bool angleIsDirty;
        internal int ListIndex = -1;
        internal ProfileSection Section;

        public Vector2 Position
        {
            get { return _Position; }
            set
            {
                //if (_Position == value)
                //    return;
                _Position = value;
                angleIsDirty = true;
            }
        }
        
        public Vector2 Normal
        {
            get { return _Normal; }
            set
            {
                //if (_Normal == value)
                //    return;
                _Normal = value;
                angleIsDirty = true;
            }
        }

        public float SideUV { get; set; }

        public Vector2 TopUV { get; set; }

        public float RadialUV { get; set; }

        public Angle RadialAngle
        {
            get
            {
                if (angleIsDirty)
                {
                    _RadialAngle = GetRadialAngle();
                    angleIsDirty = false;
                }
                return _RadialAngle;
            }
        }

        /// <summary>
        /// Radial angle offseted by vertex normal. Used to correctly order vertices with the same position.
        /// </summary>
        public Angle NormalizedRadial
        {
            get
            {
                return GetRadialAngle(true);
            }
        }

        public ProfilePoint Next
        {
            get
            {
                if (ListIndex < 0 || Section == null)
                    return null;
                return Section.Points[(ListIndex + 1) % Section.PointCount];
            }
        }

        public ProfilePoint Previous
        {
            get
            {
                if (ListIndex < 0 || Section == null)
                    return null;
                int prevIndex = (ListIndex == 0 ? Section.PointCount : ListIndex) - 1;
                return Section.Points[prevIndex];
            }
        }

        public ProfilePoint(Vector2 position, Vector2 normal)
            : this(position, normal, 0f) { }

        public ProfilePoint(Vector2 position, Vector2 normal, float uV)
        {
            Position = position;
            Normal = normal;
            SideUV = uV;
            angleIsDirty = true;
            RadialUV = RadialAngle.Degrees / 360f;
            RadialUV = Mathf.Round(RadialUV * 1000f) / 1000f;

        }

        private Angle GetRadialAngle(bool includeNormal = false)
        {
            var normPos = includeNormal? (Position * 10 + Normal) : Position;
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
            return new ProfilePoint(Position, Normal) { _RadialAngle = RadialAngle, angleIsDirty = false };
        }

        public static ProfilePoint Interpolate1(ProfilePoint p1, ProfilePoint p2, float t)
        {
            return new ProfilePoint(Vector2.Lerp(p1.Position, p2.Position, t), Vector2.Lerp(p1.Normal, p2.Normal, t));
        }

        public static ProfilePoint Interpolate(ProfilePoint p1, ProfilePoint p2, float t)
        {
            if (t == 0)
                return p1.Clone();
            if (t == 1)
                return p2.Clone();
            return new ProfilePoint(Vector2.Lerp(p1.Position, p2.Position, t), VectorUtils.SlerpNormal(p1.Normal, p2.Normal, t));
        }
    }
}
