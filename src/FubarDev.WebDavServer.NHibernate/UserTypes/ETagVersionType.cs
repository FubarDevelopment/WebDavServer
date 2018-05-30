using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Xml;

using FubarDev.WebDavServer.Model.Headers;

using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace FubarDev.WebDavServer.NHibernate.UserTypes
{
    public class ETagVersionType : IUserVersionType, IParameterizedType
    {
        private bool _useWeakTypes;

        public SqlType[] SqlTypes { get; } = { SqlTypeFactory.GetString(80) };

        public Type ReturnedType { get; } = typeof(EntityTag);

        public bool IsMutable { get; } = false;

        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;
            var tagX = (EntityTag)x;
            var tagY = (EntityTag)y;
            if (_useWeakTypes)
                return EntityTagComparer.Weak.Equals(tagX, tagY);
            return EntityTagComparer.Strong.Equals(tagX, tagY);
        }

        public int GetHashCode(object x)
        {
            if (x is null)
                return 0;
            var tag = (EntityTag)x;
            if (_useWeakTypes)
                return EntityTagComparer.Weak.GetHashCode(tag);
            return EntityTagComparer.Strong.GetHashCode(tag);
        }

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            var name = names[0];
            var index = rs.GetOrdinal(name);
            if (rs.IsDBNull(index))
                return null;
            var value = rs.GetString(index);
            return EntityTag.Parse(value).Single();
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            var dp = cmd.Parameters[index];

            if (value == null)
            {
                dp.Value = DBNull.Value;
            }
            else
            {
                dp.Value = ((EntityTag)value).ToString();
            }
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            return EntityTag.Parse((string)cached).Single();
        }

        public object Disassemble(object value)
        {
            return ((EntityTag)value).ToString();
        }

        public int Compare(object x, object y)
        {
            if (ReferenceEquals(x, y))
                return 0;
            if (x is null)
                return -1;
            if (y is null)
                return 1;
            var tagX = (EntityTag)x;
            var tagY = (EntityTag)y;
            if (tagX.IsEmpty && tagY.IsEmpty)
                return 0;
            if (tagX.IsEmpty)
                return -1;
            if (tagY.IsEmpty)
                return 1;
            if (_useWeakTypes)
                return EntityTagComparer.Weak.Equals(tagX, tagY) ? 0 : 1;
            return EntityTagComparer.Strong.Equals(tagX, tagY) ? 0 : 1;
        }

        public object Seed(ISessionImplementor session)
        {
            return new EntityTag(_useWeakTypes);
        }

        public object Next(object current, ISessionImplementor session)
        {
            return ((EntityTag)current).Update();
        }

        public void SetParameterValues(IDictionary<string, string> parameters)
        {
            if (parameters == null)
                return;

            if (parameters.TryGetValue("weak", out var weakValue))
            {
                _useWeakTypes = XmlConvert.ToBoolean(weakValue);
            }
        }
    }
}
