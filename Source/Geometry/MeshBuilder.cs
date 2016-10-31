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

        public class ShapeParams
        {
            public int Subdivisions { get; set; }
            public float Slant { get; set; }
            public Angle OffsetRotation { get; set; }

            public ShapeParams()
            {
                Subdivisions = 3;
                Slant = 0f;
                OffsetRotation = Angle.Zero;
            }

            public ShapeParams(int subdivisions, float slant, Angle offsetRotation)
            {
                Subdivisions = subdivisions;
                Slant = slant;
                OffsetRotation = offsetRotation;
            }
        }

        public static ProceduralMesh CreateProceduralMesh(ContourProfile profile, float length)
        {
            var meshShape = new MeshShape(
                new MeshLayer(profile, length / 2f),
                new MeshLayer(profile, -length / 2f)
                ) { Parameters = new ShapeParams() { Subdivisions = 0 } };

            return CreateProceduralMeshes(meshShape);
        }

        public static ProceduralMesh CreateProceduralMesh(ContourProfile top, ContourProfile bottom, float length)
        {
            return CreateProceduralMesh(top, bottom, length, new ShapeParams());
        }

        public static ProceduralMesh CreateProceduralMesh(ContourProfile top, ContourProfile bottom, float length, int subdivisions)
        {
            return CreateProceduralMesh(top, bottom, length, new ShapeParams() { Subdivisions = subdivisions });
        }

        public static ProceduralMesh CreateProceduralMesh(ContourProfile top, ContourProfile bottom, float length, ShapeParams shapeParams)
        {
            return CreateProceduralMeshes(CreateProceduralMeshShape(top, bottom, length, shapeParams));
        }

        private static MeshShape CreateProceduralMeshShape(ContourProfile top, ContourProfile bottom, float length, ShapeParams shapeParams)
        {
            if (shapeParams.OffsetRotation != Angle.Zero)
                top = ContourProfile.Rotate(top, shapeParams.OffsetRotation);

            var topAdapter = ContourProfile.CreateAdapter(top, bottom);
            var botAdapter = ContourProfile.CreateAdapter(bottom, top);

            if (botAdapter.PointCount != topAdapter.PointCount)
            {
                Debug.Log("Failed to created adapter, sides does not have same number of vertices");

                return null;
            }

            var maxWidth = Mathf.Max(topAdapter.Width, botAdapter.Width);
            var minWidth = Mathf.Min(topAdapter.Width, botAdapter.Width);

            var slantAmountX = ((maxWidth - minWidth) / 2f) * shapeParams.Slant;

            var sections = new List<MeshLayer>();

            var topLayer = new MeshLayer(topAdapter, length / 2f);
            if (slantAmountX != 0)
                topLayer.Offset = new Vector2(slantAmountX, 0);
            sections.Add(topLayer);

            float subdivRatio = 1f / (shapeParams.Subdivisions + 1);
            //float yStep = -length * subdivRatio;

            for (int i = 1; i <= shapeParams.Subdivisions; i++)
            {
                var t = subdivRatio * i;

                var subSection = ContourProfile.Lerp(topAdapter, botAdapter, t);

                var meshLayer = new MeshLayer(subSection, Mathf.Lerp(length/2f, -length/2f, t));

                if (slantAmountX != 0)
                    meshLayer.Offset = new Vector2(slantAmountX * (1f - t), 0);

                sections.Add(meshLayer);
            }

            var bottomLayer = new MeshLayer(botAdapter, -length / 2f);

            sections.Add(bottomLayer);

            return new MeshShape(sections) { Parameters = shapeParams };
        }

        private static ProceduralMesh CreateProceduralMeshes(MeshShape meshShape)
        {
            var baseMesh = CreateSideMesh(meshShape);
            var endsMesh = CreateEndsMesh(meshShape);
            var collider = CreateColliderMesh(meshShape);
            
            float volume = 0f;
            for (int i = 0; i < meshShape.SubDivCount - 1; i++)
            {
                var subDiv = meshShape.Sections[i];
                var avgArea = (subDiv.Profile.SurfaceArea + subDiv.Next.Profile.SurfaceArea) / 2f;
                volume += avgArea * (subDiv.PosY - subDiv.Next.PosY);
            }
            return new ProceduralMesh(meshShape, baseMesh, endsMesh, collider, volume);
        }

        private static UncheckedMesh WriteMesh(List<Vertex> vertices, List<int> triangleIndices)
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

        private static UncheckedMesh CreateSideMesh(MeshShape shape)
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

            return WriteMesh(vertices, triangles);
        }

        private static UncheckedMesh CreateEndsMesh(MeshShape shape)
        {
            var vertices = new List<Vertex>();
            var triangles = new List<int>();

            var topVertices = shape.Top.Points.Select(p => p.GetCapVertex())
                .RemoveDoubles((v1, v2) => v1.Pos.IsCloseTo(v2.Pos)).ToList();

            var bottomVertices = shape.Bottom.Points.Select(p => p.GetCapVertex())
                .RemoveDoubles((v1, v2) => v1.Pos.IsCloseTo(v2.Pos)).ToList();

            var topCenter = new Vertex(Vector3.up * shape.Top.PosY + new Vector3(shape.Top.Offset.x,0,shape.Top.Offset.y), 
                Vector3.up, new Vector4(-1, 0, 0, 1), new Vector2(0.5f, 0.5f));
            var botCenter = new Vertex(Vector3.up * shape.Bottom.PosY + new Vector3(shape.Bottom.Offset.x, 0, shape.Bottom.Offset.y), 
                Vector3.down, new Vector4(-1, 0, 0, -1), new Vector2(0.5f, 0.5f));

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

            return WriteMesh(vertices, triangles);
        }

        private static UncheckedMesh CreateColliderMesh(MeshShape shape)
        {
            var simplifiedTop = ContourProfile.Simplify(shape.Top.Profile);
            var simplifiedBot = ContourProfile.Simplify(shape.Bottom.Profile);

            ShapeParams collParams = new ShapeParams() { Subdivisions = 2 };
            if (shape.Parameters != null)
            {
                collParams.Slant = shape.Parameters.Slant;
                collParams.OffsetRotation = shape.Parameters.OffsetRotation;
            }

            var colliderShape = CreateProceduralMeshShape(simplifiedTop, 
                simplifiedBot, 
                shape.Length,
                collParams);
            var sideMesh = CreateSideMesh(colliderShape);
            var endsMesh = CreateEndsMesh(colliderShape);
            var collider = MergeMeshes(sideMesh, endsMesh);
            return collider;
        }

        private static IEnumerable<int> Triangulate(MeshPoint point)
        {
            //sections points are ordered by radial angle (outward), so 'point.Next' is at the left of 'point'
            return Triangulate(point.Next, point, point.Below.Next, point.Below);
        }

        private static IEnumerable<int> Triangulate(MeshPoint tl, MeshPoint tr, MeshPoint bl, MeshPoint br)
        {
            //if the points are close (eg. hard edge) skip the triangle
            if (tl.Point.Position.IsCloseTo(tr.Point.Position) &&
                bl.Point.Position.IsCloseTo(br.Point.Position))
                return new int[0];

            var triangles = new List<int>();
            //NOTE: Unity use a clockwise triangle winding

            var targetMiddle = Vector3.Lerp(Vector3.Lerp(tl.Value.Pos, tr.Value.Pos, 0.5f), Vector3.Lerp(bl.Value.Pos, br.Value.Pos, 0.5f), 0.5f);

            var tlbrMiddle = Vector3.Lerp(tl.Value.Pos, br.Value.Pos, 0.5f);
            var trblMiddle = Vector3.Lerp(tr.Value.Pos, bl.Value.Pos, 0.5f);

            var tlbrDist = (targetMiddle - tlbrMiddle).magnitude;
            var trblDist = (targetMiddle - trblMiddle).magnitude;

            //find which triangle pattern creates an edge closest to the desired shape
            if (tlbrDist < trblDist)
            {
                //From top left to bottom right |\|
                triangles.Add(tl.vIndex);
                triangles.Add(br.vIndex);
                triangles.Add(bl.vIndex);

                triangles.Add(tl.vIndex);
                triangles.Add(tr.vIndex);
                triangles.Add(br.vIndex);
            }
            else
            {
                //From top right to bottom left |/|
                triangles.Add(tl.vIndex);
                triangles.Add(tr.vIndex);
                triangles.Add(bl.vIndex);

                triangles.Add(bl.vIndex);
                triangles.Add(tr.vIndex);
                triangles.Add(br.vIndex);
            }

            return triangles;
        }

    }

}