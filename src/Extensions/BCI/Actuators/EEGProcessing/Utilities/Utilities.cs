﻿////////////////////////////////////////////////////////////////////////////
//
// Copyright 2013-2019; 2023 Intel Corporation
// SPDX-License-Identifier: Apache-2.0
//
//
// Utilities.cs
//
// Utilities for EEGProcessing
//
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace ACAT.Extensions.BCI.Actuators.EEG.EEGProcessing
{
    public static class Utilities
    {
        // Find indices in array that match  certain value
        public static int[] FindAllIndex<T>(this T[] array, Predicate<T> match)
        {
            return array.Select((value, index) => match(value) ? index : -1)
                    .Where(index => index != -1).ToArray();
        }

        /// <summary>
        /// Shuffle array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rng"></param>
        /// <param name="array"></param>
        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        /// <summary>
        /// Find indices that contain a value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static int[] FindAllIndexof<T>(this IEnumerable<T> values, T val)
        {
            return values.Select((b, i) => object.Equals(b, val) ? i : -1).Where(i => i != -1).ToArray();
        }

        public static bool IsTarget(int classIdx)
        {
            return classIdx == 1;
        }

        public static bool IsNontarget(int classIdx)
        {
            return classIdx == 0;
        }

        public static bool IsClass(int classIdx, int expectedClasIdx)
        {
            return classIdx == expectedClasIdx;
        }

        /// <summary>
        /// Writes the given object instance to a binary file.
        /// <para>Object type (and all child types) must be decorated with the [Serializable] attribute.</para>
        /// <para>To prevent a variable from being serialized, decorate it with the [NonSerialized] attribute; cannot be applied to properties.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the XML file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the XML file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
                {
                    Binder = new PreMergeToMergedDeserializationBinder()
                };
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }

        /// <summary>
        /// Reads an object instance from a binary file.
        /// </summary>
        /// <typeparam name="T">The type of object to read from the XML.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the binary file.</returns>
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
                {
                    Binder = new PreMergeToMergedDeserializationBinder()
                };

                return (T)binaryFormatter.Deserialize(stream);
            }
        }
    }
}

/// <summary>
/// Create class for serializing/desirializing avoiding problems with different assemblies
/// </summary>
internal sealed class PreMergeToMergedDeserializationBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        string currentAssemblyInfo = Assembly.GetExecutingAssembly().FullName;

        //my modification
        string currentAssemblyName = currentAssemblyInfo.Split(',')[0];
        if (assemblyName.StartsWith(currentAssemblyName)) assemblyName = currentAssemblyInfo;

        Type typeToDeserialize = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
        return typeToDeserialize;
    }
}