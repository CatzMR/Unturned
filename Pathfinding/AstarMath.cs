﻿using System;
using UnityEngine;

namespace Pathfinding
{
	public class AstarMath
	{
		public static int ComputeVertexHash(int x, int y, int z)
		{
			uint num = 2376512323u;
			uint num2 = 3625334849u;
			uint num3 = 3407524639u;
			uint num4 = (uint)((ulong)num * (ulong)((long)x) + (ulong)num2 * (ulong)((long)y) + (ulong)num3 * (ulong)((long)z));
			return (int)(num4 & 1073741823u);
		}

		public static Vector3 NearestPoint(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
		{
			Vector3 vector = Vector3.Normalize(lineEnd - lineStart);
			float num = Vector3.Dot(point - lineStart, vector);
			return lineStart + num * vector;
		}

		public static float NearestPointFactor(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
		{
			Vector3 vector = lineEnd - lineStart;
			float magnitude = vector.magnitude;
			vector /= magnitude;
			float num = Vector3.Dot(point - lineStart, vector);
			return num / magnitude;
		}

		public static float NearestPointFactor(Int3 lineStart, Int3 lineEnd, Int3 point)
		{
			Int3 rhs = lineEnd - lineStart;
			float sqrMagnitude = rhs.sqrMagnitude;
			return (float)Int3.Dot(point - lineStart, rhs) / sqrMagnitude;
		}

		public static float NearestPointFactor(Int2 lineStart, Int2 lineEnd, Int2 point)
		{
			Int2 b = lineEnd - lineStart;
			double num = (double)b.sqrMagnitudeLong;
			double num2 = (double)Int2.DotLong(point - lineStart, b) / num;
			return (float)num2;
		}

		public static Vector3 NearestPointStrict(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
		{
			Vector3 vector = lineEnd - lineStart;
			float magnitude = vector.magnitude;
			Vector3 vector2 = vector / magnitude;
			float num = Vector3.Dot(point - lineStart, vector2);
			return lineStart + Mathf.Clamp(num, 0f, magnitude) * vector2;
		}

		public static Vector3 NearestPointStrictXZ(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
		{
			lineStart.y = point.y;
			lineEnd.y = point.y;
			Vector3 vector = lineEnd - lineStart;
			Vector3 vector2 = vector;
			vector2.y = 0f;
			Vector3 vector3 = Vector3.Normalize(vector2);
			float num = Vector3.Dot(point - lineStart, vector3);
			return lineStart + Mathf.Clamp(num, 0f, vector2.magnitude) * vector3;
		}

		public static float DistancePointSegment(int x, int z, int px, int pz, int qx, int qz)
		{
			float num = (float)(qx - px);
			float num2 = (float)(qz - pz);
			float num3 = (float)(x - px);
			float num4 = (float)(z - pz);
			float num5 = num * num + num2 * num2;
			float num6 = num * num3 + num2 * num4;
			if (num5 > 0f)
			{
				num6 /= num5;
			}
			if (num6 < 0f)
			{
				num6 = 0f;
			}
			else if (num6 > 1f)
			{
				num6 = 1f;
			}
			num3 = (float)px + num6 * num - (float)x;
			num4 = (float)pz + num6 * num2 - (float)z;
			return num3 * num3 + num4 * num4;
		}

		public static float DistancePointSegment(Int3 a, Int3 b, Int3 p)
		{
			float num = (float)(b.x - a.x);
			float num2 = (float)(b.z - a.z);
			float num3 = (float)(p.x - a.x);
			float num4 = (float)(p.z - a.z);
			float num5 = num * num + num2 * num2;
			float num6 = num * num3 + num2 * num4;
			if (num5 > 0f)
			{
				num6 /= num5;
			}
			if (num6 < 0f)
			{
				num6 = 0f;
			}
			else if (num6 > 1f)
			{
				num6 = 1f;
			}
			num3 = (float)a.x + num6 * num - (float)p.x;
			num4 = (float)a.z + num6 * num2 - (float)p.z;
			return num3 * num3 + num4 * num4;
		}

		public static float DistancePointSegment2(int x, int z, int px, int pz, int qx, int qz)
		{
			Vector3 p;
			p..ctor((float)x, 0f, (float)z);
			Vector3 a;
			a..ctor((float)px, 0f, (float)pz);
			Vector3 b;
			b..ctor((float)qx, 0f, (float)qz);
			return AstarMath.DistancePointSegment2(a, b, p);
		}

		public static float DistancePointSegment2(Vector3 a, Vector3 b, Vector3 p)
		{
			float num = b.x - a.x;
			float num2 = b.z - a.z;
			float num3 = Mathf.Abs(num * (p.z - a.z) - (p.x - a.x) * num2);
			float num4 = num * num + num2 * num2;
			if (num4 > 0f)
			{
				return num3 / Mathf.Sqrt(num4);
			}
			return (a - p).magnitude;
		}

		public static float DistancePointSegmentStrict(Vector3 a, Vector3 b, Vector3 p)
		{
			Vector3 vector = AstarMath.NearestPointStrict(a, b, p);
			return (vector - p).sqrMagnitude;
		}

		public static float Hermite(float start, float end, float value)
		{
			return Mathf.Lerp(start, end, value * value * (3f - 2f * value));
		}

		public static Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			t = Mathf.Clamp01(t);
			float num = 1f - t;
			return num * num * num * p0 + 3f * num * num * t * p1 + 3f * num * t * t * p2 + t * t * t * p3;
		}

		public static float MapTo(float startMin, float startMax, float value)
		{
			value -= startMin;
			value /= startMax - startMin;
			value = Mathf.Clamp01(value);
			return value;
		}

		public static float MapToRange(float targetMin, float targetMax, float value)
		{
			value *= targetMax - targetMin;
			value += targetMin;
			return value;
		}

		public static float MapTo(float startMin, float startMax, float targetMin, float targetMax, float value)
		{
			value -= startMin;
			value /= startMax - startMin;
			value = Mathf.Clamp01(value);
			value *= targetMax - targetMin;
			value += targetMin;
			return value;
		}

		public static string FormatBytes(int bytes)
		{
			double num = (bytes < 0) ? -1.0 : 1.0;
			bytes = ((bytes < 0) ? (-bytes) : bytes);
			if (bytes < 1000)
			{
				return ((double)bytes * num).ToString() + " bytes";
			}
			if (bytes < 1000000)
			{
				return ((double)bytes / 1000.0 * num).ToString("0.0") + " kb";
			}
			if (bytes < 1000000000)
			{
				return ((double)bytes / 1000000.0 * num).ToString("0.0") + " mb";
			}
			return ((double)bytes / 1000000000.0 * num).ToString("0.0") + " gb";
		}

		public static string FormatBytesBinary(int bytes)
		{
			double num = (bytes < 0) ? -1.0 : 1.0;
			bytes = ((bytes < 0) ? (-bytes) : bytes);
			if (bytes < 1024)
			{
				return ((double)bytes * num).ToString() + " bytes";
			}
			if (bytes < 1048576)
			{
				return ((double)bytes / 1024.0 * num).ToString("0.0") + " kb";
			}
			if (bytes < 1073741824)
			{
				return ((double)bytes / 1048576.0 * num).ToString("0.0") + " mb";
			}
			return ((double)bytes / 1073741824.0 * num).ToString("0.0") + " gb";
		}

		public static int Bit(int a, int b)
		{
			return a >> b & 1;
		}

		public static Color IntToColor(int i, float a)
		{
			int num = AstarMath.Bit(i, 1) + AstarMath.Bit(i, 3) * 2 + 1;
			int num2 = AstarMath.Bit(i, 2) + AstarMath.Bit(i, 4) * 2 + 1;
			int num3 = AstarMath.Bit(i, 0) + AstarMath.Bit(i, 5) * 2 + 1;
			return new Color((float)num * 0.25f, (float)num2 * 0.25f, (float)num3 * 0.25f, a);
		}

		public static float MagnitudeXZ(Vector3 a, Vector3 b)
		{
			Vector3 vector = a - b;
			return (float)Math.Sqrt((double)(vector.x * vector.x + vector.z * vector.z));
		}

		public static float SqrMagnitudeXZ(Vector3 a, Vector3 b)
		{
			Vector3 vector = a - b;
			return vector.x * vector.x + vector.z * vector.z;
		}

		public static int Repeat(int i, int n)
		{
			while (i >= n)
			{
				i -= n;
			}
			return i;
		}

		public static float Abs(float a)
		{
			if (a < 0f)
			{
				return -a;
			}
			return a;
		}

		public static int Abs(int a)
		{
			if (a < 0)
			{
				return -a;
			}
			return a;
		}

		public static float Min(float a, float b)
		{
			return (a >= b) ? b : a;
		}

		public static int Min(int a, int b)
		{
			return (a >= b) ? b : a;
		}

		public static uint Min(uint a, uint b)
		{
			return (a >= b) ? b : a;
		}

		public static float Max(float a, float b)
		{
			return (a <= b) ? b : a;
		}

		public static int Max(int a, int b)
		{
			return (a <= b) ? b : a;
		}

		public static uint Max(uint a, uint b)
		{
			return (a <= b) ? b : a;
		}

		public static ushort Max(ushort a, ushort b)
		{
			return (a <= b) ? b : a;
		}

		public static float Sign(float a)
		{
			return (a >= 0f) ? 1f : -1f;
		}

		public static int Sign(int a)
		{
			return (a >= 0) ? 1 : -1;
		}

		public static float Clamp(float a, float b, float c)
		{
			return (a <= c) ? ((a >= b) ? a : b) : c;
		}

		public static int Clamp(int a, int b, int c)
		{
			return (a <= c) ? ((a >= b) ? a : b) : c;
		}

		public static float Clamp01(float a)
		{
			return (a <= 1f) ? ((a >= 0f) ? a : 0f) : 1f;
		}

		public static int Clamp01(int a)
		{
			return (a <= 1) ? ((a >= 0) ? a : 0) : 1;
		}

		public static float Lerp(float a, float b, float t)
		{
			return a + (b - a) * ((t <= 1f) ? ((t >= 0f) ? t : 0f) : 1f);
		}

		public static int RoundToInt(float v)
		{
			return (int)(v + 0.5f);
		}

		public static int RoundToInt(double v)
		{
			return (int)(v + 0.5);
		}
	}
}