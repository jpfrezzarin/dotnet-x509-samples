namespace Core;

public static class ByteExtensions
{
    public static byte[] FromHexStringToByteArray(this string str)
    {
        if (str.Length % 2 != 0) throw new Exception($"{str} is not a hexadecimal value");
        byte[] bytes = new byte[str.Length >> 1];

        for (int i = 0; i < str.Length >> 1; ++i)
        {
            int firstHexVal = str[i << 1].ToHexIntValue();
            if (firstHexVal is < 0 or > 15) throw new Exception($"{str} is not a hexadecimal value");
            int secondHexVal = str[(i << 1) + 1].ToHexIntValue();
            if (secondHexVal is < 0 or > 15) throw new Exception($"{str} is not a hexadecimal value");
            
            bytes[i] = (byte)((firstHexVal << 4) + secondHexVal);
        }

        return bytes;
    }

    private static int ToHexIntValue(this char hex)
    {
        int val = hex;
        return val - val switch
        {
            < 58 => 48,
            > 97 => 87,
            _ => 55
        };
    }
}