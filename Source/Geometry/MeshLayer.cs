using KSPAPIExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralParts.Geometry
{
    public class MeshLayer
    {
        private readonly ContourProfile _Profile;
        private readonly MeshPoint[] _Points;

        public int Index { get; internal set; }

        public MeshShape Mesh { get; set; }

        public ContourProfile Profile
        {
            get { return _Profile; }
        }

        public float PosY { get; set; }

        public Vector2 Offset { get; set; }

        public MeshPoint[] Points
        {
            get { return _Points; }
        }

        public int PointCount
        {
            get { return Profile.PointCount; }
        }

        public MeshLayer Previous
        {
            get
            {
                if (Mesh == null || Index == 0)
                    return null;
                return Mesh.Sections[Index - 1];
            }
        }

        public MeshLayer Next
        {
            get
            {
                if (Mesh == null || Index == Mesh.Sections.Length - 1)
                    return null;
                return Mesh.Sections[Index + 1];
            }
        }

        public float UV
        {
            get
            {
                if (Mesh == null || Index < 0)
                    return 0f;
                return /*1f - */(Index / (float)(Mesh.SubDivCount - 1));
            }
        }

        public MeshLayer(ContourProfile profile, float posY)
        {
            Index = -1;
            _Profile = profile;
            PosY = posY;
            _Points = new MeshPoint[profile.PointCount];
            for (int i = 0; i < profile.PointCount; i++)
                _Points[i] = new MeshPoint(this, i);
        }

        public void Init()
        {
            for (int i = 0; i < PointCount; i++)
                _Points[i].Init();
        }

        public Vector3 InterpolateByUV(float uv)
        {

            uv = Mathf.Clamp(uv, 0, 1f);
            for (int i = 0; i < PointCount; i++)
            {

                var pt1 = Points[i];
                var pt2 = pt1.Next ?? Points[(i + 1) % PointCount];

                var curUV = pt1.Point.RadialUV;
                var nextUV = pt2.Point.RadialUV;

                if (Mathf.Approximately(curUV, nextUV) && Mathf.Approximately(curUV, uv))
                {
                    return Vector3.Slerp(pt1.Value.Pos, pt2.Value.Pos, 0.5f);
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
                    return Vector3.Slerp(pt1.Value.Pos, pt2.Value.Pos, delta);
                }
            }

            return Vector3.zero;
        }
    }
}
