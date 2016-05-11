using System;

namespace Arm7BotNET
{
    public class ForceFilter
    {
        private const int filterSize = 39;
        public int[] filerElements = new int[filterSize];

        public ForceFilter()
        {
            for (int i = 0; i < filterSize; i++)
            {
                filerElements[i] = 0;
            }
        }

        public int filter(int dataIn)
        {
            int sum = 0;

            // 1- in put data
            for (int i = filterSize - 1; i > 0; i--)
            {
                filerElements[i] = filerElements[i - 1];
                sum += filerElements[i];
            }
            filerElements[0] = dataIn;
            sum += filerElements[0];

            return (int)(sum / filterSize);
        }
    }
}
