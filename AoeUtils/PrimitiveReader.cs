using System;
using System.IO;
using System.Text;

namespace AoeUtils {
	
	/// <summary> Class that reads primitives types,
	/// in either little endian or big endian format. </summary>
	public class PrimitiveReader : IDisposable {
		public Stream Stream;
		protected byte[] pbuffer; // Buffer for primitives such as bytes, ints, long, etc.
		public bool BigEndian;
		public bool CloseUnderlyingStream = true;
		protected bool disposed = false;

		public PrimitiveReader( Stream input ) {
			if( input == null ) throw new ArgumentNullException( "input" );
			if( !input.CanRead ) throw new ArgumentException( "Stream must be readable." );
			Stream = input;
			pbuffer = new byte[8]; // Double, ulong and long types are 8 bytes long.
		}
		
		public PrimitiveReader( string path ) {
			if( path == null ) throw new ArgumentNullException( "path" );
			Stream = File.OpenRead( path );
			pbuffer = new byte[8];
		}
		
		public PrimitiveReader( byte[] array ) {
			if( array == null ) throw new ArgumentNullException( "array" );
			Stream = new MemoryStream( array );
			pbuffer = new byte[8];
		}

		protected void Dispose( bool disposing ) {
			if( disposing && CloseUnderlyingStream ) {
				Stream.Close();
			}
			Stream = null;
			disposed = true;
		}
		
		public void Dispose() {
			Dispose( true );
		}

		/// <summary> Reads an unsigned integer 8 bits long. </summary>
		/// <returns> True if the read value is not equal to 0. </returns>
		public bool ReadBoolean() {
			return ReadUInt8() != 0;
		}

		/// <summary> Reads an unsigned integer 8 bits long. </summary>
		/// <returns> An unsigned 8 bit integer read from the stream. </returns>
		public byte ReadUInt8() {
			int byteValue = Stream.ReadByte();
			if( byteValue == -1 ) {
				throw new EndOfStreamException();
			}
			return (byte)byteValue;
		}

		/// <summary> Reads an signed integer 8 bits long. </summary>
		/// <returns> An signed 8 bit integer read from the stream. </returns>
		public sbyte ReadInt8() {
			return (sbyte)ReadUInt8();
		}

		/// <summary> Reads an signed integer 16 bits long. </summary>
		/// <returns> An signed 16 bit integer read from the stream. </returns>
		public short ReadInt16() {
			FillPrimitiveBuffer( 2 );
			if( BigEndian )
				return (short)( pbuffer[0] << 8 | pbuffer[1] );
			return (short)( pbuffer[0] | pbuffer[1] << 8 );
		}

		/// <summary> Reads an unsigned integer 16 bits long. </summary>
		/// <returns> An unsigned 16 bit integer read from the stream. </returns>
		public ushort ReadUInt16() {
			FillPrimitiveBuffer( 2 );
			if( BigEndian )
				return (ushort)( pbuffer[0] << 8 | pbuffer[1] );
			return (ushort)( pbuffer[0] | pbuffer[1] << 8 );
		}

		/// <summary> Reads an signed integer 32 bits long. </summary>
		/// <returns> An signed 32 bit integer read from the stream. </returns>
		public int ReadInt32() {
			FillPrimitiveBuffer( 4 );
			if( BigEndian )
				return pbuffer[0] << 24 | pbuffer[1] << 16 | pbuffer[2] << 8 | pbuffer[3];
			return pbuffer[0] | pbuffer[1] << 8 | pbuffer[2] << 16 | pbuffer[3] << 24;
		}

		/// <summary> Reads an unsigned integer 32 bits long. </summary>
		/// <returns> An unsigned 32 bit integer read from the stream. </returns>
		public uint ReadUInt32() {
			FillPrimitiveBuffer( 4 );
			if( BigEndian )
				return (uint)( pbuffer[0] << 24 | pbuffer[1] << 16 | pbuffer[2] << 8 | pbuffer[3] );
			return (uint)( pbuffer[0] | pbuffer[1] << 8 | pbuffer[2] << 16 | pbuffer[3] << 24 );
		}
		
		/// <summary> Reads an signed integer 64 bits long. </summary>
		/// <returns> An signed 64 bit integer read from the stream. </returns>
		public long ReadInt64() {
			FillPrimitiveBuffer( 8 );
			if( BigEndian )
				return (long)( (ulong)pbuffer[0] << 56 | (ulong)pbuffer[1] << 48 | (ulong)pbuffer[2] << 40 | (ulong)pbuffer[3] << 32 |
				              (ulong)pbuffer[4] << 24 | (ulong)pbuffer[5] << 16 | (ulong)pbuffer[6] << 8 | (ulong)pbuffer[7] );
			return (long)( (ulong)pbuffer[0] | (ulong)pbuffer[1] << 8 | (ulong)pbuffer[2] << 16 | (ulong)pbuffer[3] << 24 |
			              (ulong)pbuffer[4] << 32 | (ulong)pbuffer[5] << 40 | (ulong)pbuffer[6] << 48 | (ulong)pbuffer[7] << 56 );
		}

		/// <summary> Reads an unsigned integer 64 bits long. </summary>
		/// <returns> An unsigned 64 bit integer read from the stream. </returns>
		public ulong ReadUInt64() {
			FillPrimitiveBuffer( 8 );
			if( BigEndian )
				return (ulong)( (ulong)pbuffer[0] << 56 | (ulong)pbuffer[1] << 48 | (ulong)pbuffer[2] << 40 | (ulong)pbuffer[3] << 32 |
				               (ulong)pbuffer[4] << 24 | (ulong)pbuffer[5] << 16 | (ulong)pbuffer[6] << 8 | (ulong)pbuffer[7] );
			return (ulong)( (ulong)pbuffer[0] | (ulong)pbuffer[1] << 8 | (ulong)pbuffer[2] << 16 | (ulong)pbuffer[3] << 24 |
			               (ulong)pbuffer[4] << 32 | (ulong)pbuffer[5] << 40 | (ulong)pbuffer[6] << 48 | (ulong)pbuffer[7] << 56 );
		}

		/// <summary> Reads a floating point value 32 bits long. </summary>
		/// <returns> An 32 bit floating point value read from the stream. </returns>
		public unsafe float ReadFloat32() {
			uint value = ReadUInt32();
			return *(float*)&value;
		}

		/// <summary> Reads a floating point value 64 bits long. </summary>
		/// <returns> An 64 bit floating point value read from the stream. </returns>
		public unsafe double ReadFloat64() {
			ulong value = ReadUInt64();
			return *(double*)&value;
		}
		
		/// <summary> Attempts to fully read the requested number of bytes,
		/// throwing an exception if the given number of bytes could not read. </summary>
		/// <param name="count"> The number bytes to read. </param>
		/// <exception cref="System.ArgumentOutOfRangeException"> count is less than or equal to 0. </exception>
		/// <exception cref="System.IO.EndOfStreamException"> The end of the stream was reached. </exception>
		/// <returns> A byte array containing data read from the underlying stream. </returns>
		public byte[] ReadBytes/*Fully*/( int count ) {
			if( disposed ) {
				throw new ObjectDisposedException( "this" );
			}
			// Exit early if the count is 0, in case the underlying stream has issues with reading 0 bytes.
			if( count == 0 ) {
				return new byte[0];
			}
			byte[] buffer = new byte[count];
			int totalRead = 0;
			do {
				int read = Stream.Read( buffer, totalRead, count );
				if( read == 0 ) { // End of stream reached.
					break;
				}
				totalRead += read;
				count -= read;
			} while( count > 0 );
			
			// The end of stream was reached and not enough bytes were read.
			if( totalRead != buffer.Length ) {
				throw new EndOfStreamException();
			}
			return buffer;
		}
		
		public void FillBuffer( byte[] buffer, int count ) {
			if( disposed ) {
				throw new ObjectDisposedException( "this" );
			}
			if( count > buffer.Length )
				throw new ArgumentOutOfRangeException( "count" );
			if( count == 0 )
				return;
			
			int totalRead = 0;
			do {
				int read = Stream.Read( buffer, totalRead, count );
				if( read == 0 ) { // End of stream reached.
					break;
				}
				totalRead += read;
				count -= read;
			} while( count > 0 );
			
			// The end of stream was reached and not enough bytes were read.
			if( count > 0 ) {
				throw new EndOfStreamException();
			}
		}
		
		public string ReadASCIIString( int bytes ) {
			if( bytes == 0 ) return String.Empty;
			return Encoding.ASCII.GetString( ReadBytes( bytes ) );
		}
		
		/// <summary> Returns a string, decoding the underlying bytes as specified by the given encoding. </summary>
		/// <param name="bytes"> The number of *bytes* that make up the string, not characters. </param>
		/// <param name="encoding"> Encoding class used to decode the raw bytes into a string. </param>
		/// <returns> The string read. </returns>
		public string ReadString( int bytes, Encoding encoding ) {
			if( encoding == null ) throw new ArgumentNullException( "encoding" );
			if( bytes == 0 ) return String.Empty;
			return encoding.GetString( ReadBytes( bytes ) );
		}
		
		protected void FillPrimitiveBuffer( int numBytes ) {
			if( disposed ) {
				throw new ObjectDisposedException( "PrimitiveReader" );
			}
			if( numBytes == 1 ) {
				int byteValue = Stream.ReadByte();
				if( byteValue == -1 ) {
					throw new EndOfStreamException();
				}
				pbuffer[0] = (byte)byteValue;
				return;
			}
			byte[] buffer = ReadBytes( numBytes );
			if( buffer.Length < numBytes ) {
				throw new EndOfStreamException();
			}
			Buffer.BlockCopy( buffer, 0, pbuffer, 0, buffer.Length );
		}
		
		public Guid ReadGuid() {
			return new Guid( ReadBytes( 16 ) );
		}
		
		public string ReadAsciiFixedString( int fixedSize ) {
			return ReadFixedString( fixedSize, Encoding.ASCII );
		}
		
		public string ReadFixedString( int fixedSize, Encoding encoding ) {
			byte[] data = ReadBytes( fixedSize );
			int lastNullIndex = Array.IndexOf( data, (byte)0 ); // Cast here otherwise the non generic method is called, which always returns -1.
			int length = lastNullIndex == -1 ? data.Length : lastNullIndex;
			return encoding.GetString( data, 0, length );
		}
		
		public uint[] ReadUInt32Array( int count ) {
			uint[] data = new uint[count];
			// TODO: Should we use Buffer.BlockCopy and byte read instead?
			// May not be a good idea, as I don't have any big-endian machines
			// to test compatibility with.
			for( int i = 0; i < data.Length; i++ ) {
				data[i] = ReadUInt32();
			}
			return data;
		}
		
		public float[] ReadFloat32Array( int count ) {
			float[] data = new float[count];
			for( int i = 0; i < data.Length; i++ ) {
				data[i] = ReadFloat32();
			}
			return data;
		}
		
		// Note that this is probably a lot slower for reading arrays of unsigned bytes,
		// as streams have a specialised method for reading arrays of bytes.
		public T[] ReadArray<T>( int elementsCount, Func<PrimitiveReader, T> constructor ) {
			T[] data = new T[elementsCount];
			for( int i = 0; i < data.Length; i++ ) {
				data[i] = constructor( this );
			}
			return data;
		}
		
		/// <summary> Skips the specified number of bytes. </summary>
		/// <remarks> This is not the same as calling ReadBytes(), as
		/// this method reuses a small buffer of 4096 bytes to
		/// use as little memory as possible. </remarks>
		/// <param name="length"> Number of bytes to skip. </param>
		public void SkipData( long length ) {
			int skipSize = (int)Math.Min( 4096, length );
			byte[] buffer = new byte[skipSize];
			
			while( length > 0 ) {
				FillBuffer( buffer, skipSize );
				length -= 4096;
				skipSize = (int)Math.Min( 4096, length );
			}
		}
		
		public long SeekAbsolute( long offset ) {
			return Stream.Seek( offset, SeekOrigin.Begin );
		}
		
		public long Position {
			get { return Stream.Position; }
		}
		
		public long Length {
			get { return Stream.Length; }
		}
		
		public int ReadTimeout {
			get { return Stream.ReadTimeout; }
		}
	}
}
