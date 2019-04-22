using System;

namespace Melanchall.DryWetMidi.Common
{
    /// <summary>
    /// Internal utilities to manipulate MIDI data types.
    /// </summary>
    public static class DataTypesUtilities
    {
        #region Methods

        /// <summary>
        /// Merges two four-bit numbers into one byte.
        /// </summary>
        /// <param name="head"><see cref="FourBitNumber"/> representing left part of resulting number.</param>
        /// <param name="tail"><see cref="FourBitNumber"/> representing right part of resulting number.</param>
        /// <returns>Single byte made of four-bit halfs.</returns>
        public static byte Combine(FourBitNumber head, FourBitNumber tail)
        {
            return (byte)((head << 4) | tail);
        }

        /// <summary>
        /// Merges two seven-bit numbers into one 16-bit unsigned integer number.
        /// </summary>
        /// <param name="head"><see cref="SevenBitNumber"/> representing left part of resulting number.</param>
        /// <param name="tail"><see cref="SevenBitNumber"/> representing right part of resulting number.</param>
        /// <returns>Single unsigned 16-bit integer number made of seven-bit halfs.</returns>
        public static ushort Combine(SevenBitNumber head, SevenBitNumber tail)
        {
            return (ushort)((head << 7) | tail);
        }

        /// <summary>
        /// Merges three seven-bit numbers into one 32-bit unsigned integer number.
        /// </summary>
        /// <param name="head"><see cref="SevenBitNumber"/> representing left part of resulting number.</param>
        /// <param name="middle"><see cref="SevenBitNumber"/> representing middle part of resulting number.</param>
        /// <param name="tail"><see cref="SevenBitNumber"/> representing right part of resulting number.</param>
        /// <returns>Single unsigned 32-bit integer number made of seven-bit halfs.</returns>
        public static uint Combine(SevenBitNumber head, SevenBitNumber middle, SevenBitNumber tail)
        {
            return (uint)((head << 14) | (middle << 7) | tail);
        }

        /// <summary>
        /// Merges two signed bytes into one 16-bit signed integer number.
        /// </summary>
        /// <param name="head">Byte representing left part of resulting number.</param>
        /// <param name="tail">Byte representing right part of resulting number.</param>
        /// <returns>Single signed 16-bit integer number made of byte halfs.</returns>
        public static ushort Combine(byte head, byte tail)
        {
            return (ushort)((head << 8) | tail);
        }

        /// <summary>
        /// Merges two unsigned 16-bit numbers into one 32-bit unsigned integer number.
        /// </summary>
        /// <param name="head">16-bit unsigned number representing left part of resulting number.</param>
        /// <param name="tail">16-bit unsigned number representing right part of resulting number.</param>
        /// <returns>Single 32-bit unsigned integer number made of 16-bit unsigned integer halfs.</returns>
        public static uint Combine(ushort head, ushort tail)
        {
            return (uint)((head << 16) | tail);
        }

        /// <summary>
        /// Extracts right four-bit part of a byte.
        /// </summary>
        /// <param name="number">Byte to extract right part of.</param>
        /// <returns><see cref="FourBitNumber"/> representing the right part of <paramref name="number"/>.</returns>
        public static FourBitNumber GetTail(this byte number)
        {
            return (FourBitNumber)(number & FourBitNumber.MaxValue);
        }

        /// <summary>
        /// Extracts right seven-bit part of an unsigned 16-bit integer number.
        /// </summary>
        /// <param name="number">Number to extract right part of.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the right part of <paramref name="number"/>.</returns>
        public static SevenBitNumber GetTail(this ushort number)
        {
            return (SevenBitNumber)(number & SevenBitNumber.MaxValue);
        }

        /// <summary>
        /// Extracts right eight-bit part of an unsigned 16-bit integer number.
        /// </summary>
        /// <param name="number">Number to extract right part of.</param>
        /// <returns>Byte representing the right part of <paramref name="number"/>.</returns>
        public static byte GetTail(this short number)
        {
            return (byte)(number & byte.MaxValue);
        }

        /// <summary>
        /// Extracts right 16-bit part of an unsigned 32-bit integer number.
        /// </summary>
        /// <param name="number">Number to extract right part of.</param>
        /// <returns>16-bit unsigned integer number representing the right part of <paramref name="number"/>.</returns>
        public static ushort GetTail(this uint number)
        {
            return (ushort)(number & ushort.MaxValue);
        }

        /// <summary>
        /// Extracts left four-bit part of a byte.
        /// </summary>
        /// <param name="number">Byte to extract left part of.</param>
        /// <returns><see cref="FourBitNumber"/> representing the left part of <paramref name="number"/>.</returns>
        public static FourBitNumber GetHead(this byte number)
        {
            return (FourBitNumber)(number >> 4);
        }

        /// <summary>
        /// Extracts left seven-bit part of an unsigned 16-bit integer number.
        /// </summary>
        /// <param name="number">Number to extract left part of.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the left part of <paramref name="number"/>.</returns>
        public static SevenBitNumber GetHead(this ushort number)
        {
            return (SevenBitNumber)(number >> 7);
        }

        /// <summary>
        /// Extracts left eight-bit part of an signed 16-bit integer number.
        /// </summary>
        /// <param name="number">Number to extract left part of.</param>
        /// <returns>Byte representing the left part of <paramref name="number"/>.</returns>
        public static byte GetHead(this short number)
        {
            return (byte)(number >> 8);
        }

        /// <summary>
        /// Extracts left 16-bit part of an unsigned 32-bit integer number.
        /// </summary>
        /// <param name="number">Number to extract left part of.</param>
        /// <returns>16-bit unsigned integer number representing the left part of <paramref name="number"/>.</returns>
        public static ushort GetHead(this uint number)
        {
            return (ushort)(number >> 16);
        }

        /// <summary>
        /// Gets length of variable-length quantity (VLQ) representation of an integer number.
        /// </summary>
        /// <param name="number">Number to calculate VLQ length for.</param>
        /// <returns>Bytes count required to represent the number in VLQ.</returns>
        /// <remarks>
        /// Numbers in VLQ format are represented 7 bits per byte, most significant bits first.
        /// All bytes except the last have bit 7 set, and the last byte has bit 7 clear. If the
        /// number is between 0 and 127, it is thus represented exactly as one byte.
        /// </remarks>
        public static int GetVlqLength(this int number)
        {
            var mask = 1 << 30;
            var bitsCount = 31;

            while ((number & mask) == 0 && mask > 0)
            {
                mask >>= 1;
                bitsCount--;
            }

            return Math.Max(bitsCount / 7 + (bitsCount % 7 > 0 ? 1 : 0), 1);
        }

        /// <summary>
        /// Gets length of variable-length quantity (VLQ) representation of an integer number.
        /// </summary>
        /// <param name="number">Number to calculate VLQ length for.</param>
        /// <returns>Bytes count required to represent the number in VLQ.</returns>
        /// <remarks>
        /// Numbers in VLQ format are represented 7 bits per byte, most significant bits first.
        /// All bytes except the last have bit 7 set, and the last byte has bit 7 clear. If the
        /// number is between 0 and 127, it is thus represented exactly as one byte.
        /// </remarks>
        public static int GetVlqLength(this long number)
        {
            var mask = 1L << 62;
            var bitsCount = 63;

            while ((number & mask) == 0 && mask > 0)
            {
                mask >>= 1;
                bitsCount--;
            }

            return Math.Max(bitsCount / 7 + (bitsCount % 7 > 0 ? 1 : 0), 1);
        }

        /// <summary>
        /// Gets bytes of a number in variable-length quantity (VLQ) format.
        /// </summary>
        /// <param name="number">Number to get VLQ bytes for.</param>
        /// <returns>Bytes representing the number coded in VLQ.</returns>
        /// <remarks>
        /// Numbers in VLQ format are represented 7 bits per byte, most significant bits first.
        /// All bytes except the last have bit 7 set, and the last byte has bit 7 clear. If the
        /// number is between 0 and 127, it is thus represented exactly as one byte.
        /// </remarks>
        public static byte[] GetVlqBytes(this int number)
        {
            return GetVlqBytes((long)number);
        }

        /// <summary>
        /// Gets bytes of a number in variable-length quantity (VLQ) format.
        /// </summary>
        /// <param name="number">Number to get VLQ bytes for.</param>
        /// <returns>Bytes representing the number coded in VLQ.</returns>
        /// <remarks>
        /// Numbers in VLQ format are represented 7 bits per byte, most significant bits first.
        /// All bytes except the last have bit 7 set, and the last byte has bit 7 clear. If the
        /// number is between 0 and 127, it is thus represented exactly as one byte.
        /// </remarks>
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

        /// <summary>
        /// Extracts first byte (leftmost) of signed 32-bit integer number.
        /// </summary>
        /// <param name="number">Number to extract first byte of.</param>
        /// <returns>First byte of <paramref name="number"/>.</returns>
        public static byte GetFirstByte(this int number)
        {
            return (byte)((number >> 24) & 0xFF);
        }

        /// <summary>
        /// Extracts second byte (starting from left) of signed 32-bit integer number.
        /// </summary>
        /// <param name="number">Number to extract second byte of.</param>
        /// <returns>Second byte of <paramref name="number"/>.</returns>
        public static byte GetSecondByte(this int number)
        {
            return (byte)((number >> 16) & 0xFF);
        }

        /// <summary>
        /// Extracts third byte (starting from left) of signed 32-bit integer number.
        /// </summary>
        /// <param name="number">Number to extract third byte of.</param>
        /// <returns>Third byte of <paramref name="number"/>.</returns>
        public static byte GetThirdByte(this int number)
        {
            return (byte)((number >> 8) & 0xFF);
        }

        /// <summary>
        /// Extracts fourth byte (rightmost) of signed 32-bit integer number.
        /// </summary>
        /// <param name="number">Number to extract fourth byte of.</param>
        /// <returns>Fourth byte of <paramref name="number"/>.</returns>
        public static byte GetFourthByte(this int number)
        {
            return (byte)(number & 0xFF);
        }

        #endregion
    }
}
