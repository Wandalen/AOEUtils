using System;
using System.Diagnostics;
using System.IO;
using AoeUtils;
using Ionic.Zlib;

namespace aoetest {
	
	class Program {
		
		/*static void ReadAndDump( string input, string output ) {
			output = Path.Combine( "jk", output );
			File.WriteAllBytes( output, DeflateStream.UncompressBuffer( File.ReadAllBytes( input ) ) );
		}
		
		static void Undump( string input, string output ) {
			input = Path.Combine( "jk", input );
			output = Path.Combine( "jk", output );
			File.WriteAllBytes( output, DeflateStream.CompressBuffer( File.ReadAllBytes( input ) ) );
		}
		
		public static void Main( string[] args ) {
			
			
			/*ProfileFile profile = ProfileFile.FromStream(
			SaveGameFile f = new SaveGameFile();
			f.ReadData( Path.Combine( "ffff", "savedumpp1.bin" ) );
			
			
			string scenDirectory = Path.Combine( "D:", "age of empires", "scenario" );
			foreach( string path1 in Directory.GetFiles( scenDirectory, "*.sc*", SearchOption.AllDirectories ) ) {
				try {
					uint unknown2 = ScenarioReader.ReadScenarioUnknown( path1 );
					if( unknown2 != 0 )
						Debug.WriteLine( path1 + "," + unknown2 );
				} catch { }
			}
			Debug.WriteLine( "F" );
			using( PrimitiveReader reader1 = new PrimitiveReader( Path.Combine( "ffff", "playerinfo.bin" ) ) ) {
				ProfileFile profile = ProfileFile.FromStream( reader1 );
				System.Diagnostics.Debugger.Break();
			}*/
			
			/*string path = Path.Combine( "D:", "age of empires 2", "player.nfp" );
			ReadAndDump( Path.Combine( "D:", "age of empires 2", "savegame", "_vvv11111.gam" ), "save2.bin" );
			Undump( "save3.bin", "bvilly.gam" );
			using( FileStream fsss = File.Create( Path.Combine( "jk", "float.bin" ) ) ) {
				BinaryWriter w = new BinaryWriter( fsss );
				w.Write( 85f );
			}
			
			ProfileFile.FromStream( new PrimitiveReader( Path.Combine( "jk", "cpus.bin" ) ) );
			
			while( true ) {
				string value = Console.ReadLine();
				if( value.StartsWith( "#comp" ) ) {
					try {
						CompareFiles( value );
					} catch {
						Console.WriteLine( "Error while comparing files!" );
					}
					continue;
				}
				
				if( value.StartsWith( "#dump" ) ) {
					string[] paths = value.Split( ' ' );
					using( FileStream f1 = File.Create( Path.Combine( "jk", "float.bin" ) ) ) {
						new BinaryWriter( f1 ).Write( float.Parse( paths[1] ) );
					}
					continue;
				}
				
				if( !value.EndsWith( ".bin" ) ) {
					value += ".bin";
				}
				ReadAndDump( path, value );
				Console.WriteLine( "Decompressed given file." );
			}
			//ReadAndDump( "customVictTests.gmx", "aoeGlobalVictory.bin" );
		}
		
		static void CompareFiles( string value ) {
			string[] paths = value.Split( ' ' );
			for( int i = 0; i < paths.Length; i++ ) {
				if( !paths[i].EndsWith( ".bin" ) ) {
					paths[i] += ".bin";
				}
			}
			int differencesCount = 0;
			byte[] bytes1 = File.ReadAllBytes( Path.Combine( "jk", paths[1] ) );
			byte[] bytes2 = File.ReadAllBytes( Path.Combine( "jk", paths[2] ) );
			StreamWriter log = new StreamWriter( "complog.txt" );
			log.WriteLine( paths[1] + "," + paths[2] );
			for( int i = 0; i < bytes1.Length; i++ ) {
				byte val1 = bytes1[i];
				byte val2 = bytes2[i];
				if( val1 != val2 ) {
					string line = i + "/" + i.ToString( "X4" ) + ":" + val1 + "," + val2 + ":" + (char)val1 + "," + (char)val2;
					log.WriteLine( line );
					Console.WriteLine( line );
					differencesCount++;
				}
			}
			log.Close();
			Console.WriteLine( "Compared files, found {0} different bytes.", differencesCount );
		}
		/*public static void Main( string[] args ) {
			// dts.scx     test5.scx
			//using( FileStream fs = File.Open( "cam1.cpn", FileMode.Open, FileAccess.Read, FileShare.Read ) ) {
			//PrimitiveReader br = new PrimitiveReader( fs );
			//ScenarioReader.ReadScenario( br );
			//Campaign campaign = Campaign.FromStream( fs );
			//throw new Exception();
			//}
			
			//using( var fs = File.OpenRead( "gte7.scx" ) ) {
			//using( var fs = File.OpenRead( "g1.scx" ) ) {
			//	Scenario.FromStream( fs );
			//}
			
			//using( var fs = File.OpenRead( "g2.scx" ) ) {
			//	Scenario.FromStream( fs );
			//}
			
			//using( var fs = File.OpenRead( "gamedata_x1.drs" ) ) {
			//	DrsFile.FromStream( fs );
			//}
			
			//using( var fs = File.OpenRead( "c1s1_beg.mm" ) ) {
			//	MmFile.FromStream( fs );
			//}
			
			DatFile.RunDatTest();
			
			//ScenarioReader.HexEditTest( 17 );
			
			//using( var fs = File.OpenRead( "aocdemo.scx" ) ) {
			//	PrimitiveReader reader = new PrimitiveReader( fs );
			//	ScenarioReader.ReadScenario( reader );
			//}
			
			//using( var fs = File.OpenRead( "vics2.scx" ) ) {
			//	PrimitiveReader reader = new PrimitiveReader( fs );
			//	ScenarioReader.ReadScenario( reader );
			//}
			Console.WriteLine( "FFF" );
			Console.ReadKey( true );
		}*/
	}
}