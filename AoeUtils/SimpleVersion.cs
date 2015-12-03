using System;

namespace AoeUtils {
	
	public class SimpleVersion {
		
		public string MajorVersion { get; private set; }
		
		public int Major { get; private set; }
		
		public string MinorVersion { get; private set; }
		
		public int Minor { get; private set; }
		
		public string Version { get; private set; }
		
		const int size = 4;
		
		public SimpleVersion( string version ) {
			if( version.Length != size )
				throw new ArgumentException( "version can only be 4 characters." );
			if( version[1] != '.' )
				throw new ArgumentException( "Second character must be a period." );
			
			MajorVersion = new String( new [] { version[0] } );
			Major = ParseChar( version[0] );
			Minor = ParseChar( version[2] ) * 10 + ParseChar( version[3] );
			MinorVersion = new String( new [] { version[2], version[3] } );
			Version = version;
		}
		
		static int ParseChar( char value ) {
			return value - '0';
		}
		
	}
}
