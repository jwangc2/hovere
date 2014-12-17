using UnityEngine;
using System.Collections;

public static class TKVecMath {

	public static Vector3 DotProj(Vector3 a, Vector3 b)
	{
		float dotNum = Vector3.Dot(a, b);
		float dotDen = Vector3.Dot(a, a);

		return (dotNum / dotDen) * a;
	}
}
