using System;
using System.Runtime.InteropServices;

namespace IsometricTests {

	[StructLayoutAttribute( LayoutKind.Sequential, Pack = 1 )]
	public struct FastColour {
		public byte R, G, B, A;
		
		public static readonly FastColour Black = new FastColour( 0, 0, 0 );
		public static readonly FastColour White = new FastColour( 255, 255, 255 );
		
		public FastColour( byte r, byte g, byte b, byte a ) {
			R = r; G = g; B = b; A = a;
		}
		
		public FastColour( byte r, byte g, byte b ) {
			R = r; G = g; B = b; A = 255;
		}
		
		public static bool TryParse( string value, out FastColour result ) {
			result = White;
			if( String.IsNullOrEmpty( value ) ) {
				return false;
			}
			/*bool decimalNumber = value.IndexOf( ',' ) != 0;
			if( decimalNumber ) {
				// R,G,B
				string[] parts = value.Split( ',' );
				if( parts.Length != 3 ) {
					return false;
				}
				byte r, g, b;
				if( !Byte.TryParse( parts[0], out r ) ||
				   !Byte.TryParse( parts[1], out g ) ||
				   !Byte.TryParse( parts[2], out b ) ) {
					return false;
				}
				result = new FastColour( r, g, b );
				return true;
			} else {*/
				// #RRGGBB or RRGGBB
				return TryParseHexColor( value, out result );
			//}
		}
		
		static bool TryParseHexColor( string text, out FastColour result ) {
			result = FastColour.White;
			byte red, green, blue;
			if( !( text.Length == 6 || text.Length == 7 ) ) {
				return false;
			}
			
			int offset = text[0] == '#' ? 1 : 0;
			if( !TryParseByteHex( text, offset, out red ) ||
			   !TryParseByteHex( text, offset + 2, out green ) ||
			   !TryParseByteHex( text, offset + 4, out blue ) ) {
				return false;
			}
			
			result = new FastColour( red, green, blue, 255 );
			return true;
		}
		
		static bool TryParseByteHex( string text, int offset, out byte value ) {
			value = 0;
			byte upperNibble, lowerNibble;
			if( !TryParseCharHex( text[offset], out upperNibble )
			   || !TryParseCharHex( text[offset], out lowerNibble ) ) {
				return false;
			}
			value = (byte)( ( upperNibble << 4 ) + lowerNibble );
			return true;
		}

		static bool TryParseCharHex( char c, out byte value ) {
			value = 0;
			c = Char.ToLowerInvariant( c );
			if( c >= '0' && c <= '9' ) {
				value = (byte)( c - '0' ); // 0 = 0, 9 = 9
				return true;
			} else if( c >= 'a' && c <= 'f' ) {
				value = (byte)( c - 'a' + 10 ); // a = 10, f = 15
				return true;
			}
			return false;
		}
	}
}
