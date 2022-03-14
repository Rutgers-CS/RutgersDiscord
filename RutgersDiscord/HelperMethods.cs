using System;
//Move this class somewhere. Or the methods to different places
public static class HelperMethods
{
    public static long RandomID()
    {
        Random rand = new();
        byte[] buf = new byte[8];
        rand.NextBytes(buf);
        long longRand = BitConverter.ToInt64(buf, 0);

        return Math.Abs(longRand % long.MaxValue);
    }
}
