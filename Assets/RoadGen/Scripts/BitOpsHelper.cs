namespace RoadGen
{
    public static class BitOpsHelper
    {
        public static int PopCount(int bitmask)
        {
            bitmask = bitmask - ((bitmask >> 1) & 0x55555555);
            bitmask = (bitmask & 0x33333333) + ((bitmask >> 2) & 0x33333333);
            return (((bitmask + (bitmask >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

        public static int RandomBit(int bitmask)
        {
            return UnityEngine.Random.Range(0, PopCount(bitmask));
        }

        public static int RemoveRandomBit(ref int bitmask)
        {
            var randomBitIndex = UnityEngine.Random.Range(0, PopCount(bitmask));
            var actualBitIndex = BitOpsHelper.BFind(bitmask, randomBitIndex + 1);
            bitmask &= ~(1 << actualBitIndex);
            return actualBitIndex;
        }

        public static int Clz(int bitmask)
        {
            int i = 0;
            while ((bitmask & (1 << i)) == 0 && i < 32)
                i++;
            return i;
        }

        public static int BFind(int bitmask, int count)
        {
            int c = 0, i = 0;
            do
            {
                c += (bitmask & (1 << i)) >> i;
            } while (c != count && ++i < 32);
            return i;
        }

    }

}