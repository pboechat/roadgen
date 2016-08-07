using UnityEngine;

namespace RoadGen
{
    public static class FilterHelper
    {
        public static float[,] CreateGaussianKernel(int size, float weight)
        {
            float[,] kernel = new float[size, size];
            float calculatedEuler = 1.0f / (2.0f * Mathf.PI * Mathf.Pow(weight, 2));
            int radius = size / 2;
            float distance = 0;
            float sum = 0;
            for (int fY = -radius; fY <= radius; fY++)
            {
                for (int fX = -radius; fX <= radius; fX++)
                {
                    distance = ((fX * fX) + (fY * fY)) / (2 * (weight * weight));
                    kernel[fX + radius, fY + radius] = calculatedEuler * Mathf.Exp(-distance);
                    sum += kernel[fX + radius, fY + radius];
                }
            }
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    kernel[x, y] = kernel[x, y] * (1.0f / sum);
                }
            }
            return kernel;
        }

        public static void Convolute(float[,] src, float[,] dst, float[,] filter, int w, int h, int kernelSize, int x0 = 0, int x1 = -1, int y0 = 0, int y1 = -1)
        {
            if (x1 < 0)
                x1 = w - 1;
            if (y1 < 0)
                y1 = h - 1;
            int hKernelSize = kernelSize / 2;
            for (int gY = y0; gY <= y1; gY++)
            {
                for (int gX = x0; gX <= x1; gX++)
                {
                    dst[gX, gY] = src[gX, gY];
                    float pixel = 0;
                    int minFY = Mathf.Max(0, gY - hKernelSize),
                        maxFY = Mathf.Min(h - 1, gY + hKernelSize);
                    int minFX = Mathf.Max(0, gX - hKernelSize),
                        maxFX = Mathf.Min(w - 1, gX + hKernelSize);
                    for (int y = 0, fY = minFY; fY <= maxFY; y++, fY++)
                        for (int x = 0, fX = minFX; fX <= maxFX; x++, fX++)
                            pixel += filter[x, y] * src[fX, fY];
                    dst[gX, gY] = pixel;
                }
            }
        }

    }

}