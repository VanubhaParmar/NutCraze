using System;

namespace Tag.NutSort
{
    public static class ArrayExtensions
    {
        public static void RemoveLast<T>(ref T[] array)
        {
            if (array == null || array.Length == 0)
            {
                array = new T[0];
                return;
            }

            if (array.Length == 1)
            {
                array = new T[0];
                return;
            }

            T[] newArray = new T[array.Length - 1];
            Array.Copy(array, 0, newArray, 0, newArray.Length);
            array = newArray;
        }

        public static void RemoveAt<T>(ref T[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (index < 0 || index >= array.Length)
            {
                throw new IndexOutOfRangeException($"Index must be within the bounds of the array. Index: {index}, Length: {array.Length}");
            }

            if (array.Length == 1)
            {
                array = new T[0];
                return;
            }

            int newSize = array.Length - 1;
            T[] newArray = new T[newSize];

            if (index > 0)
            {
                Array.Copy(array, 0, newArray, 0, index);
            }

            if (index < newSize)
            {
                Array.Copy(array, index + 1, newArray, index, newSize - index);
            }

            array = newArray;
        }

        public static void Add<T>(ref T[] array, T item)
        {
            if (array == null)
            {
                array = new T[] { item };
                return;
            }

            int newSize = array.Length + 1;
            T[] newArray = new T[newSize];
            Array.Copy(array, 0, newArray, 0, array.Length);
            newArray[newSize - 1] = item;
            array = newArray;
        }

        public static void Clear<T>(ref T[] array)
        {
            if (array == null)
                array = new T[0];
            else
                Array.Clear(array, 0, array.Length);
        }

        public static T PopAt<T>(ref T[] array, int index)
        {
            if (array == null || index < 0 || index >= array.Length)
            {
                return default(T);
            }

            T item = array[index];
            int newSize = array.Length - 1;
            T[] tempNewArray = new T[newSize];

            if (index > 0)
            {
                Array.Copy(array, 0, tempNewArray, 0, index);
            }

            if (index < newSize)
            {
                Array.Copy(array, index + 1, tempNewArray, index, newSize - index);
            }

            array = tempNewArray;
            return item;
        }

        public static T PopLast<T>(ref T[] array)
        {
            if (array == null || array.Length == 0)
            {
                return default(T);
            }

            T item = array[array.Length - 1];

            if (array.Length == 1)
            {
                array = new T[0];
            }
            else
            {
                int newSize = array.Length - 1;
                T[] newArray = new T[newSize];

                Array.Copy(array, 0, newArray, 0, newSize);

                array = newArray;
            }

            return item;
        }
    }
}