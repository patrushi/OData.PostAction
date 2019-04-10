using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Query;

namespace OData.PostAction
{
    public static class PostAction
    {
        public static IEnumerable<T> PostActionOnQuery<T>(
            IQueryable<T> query, 
            ODataQueryOptions<T> options, 
            Action<ICollection<T>> postAction)
            where T : class, new()
        {
            var queryable = options.ApplyTo(query);
            var itemType = queryable.GetType().GetGenericArguments().First();
            var list = ToTypedList(queryable, itemType);
            var entityList = GetEntityList<T>(list).ToList();
            postAction(entityList);
            return entityList;
        }

        private static IList ToTypedList(IQueryable self, Type innerType)
        {
            var methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList));
            var genericMethod = methodInfo.MakeGenericMethod(innerType);
            return genericMethod.Invoke(null, new object[]
            {
                self
            }) as IList;
        }

        private static IEnumerable<T> GetEntityList<T>(IEnumerable enumerable)
            where T : class, new()
        {
            var type = typeof(T);
            foreach (var item in enumerable)
            {
                yield return (T)GetValue(type, item);
            }
        }

        private static object GetValue(Type type, object item)
        {
            if (item == null)
            {
                return null;
            }
            else if (type.IsAssignableFrom(item.GetType()))
            {
                return item;
            }
            else if (item is ISelectExpandWrapper wrapper) // $select/$expand
            {
                return ToObject(type, wrapper.ToDictionary());
            }
            else 
            {
                return null;
            }
        }

        private static object ToObject(Type type, IDictionary<string, object> source)
        {
            var newObject = Activator.CreateInstance(type);

            foreach (var item in source)
            {
                var property = type.GetProperty(item.Key);
                var propertyType = property.PropertyType;
                property.SetValue(newObject, GetValue(propertyType, item.Value), null);
            }

            return newObject;
        }
    }
}
