using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SFB;
using System.Threading.Tasks;


namespace Rabbyte
{
    public static class SbdFileHandler
    {
        public static void Encode()
        {
            string convert = "This is the string to be converted";

            // From string to byte array
            byte[] buffer = System.Text.Encoding.Default.GetBytes(convert);

            // From byte array to string
            string s = System.Text.Encoding.Default.GetString(buffer, 0, buffer.Length);
        }
        /*
         string convert = "This is the string to be converted";

            // From string to byte array
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(convert);

            // From byte array to string
            string s = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
         */
    }
}
