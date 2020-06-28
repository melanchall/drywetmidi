namespace Melanchall.DryWetMidi.Common
{
    internal static class DataTypesUtilities
    {
        #region Methods

        public static byte Combine(FourBitNumber head, FourBitNumber tail)
        {
            return (byte)((head << 4) | tail);
        }

        public static ushort Combine(SevenBitNumber head, SevenBitNumber tail)
        {
            return (ushort)((head << 7) | tail);
        }

        public static uint Combine(SevenBitNumber head, SevenBitNumber middle, SevenBitNumber tail)
        {
            return (uint)((head << 14) | (middle << 7) | tail);
        }

        public static ushort Combine(byte head, byte tail)
        {
            return (ushort)((head << 8) | tail);
        }

        public static uint Combine(ushort head, ushort tail)
        {
            return (uint)((head << 16) | tail);
        }

        public static FourBitNumber GetTail(this byte number)
        {
            return (FourBitNumber)(number & FourBitNumber.MaxValue);
        }

        public static SevenBitNumber GetTail(this ushort number)
        {
            return (SevenBitNumber)(number & SevenBitNumber.MaxValue);
        }

        public static byte GetTail(this short number)
        {
            return (byte)(number & byte.MaxValue);
        }

        public static ushort GetTail(this uint number)
        {
            return (ushort)(number & ushort.MaxValue);
        }

        public static FourBitNumber GetHead(this byte number)
        {
            return (FourBitNumber)(number >> 4);
        }

        public static SevenBitNumber GetHead(this ushort number)
        {
            return (SevenBitNumber)(number >> 7);
        }

        public static byte GetHead(this short number)
        {
            return (byte)(number >> 8);
        }

        public static ushort GetHead(this uint number)
        {
            return (ushort)(number >> 16);
        }

        public static int GetVlqLength(this int number)
        {
            var result = 1;

            if (number > 127)
            {
                result++;
                if (number > 16383)
                {
                    result++;
                    if (number > 2097151)
                    {
                        result++;
                        if (number > 268435455)
                            result++;
                    }
                }
            }

            return result;
        }

        public static int GetVlqLength(this long number)
        {
            var result = 1;

            if (number > 127)
            {
                result++;
                if (number > 16383)
                {
                    result++;
                    if (number > 2097151)
                    {
                        result++;
                        if (number > 268435455)
                        {
                            result++;
                            if (number > 34359738367)
                            {
                                result++;
                                if (number > 4398046511103)
                                {
                                    result++;
                                    if (number > 562949953421311)
                                    {
                                        result++;
                                        if (number > 72057594037927935)
                                            result++;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static byte[] GetVlqBytes(this int number)
        {
            return GetVlqBytes((long)number);
        }

        public static byte[] GetVlqBytes(this long number)
        {
            var result = new byte[number.GetVlqLength()];
            var i = result.Length - 1;

            result[i--] = (byte)(number & 127);

            while ((number >>= 7) > 0)
            {
                result[i] |= 128;
                result[i--] += (byte)(number & 127);
            }

            return result;
        }

        public static byte GetFirstByte(this int number)
        {
            return (byte)((number >> 24) & 0xFF);
        }

        public static byte GetSecondByte(this int number)
        {
            return (byte)((number >> 16) & 0xFF);
        }

        public static byte GetThirdByte(this int number)
        {
            return (byte)((number >> 8) & 0xFF);
        }

        public static byte GetFourthByte(this int number)
        {
            return (byte)(number & 0xFF);
        }

        #endregion
    }
}
