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

            for (int i = 0; i < _Points.Length; i++)
            {
                _Points[i].ListIndex = i;
                _Points[i].Section = this;
            }

            var xMin = _Points.Min(p => p.Position.x);
            var xMax = _Points.Max(p => p.Position.x);
            var yMin = _Points.Min(p => p.Position.y);
            var yMax = _Points.Max(p => p.Position.y);
            
            _Width = Mathf.Abs(xMax - xMin);
            _Height = Mathf.Abs(yMax - yMin);
            for (int i = 1; i <= _Points.Length; i++)
                _Perimeter += (_Points[i - 1].Position - _Points[i % _Points.Length].Position).magnitude;
        }

        public ProfileSection(IEnumerable<ProfilePoint> points)
            : this(points.ToArray()) { }

        public static ProfileSection CreateSorted(params ProfilePoint[] points)
        {
            return new ProfileSection(OrderProfilePoints(points));
        }

        internal static ProfilePoint[] OrderProfilePoints(IEnumerable<ProfilePoint> points)
        {
            var orderedPoints = points.OrderBy(p => p.NormalizedRadial).ToList();
            if (!orderedPoints.First().Position.CloseTo(orderedPoints.Last().Position))
            {
                int foundIdx = -1;
                for (int i = 0; i < orderedPoints.Count; i++)
                {
                    var p1 = orderedPoints[i];
                    var p2 = orderedPoints[(i + 1) % orderedPoints.Count];
                    if (p1.Position.CloseTo(p2.Position))
                    {
                        foundIdx = i;
                        break;
                    }
                }
                if (foundIdx < 0)
                    orderedPoints.Add(orderedPoints.First());
                else
                {
                    var endPoints = orderedPoints.Take(foundIdx + 1).ToArray();
                    orderedPoints.RemoveRange(0, foundIdx + 1);
                    orderedPoints.AddRange(endPoints);
                }
            }
            return orderedPoints.ToArray();
        }

        public ProfilePoint GetPointAtAngle(Angle angle)
        {
            if (Points.Any(p => p.RadialAngle.DeltaAngle(angle).Degrees < 1))
                return GetClosestToAngle(angle).Clone();
            var edge = GetEdge(angle.Radians);
            var delta = angle.DeltaAngle(edge.P1.RadialAngle) / edge.ArcDelta;

            return ProfilePoint.Interpolate(edge.P1,edge.P2, delta);
        }

        #region Section Combining
        public static ProfileSection CreateAdapter(ProfileSection section1, ProfileSection section2)
        {
            var angleError = Angle.FromDegrees(2); 

            var finalPoints = new List<ProfilePoint>();

            var combinedPoints = new List<ProfilePoint>();
            combinedPoints.AddRange(section1.Points);
            combinedPoints.AddRange(section2.Points);
            combinedPoints = combinedPoints.OrderBy(pp => pp.RadialAngle).ToList();
            var radialAngles = combinedPoints.Select(cp => cp.RadialAngle).RemoveDoubles((x,y) => Angle.DeltaAngle(x, y) < angleError).ToList();

            for (int i = 0; i < radialAngles.Count; i++)
            {
                var currentAngle = radialAngles[i];
                var pointsAtAngle = combinedPoints.Where(cp => cp.RadialAngle.DeltaAngle(currentAngle) < angleError).ToList();
                if (pointsAtAngle.All(p => p.Section == section1))
                    finalPoints.AddRange(pointsAtAngle.Select(p => p.Clone()));
                else
                {
                    if (pointsAtAngle.All(p => p.Section == section2))
                    {
                        finalPoints.AddRange(pointsAtAngle.Select(pa => section1.GetPointAtAngle(pa.RadialAngle)));
                    }
                    else if (pointsAtAngle.Count == 2)//both profiles haves a point at about the same angle
                    {
                        finalPoints.Add(section1.GetPointAtAngle(pointsAtAngle.First().RadialAngle));
                    }
                    else if (pointsAtAngle.Count == 3)
                    {
                        if(pointsAtAngle.Count(p => p.Section == section1) == 2)
                            finalPoints.AddRange(pointsAtAngle.Where(pa => pa.Section == section1).Select(p => p.Clone()));
                        else
                            finalPoints.AddRange(pointsAtAngle.Where(pa => pa.Section == section2).Select(p => section1.GetPointAtAngle(p.RadialAngle)));
                    }
                    else
                    {
                        finalPoints.AddRange(pointsAtAngle.Where(pa => pa.Section == section1).Select(p => p.Clone()));
                    }
                }
                combinedPoints.RemoveAll(cp => pointsAtAngle.Contains(cp));
            }

            return new ProfileSection(finalPoints.ToArray());
        }

        private class ProfileEdge
        {
            public ProfilePoint P1 { get; set; }
            public ProfilePoint P2 { get; set; }

            public Angle ArcDelta
            {
                get
                {
                    return Angle.DeltaAngle(P1.RadialAngle, P2.RadialAngle);
                }
            }

            public ProfileEdge(ProfilePoint p1, ProfilePoint p2)
            {
                P1 = p1;
                P2 = p2;
            }
        }
        

        private ProfileEdge GetEdge(float angleRad)
        {
            var angle = Angle.FromRadians(angleRad).Normalized();

            var pt1 = GetClosestToAngle(angle);
            var prevVert = pt1.Previous;
            if (prevVert.Position.CloseTo(pt1.Position))
                prevVert = prevVert.Previous;
            var nextVert = pt1.Next;
            if (nextVert.Position.CloseTo(pt1.Position))
                nextVert = nextVert.Next;
            var leftDiff = angle.DeltaAngle(prevVert.RadialAngle);
            var rightDiff = angle.DeltaAngle(nextVert.RadialAngle);
            if (leftDiff < rightDiff)
                return new ProfileEdge(pt1, prevVert);
            return new ProfileEdge(pt1, nextVert);
        }

        private ProfilePoint GetClosestToAngle(Angle angle)
        {
            return Points.OrderBy(p => angle.DeltaAngle(p.NormalizedRadial)).First();
        }

        #endregion


        #region Hard coded Profiles

        public static ProfileSection Mk2Profile = CreateSorted(
            new ProfilePoint(0.000f, -0.750f, -0.105f, -0.995f),
            new ProfilePoint(-0.190f, -0.730f, -0.231f, -0.973f),
            new ProfilePoint(-0.375f, -0.660f, -0.430f, -0.903f),
            new ProfilePoint(-0.775f, -0.427f, -0.504f, -0.864f),
            new ProfilePoint(-1.250f, -0.150f, -0.504f, -0.864f),
            new ProfilePoint(-1.250f, -0.150f, -1.000f, 0.000f),
            new ProfilePoint(-1.250f, 0.150f, -1.000f, 0.000f),
            new ProfilePoint(-1.250f, 0.150f, -0.504f, 0.864f),
            new ProfilePoint(-0.775f, 0.427f, -0.504f, 0.864f),
            new ProfilePoint(-0.375f, 0.660f, -0.430f, 0.903f),
            new ProfilePoint(-0.190f, 0.730f, -0.231f, 0.973f),
            new ProfilePoint(0.000f, 0.750f, -0.105f, 0.995f),
            new ProfilePoint(0.000f, 0.750f, 0.105f, 0.995f),
            new ProfilePoint(0.190f, 0.730f, 0.231f, 0.973f),
            new ProfilePoint(0.375f, 0.660f, 0.430f, 0.903f),
            new ProfilePoint(0.775f, 0.427f, 0.504f, 0.864f),
            new ProfilePoint(1.250f, 0.150f, 0.504f, 0.864f),
            new ProfilePoint(1.250f, 0.150f, 1.000f, 0.000f),
            new ProfilePoint(1.250f, -0.150f, 1.000f, 0.000f),
            new ProfilePoint(1.250f, -0.150f, 0.504f, -0.864f),
            new ProfilePoint(0.775f, -0.427f, 0.504f, -0.864f),
            new ProfilePoint(0.375f, -0.660f, 0.430f, -0.903f),
            new ProfilePoint(0.190f, -0.730f, 0.231f, -0.973f),
            new ProfilePoint(0.000f, -0.750f, 0.105f, -0.995f));

        public static ProfileSection Mk3Profile = CreateSorted(
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

        #endregion

        #region Profile scaling methods 

        public static ProfileSection GetMk2Section(float diameter)
        {
            float scale = (diameter / 1.5f);
            return new ProfileSection(Mk2Profile.Points.Select(p => new ProfilePoint(p.Position * scale, p.Normal)).ToArray());
        }

        public static ProfileSection GetMk3Section(float diameter)
        {
            float scale = (diameter / 3.75f);
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

        #endregion

    }
}
