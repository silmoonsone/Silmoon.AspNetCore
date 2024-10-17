using MongoDB.Driver;
using System.Linq.Expressions;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using Silmoon.Data.MongoDB.Models;
using Silmoon.Data.MongoDB;

namespace Silmoon.AspNetCore.Extension.CoreHelpers
{
    [Obsolete("Use Silmoon.Data.MongoDB.MongoExecute instead")]
    public abstract class MongoDBCore
    {
        public abstract MongoExecuter Executer { get; set; }
        #region common data functions
        public static string MakeTableName<T>()
        {
            string singular = typeof(T).Name;
            if (string.IsNullOrEmpty(singular)) return singular;

            // Handle some irregular plurals
            switch (singular.ToLower())
            {
                case "person": return "People";
                case "child": return "Children";
                case "foot": return "Feet";
                case "tooth": return "Teeth";
                case "mouse": return "Mice";
                    // ... add other irregular plurals as needed
            }
            singular = char.ToLowerInvariant(singular[0]) + singular[1..];

            // Regular plurals
            if (Regex.IsMatch(singular, "[sxz]$") || Regex.IsMatch(singular, "[^aeioudgkprt]h$"))
                return singular + "es";
            else if (Regex.IsMatch(singular, "[^aeiou]y$"))
                return singular.Remove(singular.Length - 1) + "ies";
            else
                return singular + "s";
        }


        public virtual void Add<T>(T obj) where T : IIdObject => Executer.AddObject(MakeTableName<T>(), obj);
        public virtual void Adds<T>(T[] objs) where T : IIdObject => Executer.AddObjects(MakeTableName<T>(), objs);

        public virtual T[] Gets<T>(Expression<Func<T, bool>> whereFunc, int? offset = null, int? count = null) where T : IIdObject
        {
            var result = Executer.GetQueryable<T>(MakeTableName<T>());
            if (whereFunc != null) result = result.Where(whereFunc);
            if (offset.HasValue) result = result.Skip(offset.Value);
            if (count.HasValue) result = result.Take(count.Value);
            return result.ToArray();
        }
        public virtual T[] Gets<T, TOrderKey>(Expression<Func<T, bool>> whereFunc, Expression<Func<T, TOrderKey>> orderFunc, bool? ascending, int? offset = null, int? count = null) where T : IIdObject
        {
            var result = Executer.GetQueryable<T>(MakeTableName<T>());
            if (whereFunc != null) result = result.Where(whereFunc);
            if (orderFunc != null)
            {
                if (ascending ?? true) result = result.OrderBy(orderFunc);
                else result = result.OrderByDescending(orderFunc);
            }
            if (offset.HasValue) result = result.Skip(offset.Value);
            if (count.HasValue) result = result.Take(count.Value);
            return result.ToArray();
        }
        public virtual IQueryable<T> GetsQuery<T>(Expression<Func<T, bool>> whereFunc, int? offset = null, int? count = null) where T : IIdObject
        {
            var result = Executer.GetQueryable<T>(MakeTableName<T>());
            if (whereFunc != null) result = result.Where(whereFunc);
            if (offset.HasValue) result = result.Skip(offset.Value);
            if (count.HasValue) result = result.Take(count.Value);
            return result;
        }
        public virtual IQueryable<T> GetsQuery<T, TOrderKey>(Expression<Func<T, bool>> whereFunc, Expression<Func<T, TOrderKey>> orderFunc, bool? ascending, int? offset = null, int? count = null) where T : IIdObject
        {
            var result = Executer.GetQueryable<T>(MakeTableName<T>());
            if (whereFunc != null) result = result.Where(whereFunc);
            if (orderFunc != null)
            {
                if (ascending ?? true) result = result.OrderBy(orderFunc);
                else result = result.OrderByDescending(orderFunc);
            }
            if (offset.HasValue) result = result.Skip(offset.Value);
            if (count.HasValue) result = result.Take(count.Value);
            return result;
        }

        public virtual T Get<T>(Expression<Func<T, bool>> whereFunc) where T : IIdObject
        {
            //return Executer.GetObject(MakeTableName<T>(),whereFunc,);
            var result = Executer.GetQueryable<T>(MakeTableName<T>());
            if (whereFunc != null) result = result.Where(whereFunc);
            return result.FirstOrDefault();
        }
        public virtual T Get<T, TOrderKey>(Expression<Func<T, bool>> whereFunc, Expression<Func<T, TOrderKey>> orderFunc, bool? ascending) where T : IIdObject
        {
            var result = Executer.GetQueryable<T>(MakeTableName<T>());
            if (whereFunc != null) result = result.Where(whereFunc);
            if (orderFunc != null)
            {
                if (ascending ?? true) result = result.OrderBy(orderFunc);
                else result = result.OrderByDescending(orderFunc);
            }
            return result.FirstOrDefault();
        }

        public virtual UpdateResult Set<T>(T obj, Expression<Func<T, bool>>? whereFunc = null, params Expression<Func<T, object>>[] updateFields) where T : IIdObject
        {
            if (whereFunc is not null)
                return Executer.SetObject(MakeTableName<T>(), obj, whereFunc, false, updateFields);
            else
                return Executer.SetObject(MakeTableName<T>(), obj, Builders<T>.Filter.Empty, false, updateFields);
        }
        public virtual UpdateResult Sets<T>(T obj, Expression<Func<T, bool>>? whereFunc = null, params Expression<Func<T, object>>[] updateFields) where T : IIdObject
        {
            if (whereFunc is not null)
                return Executer.SetObjects(MakeTableName<T>(), obj, whereFunc, false, updateFields);
            else
                return Executer.SetObjects(MakeTableName<T>(), obj, Builders<T>.Filter.Empty, false, updateFields);
        }

        public virtual DeleteResult Delete<T>(Expression<Func<T, bool>> whereFunc) where T : IIdObject
        {
            if (whereFunc is not null)
                return Executer.DeleteObject(MakeTableName<T>(), whereFunc);
            else
                return Executer.DeleteObject(MakeTableName<T>(), Builders<T>.Filter.Empty);
        }

        public virtual DeleteResult Deletes<T>(Expression<Func<T, bool>> whereFunc) where T : IIdObject
        {
            if (whereFunc is not null)
                return Executer.DeleteObjects(MakeTableName<T>(), whereFunc);
            else
                return Executer.DeleteObjects(MakeTableName<T>(), Builders<T>.Filter.Empty);
        }
        #endregion
    }
}
