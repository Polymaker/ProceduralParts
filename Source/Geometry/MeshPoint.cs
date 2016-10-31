using KSPAPIExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralParts.Geometry
{
    [System.Diagnostics.DebuggerDisplay("{Value}")]
    public class MeshPoint
    {
        public MeshLayer Section { get; set; }

        private Vertex _Value;
        private readonly int _Index;

        #region Properties

        public int Index
        {
            get { return _Index; }
        }

        public ContourPoint Point
        {
            get
            {
                return Section.Profile.Points[Index];
            }
        }

        public int vIndex
        {
            get
            {
                return (Section.Index * Section.PointCount) + Index;
            }
        }

        public Vertex Value
        {
            get { return _Value; }
        }

        public MeshPoint Above
        {
            get
            {
                if (Section == null || Section.Previous == null)
                    return null;
                return Section.Previous.Points[Index];
            }
        }

        public MeshPoint Below
        {
            get
            {
                if (Section == null || Section.Next == null)
                    return null;
                return Section.Next.Points[Index];
            }
        }

        public MeshPoint Next
        {
            get
            {
                if (Index < 0 || Section == null)
                    return null;
                return Section.Points[(Index + 1) % Section.PointCount];
            }
        }

        public MeshPoint Previous
        {

            get
            {
                if (Index < 0 || Section == null)
                    return null;
                int prevIndex = (Index == 0 ? Section.PointCount : Index) - 1;
                return Section.Points[prevIndex];
            }
        }

        #endregion

        public MeshPoint(MeshLayer section, int index)
        {
            Section = section;
            _Index = index;
        }

        public void Init()
        {
            var vp = new Vector3(Point.Position.x + Section.Offset.x, Section.PosY, Point.Position.y + Section.Offset.y);
            var vn = new Vector3(Point.Normal.x, 0, Point.Normal.y);//todo: find a way to align normal perpendicular to the mesh face

            Vector2 p1 = Vector2.zero;
            Vector2 p2 = Vector2.zero;
            if ((Next.Point.Position - Point.Position).magnitude >= 0.005f)
            {
                p1 = Point.Position;
                p2 = Vector2.Lerp(p1, Next.Point.Position, 0.5f);
            }
            else if ((Previous.Point.Position - Point.Position).magnitude >= 0.005f)
            {
                p1 = Previous.Point.Position;
                p2 = Vector2.Lerp(p1, Point.Position, 0.5f);
            }
            else
            {
                Debug.Log("Tangant ERROR!");

            }
            //var np = Next.Point.Position.IsCloseTo(Point.Position) ? Next.Next : Next;
            var tanN = (p2 - p1).normalized;

            var tan = new Vector4(tanN.x, 0, tanN.y, -1f);
            _Value = new Vertex(vp, vn, tan, new Vector2(Point.SideUV, Section.UV));
        }

        public Vertex GetCapVertex()
        {
            return new Vertex(Value.Pos,
                Section.PosY > 0 ? Vector3.up : Vector3.down,
                new Vector4(-1, 0, 0, Section.PosY > 0 ? 1 : -1),
                Point.TopUV);
        }
    }
}
