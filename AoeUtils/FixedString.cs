using System;
using System.Text;

namespace AoeUtils {
	
	/// <summary>  Represented a string with a fixed size. </summary>
	public sealed class FixedString {
		
		readonly int maxSize;
		string rawValue;
		
		public FixedString( string value, int maxLength ) {
			rawValue = value;
			maxSize = maxLength;
		}
		
		public FixedString( string value ) {
			if( String.IsNullOrEmpty( value ) )
				throw new ArgumentException( "Invalid string provided." );
			rawValue = value;
			maxSize = value.Length;
		}
		
		public int MaxLength {
			get { return maxSize; }
		}
		
		public string Value {
			get { return rawValue; }
			set {
				if( value != null ) {
					if( value.Length > maxSize )
						throw new ArgumentOutOfRangeException( "value is longer than " + maxSize + " chars." );
					rawValue = value;
				} else {
					rawValue = null;
				}
			}
		}
		
		public byte[] ToBytes( Encoding encoding ) {
			byte[] data = new byte[maxSize];
			if( rawValue != null ) {
				encoding.GetBytes( rawValue, 0, rawValue.Length, data, 0 );
			}
			return data;
		}
		
		public static implicit operator string( FixedString value ) {
			return value.rawValue;
		}
		
		public static explicit operator FixedString( string value ) {
			return new FixedString( value );
		}
		
	}
}
