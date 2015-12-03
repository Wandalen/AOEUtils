using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace AoeUtils {
	
	public class Palette {
		
		public Color[] colours;
		public int[] coloursRaw;
		
		private Palette() {
		}
		
		public static Palette FromFile( string path ) {
			Palette palette = new Palette();
			using( StreamReader reader = new StreamReader( path, Encoding.ASCII ) ) {
				
				string identifer = reader.ReadLine();
				if( identifer != "JASC-PAL" ) {
					throw new InvalidDataException( "Invalid palette identifier." );
				}
				string version = reader.ReadLine();
				if( version != "0100" ) {
					throw new NotImplementedException( "Unsupported version: " + version );
				}
				
				int count = Int32.Parse( reader.ReadLine() );
				Color[] colours = new Color[count];
				int[] coloursRaw = new int[count];
				for( int i = 0; i < count; i++ ) {
					string line = reader.ReadLine();
					string[] parts = line.Split( ' ' );
					byte r = Byte.Parse( parts[0] );
					byte g = Byte.Parse( parts[1] );
					byte b = Byte.Parse( parts[2] );
					colours[i] = Color.FromArgb( r, g, b );
					coloursRaw[i] = ( 255 << 24 ) + ( r << 16 ) + ( g << 8 ) + b;
				}
				palette.coloursRaw = coloursRaw;
				palette.colours = colours;
			}
			return palette;
		}
	}
}
