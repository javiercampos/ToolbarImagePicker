/*
	Taken from https://exposedobject.codeplex.com/ by Igor Ostrovsky
*/
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Jcl.Tools.WindowsForms.ToolbarImagePicker
{
	internal class ExposedObject : DynamicObject
	{
		private readonly object _object;
		private readonly Type _type;
		private readonly Dictionary<string, Dictionary<int, List<MethodInfo>>> _instanceMethods;
		private readonly Dictionary<string, Dictionary<int, List<MethodInfo>>> _genInstanceMethods;

		private ExposedObject(object obj)
		{
			_object = obj;
			_type = obj.GetType();

			_instanceMethods =
				_type
					.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
					.Where(m => !m.IsGenericMethod)
					.GroupBy(m => m.Name)
					.ToDictionary(
						p => p.Key,
						p => p.GroupBy(r => r.GetParameters().Length).ToDictionary(r => r.Key, r => r.ToList()));

			_genInstanceMethods =
				_type
					.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
					.Where(m => m.IsGenericMethod)
					.GroupBy(m => m.Name)
					.ToDictionary(
						p => p.Key,
						p => p.GroupBy(r => r.GetParameters().Length).ToDictionary(r => r.Key, r => r.ToList()));
		}

		public object Object => _object;

		public static dynamic New<T>()
		{
			return New(typeof(T));
		}

		public static dynamic New(Type type)
		{
			return new ExposedObject(Create(type));
		}

		private static object Create(Type type)
		{
			ConstructorInfo constructorInfo = GetConstructorInfo(type);
			return constructorInfo.Invoke(new object[0]);
		}

		private static ConstructorInfo GetConstructorInfo(Type type, params Type[] args)
		{
			ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, args, null);
			if (constructorInfo != null)
			{
				return constructorInfo;
			}

			throw new MissingMemberException(type.FullName, string.Format(".ctor({0})", string.Join(", ", Array.ConvertAll(args, t => t.FullName))));
		}

		public static dynamic From(object obj)
		{
			return new ExposedObject(obj);
		}

		public static T Cast<T>(ExposedObject t)
		{
			return (T)t._object;
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			// Get type args of the call
			Type[] typeArgs = ExposedObjectHelper.GetTypeArgs(binder);
			if (typeArgs != null && typeArgs.Length == 0) typeArgs = null;

			//
			// Try to call a non-generic instance method
			//
			if (typeArgs == null
					&& _instanceMethods.ContainsKey(binder.Name)
					&& _instanceMethods[binder.Name].ContainsKey(args.Length)
					&& ExposedObjectHelper.InvokeBestMethod(args, _object, _instanceMethods[binder.Name][args.Length], out result))
			{
				return true;
			}

			//
			// Try to call a generic instance method
			//
			if (_instanceMethods.ContainsKey(binder.Name)
					&& _instanceMethods[binder.Name].ContainsKey(args.Length))
			{
				List<MethodInfo> methods = new List<MethodInfo>();

				foreach (var method in _genInstanceMethods[binder.Name][args.Length])
				{
					if (method.GetGenericArguments().Length == typeArgs.Length)
					{
						methods.Add(method.MakeGenericMethod(typeArgs));
					}
				}

				if (ExposedObjectHelper.InvokeBestMethod(args, _object, methods, out result))
				{
					return true;
				}
			}

			result = null;
			return false;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			var propertyInfo = _type.GetProperty(
				binder.Name,
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

			if (propertyInfo != null)
			{
				propertyInfo.SetValue(_object, value, null);
				return true;
			}

			var fieldInfo = _type.GetField(
				binder.Name,
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

			if (fieldInfo != null)
			{
				fieldInfo.SetValue(_object, value);
				return true;
			}

			return false;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			var propertyInfo = _object.GetType().GetProperty(
				binder.Name,
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

			if (propertyInfo != null)
			{
				result = propertyInfo.GetValue(_object, null);
				return true;
			}

			var fieldInfo = _object.GetType().GetField(
				binder.Name,
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

			if (fieldInfo != null)
			{
				result = fieldInfo.GetValue(_object);
				return true;
			}

			result = null;
			return false;
		}

		public override bool TryConvert(ConvertBinder binder, out object result)
		{
			result = _object;
			return true;
		}
	}
}


