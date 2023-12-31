﻿using OpenTK.Mathematics;

namespace DistributedFiltering.Filters.Extensions;

public static class VectorExtensions
{
	public static Vector4d Abs(this Vector4d v)
	{
		return new(
			Math.Abs(v.X),
			Math.Abs(v.Y),
			Math.Abs(v.Z),
			Math.Abs(v.W)
		);
	}

	public static Vector4i Abs(this Vector4i v)
	{
		return new(
			Math.Abs(v.X),
			Math.Abs(v.Y),
			Math.Abs(v.Z),
			Math.Abs(v.W)
		);
	}

	public static Vector4d Div(this Vector4d u, Vector4d v)
	{
		return new(
			u.X / v.X,
			u.Y / v.Y,
			u.Z / v.Z,
			u.W / v.W
		);
	}

	public static Vector2i Abs(this Vector2i u)
	{
		return new(
			Math.Abs(u.X),
			Math.Abs(u.Y)
		);
	}
}
