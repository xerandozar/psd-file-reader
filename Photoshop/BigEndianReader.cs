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
using System.Buffers.Binary;
using System.Text;

namespace PsdFileReader.Photoshop
{
    public class BigEndianReader : IDisposable
    {
        public long Position
        {
            get { return m_Reader.BaseStream.Position; }
            set { m_Reader.BaseStream.Position = value; }
        }

        private BinaryReader m_Reader;

        public BigEndianReader(Stream stream)
        {
            m_Reader = new BinaryReader(stream, Encoding.ASCII);
        }

        public byte ReadByte()
        {
            return m_Reader.ReadByte();
        }

        public byte[] ReadBytes(int count)
        {
            return m_Reader.ReadBytes(count);
        }

        public bool ReadBoolean()
        {
            return m_Reader.ReadBoolean();
        }

        public short ReadInt16()
        {
            Span<byte> data = new Span<byte>(new byte[2]);
            m_Reader.Read(data);
            return BinaryPrimitives.ReadInt16BigEndian(data);
        }

        public int ReadInt32()
        {
            Span<byte> data = new Span<byte>(new byte[4]);
            m_Reader.Read(data);
            return BinaryPrimitives.ReadInt32BigEndian(data);
        }

        public long ReadInt64()
        {
            Span<byte> data = new Span<byte>(new byte[8]);
            m_Reader.Read(data);
            return BinaryPrimitives.ReadInt64BigEndian(data);
        }

        public string ReadASCII(int count)
        {
            var bytes = m_Reader.ReadBytes(count);
            return Encoding.ASCII.GetString(bytes);
        }

        public string ReadPascalString(int padding)
        {
            byte stringLength = m_Reader.ReadByte();
            byte[] bytes = m_Reader.ReadBytes(stringLength);

            if (padding > 0)
            {
                int mod = (stringLength + 1) % padding;
                if (mod != 0)
                    Position += padding - mod;
            }

            return Encoding.UTF8.GetString(bytes);
        }

        public void Dispose()
        {
            m_Reader.Dispose();
        }
    }
}