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
    public static class RleCompression
    {
        unsafe public static void Unpack(byte[] source, byte[] destination)
        {
            fixed (byte* readStart = &source[0])
            fixed (byte* writeStart = &destination[0])
            {
                byte* read = readStart;
                byte* write = writeStart;

                int packedBytes = destination.Length;
                while (packedBytes > 0)
                {
                    sbyte readFlag = unchecked((sbyte)*read);
                    read++;

                    // (1 + n) literal bytes of data
                    if (readFlag > 0)
                    {
                        int readLength = readFlag + 1;

                        Buffer.MemoryCopy(read, write, readLength, readLength);
                        read += readLength;
                        write += readLength;

                        packedBytes -= readLength;
                    }
                    // One byte of data, repeated (1 − n) times in the output
                    else if (readFlag > -128)
                    {
                        int runLength = 1 - readFlag;

                        byte value = (byte)*read;
                        read++;

                        for (int i = 0; i < runLength; i++)
                        {
                            *write = value;
                            write++;
                        }

                        packedBytes -= runLength;
                    }
                }
            }
        }
    }
}