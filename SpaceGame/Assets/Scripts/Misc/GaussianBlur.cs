using UnityEngine;

public static class GaussianBlur
{
    public static Texture2D Apply(Texture2D sourceTexture, int radius, float sigma)
    {
        // Clone the source texture
        Texture2D blurredTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);
        blurredTexture.SetPixels(sourceTexture.GetPixels());
        blurredTexture.Apply();

        // Calculate the kernel for the Gaussian blur
        int kernelSize = radius * 2 + 1;
        float[] kernel = CalculateGaussianKernel(kernelSize, sigma);

        // Apply the blur horizontally
        ApplyBlur(blurredTexture, kernel, true);

        // Apply the blur vertically
        ApplyBlur(blurredTexture, kernel, false);

        return blurredTexture;
    }

    private static void ApplyBlur(Texture2D texture, float[] kernel, bool horizontal)
    {
        int width = texture.width;
        int height = texture.height;
        int radius = kernel.Length / 2;

        Color[] pixels = texture.GetPixels();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float r = 0f, g = 0f, b = 0f, a = 0f;
                int kernelIndex = 0;

                for (int i = -radius; i <= radius; i++)
                {
                    int pixelX = horizontal ? x + i : x;
                    int pixelY = horizontal ? y : y + i;

                    pixelX = Mathf.Clamp(pixelX, 0, width - 1);
                    pixelY = Mathf.Clamp(pixelY, 0, height - 1);

                    Color pixelColor = pixels[pixelY * width + pixelX];

                    float weight = kernel[kernelIndex];
                    r += pixelColor.r * weight;
                    g += pixelColor.g * weight;
                    b += pixelColor.b * weight;
                    a += pixelColor.a * weight;

                    kernelIndex++;
                }

                Color blurredColor = new Color(r, g, b, a);
                pixels[y * width + x] = blurredColor;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
    }

    private static float[] CalculateGaussianKernel(int size, float sigma)
    {
        float[] kernel = new float[size];
        float twoSigmaSquared = 2f * sigma * sigma;
        float sqrtTwoPiSigma = Mathf.Sqrt(2f * Mathf.PI) * sigma;
        float total = 0f;

        int radius = size / 2;

        for (int i = -radius; i <= radius; i++)
        {
            float distance = i * i;
            int index = i + radius;
            kernel[index] = Mathf.Exp(-distance / twoSigmaSquared) / sqrtTwoPiSigma;
            total += kernel[index];
        }

        // Normalize the kernel
        for (int i = 0; i < size; i++)
        {
            kernel[i] /= total;
        }

        return kernel;
    }
}