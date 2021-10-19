using System;
using System.IO;
using System.Text;

namespace BadNews.Repositories.News
{
    public class SeekableStreamTextReader : TextReader
    {
        private const int DefaultBufferSize = 1024;
        private const int MinBufferSize = 128;

        private Stream stream;
        private Encoding encoding;
        private Decoder decoder;

        private byte[] byteBuffer;
        // Record the number of valid bytes in the byteBuffer, for a few checks.
        private int byteLen;

        private char[] charBuffer;
        private int charLen;
        private int charPos;

        private bool isBlocked;

        public SeekableStreamTextReader(Stream stream, Encoding encoding, int bufferSize = DefaultBufferSize)
        {
            if (stream == null || encoding == null)
                throw new ArgumentNullException((stream == null ? "stream" : "encoding"));
            if (!stream.CanRead)
                throw new ArgumentException("Can't read stream");
            if (!stream.CanSeek)
                throw new ArgumentException("Can't seek stream");
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize");

            this.stream = stream;
            this.encoding = encoding;
            decoder = encoding.GetDecoder();

            if (bufferSize < MinBufferSize) bufferSize = MinBufferSize;
            byteBuffer = new byte[bufferSize];
            byteLen = 0;

            var maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
            charBuffer = new char[maxCharsPerBuffer];

            isBlocked = false;
        }

        public Encoding CurrentEncoding => encoding;
        public Stream BaseStream => stream;
        public long UsedBytes => stream.Position - byteLen + encoding.GetBytes(charBuffer, 0, charPos).Length;

        public void Seek(long offset, SeekOrigin seekOrigin)
        {
            stream.Seek(offset, seekOrigin);
            DiscardBufferedData();
        }

        private void DiscardBufferedData()
        {
            byteLen = 0;
            charLen = 0;
            charPos = 0;
            decoder = encoding.GetDecoder();
            isBlocked = false;
        }

        public override int Peek()
        {
            if (stream == null)
                throw new InvalidOperationException("Reader closed");

            if (charPos == charLen)
            {
                if (isBlocked || ReadBuffer() == 0) return -1;
            }
            return charBuffer[charPos];
        }

        public override int Read()
        {
            if (stream == null)
                throw new InvalidOperationException("Reader closed");

            if (charPos == charLen)
            {
                if (ReadBuffer() == 0) return -1;
            }
            int result = charBuffer[charPos];
            charPos++;
            return result;
        }

        public override string ReadLine()
        {
            if (stream == null)
                throw new InvalidOperationException("Reader closed");

            if (charPos == charLen)
            {
                if (ReadBuffer() == 0) return null;
            }

            StringBuilder sb = null;
            do
            {
                int i = charPos;
                do
                {
                    char ch = charBuffer[i];
                    // Note the following common line feed chars:
                    // \n - UNIX   \r\n - DOS   \r - Mac
                    if (ch == '\r' || ch == '\n')
                    {
                        string s;
                        if (sb != null)
                        {
                            sb.Append(charBuffer, charPos, i - charPos);
                            s = sb.ToString();
                        }
                        else
                        {
                            s = new string(charBuffer, charPos, i - charPos);
                        }
                        charPos = i + 1;
                        if (ch == '\r' && (charPos < charLen || ReadBuffer() > 0))
                        {
                            if (charBuffer[charPos] == '\n') charPos++;
                        }
                        return s;
                    }
                    i++;
                } while (i < charLen);
                i = charLen - charPos;
                if (sb == null) sb = new StringBuilder(i + 80);
                sb.Append(charBuffer, charPos, i);
            } while (ReadBuffer() > 0);
            return sb.ToString();
        }

        private int ReadBuffer()
        {
            charLen = 0;
            charPos = 0;
            byteLen = 0;

            do
            {
                byteLen = stream.Read(byteBuffer, 0, byteBuffer.Length);
                if (byteLen == 0)  // We're at EOF
                    return charLen;

                isBlocked = (byteLen < byteBuffer.Length);

                charLen += decoder.GetChars(byteBuffer, 0, byteLen, charBuffer, charLen);
            } while (charLen == 0);
            return charLen;
        }

        public override void Close()
        {
            Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (stream != null))
                    stream.Close();
            }
            finally
            {
                if (stream != null)
                {
                    stream = null;
                    byteBuffer = null;
                    charBuffer = null;
                    charPos = 0;
                    charLen = 0;
                    base.Dispose(disposing);
                }
            }
        }
    }
}
