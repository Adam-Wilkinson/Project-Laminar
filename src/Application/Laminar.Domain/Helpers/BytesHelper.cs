namespace Laminar.Domain.Helpers;

public static class BytesHelper
{
    // Copyright (c) 2008-2013 Hafthor Stefansson
    // Distributed under the MIT/X11 software license
    // Ref: http://www.opensource.org/licenses/mit-license.php.
    public static unsafe bool Equals(byte[]? a1, byte[]? a2)
    {
        if (a1 == a2)
            return true;

        if (a1 is null || a2 is null || a1.Length != a2.Length)
            return false;

        fixed (byte* p1 = a1, p2 = a2)
        {
            byte* x1 = p1, x2 = p2;

            int l = a1.Length;
            for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
            {
                if (*(long*)x1 != *(long*)x2)
                    return false;
            }

            if ((l & 4) != 0)
            {
                if (*(int*)x1 != *(int*)x2)
                    return false;

                x1 += 4;
                x2 += 4;
            }

            if ((l & 2) != 0)
            {
                if (*(short*)x1 != *(short*)x2)
                    return false;

                x1 += 2;
                x2 += 2;
            }

            return (l & 1) == 0 || *x1 == *x2;
        }
    }
}