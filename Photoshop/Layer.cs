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
using System.Collections.Generic;

namespace PsdFileReader.Photoshop
{
    public class Layer
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        private List<Channel> m_Channels;
        private bool m_ImageDataLoaded;

        public ImageData ImageData { get; private set; }

        public string BlendMode { get; private set; }
        public byte Opacity { get; private set; }
        public bool Clipping { get; private set; }
        public bool Visibility { get; private set; }

        public string Name { get; private set; }

        public Layer(int version, List<ChannelUpdate> channelUpdates, BigEndianReader reader)
        {
            int top = reader.ReadInt32();
            int left = reader.ReadInt32();
            int bottom = reader.ReadInt32();
            int right = reader.ReadInt32();

            X = left;
            Y = top;
            Width = right - left;
            Height = bottom - top;

            m_Channels = new List<Channel>();
            m_ImageDataLoaded = false;

            int channelCount = reader.ReadInt16();

            for (int i = 0; i < channelCount; i++)
            {
                var channel = new Channel(version, reader);
                channelUpdates.Add(channel.UpdateImageDataPositions);
                m_Channels.Add(channel);
            }

            ImageData = new ImageData(Width, Height, m_Channels);

            string blendModeSignature = reader.ReadASCII(4);
            if (blendModeSignature != "8BIM")
                throw (new Exception($"LayerInfo: Invalid blend mode signature {blendModeSignature}"));

            BlendMode = reader.ReadASCII(4);
            Opacity = reader.ReadByte();
            Clipping = reader.ReadBoolean();
            Visibility = reader.ReadBoolean();

            // Not implemented
            int fillerByteLength = 1;
            reader.Position += fillerByteLength;

            int extraDataFieldLength = reader.ReadInt32();
            long extraDataFieldEnd = reader.Position + extraDataFieldLength;

            // Not implemented
            int maskDataLength = reader.ReadInt32();
            reader.Position += maskDataLength;

            // Not implemented
            int blendingRangesLength = reader.ReadInt32();
            reader.Position += blendingRangesLength;

            Name = reader.ReadPascalString(4);

            // Skip: Adjustment Layers            
            reader.Position = extraDataFieldEnd;
        }

        public void LoadImageData(BigEndianReader reader)
        {
            if (ImageData.Type == ImageType.Unsupported)
                return;

            if (m_ImageDataLoaded)
                throw new Exception("Layer: Image data is already loaded");

            bool invalidChannel = false;

            foreach (var channel in m_Channels)
            {
                switch (channel.Color)
                {
                    case ChannelColor.Alpha:
                        if (!channel.LoadColors(ImageData.Alpha, Width, Height, reader))
                            invalidChannel = true;
                        break;
                    case ChannelColor.Red:
                        if (!channel.LoadColors(ImageData.Red, Width, Height, reader))
                            invalidChannel = true;
                        break;
                    case ChannelColor.Green:
                        if (!channel.LoadColors(ImageData.Green, Width, Height, reader))
                            invalidChannel = true;
                        break;
                    case ChannelColor.Blue:
                        if (!channel.LoadColors(ImageData.Blue, Width, Height, reader))
                            invalidChannel = true;
                        break;
                    default:
                        continue;
                }
            }

            if (invalidChannel)
                ImageData = ImageData.Unsupported();

            m_ImageDataLoaded = true;
        }
    }
}