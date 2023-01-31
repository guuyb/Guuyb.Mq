using System;
using System.Collections.Generic;
using EasyNetQ;
using Error = EasyNetQ.SystemMessages.Error;

namespace Guuyb.Mq.Conventions
{
    /// <summary>
    /// Сериализатор, который делает возможной работу шины данных
    /// только с предопределенным набором типов
    /// </summary>
    public class MapTypeNameSerializer : ITypeNameSerializer
    {
        private readonly Dictionary<string, Type> _typeNameToTypeMap;
        private readonly Dictionary<Type, string> _typeToTypeNameMap;

        public MapTypeNameSerializer()
        {
            _typeNameToTypeMap = new Dictionary<string, Type>();
            _typeToTypeNameMap = new Dictionary<Type, string>();

            Use<Error>();
        }

        public Type DeSerialize(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentNullException(typeName);

            if (!_typeNameToTypeMap.ContainsKey(typeName))
                throw new ArgumentOutOfRangeException($"Can't deserialize type: {typeName}. Please add it using serializer => serializer.Use(\"{typeName}\", {typeName})");

            return _typeNameToTypeMap[typeName];
        }

        public string Serialize(Type type)
        {
            if (!_typeToTypeNameMap.ContainsKey(type))
                throw new ArgumentOutOfRangeException($"Can't serialize type: {type}. Please add it using serializer => serializer.Use<{type}>()");

            return _typeToTypeNameMap[type];
        }

        /// <summary>
        /// Добавляет наименование типа и тип,
        /// наименование которого отличается
        /// </summary>
        /// <param name="specificTypeName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public MapTypeNameSerializer Use(string specificTypeName, Type type)
        {
            var typeName = CreateTypeName(type, specificTypeName);
            _typeNameToTypeMap.Add(typeName, type);
            _typeToTypeNameMap.Add(type, typeName);
            return this;
        }

        /// <summary>
        /// Добавляет наименование типа и тип,
        /// наименование которого отличается,
        /// если ранее не было добавления
        /// </summary>
        public MapTypeNameSerializer TryUse<TType>(string specificTypeName = null)
        {
            var type = typeof(TType);
            var typeName = CreateTypeName(type, specificTypeName);
            if (!_typeNameToTypeMap.ContainsKey(typeName))
            {
                _typeNameToTypeMap.Add(typeName, type);
            }
            if (!_typeToTypeNameMap.ContainsKey(type))
            {
                _typeToTypeNameMap.Add(type, specificTypeName);
            }

            return this;
        }

        public MapTypeNameSerializer Use<TType>(string specificTypeName)
            => Use(specificTypeName, typeof(TType));

        public MapTypeNameSerializer Use<TType>()
        {
            var type = typeof(TType);
            Use(null, type);

            var fullSpecificTypeName = $"{type.FullName}, {type.Assembly.GetName().Name}";
            _typeNameToTypeMap.Add(fullSpecificTypeName, type);

            return this;
        }

        private string CreateTypeName(Type type, string specificTypeName = null)
        {
            if (specificTypeName != null)
                return specificTypeName;

            return type.Name;
        }
    }
}
