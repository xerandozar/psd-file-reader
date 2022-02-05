/*
MIT License

Copyright (c) 2022 Jani Eronen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections.Generic;

namespace PsdFileReader.Photoshop
{
    public enum ImageType
    {
        RGBA,
        Unsupported
    }

    public class ImageData
    {
        public readonly ImageType Type;

        public readonly int Width;
        public readonly int Height;

        public readonly byte[] Red;
        public readonly byte[] Green;
        public readonly byte[] Blue;
        public readonly byte[] Alpha;

        public ImageData(int width, int height, List<Channel> channels)
        {
            Type = GetImageType(channels);

            Width = width;
            Height = height;

            if (Type == ImageType.RGBA)
            {
                Red = new byte[Width * Height];
                Green = new byte[Width * Height];
                Blue = new byte[Width * Height];
                Alpha = new byte[Width * Height];
            }
            else
            {
                Type = ImageType.Unsupported;
                Red = new byte[0];
                Green = new byte[0];
                Blue = new byte[0];
                Alpha = new byte[0];
            }
        }

        private ImageType GetImageType(List<Channel> channels)
        {
            int alphaChannels = 0;
            int redChannels = 0;
            int greenChannels = 0;
            int blueChannels = 0;

            foreach (var channel in channels)
            {
                switch (channel.Color)
                {
                    case ChannelColor.Alpha:
                        alphaChannels++;
                        break;
                    case ChannelColor.Red:
                        redChannels++;
                        break;
                    case ChannelColor.Green:
                        greenChannels++;
                        break;
                    case ChannelColor.Blue:
                        blueChannels++;
                        break;
                    default:
                        return ImageType.Unsupported;
                }
            }

            if (redChannels == 1 && greenChannels == 1 && blueChannels == 1 && alphaChannels == 1)
                return ImageType.RGBA;

            return ImageType.Unsupported;
        }

        public static ImageData Unsupported()
        {
            return new ImageData(0, 0, new List<Channel>());
        }
    }
}