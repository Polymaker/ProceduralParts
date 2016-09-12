using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralParts.Geometry
{
    public class ProfileSection
    {

        #region Fields

        private float _SurfaceArea;
        private float _Perimeter;
        private float _Height;
        private float _Width;
        private ProfilePoint[] _Points;

        #endregion

        #region Properties

        public ProfilePoint[] Points
        {
            get { return _Points; }
        }

        public int PointCount 
        {
            get { return _Points.Length; } 
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

        #endregion

        #region Ctors

        public ProfileSection(params ProfilePoint[] points)
        {
            if (points.Length == 0)
                throw new Exception("Cannot create section with no points");
            _Points = points;

            CalculateBounds();

            InitializePoints();

            CalculateTopUVs();

            FixRadialUVs();
        }

        public ProfileSection(IEnumerable<ProfilePoint> points)
            : this(points.ToArray()) { }

        #endregion

        #region Initializations & calculations

        private void CalculateBounds()
        {
            var max = _Points.Select(p=>p.Position).Aggregate((x, y) => Vector2.Max(x, y));
            var min = _Points.Select(p => p.Position).Aggregate((x, y) => Vector2.Min(x, y));
            _Width = Mathf.Abs(max.x - min.x);
            _Height = Mathf.Abs(max.y - min.y);
        }

        private void InitializePoints()
        {
            //ensure that we have a point at 0° to preserve texture alignment
            if (!_Points.Any(p => Mathf.Approximately(p.RadialUV, 0f)))
            {
                var pointList = _Points.ToList();
                var pointAtZero = InterpolateByUV(0f);
                if (pointAtZero.RadialUV != 0)
                {
                    pointAtZero.Position = new Vector2(pointAtZero.Position.x, 0f);
                    pointAtZero.CalculateAngles();
                }
                pointList.Add(pointAtZero);
                _Points = OrderProfilePoints(pointList);
            }

            for (int i = 0; i < _Points.Length; i++)
            {
                _Points[i].Init(this, i);
                var curPoint = _Points[i];
                curPoint.SideUV = _Perimeter;
                _Perimeter += (curPoint.Next.Position - curPoint.Position).magnitude;
                _SurfaceArea += GetSurfaceArea(curPoint);
            }

            for (int i = 0; i < _Points.Length; i++)
                _Points[i].SideUV = Mathf.Clamp(_Points[i].SideUV / _Perimeter, 0f, 1f);

            _Points[0].SideUV = 0f;
            _Points[PointCount - 1].SideUV = 1f;
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

        private static float GetSurfaceArea(ProfilePoint point)
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

        #endregion

        

        public static ProfileSection CreateSorted(params ProfilePoint[] points)
        {
            return new ProfileSection(OrderProfilePoints(points));
        }

        internal static ProfilePoint[] OrderProfilePoints(IEnumerable<ProfilePoint> points)
        {
            var orderedPoints = points.OrderBy(p => p.NormRadialUV).ToList();
            if (!orderedPoints.First().Position.IsCloseTo(orderedPoints.Last().Position))
            {
                //var wrapPoint = ProfilePoint.Interpolate(orderedPoints.First(), orderedPoints.Last(), 0.5f);
                //orderedPoints.Insert(0, wrapPoint);
                //orderedPoints.Add(wrapPoint);
                
                int foundIdx = -1;

                if (orderedPoints.Count(p => Mathf.Approximately(p.RadialUV, 0)) == 1)
                {
                    var pointAtZero = orderedPoints.First(p => Mathf.Approximately(p.RadialUV, 0));
                    foundIdx = orderedPoints.IndexOf(pointAtZero);
                    orderedPoints.Insert(foundIdx, pointAtZero.Clone());
                }
                else
                {
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


        public ProfilePoint InterpolateByUV(float uv)
        {
            for (int i = 0; i < PointCount; i++)
            {
                var pt1 = Points[i];
                var pt2 = pt1.Next ?? Points[(i+1) % PointCount];

                var curUV = pt1.RadialUV;
                var nextUV = pt2.RadialUV;

                if (Mathf.Approximately(curUV, nextUV) && Mathf.Approximately(curUV, uv))
                {
                    return ProfilePoint.Slerp(pt1, pt2, 0.5f);
                }

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
                    return ProfilePoint.Slerp(pt1, pt2, delta);
                }
            }
            return null;
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
            combinedPoints = combinedPoints.OrderBy(cp => cp.RadialUV).ThenBy(cp => cp.NormRadialUV).ToList();


            const float errorDist = 0.0001f;
            var uvList = new List<float>();

            uvList.AddRange(new float[] { 0f, 0.125f, 0.25f, 0.375f, 0.5f, 0.625f, 0.75f, 0.875f });

            uvList.AddRange(section1.Points.Select(p => p.RadialUV));
            uvList.AddRange(section2.Points.Select(p => p.RadialUV));
            uvList.OrderListBy(uv => uv);
            uvList = uvList.Distinct().ToList();

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
                        finalPoints.Add(section1.InterpolateByUV(currentUV - errorDist));
                        finalPoints.Add(section1.InterpolateByUV(currentUV + errorDist));
                        continue;
                    }
                }

                finalPoints.Add(section1.InterpolateByUV(currentUV));
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
                finalPoints.Add(ProfilePoint.Slerp(section1.Points[i], section2.Points[i], t));
            }
            return new ProfileSection(finalPoints);
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

        #region Profile generation (mostly scaling) methods 

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
                var pt1 = new ProfilePoint(ProfilePoint.GetPoint(curAngle - halfT, radius), norm);
                var pt2 = new ProfilePoint(ProfilePoint.GetPoint(curAngle + halfT, radius), norm);
                points.Add(pt1);
                //if (s == 0)
                //    points.Add(ProfilePoint.Lerp(pt1, pt2, 0.5f));
                points.Add(pt2);
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
