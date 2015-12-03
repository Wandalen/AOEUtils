using System;
using System.IO;
using Ionic.Zlib;

namespace AoeUtils {

	public class DatFile {
		
		public static void RunDatTest() {
			byte[] rawData = File.ReadAllBytes( "empires2_x1_p1.dat" );
			byte[] uncompressedData = DeflateStream.UncompressBuffer( rawData );
			File.WriteAllBytes( "empires2.bin", uncompressedData );
		}
	}
}
