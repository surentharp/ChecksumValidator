﻿using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Gardian.Utilities.ChecksumValidator
{

    /// <summary>
    /// </summary>
    internal static class Checksum
    {

        //-------------------------------------------------
        /// <summary>
        /// </summary>
        public static string ComputeChecksum(string sourceFile, ChecksumMethod method, Action<decimal> progressNotifier)
        {
            System.Threading.Thread.Sleep(100);
            using (var hashAlgorithm = CreateHashAlgorithm(method))
            using (var source = new TrackingStream(File.OpenRead(sourceFile), progressNotifier))
            {
                var hash = hashAlgorithm.ComputeHash(source);

                var msg = new StringBuilder(128);
                foreach (var byteValue in hash)
                {
                    msg.AppendFormat(byteValue.ToString("X2", CultureInfo.InvariantCulture));
                }
                return msg.ToString();
            }
        }


        //-------------------------------------------------
        /// <summary>
        /// </summary>
        private static HashAlgorithm CreateHashAlgorithm(ChecksumMethod method)
        {
            switch (method)
            {
                case ChecksumMethod.SHA1: return new SHA1CryptoServiceProvider();
                case ChecksumMethod.MD5: return new MD5CryptoServiceProvider();
                case ChecksumMethod.CRC32: return new CRC32();
                default: throw new NotSupportedException(string.Concat("Requested checksum method ", method, " is not supported"));
            }
        }


        //-------------------------------------------------
        /// <summary>
        /// </summary>
        private sealed class TrackingStream : Stream
        {
            public TrackingStream(Stream trackedStream, Action<decimal> progressNotifier)
            {
                if (trackedStream == null) { throw new ArgumentNullException("trackedStream"); }
                this._trackedStream = trackedStream;
                this._progressNotifier = progressNotifier;
            }
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing)
                {
                    this._trackedStream.Dispose();
                }
            }
            public override void Flush()
            {
                throw new NotImplementedException();
            }
            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }
            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }
            public override int Read(byte[] buffer, int offset, int count)
            {
                var ret = this._trackedStream.Read(buffer, offset, count);

                if (this._progressNotifier != null)
                {
                    var position = this._trackedStream.Position;
                    const long mb = 2L << 19;
                    var megabyte = position / mb;
                    if (megabyte > this._lastMegabyte)
                    {
                        this._lastMegabyte = megabyte;
                        var percentage = ((decimal)position / this._trackedStream.Length);
                        this._progressNotifier(percentage);
                    }
                }
                return ret;
            }
            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }
            public override bool CanRead
            {
                get { throw new NotImplementedException(); }
            }
            public override bool CanSeek
            {
                get { throw new NotImplementedException(); }
            }
            public override bool CanWrite
            {
                get { throw new NotImplementedException(); }
            }
            public override long Length
            {
                get { throw new NotImplementedException(); }
            }
            public override long Position
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            private long _lastMegabyte;
            private readonly Action<decimal> _progressNotifier;
            private readonly Stream _trackedStream;

        }

    }

}