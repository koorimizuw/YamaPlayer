using VRC.SDK3.Data;

namespace Koyashiro.GenericDataContainer.Internal
{
    public static class DataTokenUtil
    {
        public static DataToken NewDataToken<T>(T obj)
        {
            var objType = obj.GetType();

            // TokenType.Boolean
            if (objType == typeof(bool))
            {
                return new DataToken((bool)(object)obj);
            }
            // TokenType.SByte
            else if (objType == typeof(sbyte))
            {
                return new DataToken((sbyte)(object)obj);
            }
            // TokenType.Byte
            else if (objType == typeof(byte))
            {
                return new DataToken((byte)(object)obj);
            }
            // TokenType.Short
            else if (objType == typeof(short))
            {
                return new DataToken((short)(object)obj);
            }
            // TokenType.UShort
            else if (objType == typeof(ushort))
            {
                return new DataToken((ushort)(object)obj);
            }
            // TokenType.Int
            else if (objType == typeof(int))
            {
                return new DataToken((int)(object)obj);
            }
            // TokenType.UInt
            else if (objType == typeof(uint))
            {
                return new DataToken((uint)(object)obj);
            }
            // TokenType.Long
            else if (objType == typeof(long))
            {
                return new DataToken((long)(object)obj);
            }
            // TokenType.ULong
            else if (objType == typeof(ulong))
            {
                return new DataToken((ulong)(object)obj);
            }
            // TokenType.Float
            else if (objType == typeof(float))
            {
                return new DataToken((float)(object)obj);
            }
            // TokenType.Double
            else if (objType == typeof(double))
            {
                return new DataToken((double)(object)obj);
            }
            // TokenType.String
            else if (objType == typeof(string))
            {
                return new DataToken((string)(object)obj);
            }
            // TokenType.DataList
            else if (objType == typeof(DataList))
            {
                return new DataToken((DataList)(object)obj);
            }
            // TokenType.DataDictionary
            else if (objType == typeof(DataDictionary))
            {
                return new DataToken((DataDictionary)(object)obj);
            }
            // TokenType.Error
            else if (objType == typeof(DataError))
            {
                return new DataToken((DataError)(object)obj);
            }
            // TokenType.Reference
            else
            {
                return new DataToken(obj);
            }
        }

        public static DataToken[] NewDataTokens<T>(T[] array)
        {
            var length = array.Length;
            var tokens = new DataToken[length];
            var arrayType = array.GetType();

            // TokenType.Boolean
            if (arrayType == typeof(bool[]))
            {
                var boolArray = (bool[])(object)array;
                for (var i = 0; i < length; i++)
                {
                    tokens[i] = new DataToken(boolArray[i]);
                }
            }
            // TokenType.SByte
            else if (arrayType == typeof(sbyte[]))
            {
                var sbyteArray = (sbyte[])(object)array;
                for (var i = 0; i < length; i++)
                {
                    tokens[i] = new DataToken(sbyteArray[i]);
                }
            }
            // TokenType.Byte
            else if (arrayType == typeof(byte[]))
            {
                var byteArray = (byte[])(object)array;
                for (var i = 0; i < length; i++)
                {
                    tokens[i] = new DataToken(byteArray[i]);
                }
            }
            // TokenType.Short
            else if (arrayType == typeof(short[]))
            {
                var shortArray = (short[])(object)array;
                for (var i = 0; i < length; i++)
                {
                    tokens[i] = new DataToken(shortArray[i]);
                }
            }
            // TokenType.UShort
            else if (arrayType == typeof(ushort[]))
            {
                var ushortArray = (ushort[])(object)array;
                for (var i = 0; i < length; i++)
                {
                    tokens[i] = new DataToken(ushortArray[i]);
                }
            }
            // TokenType.Int
            else if (arrayType == typeof(int[]))
            {
                var intArray = (int[])(object)array;
                for (var i = 0; i < length; i++)
                {
                    tokens[i] = new DataToken(intArray[i]);
                }
            }
            // TokenType.UInt
            else if (arrayType == typeof(uint[]))
            {
                var uintArray = (uint[])(object)array;
                for (var i = 0; i < length; i++)
                {
                    tokens[i] = new DataToken(uintArray[i]);
                }
            }
            // TokenType.Long
            else if (arrayType == typeof(long[]))
            {
                var longArray = (long[])(object)array;
                for (var i = 0; i < length; i++)
                {
                    tokens[i] = new DataToken(longArray[i]);
                }
            }
            // TokenType.ULong
            else if (arrayType == typeof(ulong[]))
            {
                var ulongArray = (ulong[])(object)array;
                for (var i = 0; i < length; i++)
                {
                    tokens[i] = new DataToken(ulongArray[i]);
                }
            }
            // TokenType.Float
            else if (arrayType == typeof(float[]))
            {
                var floatArray = (float[])(object)array;
                for (var i = 0; i < length; i++)
                {
                    tokens[i] = new DataToken(floatArray[i]);
                }
            }
            // TokenType.Double
            else if (arrayType == typeof(double[]))
            {
                var doubleArray = (double[])(object)array;
                for (var i = 0; i < length; i++)
                {
                    tokens[i] = new DataToken(doubleArray[i]);
                }
            }
            // TokenType.String
            else if (arrayType == typeof(string[]))
            {
                var stringArray = (string[])(object)array;
                for (var i = 0; i < length; i++)
                {
                    tokens[i] = new DataToken(stringArray[i]);
                }
            }
            // TokenType.DataList
            else if (arrayType == typeof(DataList[]))
            {
                var dataListArray = (DataList[])(object)array;
                for (var i = 0; i < length; i++)
                {
                    tokens[i] = new DataToken(dataListArray[i]);
                }
            }
            // TokenType.DataDictionary
            else if (arrayType == typeof(DataDictionary[]))
            {
                var dataDictionaryArray = (DataDictionary[])(object)array;
                for (var i = 0; i < length; i++)
                {
                    tokens[i] = new DataToken(dataDictionaryArray[i]);
                }
            }
            // TokenType.Error
            else if (arrayType == typeof(DataError[]))
            {
                var errorArray = (DataError[])(object)array;
                for (var i = 0; i < length; i++)
                {
                    tokens[i] = new DataToken(errorArray[i]);
                }
            }
            // TokenType.Reference
            else
            {
                for (var i = 0; i < length; i++)
                {
                    tokens[i] = new DataToken(array[i]);
                }
            }

            return tokens;
        }
    }
}
