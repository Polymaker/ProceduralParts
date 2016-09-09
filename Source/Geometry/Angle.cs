using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProceduralParts.Geometry
{
    public struct Angle : IComparable<Angle>
    {
        public static readonly Angle Zero = new Angle();
        public static AngleUnit DefaultConvertionUnit = AngleUnit.Degrees;
        public const float PI = 3.1415926535897931f;

        private float _Degrees;

        public float Degrees
        {
            get { return _Degrees; }
            set
            {
                if (ReferenceEquals(this, Zero))
                    return;
                _Degrees = value;
            }
        }

        public float Radians
        {
            get { return ToRadians(Degrees); }
            set
            {
                if (object.ReferenceEquals(this, Zero))
                    return;
                _Degrees = ToDegrees(value);
            }
        }

        public bool IsNormalized
        {
            get { return _Degrees >= 0f && _Degrees <= 260f; }
        }

        #region Static Ctors

        public static Angle FromDegrees(float degrees)
        {
            return new Angle { Degrees = degrees };
        }

        public static Angle FromRadians(float radians)
        {
            return new Angle { Radians = radians };
        }

        #endregion

        #region Operators

        public static explicit operator float(Angle angle)
        {
            return DefaultConvertionUnit == AngleUnit.Degrees ? angle.Degrees : angle.Radians;
        }

        public static implicit operator Angle(float angle)
        {
            return DefaultConvertionUnit == AngleUnit.Degrees ? FromDegrees(angle) : FromRadians(angle);
        }

        public static Angle operator +(Angle a1, Angle a2)
        {
            return FromDegrees(a1.Degrees + a2.Degrees);
        }

        public static Angle operator -(Angle a1, Angle a2)
        {
            return FromDegrees(a1.Degrees - a2.Degrees);
        }

        public static Angle operator *(Angle a1, float value)
        {
            return FromDegrees(a1.Degrees * value);
        }

        //public static Angle operator *(Angle a1, Angle a2)
        //{
        //    return FromDegrees(a1.Degrees * a2.Degrees);
        //}

        public static Angle operator /(Angle a1, float value)
        {
            return FromDegrees(a1.Degrees / value);
        }

        public static float operator /(Angle a1, Angle a2)
        {
            return a1.Degrees / a2.Degrees;
        }

        public static Angle operator %(Angle a1, float value)
        {
            return FromDegrees(a1.Degrees % value);
        }

        public static bool operator ==(Angle a1, Angle a2)
        {
            return a1.Degrees == a2.Degrees;
        }

        public static bool operator !=(Angle a1, Angle a2)
        {
            return a1.Degrees != a2.Degrees;
        }

        public static bool operator >(Angle a1, Angle a2)
        {
            return a1.Degrees > a2.Degrees;
        }

        public static bool operator <(Angle a1, Angle a2)
        {
            return a1.Degrees < a2.Degrees;
        }

        public static bool operator >=(Angle a1, Angle a2)
        {
            return a1.Degrees >= a2.Degrees;
        }

        public static bool operator <=(Angle a1, Angle a2)
        {
            return a1.Degrees <= a2.Degrees;
        }

        public override bool Equals(object obj)
        {
            if (obj is Angle)
                return Degrees.Equals(((Angle)obj).Degrees);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _Degrees.GetHashCode();
        }

        #endregion

        /// <summary>
        /// Clamps the angle between 0-360
        /// </summary>
        public void Normalize()
        {
            if (IsNormalized)
                return;
            _Degrees = NormalizeDegrees(_Degrees);
        }

        public Angle Normalized()
        {
            if (IsNormalized)
                return this;
            return FromDegrees(NormalizeDegrees(_Degrees));
        }

        public Angle DeltaAngle(Angle other)
        {
            return DeltaAngle(this, other);
        }

        public static Angle DeltaAngle(Angle a1, Angle a2)
        {
            a1.Normalize();
            a2.Normalize();
            var delta = Max(a1, a2) - Min(a1, a2);
            return delta.Degrees <= 180 ? delta : FromDegrees(360f - delta.Degrees).Normalized();
        }

        public Angle Distance(Angle other)
        {
            return Distance(this, other);
        }

        public static Angle Distance(Angle a1, Angle a2)
        {
            a1.Normalize();
            a2.Normalize();
            
            if (a2 < a1 && (a1 - a2).Degrees > 180f)
            {
                a2 = a1 + DeltaAngle(a1, a2);
            }
            return a2 - a1;
        }

        public static Angle Max(Angle angle1, Angle angle2)
        {
            return angle1 > angle2 ? angle1 : angle2;
        }

        public static Angle Min(Angle angle1, Angle angle2)
        {
            return angle1 < angle2 ? angle1 : angle2;
        }

        public bool IsBetween(Angle a1, Angle a2)
        {
            a1.Normalize();
            a2.Normalize();

            var a3 = Normalized();
            if (a1 == a2)
                return a1 == a3;

            var delta = (a2 - a1).Normalized();

            if (delta.Degrees > 355f)
                return false;

            if (a1.Degrees + delta.Degrees > 360f || a2.Degrees - delta.Degrees < 0)
            {
                var start1 = a1; var end1 = a1 + delta;
                var start2 = a2 - delta; var end2 = a2;
                return (a3 >= start1 && a3 <= end1) || (a3 >= start2 && a3 <= end2);
            }

            return a3 >= a1 && a3 <= a2;
        }

        public bool IsBetween(Angle a1, Angle a2, bool exclusive)
        {
            if (!exclusive)
                return this.IsBetween(a1, a2);
            a1.Normalize();
            a2.Normalize();

            var a3 = Normalized();
            if (a1 == a2)
                return a1 == a3;

            var delta = (a2 - a1).Normalized();

            if (delta.Degrees > 355f)
                return false;

            if (a1.Degrees + delta.Degrees > 360f || a2.Degrees - delta.Degrees < 0)
            {
                var start1 = a1; var end1 = a1 + delta;
                var start2 = a2 - delta; var end2 = a2;
                return (a3 > start1 && a3 < end1) || (a3 > start2 && a3 < end2);
            }

            return a3 > a1 && a3 < a2;
        }

        #region Convertion

        public static float ToDegrees(float radians)
        {
            return (radians * 180.0f) / (float)Math.PI;
        }

        public static float ToRadians(float degrees)
        {
            return (float)Math.PI * degrees / 180.0f;
        }

        #endregion

        public static float NormalizeDegrees(float degrees)
        {
            degrees = degrees % 360f;
            if (Math.Abs(degrees) <= float.Epsilon)
                return 0f;
            if (degrees < 0f)
                degrees += 360f;
            return degrees;
        }

        public static float NormalizeRadians(float radians)
        {
            radians = radians % (PI * 2f);
            if (radians < 0f)
                radians += PI * 2f;
            return radians;
        }

        public override string ToString()
        {
            if (DefaultConvertionUnit == AngleUnit.Degrees)
                return string.Format("{0}°", Degrees);
            else
                return string.Format("{0}ᶜ", Radians);
        }

        public int CompareTo(Angle other)
        {
            return Degrees.CompareTo(other.Degrees);
        }
    }

    public enum AngleUnit
    {
        Degrees,
        Radians
    }
}
