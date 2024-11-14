// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("wkGfYsvtShhuTrfZ4G5dyBf6h5O77mct/TQATZLKhTToIWnPg8BDoimbGDspFB8QM59Rn+4UGBgYHBkaAyOJ9tHyHU5aABjtNBvDHSFrkWdGc6+yXlgAV0Tz1sXY9Vqd2uPI5Arjok0447MVD2POmBA1KAwELroIf9tJHRyXEjf5H9Ag6AXg1f2HHbtfVWgpF2CfJk8rsOC0p/gRtjre6BwVGpty5zU85aZQ6O6hw4vRRStOGYuYySjPdZZKv/aVAj5xGRKZxjY4cYhL3t8UyEKwsnkXT/LeWDERBc4H1N7JcvrvRM3b9K4s5e1mY9RdmxgWGSmbGBMbmxgYGdJokspbosuO7Lutng4bv0ylfHkOFaGf0Jy8AHYjMgBLpYFZnhsaGBkY");
        private static int[] order = new int[] { 2,10,2,6,9,13,9,11,12,9,12,11,12,13,14 };
        private static int key = 25;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
