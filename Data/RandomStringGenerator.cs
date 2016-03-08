/*************************************************************************************************
* 
* THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
* KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
* PARTICULAR PURPOSE.
* 
*************************************************************************************************/
//====================================================
//| Downloaded From                                  |
//| Visual C# Kicks - http://www.vcskicks.com/       |
//| License - http://www.vcskicks.com/license.html   |
//====================================================
using System;

namespace Figaro.Web.ApplicationServices.Data
{
    internal sealed class RandomStringGenerator
    {
        private readonly Random r;
        const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string Numbers = "0123456789";
        const string Symbols = @"~`!@#$%^&*()-_=+<>?:,./\[]{}|'";

        public RandomStringGenerator()
        {
            r = new Random();
        }

        public RandomStringGenerator(int seed)
        {
            r = new Random(seed);
        }

        public string NextString(int length, bool lowerCase = true, bool upperCase = true, bool numbers = true, bool symbols = true)
        {
            char[] charArray = new char[length];
            string charPool = string.Empty;

            //Build character pool
            if (lowerCase)
                charPool += Lowercase;

            if (upperCase)
                charPool += Uppercase;

            if (numbers)
                charPool += Numbers;

            if (symbols)
                charPool += Symbols;

            //Build the output character array
            for (int i = 0; i < charArray.Length; i++)
            {
                //Pick a random integer in the character pool
                int index = r.Next(0, charPool.Length);

                //Set it to the output character array
                charArray[i] = charPool[index];
            }

            return new string(charArray);
        }
    }
}
