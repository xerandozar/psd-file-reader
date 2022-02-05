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

using System;

namespace PsdFileReader.Photoshop
{
    public enum ImageCompression : short
    {
        Raw = 0,
        Rle = 1,
        Zip = 2,
        ZipPrediction = 3
    }

    public enum ChannelColor : short
    {
        Alpha = -1,
        Red = 0,
        Green = 1,
        Blue = 2
    }

    public class Channel
    {
        public ChannelColor Color => (ChannelColor)m_Id;

        private int m_Version;

        private short m_Id;
        private long m_Length;

        private long m_ImageDataStartPosition;
        private long m_ImageDataEndPosition;

        public Channel(int version, BigEndianReader reader)
        {
            m_Version = version;

            m_Id = reader.ReadInt16();
            m_Length = VersionReader.ReadChannelLength(m_Version, reader);

            m_ImageDataStartPosition = 0;
            m_ImageDataEndPosition = 0;
        }

        public void UpdateImageDataPositions(BigEndianReader reader)
        {
            m_ImageDataStartPosition = reader.Position;
            m_ImageDataEndPosition = m_ImageDataStartPosition + m_Length;

            reader.Position = m_ImageDataEndPosition;
        }

        public bool LoadColors(byte[] colors, int width, int height, BigEndianReader reader)
        {
            reader.Position = m_ImageDataStartPosition;
            var imageCompression = (ImageCompression)reader.ReadInt16();

            switch (imageCompression)
            {
                case ImageCompression.Raw:
                    // Not implemented                    
                    return false;
                case ImageCompression.Rle:
                    for (int i = 0; i < height; i++)
                    {
                        // Skip: Channel scan line
                        VersionReader.ReadChannelScanLineLength(m_Version, reader);
                    }

                    int rleDataLength = (int)(m_ImageDataEndPosition - reader.Position);
                    byte[] rleData = reader.ReadBytes(rleDataLength);
                    RleCompression.Unpack(rleData, colors);
                    return true;
                case ImageCompression.Zip:
                    // Not implemented                    
                    return false;
                case ImageCompression.ZipPrediction:
                    // Not implemented                    
                    return false;
                default:
                    throw (new Exception($"Channel: Invalid image compression {imageCompression}"));
            }
        }
    }
}