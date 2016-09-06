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
        public static UncheckedMesh ExtrudeSides(ProfileSection profile, float length)
        {
            var vertices = new List<Vertex>();
            var triangles = new List<int>();
            ProfilePoint lastPoint = null;
            float uvPos = 0;
            for (int i = 0; i < profile.PointCount; i++)
            {
                var curPoint = profile.Points[i];
                if (lastPoint != null)
                    uvPos += (curPoint.Position - lastPoint.Position).magnitude / profile.Perimeter;

                //I use Insert then Add to keep the top vertices at the start and the bottom ones at the end
                vertices.Insert(i, GetVertex(curPoint, length / 2f, uvPos));//top
                vertices.Add(GetVertex(curPoint, -length / 2f, uvPos));//bottom

                lastPoint = curPoint;
            }

            for (int i = 0; i < profile.PointCount - 1; i++)
            {
                triangles.Add(profile.PointCount + i);//bottom left
                triangles.Add(profile.PointCount + i + 1);//bottom right
                triangles.Add(i + 1);//top right

                triangles.Add(profile.PointCount + i);//bottom left
                triangles.Add(i + 1);//top right
                triangles.Add(i);//top left
            }

            return CreateMesh(vertices, triangles);
        }

        public static UncheckedMesh CreateCaps(ProfileSection profile, float length)
        {
            var vertices = new List<Vertex>();
            var triangles = new List<int>();
            var centerOffset = new Vector2(profile.Width / 2f, profile.Height / 2f);

            vertices.Add(new Vertex(new Vector3(0, 0.5f * length, 0), new Vector3(0, 1, 0), new Vector4(-1, 0, 0, -1), new Vector2(0.5f, 0.5f)));//top center
            
            for (int i = 0; i < profile.PointCount; i++)
            {
                var curPoint = profile.Points[i];
                var uv = new Vector2((curPoint.Position.x + centerOffset.x) / profile.Width, (curPoint.Position.y + centerOffset.y) / profile.Height);
                vertices.Add(new Vertex(
                    new Vector3(curPoint.Position.x, 0.5f * length, curPoint.Position.y),
                    new Vector3(0, 1, 0), new Vector4(-1, 0, 0, -1), uv));
            }

            vertices.Add(new Vertex(new Vector3(0, -0.5f * length, 0), new Vector3(0, -1, 0), new Vector4(-1, 0, 0, 1), new Vector2(0.5f, 0.5f)));//bottom center

            for (int i = 0; i < profile.PointCount; i++)
            {
                var curPoint = profile.Points[i];
                var uv = new Vector2((curPoint.Position.x + centerOffset.x) / profile.Width, (curPoint.Position.y + centerOffset.y) / profile.Height);
                vertices.Add(new Vertex(
                    new Vector3(curPoint.Position.x, -0.5f * length, curPoint.Position.y),
                    new Vector3(0, -1, 0), new Vector4(-1, 0, 0, 1), uv));
            }

            for (int s = 0; s < profile.PointCount - 1; s++)
            {
                //top
                triangles.Add(0);//center
                triangles.Add(2 + s);
                triangles.Add(1 + s);
                //bottom
                triangles.Add(profile.PointCount);//center
                triangles.Add(profile.PointCount + 1 + s);
                triangles.Add(profile.PointCount + 2 + s);
            }

            return CreateMesh(vertices, triangles);
        }

        private static Vertex GetVertex(ProfilePoint pp, float y, float uv)
        {
            var vp = new Vector3(pp.Position.x, y, pp.Position.y);
            var vn = new Vector3(pp.Normal.x, 0, pp.Normal.y);
            return new Vertex(vp, vn, GetTangentFromNormal(vn), new Vector2(uv, y > 0 ? 0 : 1)); 
        }

        private static Vertex GetVertex(ProfilePoint pp, float y, Vector2 uv)
        {
            var vp = new Vector3(pp.Position.x, y, pp.Position.y);
            var vn = new Vector3(pp.Normal.x, 0, pp.Normal.y);
            return new Vertex(vp, vn, GetTangentFromNormal(vn), uv);
        }

        private static Vector4 GetTangentFromNormal(Vector3 normal)
        {
            Vector3 t1 = Vector3.Cross(normal, Vector3.forward);
            Vector3 t2 = Vector3.Cross(normal, Vector3.up);
            return t1.magnitude > t2.magnitude ? t1 : t2;
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
    }
}
