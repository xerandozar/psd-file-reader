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
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace PsdFileReader.Photoshop
{
    public delegate void ChannelUpdate(BigEndianReader reader);

    public enum PsdColorMode : short
    {
        Bitmap = 0,
        Grayscale = 1,
        Indexed = 2,
        RGB = 3,
        CMYK = 4,
        Multichannel = 7,
        Duotone = 8,
        Lab = 9
    }

    public class PsdFile
    {
        public IEnumerable<Layer> Layers => m_Layers;
        private List<Layer> m_Layers;

        private bool m_ImageDataLoaded;

        public short Version { get; private set; }
        public short ChannelCount { get; private set; }

        public int Height { get; private set; }
        public int Width { get; private set; }

        public short BitDepth { get; private set; }
        public PsdColorMode ColorMode { get; private set; }

        public PsdFile(BigEndianReader reader, bool loadImageData)
        {
            m_Layers = new List<Layer>();
            m_ImageDataLoaded = false;

            ReadFileHeader(reader);
            ReadColorModeData(reader);
            ReadImageResources(reader);
            ReadLayerAndMaskInfo(reader);

            if (loadImageData)
                LoadImageData(reader);
        }

        private void ReadFileHeader(BigEndianReader reader)
        {
            string signature = reader.ReadASCII(4);
            if (signature != "8BPS")
                throw new Exception($"PsdFile: Invalid signature {signature}");

            Version = reader.ReadInt16();
            if (!VersionReader.IsSupportedVersion(Version))
                throw new Exception($"PsdFile: Invalid version {Version}");

            byte[] reserved = reader.ReadBytes(6);
            if (!reserved.SequenceEqual(new byte[6]))
                throw new Exception($"PsdFile: Invalid reserved bytes");

            ChannelCount = reader.ReadInt16();
            if (ChannelCount < 1 || ChannelCount > 56)
                throw new Exception($"PsdFile: Invalid channel count");

            Height = reader.ReadInt32();
            Width = reader.ReadInt32();

            BitDepth = reader.ReadInt16();
            if (BitDepth != 1 && BitDepth != 8 && BitDepth != 16 && BitDepth != 32)
                throw new Exception("PsdFile: Invalid bit depth");

            ColorMode = (PsdColorMode)reader.ReadInt16();
            if (ColorMode < (PsdColorMode)0 || ColorMode > (PsdColorMode)9)
                throw new Exception("PsdFile: Invalid color mode");
        }

        private void ReadColorModeData(BigEndianReader reader)
        {
            // Not implemented
            int colorModeDataLength = reader.ReadInt32();
            reader.Position += colorModeDataLength;
        }

        private void ReadImageResources(BigEndianReader reader)
        {
            // Not implemented
            int imageResourcesLength = reader.ReadInt32();
            reader.Position += imageResourcesLength;
        }

        private void ReadLayerAndMaskInfo(BigEndianReader reader)
        {
            long layerAndMaskInfoLength
                = VersionReader.ReadLayerAndMaskInfoLength(Version, reader);

            if (layerAndMaskInfoLength <= 0)
                return;

            long layerAndMaskInfoEnd = reader.Position + layerAndMaskInfoLength;

            ReadLayerInfo(reader);
            // Skip: Global Layer Mask Info
            // Skip: Additional Layer Info

            reader.Position = layerAndMaskInfoEnd;
        }

        private void ReadLayerInfo(BigEndianReader reader)
        {
            long layerInfoLength
                = VersionReader.ReadLayerInfoLength(Version, reader);

            if (layerInfoLength <= 0)
                return;

            long layerInfoEnd = reader.Position + layerInfoLength;

            // Absolute value is the number of layers (Adobe Photoshop Documentation)
            short layerCount = Math.Abs(reader.ReadInt16());

            if (layerCount == 0)
                return;

            var channelUpdates = new List<ChannelUpdate>();

            for (int i = 0; i < layerCount; i++)
            {
                var layer = new Layer(Version, channelUpdates, reader);
                m_Layers.Add(layer);
            }

            // Channels image data positions are defined after layers
            foreach (var channelUpdate in channelUpdates)
                channelUpdate(reader);

            reader.Position = layerInfoEnd;
        }

        public void LoadImageData(BigEndianReader reader)
        {
            if (m_ImageDataLoaded)
                throw new Exception("PsdFile: Image data is already loaded");

            foreach (var layer in m_Layers)
                layer.LoadImageData(reader);

            m_ImageDataLoaded = true;
        }

        public static PsdFile LoadFile(string path, bool loadImageData)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            using (var reader = new BigEndianReader(stream))
            {
                return new PsdFile(reader, loadImageData);
            }
        }
    }
}