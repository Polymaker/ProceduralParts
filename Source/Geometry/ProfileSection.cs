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
        private float _SurfaceArea;
        private float _Perimeter;
        private float _Height;
        private float _Width;
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

        public float SurfaceArea
        {
            get { return _SurfaceArea; }
        }
        
        public int PointCount { get { return _Points.Length; } }

        public ProfileSection(params ProfilePoint[] points)
        {
            if (points.Length == 0)
                throw new Exception("Cannot create section with no points");
            _Points = points;

            CalculateSize();
            
            InitializePoints();

            CalculateTopUVs();

            FixRadialUVs();
        }

        public ProfileSection(IEnumerable<ProfilePoint> points)
            : this(points.ToArray()) { }

        private void CalculateSize()
        {
            var xMin = _Points.Min(p => p.Position.x);
            var xMax = _Points.Max(p => p.Position.x);
            var yMin = _Points.Min(p => p.Position.y);
            var yMax = _Points.Max(p => p.Position.y);

            _Width = Mathf.Abs(xMax - xMin);
            _Height = Mathf.Abs(yMax - yMin);
        }

        private void InitializePoints()
        {
            bool calculateUV = _Points.All(pp => pp.SideUV == 0);
            for (int i = 0; i < _Points.Length; i++)
            {
                var curPoint = _Points[i];
                curPoint.ListIndex = i;
                curPoint.Section = this;
                if (calculateUV)
                    curPoint.SideUV = _Perimeter;
                _Perimeter += (curPoint.Next.Position - curPoint.Position).magnitude;
                _SurfaceArea += GetSurfaceArea(curPoint);
            }

            if (calculateUV)
            {
                for (int i = 0; i < _Points.Length; i++)
                    _Points[i].SideUV = Mathf.Clamp(_Points[i].SideUV / _Perimeter, 0f, 1f);
                _Points[0].SideUV = 0f;
                _Points[PointCount - 1].SideUV = 1f;
            }
        }

        private void CalculateTopUVs()
        {
            var centerOffset = new Vector2(Width / 2f, Height / 2f);
            var maxSize = Mathf.Max(Width, Height);
            var uvOffset = new Vector2((maxSize - Width) / 2f, (maxSize - Height) / 2f);

            for (int i = 0; i < _Points.Length; i++)
            {
                var curPoint = _Points[i];
                curPoint.TopUV = new Vector2(
                    (curPoint.Position.x + centerOffset.x + uvOffset.x) / maxSize, 
                    (curPoint.Position.y + centerOffset.y + uvOffset.y) / maxSize);
            }
        }

        private float GetSurfaceArea(ProfilePoint point)
        {
            var a = point.Position.magnitude;
            var b = point.Next.Position.magnitude;
            var c = (point.Next.Position - point.Position).magnitude;
            var p = (a + b + c) / 2f;
            return Mathf.Sqrt(p * (p - a) * (p - b) * (p - c));
        }

        private void FixRadialUVs()
        {
            for (int i = 1; i < PointCount - 2; i++)
            {
                var pt1 = Points[i];
                var pt2 = pt1.Next;
                if (Mathf.Abs(pt1.RadialUV - pt2.RadialUV) < 0.005f && pt1.RadialUV > pt2.RadialUV)
                {
                    var tmpUv = pt1.RadialUV;
                    pt1.RadialUV = pt2.RadialUV;
                    pt2.RadialUV = tmpUv;
                }
            }
        }

        public static ProfileSection CreateSorted(params ProfilePoint[] points)
        {
            return new ProfileSection(OrderProfilePoints(points));
        }

        internal static ProfilePoint[] OrderProfilePoints(IEnumerable<ProfilePoint> points)
        {
            var orderedPoints = points.OrderBy(p => p.NormalizedRadial).ToList();
            if (!orderedPoints.First().Position.IsCloseTo(orderedPoints.Last().Position))
            {
                //var wrapPoint = ProfilePoint.Interpolate(orderedPoints.First(), orderedPoints.Last(), 0.5f);
                //orderedPoints.Insert(0, wrapPoint);
                //orderedPoints.Add(wrapPoint);
                int foundIdx = -1;
                for (int i = 0; i < orderedPoints.Count; i++)
                {
                    var p1 = orderedPoints[i];
                    var p2 = orderedPoints[(i + 1) % orderedPoints.Count];
                    if (p1.Position.IsCloseTo(p2.Position))
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
            for (int i = orderedPoints.Count - 2; i > 0; i--)
            {
                var p1 = orderedPoints[i];
                var p2 = orderedPoints[i - 1];
                if (p2.Position == p1.Position && p1.Normal == p2.Normal)
                    orderedPoints.Remove(p1);
            }
            return orderedPoints.ToArray();
        }

        public ProfilePoint GetPointAtAngle(Angle angle)
        {
            var edge = GetEdge(angle);
            var dist = edge.P1.RadialAngle.Distance(angle);
            if (Math.Abs(dist.Degrees) < float.Epsilon)
            {
                if (edge.P1.Normal == edge.P2.Normal)
                    return edge.P1.Clone();
                return ProfilePoint.Interpolate(edge.P1, edge.P2, 0.5f);
            }
            var delta = dist.Degrees / edge.ArcDelta.Degrees;
            var result = InterpolatePoint(edge, angle, delta, 0);
            
            return result;
        }

        public ProfilePoint InterpolateUV(float uv)
        {
            for (int i = 0; i < PointCount; i++)
            {

                var curUV = Points[i].RadialUV;
                var nextUV = Points[i].Next.RadialUV;

                if (nextUV < curUV)
                {
                    if (uv < nextUV)
                        curUV = curUV - 1f;
                    else
                        nextUV += 1f;
                }

                if (uv >= curUV && uv <= nextUV)
                {
                    var delta = (uv - curUV) / (nextUV - curUV);
                    if (float.IsNaN(delta))
                        delta = 0.5f;
                    return ProfilePoint.Interpolate(Points[i], Points[i].Next, delta);
                }
            }
            return null;
        }

        private static ProfilePoint InterpolatePoint(ProfileEdge edge, Angle targetAngle, float delta, int iteration)
        {
            var ip = ProfilePoint.Interpolate(edge.P1, edge.P2, delta);
            var angleDist = ip.RadialAngle.Distance(targetAngle);
            if (Mathf.Abs(angleDist.Degrees) <= 0.001f/* || iteration > 3*/)
                return ip;
            if (iteration > 4)
                return ip;
            delta += angleDist.Degrees / edge.ArcDelta.Degrees;
            return InterpolatePoint(edge, targetAngle, delta ,++iteration);
        }

        private static bool AreSimilar(ProfileSection section1, ProfileSection section2)
        {
            if (section1 == section2)
                return true;
            if (section1.PointCount == section2.PointCount)
            {
                for (int i = 0; i < section1.PointCount; i += 4)
                {
                    if (Mathf.Abs(section1.Points[i].SideUV - section2.Points[i].SideUV) > 0.01f)
                        return false;
                }
                return true;
            }
            return false;
        }

        #region Section Combining

        public static ProfileSection CreateAdapter(ProfileSection section1, ProfileSection section2)
        {
            if (AreSimilar(section1, section2))
                return section1;

            var finalPoints = new List<ProfilePoint>();

            var combinedPoints = new List<ProfilePoint>();
            combinedPoints.AddRange(section1.Points);
            combinedPoints.AddRange(section2.Points);
            combinedPoints = combinedPoints.OrderBy(cp => cp.RadialUV).ThenBy(cp => cp.NormalizedRadial).ToList();


            const float errorDist = 0.0001f;
            var uvList = new List<float>();

            uvList.AddRange(new float[] { 0f, 0.125f, 0.25f, 0.375f, 0.5f, 0.625f, 0.75f, 0.875f });

            uvList.AddRange(section1.Points.Select(p => p.RadialUV));
            uvList.AddRange(section2.Points.Select(p => p.RadialUV));
            uvList.OrderListBy(uv => uv);
            uvList = uvList.Distinct().ToList();
            //uvList = uvList.RemoveDoubles((x, y) => Mathf.Abs(x - y) < errorDist).ToList();

            if (uvList.Count > 100)
            {
                uvList = uvList.RemoveDoubles((x, y) => Mathf.Abs(x - y) < 0.001f).ToList();
            }

            
            for (int i = 0; i < uvList.Count; i++)
            {
                var currentUV = uvList[i];

                var sec1Points = combinedPoints.Where(p => p.Section == section1 && Mathf.Abs(p.RadialUV - currentUV) < errorDist).ToList();
                var sec2Points = combinedPoints.Where(p => p.Section == section2 && Mathf.Abs(p.RadialUV - currentUV) < errorDist).ToList();
                combinedPoints.Remove(sec1Points);
                combinedPoints.Remove(sec2Points);

                if (sec1Points.Count >= 2)
                {
                    var pt1 = sec1Points.First().Clone();
                    var pt2 = sec1Points.Last().Clone();
                    if (!(pt1.Position == pt2.Position && pt1.Normal == pt2.Normal))
                    {
                        finalPoints.Add(pt1);
                        finalPoints.Add(pt2);
                        continue;
                    }

                }
                else if (sec2Points.Count >= 2)
                {
                    var pt1 = sec2Points.First().Clone();
                    var pt2 = sec2Points.Last().Clone();
                    if (!(pt1.Position == pt2.Position && pt1.Normal == pt2.Normal))
                    {
                        finalPoints.Add(section1.InterpolateUV(currentUV - errorDist));
                        finalPoints.Add(section1.InterpolateUV(currentUV + errorDist));
                        continue;
                    }
                }

                finalPoints.Add(section1.InterpolateUV(currentUV));
            }

            finalPoints.Add(finalPoints[0].Clone());
            return new ProfileSection(finalPoints.ToArray());
        }

        public static ProfileSection Lerp(ProfileSection section1, ProfileSection section2, float t)
        {
            if (section1.PointCount != section2.PointCount)
                throw new InvalidOperationException();
            var finalPoints = new List<ProfilePoint>();

            for (int i = 0; i < section1.PointCount; i++)
            {
                finalPoints.Add(ProfilePoint.Interpolate(section1.Points[i], section2.Points[i], t));
            }
            return new ProfileSection(finalPoints);
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

        private ProfileEdge GetEdge(Angle angle)
        {
            //var edgesFound = new List<ProfileEdge>();
            foreach (var pp in Points)
            {
                if (angle.IsBetween(pp.RadialAngle, pp.Next.RadialAngle, true))
                    return new ProfileEdge(pp, pp.Next);
                    //edgesFound.Add(new ProfileEdge(pp, pp.Next));
            }
            foreach (var pp in Points)
            {
                if (angle.IsBetween(pp.RadialAngle, pp.Next.RadialAngle))
                    return new ProfileEdge(pp, pp.Next);
                //edgesFound.Add(new ProfileEdge(pp, pp.Next));
            }
            return null;
            //if (edgesFound.Count == 1)
            //    return edgesFound[0];

            //return edgesFound.OrderByDescending(e=>angle.IsBetween(e.P1.NormalizedRadial,e.P2.NormalizedRadial)).First();
        }

        #endregion

        #region Hard coded Profiles

        public static ProfileSection Mk2Profile = CreateSorted(
            new ProfilePoint(0.000f, -0.750f, 0f, -1f),
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
            new ProfilePoint(0.000f, 0.750f, 0f, 1f),
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
            new ProfilePoint(0.000f, -0.750f, 0f, -1f));

        public static ProfileSection Mk3Profile = CreateSorted(
            new ProfilePoint(0.000f, -1.875f, 0f, -1f),
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
            new ProfilePoint(0.000f, 1.875f, 0f, 1f),
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
            new ProfilePoint(0.000f, -1.875f, 0f, -1f));

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
                
                points.Add(new ProfilePoint(ProfilePoint.GetPoint(curAngle, diameter / 2f), norm, s / (float)resolution));

            }

            points[resolution].Position = points[0].Position;
            points[resolution].Normal = points[0].Normal;
            return new ProfileSection(points.ToArray());
        }

        #endregion

    }
}
