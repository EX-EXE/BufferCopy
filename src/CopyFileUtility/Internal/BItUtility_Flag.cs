

using System.Runtime.CompilerServices;

namespace CopyFileUtility_Internal
{
    internal static partial class BitUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFillInt(int flagNum)
        {
            if(flagNum == 0 )
            {
                return 0b00000000000000000000000000000000;
            }
            if(flagNum == 1 )
            {
                return 0b00000000000000000000000000000001;
            }
            if(flagNum == 2 )
            {
                return 0b00000000000000000000000000000011;
            }
            if(flagNum == 3 )
            {
                return 0b00000000000000000000000000000111;
            }
            if(flagNum == 4 )
            {
                return 0b00000000000000000000000000001111;
            }
            if(flagNum == 5 )
            {
                return 0b00000000000000000000000000011111;
            }
            if(flagNum == 6 )
            {
                return 0b00000000000000000000000000111111;
            }
            if(flagNum == 7 )
            {
                return 0b00000000000000000000000001111111;
            }
            if(flagNum == 8 )
            {
                return 0b00000000000000000000000011111111;
            }
            if(flagNum == 9 )
            {
                return 0b00000000000000000000000111111111;
            }
            if(flagNum == 10 )
            {
                return 0b00000000000000000000001111111111;
            }
            if(flagNum == 11 )
            {
                return 0b00000000000000000000011111111111;
            }
            if(flagNum == 12 )
            {
                return 0b00000000000000000000111111111111;
            }
            if(flagNum == 13 )
            {
                return 0b00000000000000000001111111111111;
            }
            if(flagNum == 14 )
            {
                return 0b00000000000000000011111111111111;
            }
            if(flagNum == 15 )
            {
                return 0b00000000000000000111111111111111;
            }
            if(flagNum == 16 )
            {
                return 0b00000000000000001111111111111111;
            }
            if(flagNum == 17 )
            {
                return 0b00000000000000011111111111111111;
            }
            if(flagNum == 18 )
            {
                return 0b00000000000000111111111111111111;
            }
            if(flagNum == 19 )
            {
                return 0b00000000000001111111111111111111;
            }
            if(flagNum == 20 )
            {
                return 0b00000000000011111111111111111111;
            }
            if(flagNum == 21 )
            {
                return 0b00000000000111111111111111111111;
            }
            if(flagNum == 22 )
            {
                return 0b00000000001111111111111111111111;
            }
            if(flagNum == 23 )
            {
                return 0b00000000011111111111111111111111;
            }
            if(flagNum == 24 )
            {
                return 0b00000000111111111111111111111111;
            }
            if(flagNum == 25 )
            {
                return 0b00000001111111111111111111111111;
            }
            if(flagNum == 26 )
            {
                return 0b00000011111111111111111111111111;
            }
            if(flagNum == 27 )
            {
                return 0b00000111111111111111111111111111;
            }
            if(flagNum == 28 )
            {
                return 0b00001111111111111111111111111111;
            }
            if(flagNum == 29 )
            {
                return 0b00011111111111111111111111111111;
            }
            if(flagNum == 30 )
            {
                return 0b00111111111111111111111111111111;
            }
            if(flagNum == 31 )
            {
                return 0b01111111111111111111111111111111;
            }
            throw new OverflowException(flagNum.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFlagInt(int flagNum)
        {
            if(flagNum == 0 )
            {
                return 0b00000000000000000000000000000001;
            }
            if(flagNum == 1 )
            {
                return 0b00000000000000000000000000000010;
            }
            if(flagNum == 2 )
            {
                return 0b00000000000000000000000000000100;
            }
            if(flagNum == 3 )
            {
                return 0b00000000000000000000000000001000;
            }
            if(flagNum == 4 )
            {
                return 0b00000000000000000000000000010000;
            }
            if(flagNum == 5 )
            {
                return 0b00000000000000000000000000100000;
            }
            if(flagNum == 6 )
            {
                return 0b00000000000000000000000001000000;
            }
            if(flagNum == 7 )
            {
                return 0b00000000000000000000000010000000;
            }
            if(flagNum == 8 )
            {
                return 0b00000000000000000000000100000000;
            }
            if(flagNum == 9 )
            {
                return 0b00000000000000000000001000000000;
            }
            if(flagNum == 10 )
            {
                return 0b00000000000000000000010000000000;
            }
            if(flagNum == 11 )
            {
                return 0b00000000000000000000100000000000;
            }
            if(flagNum == 12 )
            {
                return 0b00000000000000000001000000000000;
            }
            if(flagNum == 13 )
            {
                return 0b00000000000000000010000000000000;
            }
            if(flagNum == 14 )
            {
                return 0b00000000000000000100000000000000;
            }
            if(flagNum == 15 )
            {
                return 0b00000000000000001000000000000000;
            }
            if(flagNum == 16 )
            {
                return 0b00000000000000010000000000000000;
            }
            if(flagNum == 17 )
            {
                return 0b00000000000000100000000000000000;
            }
            if(flagNum == 18 )
            {
                return 0b00000000000001000000000000000000;
            }
            if(flagNum == 19 )
            {
                return 0b00000000000010000000000000000000;
            }
            if(flagNum == 20 )
            {
                return 0b00000000000100000000000000000000;
            }
            if(flagNum == 21 )
            {
                return 0b00000000001000000000000000000000;
            }
            if(flagNum == 22 )
            {
                return 0b00000000010000000000000000000000;
            }
            if(flagNum == 23 )
            {
                return 0b00000000100000000000000000000000;
            }
            if(flagNum == 24 )
            {
                return 0b00000001000000000000000000000000;
            }
            if(flagNum == 25 )
            {
                return 0b00000010000000000000000000000000;
            }
            if(flagNum == 26 )
            {
                return 0b00000100000000000000000000000000;
            }
            if(flagNum == 27 )
            {
                return 0b00001000000000000000000000000000;
            }
            if(flagNum == 28 )
            {
                return 0b00010000000000000000000000000000;
            }
            if(flagNum == 29 )
            {
                return 0b00100000000000000000000000000000;
            }
            if(flagNum == 30 )
            {
                return 0b01000000000000000000000000000000;
            }
            throw new OverflowException(flagNum.ToString());
        }
    }
}
