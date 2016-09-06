using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralParts.Geometry
{
    public class ProfilePoint
    {
        public Vector2 Position { get; set; }

        public Vector2 Normal { get; set; }

        public float UV { get; set; }

        public ProfilePoint(Vector2 position, Vector2 normal)
        {
            Position = position;
            Normal = normal;
            UV = 0f;
        }

        public ProfilePoint(Vector2 position, Vector2 normal, float uV)
        {
            Position = position;
            Normal = normal;
            UV = uV;
        }

        public ProfilePoint(float pX, float pZ, float nX, float nZ)
            : this(new Vector2(pX, pZ), new Vector2(nX, nZ)) { }

        public ProfilePoint(float pX, float pZ, float nX, float nZ, float uv)
            : this(new Vector2(pX, pZ), new Vector2(nX, nZ), uv) { }
        
        //public static ProfilePoint[] Mk2Profile = new ProfilePoint[] //Diam=1.25
        //{
        //    new ProfilePoint( 0.000f, -0.750f,  0.105f, -0.995f/*, 0.270f*/),
        //    new ProfilePoint( 0.190f, -0.730f,  0.231f, -0.973f/*, 0.234f*/),
        //    new ProfilePoint( 0.375f, -0.660f,  0.430f, -0.903f/*, 0.196f*/),
        //    new ProfilePoint( 0.775f, -0.427f,  0.504f, -0.864f/*, 0.108f*/),
        //    new ProfilePoint( 1.250f, -0.150f,  0.504f, -0.864f/*, 0.001f*/),
        //    new ProfilePoint( 1.250f, -0.150f,  1.000f,  0.000f/*, 0.994f*/),
        //    new ProfilePoint( 1.250f,  0.150f,  1.000f,  0.000f/*, 0.994f*/),
        //    new ProfilePoint( 1.250f,  0.150f,  0.504f,  0.864f/*, 0.281f*/),
        //    new ProfilePoint( 0.775f,  0.427f,  0.504f,  0.864f/*, 0.429f*/),
        //    new ProfilePoint( 0.375f,  0.660f,  0.430f,  0.903f/*, 0.543f*/),
        //    new ProfilePoint( 0.190f,  0.730f,  0.231f,  0.973f/*, 0.589f*/),
        //    new ProfilePoint( 0.000f,  0.750f,  0.105f,  0.995f/*, 0.633f*/),
        //    new ProfilePoint( 0.000f,  0.750f, -0.105f,  0.995f/*, 0.633f*/),
        //    new ProfilePoint(-0.190f,  0.730f, -0.231f,  0.973f/*, 0.589f*/),
        //    new ProfilePoint(-0.375f,  0.660f, -0.430f,  0.903f/*, 0.543f*/),
        //    new ProfilePoint(-0.775f,  0.427f, -0.504f,  0.864f/*, 0.429f*/),
        //    new ProfilePoint(-1.250f,  0.150f, -0.504f,  0.864f/*, 0.281f*/),
        //    new ProfilePoint(-1.250f,  0.150f, -1.000f,  0.000f/*, 0.994f*/),
        //    new ProfilePoint(-1.250f, -0.150f, -1.000f,  0.000f/*, 0.994f*/),
        //    new ProfilePoint(-1.250f, -0.150f, -0.504f, -0.864f/*, 0.001f*/),
        //    new ProfilePoint(-0.775f, -0.427f, -0.504f, -0.864f/*, 0.108f*/),
        //    new ProfilePoint(-0.375f, -0.660f, -0.430f, -0.903f/*, 0.196f*/),
        //    new ProfilePoint(-0.190f, -0.730f, -0.231f, -0.973f/*, 0.234f*/),
        //    new ProfilePoint( 0.000f, -0.750f, -0.105f, -0.995f/*, 0.270f*/)
        //};

        //public static ProfilePoint[] Mk3Profile = new ProfilePoint[] //Diam=3.75
        //{
        //    new ProfilePoint( 0.000f, -1.875f,  0.131f, -0.991f),
        //    new ProfilePoint( 0.485f, -1.811f,  0.259f, -0.966f),
        //    new ProfilePoint( 0.938f, -1.624f,  0.500f, -0.866f),
        //    new ProfilePoint( 1.326f, -1.326f,  0.609f, -0.793f),
        //    new ProfilePoint( 1.326f, -1.326f,  0.793f, -0.609f),
        //    new ProfilePoint( 1.624f, -0.938f,  0.793f, -0.609f),
        //    new ProfilePoint( 1.624f, -0.938f,  1.000f,  0.000f),
        //    new ProfilePoint( 1.624f, -0.900f,  1.000f,  0.000f),
        //    new ProfilePoint( 1.624f,  0.900f,  1.000f,  0.000f),
        //    new ProfilePoint( 1.624f,  0.937f,  1.000f,  0.000f),
        //    new ProfilePoint( 1.624f,  0.937f,  0.793f,  0.609f),
        //    new ProfilePoint( 1.326f,  1.326f,  0.793f,  0.609f),
        //    new ProfilePoint( 1.326f,  1.326f,  0.609f,  0.793f),
        //    new ProfilePoint( 0.938f,  1.624f,  0.500f,  0.866f),
        //    new ProfilePoint( 0.485f,  1.811f,  0.259f,  0.966f),
        //    new ProfilePoint( 0.000f,  1.875f,  0.131f,  0.991f),
        //    new ProfilePoint( 0.000f,  1.875f, -0.131f,  0.991f),
        //    new ProfilePoint(-0.485f,  1.811f, -0.259f,  0.966f),
        //    new ProfilePoint(-0.938f,  1.624f, -0.500f,  0.866f),
        //    new ProfilePoint(-1.326f,  1.326f, -0.609f,  0.793f),
        //    new ProfilePoint(-1.326f,  1.326f, -0.793f,  0.609f),
        //    new ProfilePoint(-1.624f,  0.937f, -0.793f,  0.609f),
        //    new ProfilePoint(-1.624f,  0.937f, -1.000f,  0.000f),
        //    new ProfilePoint(-1.624f,  0.900f, -1.000f,  0.000f),
        //    new ProfilePoint(-1.624f, -0.900f, -1.000f,  0.000f),
        //    new ProfilePoint(-1.624f, -0.938f, -1.000f,  0.000f),
        //    new ProfilePoint(-1.624f, -0.938f, -0.793f, -0.609f),
        //    new ProfilePoint(-1.326f, -1.326f, -0.793f, -0.609f),
        //    new ProfilePoint(-1.326f, -1.326f, -0.609f, -0.793f),
        //    new ProfilePoint(-0.938f, -1.624f, -0.500f, -0.866f),
        //    new ProfilePoint(-0.485f, -1.811f, -0.259f, -0.966f),
        //    new ProfilePoint( 0.000f, -1.875f, -0.131f, -0.991f)
        //};

        /*
        public static ProfilePoint[] GetMk2Profile(float diameter)
        {
            float scale = diameter / 1.25f;
            return Mk2Profile.Select(p => new ProfilePoint(p.Position * scale, p.Normal)).ToArray();
        }

        public static ProfilePoint[] GetMk3Profile(float diameter)
        {
            float scale = diameter / 3.75f;
            return Mk3Profile.Select(p => new ProfilePoint(p.Position * scale, p.Normal)).ToArray();
        }

        public static ProfilePoint[] GetPolygonProfile(int sideCount, float diameter)
        {
            var points = new List<ProfilePoint>();
            float theta = (Mathf.PI * 2f) / (float)sideCount;
            float halfT = theta / 2f;

            for (int s = 0; s < sideCount; s++)
            {
                var curAngle = theta * s;
                var norm = GetPoint(curAngle, 1f);
                points.Add(new ProfilePoint(GetPoint(curAngle - halfT, diameter), norm, s / (float)sideCount));
                points.Add(new ProfilePoint(GetPoint(curAngle + halfT, diameter), norm, (s + 1) / (float)sideCount));
            }

            return points.ToArray();
        }

        public static ProfilePoint[] GetCylinderProfile(float diameter, int resolution = 64)
        {
            var points = new List<ProfilePoint>();
            float theta = (Mathf.PI * 2f) / (float)resolution;
            for (int s = 0; s <= resolution; s++)
            {
                var curAngle = theta * s;
                var norm = GetPoint(curAngle, 1f);
                points.Add(new ProfilePoint(GetPoint(curAngle, diameter), norm, s / (float)resolution));
            }
            return points.ToArray();
        }*/

        internal static Vector2 GetPoint(float angle, float dist)
        {
            return new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle)) * dist;
        }
    }
}
