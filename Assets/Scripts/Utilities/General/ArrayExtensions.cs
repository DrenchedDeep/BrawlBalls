﻿using UnityEngine;

namespace Utilities.General
{
    //Generated by GPT because I'm lazy. It's a Fisher-Yates shuffle :p
    public static class ArrayExtensions
    {
        /// <summary>
        /// Shuffles the elements of the array in place using UnityEngine.Random.
        /// </summary>
        /// <typeparam name="T">The type of the array elements.</typeparam>
        /// <param name="array">The array to shuffle.</param>
        public static void Shuffle<T>(this T[] array)
        {
            if (array == null)
                throw new System.ArgumentNullException(nameof(array));

            int n = array.Length;
            for (int i = n - 1; i > 0; i--)
            {
                // UnityEngine.Random.Range is min inclusive and max exclusive.
                int j = Random.Range(0, i + 1);

                // Swap elements
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
    }
}