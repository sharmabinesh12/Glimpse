﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Glimpse.Core2.Extensibility;

namespace Glimpse.Core2.Framework
{
    public class ReflectionDiscoverableCollection<T> : IDiscoverableCollection<T>
    {
        internal List<T> Items { get; set; }
        internal List<Type> IgnoredTypes { get; set; }
        internal ILogger Logger { get; set; }

        public ReflectionDiscoverableCollection(ILogger logger)
        {
            Items = new List<T>();
            IgnoredTypes = new List<Type>();
            AutoDiscover = true;
            Logger = logger;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public void Add(T item)
        {
            Items.Add(item);
            Logger.Debug(Resources.DiscoverableCollectionAdd, typeof(T).Name, item.GetType());
        }

        public void Clear()
        {
            Items.Clear();
            Logger.Debug(Resources.DiscoverableCollectionClear, typeof(T).Name);
        }

        public bool Contains(T item)
        {
            return Items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            var result = Items.Remove(item);

            if (result)
                Logger.Debug(Resources.DiscoverableCollectionRemove, typeof(T).Name, item.GetType());

            return result;
        }

        public int Count
        {
            get { return Items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool AutoDiscover { get; set; }

        public void IgnoreType(Type type)
        {
            IgnoredTypes.Add(type);
        }

        public void Discover()
        {
            var results = new List<T>();


            foreach (var file in Directory.GetFiles(DiscoveryLocation, "*.dll", SearchOption.AllDirectories))
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(file);

                    var concreteTypes = assembly.GetTypes().Where(type => typeof (T).IsAssignableFrom(type) &&
                                                                          !type.IsInterface &&
                                                                          !type.IsAbstract &&
                                                                          !IgnoredTypes.Contains(type));
                    foreach (var type in concreteTypes)
                    {
                        try
                        {
                            var instance = (T) Activator.CreateInstance(type);
                            results.Add(instance);
                        }
                        catch (Exception exception)
                        {
                            Logger.Error(Resources.DiscoverCreateInstance, exception, typeof(T), type);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Logger.Error(Resources.DiscoverLoadAssembly, exception, file);
                }
            }

            if (results.Count > 0)
            {
                Items.Clear();

                foreach (var result in results)
                {
                    Logger.Debug(string.Format(Resources.DiscoverableCollectionDiscover, typeof(T).Name, result.GetType()));
                }

                Items.AddRange(results);
            }
        }

        private string discoveryLocation;

        public string DiscoveryLocation
        {
            get { return discoveryLocation ?? (discoveryLocation = AppDomain.CurrentDomain.BaseDirectory); }
            set
            {
                var result = Path.IsPathRooted(value)
                                        ? value
                                        : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, value);

                if (!Directory.Exists(result))
                    throw new DirectoryNotFoundException(string.Format(Resources.SetDiscoveryLocationDirectoryNotFoundMessage, value, result));
                
                discoveryLocation = result;
            }
        }
    }
}