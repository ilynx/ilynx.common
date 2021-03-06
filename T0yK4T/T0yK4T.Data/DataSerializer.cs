﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace T0yK4T.Data
{
    /// <summary>
    /// A simple implementation of <see cref="IDataSerializer{T}"/>
    /// <para/>
    /// Please note that this serializer will only serialize Types that have properties marked with <see cref="DataPropertyAttribute"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataSerializer<T> : IDataSerializer<T> where T : new()
    {
        private Dictionary<string, PropertyInfo> serializedFields = new Dictionary<string, PropertyInfo>();
        private Dictionary<string, IDataProperty<T>> template = new Dictionary<string, IDataProperty<T>>();
        private Type handledType = typeof(T);

        /// <summary>
        /// Initializes a new instance of <see cref="DataSerializer{T}"/> and builds the internal format table
        /// </summary>
        public DataSerializer()
        {
            this.BuildFormat();
        }

        /// <summary>
        /// ...
        /// </summary>
        private void BuildFormat()
        {
            PropertyInfo[] properties = handledType.GetProperties();
            foreach (PropertyInfo pInfo in properties)
            {
                if (Attribute.IsDefined(pInfo, typeof(DataPropertyAttribute)) && pInfo.CanRead && pInfo.CanWrite)
                {
                    this.serializedFields.Add(pInfo.Name, pInfo);
                    this.template.Add(pInfo.Name, new DataProperty<T>(pInfo.Name, null, pInfo.PropertyType));
                }
            }
        }

        /// <summary>
        /// Serializes the specified value using the internal format table
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public IEnumerable<IDataProperty<T>> Serialize(T value)
        {
            List<IDataProperty<T>> retVal = new List<IDataProperty<T>>();
            foreach (KeyValuePair<string, PropertyInfo> kvp in this.serializedFields)
            {
                string name = kvp.Key;
                PropertyInfo pInfo = kvp.Value;
                DataProperty<T> finalProperty = new DataProperty<T>(this.template[name]);
                finalProperty.Value = pInfo.GetValue(value, null);
                retVal.Add(finalProperty);
                //retVal.Add(new DataProperty<T> { Value = kvp.Value.GetValue(value, null), PropertyName = kvp.Key, DataType =  });
            }
            return retVal;
        }

        /// <summary>
        /// Deserializes the speicifed collection of properties in to a new instance of {T} with it's properties set to the specified values
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public T Deserialize(IEnumerable<IDataProperty<T>> fields)
        {
            T instance = new T();
            foreach (IDataProperty<T> df in fields)
                this.serializedFields[df.PropertyName].SetValue(instance, df.Value, null);

            return instance;
        }

        /// <summary>
        /// Attempts to build a collection of set values from the specified generic instance - <paramref name="val"/>
        /// </summary>
        /// <param name="val">The value to get a filter for</param>
        /// <returns></returns>
        public IDataProperty<T>[] GetUsableFilter(T val)
        {
            return this.GetUsableFilter(val, null);
            //List<DataProperty<T>> filter = new List<DataProperty<T>>();
            //foreach (KeyValuePair<string, PropertyInfo> kvp in this.serializedFields)
            //{
            //    object propertyValue = kvp.Value.GetValue(val, null);
            //    if (propertyValue != GetDefaultValue(kvp.Value.PropertyType))
            //        filter.Add(new DataProperty<T>(kvp.Key, propertyValue, kvp.Value.PropertyType));
            //}
            //return filter.ToArray();
        }

        /// <summary>
        /// When implemented in a derrived class, returns a collection of <see cref="DataProperty{T}"/> objects that can be used to search a <see cref="IDataAdapter{T}"/>
        /// <para/>
        /// - Any properties who's name is contained in <paramref name="excludeProperties"/> will not be returned in the filter array
        /// </summary>
        /// <param name="val"></param>
        /// <param name="excludeProperties"></param>
        /// <returns></returns>
        public IDataProperty<T>[] GetUsableFilter(T val, params string[] excludeProperties)
        {
            List<IDataProperty<T>> filter = new List<IDataProperty<T>>();
            if (excludeProperties == null)
                excludeProperties = new string[0];

            foreach (KeyValuePair<string, PropertyInfo> kvp in this.serializedFields)
            {
                if (excludeProperties.Contains(kvp.Key))
                    continue;
                object propertyValue = kvp.Value.GetValue(val, null);
                if (propertyValue != GetDefaultValue(kvp.Value.PropertyType))
                    filter.Add(new DataProperty<T>(kvp.Key, propertyValue, kvp.Value.PropertyType));
            }
            return filter.ToArray();
        }

        /// <summary>
        /// Gets the template used when serializing and deserializing data using this <see cref="DataSerializer{T}"/>
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, IDataProperty<T>> GetDataTemplate()
        {
            return new Dictionary<string, IDataProperty<T>>(this.template);
        }

        /// <summary>
        /// Gets the default value for the specified type <paramref name="t"/>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);
            return null;
        }
    }

    /// <summary>
    /// Attribute that should be used on properties that should be "serialized" by the <see cref="DataSerializer{T}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited=false)]
    public class DataPropertyAttribute : Attribute
    {
        /// <summary>
        /// Empty constructor
        /// </summary>
        public DataPropertyAttribute()
        {
            
        }
    }

    /// <summary>
    /// A structure used to represent a single property in <see cref="DataSerializer{T}"/>
    /// </summary>
    /// <typeparam name="TOwner">Used to constrain types</typeparam>
    public struct DataProperty<TOwner> : IDataProperty<TOwner>
    {
        private object value;
        private string name;
        private Type dataType;

        /// <summary>
        /// Gets or Sets the value of this property
        /// </summary>
        public object Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        /// <summary>
        /// Gets or Sets the name of this property
        /// </summary>
        public string PropertyName
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>
        /// Gets or Sets the type of data that this <see cref="DataProperty{T}"/> will eventually contain
        /// </summary>
        public Type DataType
        {
            get { return this.dataType; }
            set { this.dataType = value; }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DataProperty{T}"/> and sets the <see cref="DataProperty{T}.Value"/> and <see cref="DataProperty{T}.PropertyName"/> to the specified values
        /// <para/>
        /// The <see cref="DataProperty{T}.DataType"/> property is set to the <paramref name="dataType"/> argument
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property</param>
        /// <param name="dataType">The type of data</param>
        public DataProperty(string name, object value, Type dataType)
        {
            this.value = value;
            this.name = name;
            this.dataType = dataType;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DataProperty{T}"/> and sets the <see cref="DataProperty{T}.Value"/> and <see cref="DataProperty{T}.PropertyName"/> to the specified values
        /// <para/>
        /// The <see cref="DataProperty{T}.DataType"/> property is set to the result of value.GetType
        /// <para/>
        /// Please note that value cannot be null for this to work
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property</param>
        public DataProperty(string name, object value)
        {
            this.value = value;
            this.name = name;
            if (value == null)
                throw new ArgumentNullException("value");
            this.dataType = value.GetType();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DataProperty{T}"/> and sets the name and datatype to those contained in the specified template
        /// <para/>
        /// Value is set to null
        /// </summary>
        /// <param name="template"></param>
        public DataProperty(IDataProperty<TOwner> template)
        {
            this.value = null;
            this.name = template.PropertyName;
            this.dataType = template.DataType;
        }
    }
}
