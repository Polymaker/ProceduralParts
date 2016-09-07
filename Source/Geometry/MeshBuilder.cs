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

        private static List<Vector2> GetProfileCapVertices(ProfileSection profile)
        {
            Vector2 lastPoint = Vector2.zero;
            var vertices = new List<Vector2>();
            for (int i = 1; i <= profile.PointCount; i++)
            {
                var curPoint = profile.Points[i % profile.PointCount].Position;
                if ((curPoint - lastPoint).magnitude > float.Epsilon)
                {
                    if (i < profile.PointCount)
                        vertices.Add(curPoint);
                    else
                        vertices.Insert(0, curPoint);
                }
                lastPoint = curPoint;
            }
            return vertices;
        }

        public static UncheckedMesh CreateCaps(ProfileSection profile, float length)
        {
            var vertices = new List<Vertex>();
            var triangles = new List<int>();
            var centerOffset = new Vector2(profile.Width / 2f, profile.Height / 2f);
            var maxSize = Math.Max(profile.Width, profile.Height);
            var uvOffset = new Vector2((maxSize - profile.Width) / 2f, (maxSize - profile.Height) / 2f);
            var distinctPoints = GetProfileCapVertices(profile);

            //Note to myself: face orientation has nothing to do with vertex normals... just double check triangle winding order

            //TOP VERTICES
            vertices.Add(new Vertex(new Vector3(0, 0.5f * length, 0), new Vector3(0, 1, 0), new Vector4(-1, 0, 0, -1), new Vector2(0.5f, 0.5f)));//top center
            for (int i = 0; i < distinctPoints.Count; i++)
            {
                var curPoint = distinctPoints[i];
                var uv = new Vector2((curPoint.x + centerOffset.x + uvOffset.x) / maxSize, (curPoint.y + centerOffset.y + uvOffset.y) / maxSize);
                vertices.Add(new Vertex(
                    new Vector3(curPoint.x, 0.5f * length, curPoint.y),
                    new Vector3(0, 1, 0), new Vector4(-1, 0, 0, -1), uv));
            }

            //BOTTOM VERTICES
            vertices.Add(new Vertex(new Vector3(0, -0.5f * length, 0), new Vector3(0, -1, 0), new Vector4(-1, 0, 0, 1), new Vector2(0.5f, 0.5f)));//bottom center
            for (int i = 0; i < distinctPoints.Count; i++)
            {
                var curPoint = distinctPoints[i];
                var uv = new Vector2((curPoint.x + centerOffset.x + uvOffset.x) / maxSize, (curPoint.y + centerOffset.y + uvOffset.y) / maxSize);
                vertices.Add(new Vertex(
                    new Vector3(curPoint.x, -0.5f * length, curPoint.y),
                    new Vector3(0, -1, 0), new Vector4(-1, 0, 0, 1), uv));
            }

            int vPerSide = distinctPoints.Count + 1;
            for (int s = 0; s < distinctPoints.Count; s++)
            {
                //top
                triangles.Add(0);//center
                triangles.Add(1 + s);
                triangles.Add(1 + (s + 1) % distinctPoints.Count);
                
                //bottom
                triangles.Add(vPerSide);//center
                triangles.Add(vPerSide + 1 + (s + 1) % distinctPoints.Count);
                triangles.Add(vPerSide + 1 + s);
            }

            return CreateMesh(vertices, triangles);
        }

        public static UncheckedMesh CreateCollider(ProfileSection profile, float length)
        {
            var sides = ExtrudeSides(profile, length);
            var caps = CreateCaps(profile, length);
            var finalMesh = new UncheckedMesh(sides.verticies.Length + caps.verticies.Length, sides.triangles.Length + caps.triangles.Length);

            for (int i = 0; i < sides.nVrt; i++)
            {
                finalMesh.verticies[i] = sides.verticies[i];
                finalMesh.normals[i] = sides.normals[i];
                finalMesh.tangents[i] = sides.tangents[i];
                finalMesh.uv[i] = sides.uv[i];
            }

            for (int i = 0; i < caps.nVrt; i++)
            {
                finalMesh.verticies[sides.nVrt + i] = caps.verticies[i];
                finalMesh.normals[sides.nVrt + i] = caps.normals[i];
                finalMesh.tangents[sides.nVrt + i] = caps.tangents[i];
                finalMesh.uv[sides.nVrt + i] = caps.uv[i];
            }

            for (int i = 0; i < sides.nTri; i++)
            {
                finalMesh.triangles[i] = sides.triangles[i];
            }

            for (int i = 0; i < caps.nTri; i++)
            {
                finalMesh.triangles[sides.nTri + i] = caps.triangles[i];
            }

            return finalMesh;
        }

        private static Vertex GetVertex(ProfilePoint pp, float y, float uv)
        {
            var vp = new Vector3(pp.Position.x, y, pp.Position.y);
            var vn = new Vector3(pp.Normal.x, 0, pp.Normal.y);
            return new Vertex(vp, vn, GetTangentFromNormal(vn), new Vector2(uv, y > 0 ? 0 : 1)); 
        }

        //private static Vertex GetVertex(ProfilePoint pp, float y, Vector2 uv)
        //{
        //    var vp = new Vector3(pp.Position.x, y, pp.Position.y);
        //    var vn = new Vector3(pp.Normal.x, 0, pp.Normal.y);
        //    return new Vertex(vp, vn, GetTangentFromNormal(vn), uv);
        //}

        private static Vector4 GetTangentFromNormal(Vector3 normal, bool top = true)
        {
            Vector3 t1 = Vector3.Cross(normal, Vector3.forward);
            Vector3 t2 = Vector3.Cross(normal, Vector3.up * (top ? 1f : -1f));
            var final = t1.magnitude > t2.magnitude ? t1 : t2;
            return new Vector4(final.x, final.y, final.z, -1);
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
