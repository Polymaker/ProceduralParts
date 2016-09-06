using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralParts.Geometry
{
    public class ProfileSection
    {
        // Fields...
        private float _Perimeter;
        private readonly float _Height;
        private readonly float _Width;
        private readonly ProfilePoint[] _Points;

        public ProfilePoint[] Points
        {
            get { return _Points; }
        }

        public float Width
        {
            get { return _Width; }
        }

        public float Height
        {
            get { return _Height; }
        }

        public float Perimeter
        {
            get { return _Perimeter; }
        }

        public int PointCount { get { return _Points.Length; } }

        public ProfileSection(params ProfilePoint[] points)
        {
            _Points = points;
            var xMin = _Points.Min(p => p.Position.x);
            var xMax = _Points.Max(p => p.Position.x);
            var yMin = _Points.Min(p => p.Position.y);
            var yMax = _Points.Max(p => p.Position.y);
            _Width = xMax - xMin;
            _Height = yMax - yMin;
            for (int i = 1; i <= _Points.Length; i++)
                _Perimeter += (_Points[i - 1].Position - _Points[i % _Points.Length].Position).magnitude;
        }



        public static ProfileSection Mk2Profile = new ProfileSection(
            new ProfilePoint(0.000f, -0.750f, 0.105f, -0.995f/*, 0.270f*/),
            new ProfilePoint(0.190f, -0.730f, 0.231f, -0.973f/*, 0.234f*/),
            new ProfilePoint(0.375f, -0.660f, 0.430f, -0.903f/*, 0.196f*/),
            new ProfilePoint(0.775f, -0.427f, 0.504f, -0.864f/*, 0.108f*/),
            new ProfilePoint(1.250f, -0.150f, 0.504f, -0.864f/*, 0.001f*/),
            new ProfilePoint(1.250f, -0.150f, 1.000f, 0.000f/*, 0.994f*/),
            new ProfilePoint(1.250f, 0.150f, 1.000f, 0.000f/*, 0.994f*/),
            new ProfilePoint(1.250f, 0.150f, 0.504f, 0.864f/*, 0.281f*/),
            new ProfilePoint(0.775f, 0.427f, 0.504f, 0.864f/*, 0.429f*/),
            new ProfilePoint(0.375f, 0.660f, 0.430f, 0.903f/*, 0.543f*/),
            new ProfilePoint(0.190f, 0.730f, 0.231f, 0.973f/*, 0.589f*/),
            new ProfilePoint(0.000f, 0.750f, 0.105f, 0.995f/*, 0.633f*/),
            new ProfilePoint(0.000f, 0.750f, -0.105f, 0.995f/*, 0.633f*/),
            new ProfilePoint(-0.190f, 0.730f, -0.231f, 0.973f/*, 0.589f*/),
            new ProfilePoint(-0.375f, 0.660f, -0.430f, 0.903f/*, 0.543f*/),
            new ProfilePoint(-0.775f, 0.427f, -0.504f, 0.864f/*, 0.429f*/),
            new ProfilePoint(-1.250f, 0.150f, -0.504f, 0.864f/*, 0.281f*/),
            new ProfilePoint(-1.250f, 0.150f, -1.000f, 0.000f/*, 0.994f*/),
            new ProfilePoint(-1.250f, -0.150f, -1.000f, 0.000f/*, 0.994f*/),
            new ProfilePoint(-1.250f, -0.150f, -0.504f, -0.864f/*, 0.001f*/),
            new ProfilePoint(-0.775f, -0.427f, -0.504f, -0.864f/*, 0.108f*/),
            new ProfilePoint(-0.375f, -0.660f, -0.430f, -0.903f/*, 0.196f*/),
            new ProfilePoint(-0.190f, -0.730f, -0.231f, -0.973f/*, 0.234f*/),
            new ProfilePoint(0.000f, -0.750f, -0.105f, -0.995f/*, 0.270f*/));

        public static ProfileSection Mk3Profile = new ProfileSection(
            new ProfilePoint(0.000f, -1.875f, 0.131f, -0.991f),
            new ProfilePoint(0.485f, -1.811f, 0.259f, -0.966f),
            new ProfilePoint(0.938f, -1.624f, 0.500f, -0.866f),
            new ProfilePoint(1.326f, -1.326f, 0.609f, -0.793f),
            new ProfilePoint(1.326f, -1.326f, 0.793f, -0.609f),
            new ProfilePoint(1.624f, -0.938f, 0.793f, -0.609f),
            new ProfilePoint(1.624f, -0.938f, 1.000f, 0.000f),
            new ProfilePoint(1.624f, -0.900f, 1.000f, 0.000f),
            new ProfilePoint(1.624f, 0.900f, 1.000f, 0.000f),
            new ProfilePoint(1.624f, 0.937f, 1.000f, 0.000f),
            new ProfilePoint(1.624f, 0.937f, 0.793f, 0.609f),
            new ProfilePoint(1.326f, 1.326f, 0.793f, 0.609f),
            new ProfilePoint(1.326f, 1.326f, 0.609f, 0.793f),
            new ProfilePoint(0.938f, 1.624f, 0.500f, 0.866f),
            new ProfilePoint(0.485f, 1.811f, 0.259f, 0.966f),
            new ProfilePoint(0.000f, 1.875f, 0.131f, 0.991f),
            new ProfilePoint(0.000f, 1.875f, -0.131f, 0.991f),
            new ProfilePoint(-0.485f, 1.811f, -0.259f, 0.966f),
            new ProfilePoint(-0.938f, 1.624f, -0.500f, 0.866f),
            new ProfilePoint(-1.326f, 1.326f, -0.609f, 0.793f),
            new ProfilePoint(-1.326f, 1.326f, -0.793f, 0.609f),
            new ProfilePoint(-1.624f, 0.937f, -0.793f, 0.609f),
            new ProfilePoint(-1.624f, 0.937f, -1.000f, 0.000f),
            new ProfilePoint(-1.624f, 0.900f, -1.000f, 0.000f),
            new ProfilePoint(-1.624f, -0.900f, -1.000f, 0.000f),
            new ProfilePoint(-1.624f, -0.938f, -1.000f, 0.000f),
            new ProfilePoint(-1.624f, -0.938f, -0.793f, -0.609f),
            new ProfilePoint(-1.326f, -1.326f, -0.793f, -0.609f),
            new ProfilePoint(-1.326f, -1.326f, -0.609f, -0.793f),
            new ProfilePoint(-0.938f, -1.624f, -0.500f, -0.866f),
            new ProfilePoint(-0.485f, -1.811f, -0.259f, -0.966f),
            new ProfilePoint(0.000f, -1.875f, -0.131f, -0.991f));

        public static ProfileSection GetMk2Section(float diameter)
        {
            float scale = diameter / 1.25f;
            return new ProfileSection(Mk2Profile.Points.Select(p => new ProfilePoint(p.Position * scale, p.Normal)).ToArray());
        }

        public static ProfileSection GetMk3Section(float diameter)
        {
            float scale = diameter / 3.75f;
            return new ProfileSection(Mk3Profile.Points.Select(p => new ProfilePoint(p.Position * scale, p.Normal)).ToArray());
        }

        public static ProfileSection GetPrismSection(int sideCount, float diameter)
        {
            var points = new List<ProfilePoint>();
            float theta = (Mathf.PI * 2f) / (float)sideCount;
            float halfT = theta / 2f;
            float radius = diameter / 2f;
            for (int s = 0; s < sideCount; s++)
            {
                var curAngle = theta * s;
                var norm = ProfilePoint.GetPoint(curAngle, 1f);
                points.Add(new ProfilePoint(ProfilePoint.GetPoint(curAngle - halfT, radius), norm, s / (float)sideCount));
                points.Add(new ProfilePoint(ProfilePoint.GetPoint(curAngle + halfT, radius), norm, (s + 1) / (float)sideCount));
            }

            return new ProfileSection(points.ToArray());
        }

        public static ProfileSection GetPrismCap(int sideCount, float diameter)
        {
            var points = new List<ProfilePoint>();
            float theta = (Mathf.PI * 2f) / (float)sideCount;
            float halfT = theta / 2f;
            float radius = diameter / 2f;
            for (int s = 0; s < sideCount; s++)
            {
                var curAngle = (theta * s) - halfT;
                points.Add(new ProfilePoint(ProfilePoint.GetPoint(curAngle, radius), Vector2.zero, s / (float)sideCount));
            }

            return new ProfileSection(points.ToArray());
        }

        public static ProfileSection GetCylinderSection(float diameter, int resolution = 64)
        {
            var points = new List<ProfilePoint>();
            float theta = (Mathf.PI * 2f) / (float)resolution;
            for (int s = 0; s <= resolution; s++)
            {
                var curAngle = theta * s;
                var norm = ProfilePoint.GetPoint(curAngle, 1f);
                points.Add(new ProfilePoint(ProfilePoint.GetPoint(curAngle, diameter), norm, s / (float)resolution));
            }
            return new ProfileSection(points.ToArray());
        }
    }
}
