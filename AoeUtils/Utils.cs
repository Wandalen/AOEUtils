using System;
using System.IO;
using System.Text;

namespace AoeUtils {

	public static class Utils {

		public static void CheckFixedString( string value, int maxSize ) {
			if( value == null ) throw new ArgumentNullException( "value" );
			if( value.Length > maxSize ) throw new ArgumentOutOfRangeException( "value", "String can only be " + maxSize + " characters at maximum." );
		}
		
		public static void WriteFixedString( Stream output, string value, int fixedSize ) {
			if( output == null ) throw new ArgumentNullException( "output" );
			if( value == null ) throw new ArgumentNullException( "value" );
			if( value.Length > fixedSize ) throw new ArgumentOutOfRangeException( "value is greater than fixed size." );
			
			byte[] data = new byte[fixedSize]; // Initalised to all '\0'.
			TextEncoding.GetBytes( value, 0, value.Length, data, 0 );
			output.Write( data, 0, data.Length );
		}
		
		public static void WriteAsciiString( Stream output, string value ) {
			if( output == null ) throw new ArgumentNullException( "output" );
			if( value == null ) throw new ArgumentNullException( "value" );
			
			byte[] data = TextEncoding.GetBytes( value );
			output.Write( data, 0, data.Length );
		}
		
		public static void WriteFixedString( BinaryWriter output, string value, int fixedSize ) {
			WriteFixedString( output.BaseStream, value, fixedSize );
		}
		
		public static string ReadUInt16LengthPrefixedString( PrimitiveReader reader ) {
			ushort length = reader.ReadUInt16();
			return reader.ReadString( length, TextEncoding );
		}
		
		public static string ReadUInt32LengthPrefixedString( PrimitiveReader reader ) {
			uint length = reader.ReadUInt32();
			if( length == 0xFFFFFFFF ) // -1
				return String.Empty;	
			return reader.ReadString( (int)length, TextEncoding );
		}
		
		public static void WriteUInt16LengthPrefixedString( BinaryWriter writer, string value ) {
			if( value == null ) {
				WriteUInt16LengthPrefixedString( writer, String.Empty );
				return;
			}
			if( value.Length > ushort.MaxValue )
				throw new ArgumentOutOfRangeException( "value has more than 65,535 chars." );
			
			writer.Write( (ushort)value.Length );
			writer.Write( TextEncoding.GetBytes( value ) );
		}
		
		public static void WriteUInt32LengthPrefixedString( BinaryWriter writer, string value ) {
			if( value == null ) {
				WriteUInt32LengthPrefixedString( writer, String.Empty );
				return;
			}
			writer.Write( (uint)value.Length );
			writer.Write( TextEncoding.GetBytes( value ) );
		}
		
		static readonly DateTime unixEpoch = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
		
		public static uint GetCurrentUnixTime() {
			return (uint)( DateTime.UtcNow - unixEpoch ).TotalSeconds;
		}
		
		public static DateTime FromUnixTime( uint timespan ) {
			return unixEpoch.AddSeconds(  timespan );
		}	
				
		public static string GetFixedString( byte[] data, int offset, int count ) {
			int nullIndex = Array.IndexOf( data, (byte)0, offset, count ); // Cast here otherwise the non generic method is called, which always returns -1.
			int length = nullIndex == -1 ? count : nullIndex - offset;
			return TextEncoding.GetString( data, offset, length );
		}
		
		static Encoding textEncoding;
		static void InitialiseEncoding() {
			try {
				textEncoding = Encoding.GetEncoding( "iso8859-1" );
			} catch {
				Console.WriteLine( "ISO/IEC 8859-1 encoding not found. Trying Windows-1252 encoding fallback..." );
				try {
					textEncoding = Encoding.GetEncoding( "Windows-1252" );
				} catch {
					Console.WriteLine( "Windows-1252 encoding not found. Setting to ASCII encoding..." );
					textEncoding = Encoding.ASCII;
				}
			}
		}
		
		public static Encoding TextEncoding {
			get {
				if( textEncoding == null )
					InitialiseEncoding();
				return textEncoding;
			}
		}
	}
}
