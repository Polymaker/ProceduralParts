using KSPAPIExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralParts.Geometry
{
    public static class MeshBuilder
    {

        public static ProceduralMesh CreateProceduralMesh(ProfileSection profile, float length)
        {
            var meshShape = new MeshProfile(
                new MeshSection(profile, length / 2f), 
                new MeshSection(profile, -length / 2f)
                );

            return CreateProceduralMesh(meshShape);
        }

        public static ProceduralMesh CreateProceduralMesh(ProfileSection top, ProfileSection bottom, float length, int subdivisions = 1)
        {
            var topAdapter = ProfileSection.CreateAdapter(top, bottom);
            var botAdapter = ProfileSection.CreateAdapter(bottom, top);

            if (botAdapter.PointCount != topAdapter.PointCount)
            {
                Debug.Log("Failed to created adapter, sides does not have same number of vertices");
                return null;
            }

            var sections = new List<MeshSection>();
            sections.Add(new MeshSection(topAdapter, length / 2f));

            float subdivRatio = 1f / (subdivisions + 1);
            float yStep = -length * subdivRatio;

            for (int i = 1; i <= subdivisions; i++)
            {
                var subSection = ProfileSection.Lerp(topAdapter, botAdapter, subdivRatio * i);
                sections.Add(new MeshSection(subSection, length / 2f + yStep * i));
            }

            sections.Add(new MeshSection(botAdapter, -length / 2f));

            var meshShape = new MeshProfile(sections);

            return CreateProceduralMesh(meshShape);
        }

        private static ProceduralMesh CreateProceduralMesh(MeshProfile meshShape)
        {
            var baseMesh = CreateSideMesh(meshShape);
            var endsMesh = CreateEndsMesh(meshShape);
            var collider = MergeMeshes(baseMesh, endsMesh);
            float volume = 0f;
            for (int i = 0; i < meshShape.SubDivCount - 1; i++)
            {
                var subDiv = meshShape.Sections[i];
                var avgArea = (subDiv.Profile.SurfaceArea + subDiv.Next.Profile.SurfaceArea) / 2f;
                volume += avgArea * (subDiv.PosY - subDiv.Next.PosY);
            }
            return new ProceduralMesh(baseMesh, endsMesh, collider, volume);
        }

        public static UncheckedMesh CreateAdapterSides(ProfileSection bottom, ProfileSection top, float length, int subdivisions = 1)
        {

            var bAdapterSection = ProfileSection.CreateAdapter(bottom, top);
            var tAdapterSection = ProfileSection.CreateAdapter(top, bottom);

            if (bAdapterSection.PointCount != tAdapterSection.PointCount)
            {
                Debug.Log("Failed to created adapter, sides does not have same number of vertices");
                return new UncheckedMesh(0, 0);
            }
            var sections = new List<MeshSection>();
            sections.Add(new MeshSection(tAdapterSection, length / 2f));
            
            float subdivRatio = 1f / (subdivisions + 1);
            float yStep = -length * subdivRatio;
            for (int i = 1; i <= subdivisions; i++)
            {
                var subdivSection = ProfileSection.Lerp(tAdapterSection, bAdapterSection, subdivRatio * i);
                sections.Add(new MeshSection(subdivSection, length / 2f + yStep * i));
            }
            sections.Add(new MeshSection(bAdapterSection, -length / 2f));

            var adapterShape = new MeshProfile(sections);

            return CreateSideMesh(adapterShape);
        }
        
        private static UncheckedMesh CreateMesh(List<Vertex> vertices, List<int> triangleIndices)
        {
            var mesh = new UncheckedMesh(vertices.Count, triangleIndices.Count / 3);
            for (int i = 0; i < vertices.Count; i++)
            {
                mesh.verticies[i] = vertices[i].Pos;
                mesh.normals[i] = vertices[i].Norm;
                mesh.tangents[i] = vertices[i].Tan;
                mesh.uv[i] = vertices[i].Uv;
            }
            for (int i = 0; i < triangleIndices.Count; i++)
                mesh.triangles[i] = triangleIndices[i];

            return mesh;
        }

        public static UncheckedMesh MergeMeshes(UncheckedMesh mesh1, UncheckedMesh mesh2)
        {
            var finalMesh = new UncheckedMesh(mesh1.verticies.Length + mesh2.verticies.Length, mesh1.nTri + mesh2.nTri);
            var mesh1VertCount = mesh1.verticies.Length;
            var mesh1TriCount = mesh1.triangles.Length;

            for (int i = 0; i < mesh1.verticies.Length; i++)
            {
                finalMesh.verticies[i] = mesh1.verticies[i];
                finalMesh.normals[i] = mesh1.normals[i];
                finalMesh.tangents[i] = mesh1.tangents[i];
                finalMesh.uv[i] = mesh1.uv[i];
            }

            for (int i = 0; i < mesh2.verticies.Length; i++)
            {
                finalMesh.verticies[mesh1VertCount + i] = mesh2.verticies[i];
                finalMesh.normals[mesh1VertCount + i] = mesh2.normals[i];
                finalMesh.tangents[mesh1VertCount + i] = mesh2.tangents[i];
                finalMesh.uv[mesh1VertCount + i] = mesh2.uv[i];
            }

            for (int i = 0; i < mesh1.triangles.Length; i++)
            {
                finalMesh.triangles[i] = mesh1.triangles[i];
            }

            for (int i = 0; i < mesh2.triangles.Length; i++)
            {
                finalMesh.triangles[mesh1TriCount + i] = mesh1VertCount + mesh2.triangles[i];
            }

            return finalMesh;
        }

        private static UncheckedMesh CreateSideMesh(MeshProfile shape)
        {
            var vertices = new List<Vertex>();
            var triangles = new List<int>();

            for (int i = 0; i < shape.SubDivCount; i++)
                vertices.AddRange(shape.Sections[i].Points.Select(p=>p.Value));

            for (int i = 0; i < shape.PointCount - 1; i++)
            {
                for (int s = 0; s < shape.SubDivCount - 1; s++)
                {
                    triangles.AddRange(Triangulate(shape.Sections[s].Points[i]));
                }
            }

            return CreateMesh(vertices, triangles);
        }

        private static UncheckedMesh CreateEndsMesh(MeshProfile shape)
        {
            var vertices = new List<Vertex>();
            var triangles = new List<int>();

            var topVertices = shape.Top.Points.Select(p => p.GetCapVertex())
                .RemoveDoubles((v1, v2) => v1.Pos.IsCloseTo(v2.Pos)).ToList();

            var bottomVertices = shape.Bottom.Points.Select(p => p.GetCapVertex())
                .RemoveDoubles((v1, v2) => v1.Pos.IsCloseTo(v2.Pos)).ToList();

            var topCenter = new Vertex(Vector3.up * shape.Top.PosY, Vector3.up, new Vector4(-1, 0, 0, 1), new Vector2(0.5f, 0.5f));
            var botCenter = new Vertex(Vector3.up * shape.Bottom.PosY, Vector3.down, new Vector4(-1, 0, 0, -1), new Vector2(0.5f, 0.5f));

            vertices.Add(topCenter);
            vertices.AddRange(topVertices);
            vertices.Add(botCenter);
            vertices.AddRange(bottomVertices);

            for (int i = 0; i < topVertices.Count; i++)
            {
                triangles.Add(0);//top center
                triangles.Add(1 + i);
                triangles.Add(1 + (i + 1) % topVertices.Count);
            }

            int botcenterIdx = topVertices.Count + 1;

            for (int i = 0; i < bottomVertices.Count; i++)
            {
                triangles.Add(botcenterIdx);//bottom center
                triangles.Add(botcenterIdx + 1 + (i + 1) % bottomVertices.Count);
                triangles.Add(botcenterIdx + 1 + i);
            }

            return CreateMesh(vertices, triangles);
        }

        private static List<int> Triangulate(MeshPoint point)
        {
            //sections points are ordered by radial angle (outward), so 'point.Next' is at the left of 'point'
            return Triangulate(point.Next, point, point.Bottom.Next, point.Bottom);
        }

        private static List<int> Triangulate(MeshPoint tl, MeshPoint tr, MeshPoint bl, MeshPoint br)
        {
            var triangles = new List<int>();


            //NOTE: Unity use a clockwise triangle winding

            //method #1 |/|
            //if (!tl.Value.Pos.CloseTo(tr.Value.Pos))
            //{
                triangles.Add(tl.vIndex);
                triangles.Add(tr.vIndex);
                triangles.Add(bl.vIndex);
            //}

            //if (!bl.Value.Pos.CloseTo(br.Value.Pos))
            //{
                triangles.Add(bl.vIndex);
                triangles.Add(tr.vIndex);
                triangles.Add(br.vIndex);
            //}

            ////method #2 |\|
            //triangles.Add(tl.vIndex);
            //triangles.Add(br.vIndex);
            //triangles.Add(bl.vIndex);

            //triangles.Add(tl.vIndex);
            //triangles.Add(tr.vIndex);
            //triangles.Add(br.vIndex);
            return triangles;
        }

        #region Helper Classes

        private class MeshProfile
        {
            public MeshSection[] Sections { get; set; }
            
            public int PointCount
            {
                get { return Sections[0].PointCount; }
            }

            public int SubDivCount
            {
                get { return Sections.Length; }
            }

            public MeshSection Top
            {
                get { return Sections[0]; }
            }

            public MeshSection Bottom
            {
                get { return Sections[Sections.Length - 1]; }
            }

            public float Length
            {
                get { return Top.PosY - Bottom.PosY; }
            }

            public MeshProfile(IEnumerable<MeshSection> sections)
            {
                Sections = sections.OrderByDescending(s => s.PosY).ToArray();
                for (int i = 0; i < Sections.Length; i++)
                {
                    Sections[i].Mesh = this;
                    Sections[i].Index = i;
                }

                for (int i = 0; i < Sections.Length; i++)
                    Sections[i].Init();
            }

            public MeshProfile(params MeshSection[] sections)
            {
                Sections = sections.OrderByDescending(s => s.PosY).ToArray();
                for (int i = 0; i < Sections.Length; i++)
                {
                    Sections[i].Mesh = this;
                    Sections[i].Index = i;
                }

                for (int i = 0; i < Sections.Length; i++)
                    Sections[i].Init();
            }
        }

        private class MeshSection
        {
            private MeshPoint[] _Points;

            public int Index { get; set; }

            public MeshProfile Mesh { get; set; }

            public ProfileSection Profile { get; set; }

            public float PosY { get; set; }

            public MeshPoint[] Points
            {
                get { return _Points; }
            }
            
            public int PointCount
            {
                get { return Profile.PointCount; }
            }

            public MeshSection Previous
            {
                get
                {
                    if (Mesh == null || Index == 0)
                        return null;
                    return Mesh.Sections[Index - 1];
                }
            }

            public MeshSection Next
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

            public MeshSection(ProfileSection profile, float posY)
            {
                Index = -1;
                Profile = profile;
                PosY = posY;
                _Points = new MeshPoint[profile.PointCount];
                for (int i = 0; i < profile.PointCount; i++)
                {
                    _Points[i] = new MeshPoint(this, i);
                }
            }

            public void Init()
            {
                for (int i = 0; i < PointCount; i++)
                    _Points[i].Init();

                //for (int i = 0; i < PointCount; i++)
                //    _Points[i].LateInit();
            }
        }

        [System.Diagnostics.DebuggerDisplay("{Value}")]
        private class MeshPoint
        {
            public MeshSection Section { get; set; }

            private Vertex _Value;
            private int _Index;

            #region Properties

            public int Index
            {
                get { return _Index; }
            }

            public ProfilePoint Point
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

            public MeshPoint Top
            {
                get
                {
                    if (Section == null || Section.Previous == null)
                        return null;
                    return Section.Previous.Points[Index];
                }
            }

            public MeshPoint Bottom
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

            public MeshPoint(MeshSection section, int index)
            {
                Section = section;
                _Index = index;
            }

            public void Init()
            {
                var vp = new Vector3(Point.Position.x, Section.PosY, Point.Position.y);
                var vn = new Vector3(Point.Normal.x, 0, Point.Normal.y);
                

                var np = Next.Point.Position.IsCloseTo(Point.Position) ? Next.Next : Next;
                var tanN = (np.Point.Position - Point.Position).normalized;
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

            //public void LateInit()
            //{
            //    //var deltaP = Next.Value.Pos - Value.Pos;

            //}
        }

        #endregion
    }
}
