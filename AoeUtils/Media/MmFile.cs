using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AoeUtils {
	
	public sealed class MmFile {
		
		public class MmElement {
			public uint Id;
			public string Type;
			public uint X;
			public uint Y;
			public uint Width;
			public uint Height;
			public uint StartTime;
			public uint DisplayTime;
			public uint Frame;
			public string Value;
			public byte TextColourRed, TextColourGreen, TextColourBlue;
			
			internal static MmElement ParseLine( string line ) {
				if( String.IsNullOrWhiteSpace( line ) )
					throw new ArgumentException( "Invalid line provided." );
				
				int offset = 0;
				MmElement element = new MmElement();
				
				// I'm not entirely sure what purpose this serves..
				// TODO: I think this signifies unused? something like that.
				if( ReadToken( line, ref offset ) != "rem" ) {
					offset = 0;
				}
				
				element.Id = UInt32.Parse( ReadToken( line, ref offset ) );
				element.Type = ReadToken( line, ref offset );
				element.X = UInt32.Parse( ReadToken( line, ref offset ) );
				element.Y = UInt32.Parse( ReadToken( line, ref offset ) );
				element.Width = UInt32.Parse( ReadToken( line, ref offset ) );
				element.Height = UInt32.Parse( ReadToken( line, ref offset ) );
				element.StartTime = UInt32.Parse( ReadToken( line, ref offset ) );
				element.DisplayTime = UInt32.Parse( ReadToken( line, ref offset ) );
				element.Frame = UInt32.Parse( ReadToken( line, ref offset ) );
				element.Value = ReadToken( line, ref offset );
				element.TextColourRed = Byte.Parse( ReadToken( line, ref offset ) );
				element.TextColourGreen = Byte.Parse( ReadToken( line, ref offset ) );
				element.TextColourBlue = Byte.Parse( ReadToken( line, ref offset ) );
				return element;
			}
		}
		
		static string ReadToken( string value, ref int offset ) {
			// Find the first character of the token. Multiple spaces can be used to separate tokens.
			while( value[offset] == ' ' ) {
				offset++;
			}
			
			// String token
			if( value[offset] == '"' ) {
				offset++;
				int nextQuote = value.IndexOf( '"', offset );
				if( nextQuote == -1 ) {
					throw new InvalidOperationException( "String values require an ending quote." );
				}
				int length = nextQuote - offset;
				string token = value.Substring( offset, length );
				offset += length + 1;
				return token;
			}
			// Normal token
			else {
				int nextSeparator = value.IndexOf( ' ', offset );
				int length = nextSeparator == -1 ? value.Length - offset : nextSeparator - offset;
				
				string token = value.Substring( offset, length );
				offset += length;
				return token;
			}
		}
		
		List<MmElement> elements = new List<MmElement>();
		
		public static MmFile FromStream( Stream stream ) {
			// ".mm" files are an ASCII text based format.
			StreamReader reader = new StreamReader( stream, Encoding.ASCII );
			
			string header = reader.ReadLine();
			string line;		
			List<MmElement> elements = new List<MmElement>();
			
			while( ( line = reader.ReadLine() ) != null ) {
				if( String.IsNullOrWhiteSpace( line ) )
					continue;
				
				elements.Add( MmElement.ParseLine( line ) );
			}
			throw new Exception();
			return null;
		}
	}
}
