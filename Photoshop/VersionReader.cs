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
    public static class VersionReader
    {
        public static bool IsSupportedVersion(int version)
        {
            switch (version)
            {
                case 1:
                    return true;
                case 2:
                    return true;
                default:
                    return false;
            }
        }

        public static long ReadLayerAndMaskInfoLength(int version, BigEndianReader reader)
        {
            switch (version)
            {
                case 1:
                    return reader.ReadInt32();
                case 2:
                    return reader.ReadInt64();
                default:
                    throw new Exception($"Invalid version {version}");
            }
        }

        public static long ReadLayerInfoLength(int version, BigEndianReader reader)
        {
            switch (version)
            {
                case 1:
                    return RoundUpByMultipilication(reader.ReadInt32(), 2);
                case 2:
                    return RoundUpByMultipilication(reader.ReadInt64(), 2);
                default:
                    throw new Exception($"Invalid version {version}");
            }
        }

        public static long ReadChannelLength(int version, BigEndianReader reader)
        {
            switch (version)
            {
                case 1:
                    return reader.ReadInt32();
                case 2:
                    return reader.ReadInt64();
                default:
                    throw new Exception($"Invalid version {version}");
            }
        }

        public static int ReadChannelScanLineLength(int version, BigEndianReader reader)
        {
            switch (version)
            {
                case 1:
                    return reader.ReadInt16();
                case 2:
                    return reader.ReadInt32();
                default:
                    throw new Exception($"Invalid version {version}");
            }
        }

        public static long RoundUpByMultipilication(long value, int division)
        {
            return value + (value % division);
        }
    }
}