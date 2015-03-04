using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

public static class Utility
{
	public static void Swap<T>(ref T lhs, ref T rhs, Action<T,T> preAction = null, Action<T,T> postAction = null)
	{
		if (preAction != null) preAction(lhs, rhs);
		T temp = lhs;
		lhs = rhs;
		rhs = temp;
		if (postAction != null) postAction(lhs, rhs);
	}

	public static T[] Flatten<T>(T[][] array)
	{
		List<T> list = new List<T>();
		foreach (T[] t in array)
			list.AddRange(t);
		return list.ToArray();
	}

	public static T Last<T>(this Array arr)
	{
		return (T)arr.GetValue(arr.Length - 1);
	}

	[Serializable]
	public class Pair<T1, T2>
	{
		public T1 first;
		public T2 second;
		public Pair()
		{
			ConstructorInfo T1constructor = typeof(T1).GetConstructor(Type.EmptyTypes);
			if (T1constructor != null)
			{
				first = (T1)T1constructor.Invoke(null);
			}

			ConstructorInfo T2constructor = typeof(T2).GetConstructor(Type.EmptyTypes);
			if (T2constructor != null)
			{
				second = (T2)T2constructor.Invoke(null);
			}
		}
		public Pair(T1 t1, T2 t2)
		{
			first = t1; second = t2;
		}
		public void Set(T1 t1, T2 t2)
		{
			first = t1; second = t2;
		}
	}
}