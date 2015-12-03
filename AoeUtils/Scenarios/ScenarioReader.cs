//#define DEBUG_ALPHA
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;

namespace AoeUtils {
	
	public static class ScenarioReader {
		
		static int GetDecimalValue( char value ) {
			return (int)( value - '0' );
		}
		
		static readonly DateTime epoch = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
		static string version1;
		static Version vv1;
		static Version vv2;
		
		public static void ReadFile( string file ) {
			using( FileStream fs = File.OpenRead( file ) ) {
				PrimitiveReader reader = new PrimitiveReader( fs );
				ReadScenario( reader );
			}
		}
		
		public static void ReadScenario( PrimitiveReader reader ) {
			string version = reader.ReadASCIIString( 4 );
			version1 = version;
			majorVersion = GetDecimalValue( version[0] );
			minorVersion = GetDecimalValue( version[2] ) * 10 + GetDecimalValue( version[3] );
			vv1 = Version.Parse( version );
			
			uint dataOffset = reader.ReadUInt32();
			uint headerVersion = reader.ReadUInt32();
			if( headerVersion >= 2 ) {
				uint timeStamp = reader.ReadUInt32();
				DateTime utcTime = epoch.AddSeconds( timeStamp );
				DateTime localTime = utcTime.ToLocalTime();
			}
			
			uint scenarioIntroductionLength = reader.ReadUInt32();
			string scenarioIntroduction = "";
			if( scenarioIntroductionLength > 0 ) {
				int length = (int)scenarioIntroductionLength;
				scenarioIntroduction = reader.ReadASCIIString( length );
			}
			uint unknown = reader.ReadUInt32();
			uint playersCount = reader.ReadUInt32();
			
			// Now read the compressed data.
			//reader.Stream.Seek( dataOffset, SeekOrigin.Begin );
			ReadCompressedData( reader );
			throw new Exception();
		}
		
		
		public static uint ReadScenarioUnknown( string file ) {
			PrimitiveReader reader = new PrimitiveReader( file );
			string version = reader.ReadASCIIString( 4 );
			uint dataOffset = reader.ReadUInt32();
			uint headerVersion = reader.ReadUInt32();
			
			if( headerVersion >= 2 ) {
				uint timeStamp = reader.ReadUInt32();
			}
			
			uint scenarioIntroductionLength = reader.ReadUInt32();
			if( scenarioIntroductionLength > 0 ) {
				int length = (int)scenarioIntroductionLength;
				reader.ReadASCIIString( length );
			}
			uint unknown = reader.ReadUInt32();
			reader.Dispose();
			return unknown;
		}
		
		
		public static void HexEditTestsssss() {
			for( byte terrainType = 0; terrainType <= 30; terrainType++ ) {
				HexEditTest( terrainType );
				File.Copy( "hex.scx", "hex" + terrainType + ".scx" );
			}
		}
		
		public static void HexEditTest( byte terrainType ) {
			if( File.Exists( "hex.scx" ) ) {
				File.Delete( "hex.scx" );
			}
			File.Copy( "ff.scx", "hex.scx" );
			using( FileStream fs = File.Open( "hex.scx", FileMode.Open ) ) {
				PrimitiveReader reader = new PrimitiveReader( fs );
				reader.ReadBytes( 4 ); // version
				uint dataOffset = reader.ReadUInt32();
				fs.SetLength( dataOffset + 8 );
				fs.Seek( 0, SeekOrigin.End );
				/*using( FileStream fs2 = File.Open( "test.bin", FileMode.Open ) ) {
					fs2.Seek( 19337, SeekOrigin.Begin );
					for( int i = 0; i < 1600; i++ ) {
						fs2.WriteByte( terrainType );
						fs2.ReadByte();
						fs2.ReadByte();
					}
				}*/
				byte[] uncompressedData = File.ReadAllBytes( "testff.bin" );
				byte[] data = Ionic.Zlib.DeflateStream.CompressBuffer( uncompressedData );
				fs.Write( data, 0, data.Length );
			}
		}
		
		static int majorVersion;
		static int minorVersion;
		static int majorVersion2;
		static int minorVersion2;
		
		
		const int playersCount = 16;
		const int playerNameLength = 256;
		
		//=======================================
		//| Uncompressed | File Header          |
		//---------------------------------------
		
		static void ReadCompressedData( PrimitiveReader reader ) {
			Stream stream = reader.Stream;
			string path = ( stream as FileStream ).Name;
			path = Path.ChangeExtension( path, ".bin" );
			
			int compressedLength = (int)( stream.Length - stream.Position );
			byte[] buffer = reader.ReadBytes( compressedLength );
			byte[] data = Ionic.Zlib.DeflateStream.UncompressBuffer( buffer );
			File.WriteAllBytes( path, data );
			reader.Stream = new MemoryStream( data );
			
			//|              | Compressed Header    |
			//|              | Message & Cinematics |
			//|              | Player Data 2        |
			//|              | Global Victory       |
			//| Compressed   | Map                  |
			//|              | Units                |
			//|              | Player Data 3        |
			//|              | Triggers             |
			//|              | AI Files             |
			#if !DEBUG_ALPHA
			ReadCompressedHeader( reader );
			ReadMessagesAndCinematics( reader );
			ReadPlayerData2( reader );
			ReadGlobalVictory( reader );
			ReadDiplomacy( reader );
			ReadDisables( reader );	
			ReadMap( reader );
			ReadUnits( reader );
			#else
			reader.SeekAbsolute( 72916 );
			#endif
			ReadPlayerData3( reader );
		}
		
		static void ReadCompressedHeader( PrimitiveReader reader ) {
			uint nextUnitId = reader.ReadUInt32();
			float version2 = reader.ReadFloat32();
			majorVersion2 = (int)version2; // Get rid of decimal portion.
			minorVersion2 = (int)( (decimal)version2 * 100 ) % 100; // Otherwise some rounding errors are observable.
			// Good example on my machine: 1.3X -> 3X - 1, so 1.30 -> 29
			Console.WriteLine( version1 + "," + version2 );
			
			vv2 = Version.Parse( version2.ToString( "0.00" ) );
			Console.WriteLine( vv1 + "," + vv2 );
			
			if( minorVersion >= 9 ) {
				string[] playernames = new string[playersCount];
				for( int i = 0; i < playernames.Length; i++ ) {
					playernames[i] = reader.ReadASCIIString( playerNameLength );
				}
			}
			
			if( minorVersion2 >= 17 ) {
				uint[] playernameIndices = new uint[playersCount];
				for( int i = 0; i < playernameIndices.Length; i++ ) {
					playernameIndices[i] = reader.ReadUInt32();
				}
			}
			
			if( minorVersion >= 9 ) {
				PlayerData1[] playersData = new PlayerData1[playersCount];
				for( int i = 0; i < playersData.Length; i++ ) {
					PlayerData1 playerData;
					playerData.Active = reader.ReadUInt32() != 0;
					playerData.Human = reader.ReadUInt32() != 0;
					playerData.Civilization = reader.ReadUInt32();
					playerData.Unknown = reader.ReadUInt32();
					playersData[i] = playerData;
				}
			}
			
			uint unknown1 = reader.ReadUInt32();
			if( minorVersion >= 9 ) {
				byte unknown2 = reader.ReadUInt8();
			}
			float unknown3 = reader.ReadFloat32();
			
			string originalFilename = ReadUInt16LengthPrefixedString( reader );
			//System.Diagnostics.Debugger.Break();
		}
		
		static void ReadMessagesAndCinematics( PrimitiveReader reader ) {
			if( minorVersion2 >= 17 ) {
				uint instructionsIndex = reader.ReadUInt32();
				uint hintsIndex = reader.ReadUInt32();
				uint victoryIndex = reader.ReadUInt32();
				uint lossIndex = reader.ReadUInt32();
				uint historyIndex = reader.ReadUInt32();
				Console.WriteLine( minorVersion );
				if( minorVersion >= 21 ) { // 22 for ver 2
					uint scoutsIndex = reader.ReadUInt32();
				}
			}
			string instructions = ReadUInt16LengthPrefixedString( reader );
			if( minorVersion >= 9 ) {
				string hints = ReadUInt16LengthPrefixedString( reader );
				string victory = ReadUInt16LengthPrefixedString( reader );
				string loss = ReadUInt16LengthPrefixedString( reader );
				string history = ReadUInt16LengthPrefixedString( reader );
			}
			if( minorVersion >= 21 ) {
				string scouts = ReadUInt16LengthPrefixedString( reader );
			}
			if( minorVersion >= 9 ) {
				string pregameCinematic = ReadUInt16LengthPrefixedString( reader );
				string victoryCinematic = ReadUInt16LengthPrefixedString( reader );
				string lossCinematic = ReadUInt16LengthPrefixedString( reader );
				string background = ReadUInt16LengthPrefixedString( reader );
			}
			
			//System.Diagnostics.Debugger.Break();
			if( minorVersion >= 9 ) {
				bool bitmapIncluded = reader.ReadUInt32() != 0;
				uint bitmapWidth = reader.ReadUInt32();
				uint bitmapHeight = reader.ReadUInt32();
				short unknown1 = reader.ReadInt16(); // -1 when bitmap, 1 otherwise.
				if( bitmapIncluded ) {
					ReadBitmap( reader );
				}
			}
		}
		
		unsafe static Bitmap ReadBitmap( PrimitiveReader reader ) {
			uint headerSize = reader.ReadUInt32();
			int bmpWidth = reader.ReadInt32();
			int bmpHeight = reader.ReadInt32();
			ushort bmpPlanes = reader.ReadUInt16();
			ushort bmpBitCount = reader.ReadUInt16();
			uint bmpCompression = reader.ReadUInt32();
			uint bmpSize = reader.ReadUInt32();
			int bmpPpmX = reader.ReadInt32();
			int bmpPpmY = reader.ReadInt32();
			uint coloursUsed = reader.ReadUInt32();
			uint coloursImportant = reader.ReadUInt32();
			
			int[] colourTable = new int[256];
			Stream stream = reader.Stream;
			for( int i = 0; i < colourTable.Length; i++ ) {
				int r = stream.ReadByte();
				int g = stream.ReadByte();
				int b = stream.ReadByte();
				int a = stream.ReadByte();
				colourTable[i] = ( r << 16 ) | ( g << 8 ) | b;
				
			}
			Bitmap bmp = new Bitmap( bmpWidth, bmpHeight, PixelFormat.Format32bppRgb );
			BitmapData data = bmp.LockBits( new Rectangle( 0, 0, bmpWidth, bmpHeight ),
			                               ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb );
			// Ugly but fast.
			bmpHeight = Math.Abs( bmpHeight );
			bmpWidth = Math.Abs( bmpWidth );
			
			int bytesRead = 0;
			int* ptr = (int*)data.Scan0;
			int index = 0;
			for( int y = 0; y < bmpHeight; y++ ) {
				for( int x = 0; x < bmpWidth; x++ ) {
					ptr[index] = colourTable[stream.ReadByte()];
					index++;
					bytesRead++;
				}
			}
			// Test case: World Conquest
			long bytesLeft = (long)bmpSize - ( bmpWidth * bmpHeight );
			if( bytesLeft > 0 ) {
				Console.WriteLine( "Skipping " + bytesLeft + " bytes.." );
				reader.SkipData( bytesLeft );
			}
			bmp.UnlockBits( data );
			bmp.RotateFlip( RotateFlipType.RotateNoneFlipY );
			bmp.Save( "DUMP1111.bmp" );
			return bmp;
		}
		
		static void ReadPlayerData2( PrimitiveReader reader ) {
			//System.Diagnostics.Debugger.Break();
			string[] aiStrategies = new string[playersCount];
			for( int i = 0; i < aiStrategies.Length; i++ ) {
				aiStrategies[i] = ReadUInt16LengthPrefixedString( reader );
			}
			
			string[] aiCityPlans = new string[playersCount];
			for( int i = 0; i < aiCityPlans.Length; i++ ) {
				aiCityPlans[i] = ReadUInt16LengthPrefixedString( reader );
			}
			
			string[] aiNames = new string[playersCount];
			for( int i = 0; i < aiNames.Length; i++ ) {
				aiNames[i] = ReadUInt16LengthPrefixedString( reader );
			}
			//System.Diagnostics.Debugger.Break();
			
			AiFile[] aiFiles = new AiFile[playersCount];
			for( int i = 0; i < aiFiles.Length; i++ ) {
				aiFiles[i].ReadData( reader, vv1, vv2 );
			}
			
			if( minorVersion >= 18 ) {
				AiType[] aiTypes = new AiType[playersCount];
				for( int i = 0; i < aiTypes.Length; i++ ) {
					aiTypes[i] = (AiType)reader.ReadUInt8();
				}
			}
			
			#if DEBUG_ALPHA
			int count = 0;
			while( reader.ReadUInt8() != 0x9d ) {
				count++;
			}
			reader.Stream.Seek( -1, SeekOrigin.Current );
			#endif
			
			uint separator = reader.ReadUInt32();
			if( separator != 0xFFFFFF9D ) {
				throw new InvalidDataException();
			}
			PlayerResources[] resources = new PlayerResources[playersCount];
			for( int i = 0; i < resources.Length; i++ ) {
				PlayerResources resource = new PlayerResources();
				resource.GoldCount = reader.ReadUInt32();
				resource.WoodCount = reader.ReadUInt32();
				resource.FoodCount = reader.ReadUInt32();
				resource.StoneCount = reader.ReadUInt32();
				if( minorVersion2 >= 17 ) {
					resource.OreXCount = reader.ReadUInt32();
					resource.OreYCount = reader.ReadUInt32();
				}
				resources[i] = resource;
			}
		}
		
		static void ReadGlobalVictory( PrimitiveReader reader ) {
			uint separator = reader.ReadUInt32();
			if( separator != 0xFFFFFF9D ) {
				throw new InvalidDataException();
			}
			uint conqRawValue = reader.ReadUInt32();
			bool conquestRequired = conqRawValue != 0;
			uint ruinsRequired = reader.ReadUInt32(); // Only used in AoE?
			uint relicsRequired = reader.ReadUInt32();
			uint discoveriesRequired = reader.ReadUInt32(); // Only used in AoE?
			uint exploredPercentageRequired = reader.ReadUInt32();
			uint unknown3 = reader.ReadUInt32();
			bool allCustomConditionsRequired = reader.ReadUInt32() != 0;
			VictoryMode mode = (VictoryMode)reader.ReadUInt32();
			uint scoreRequired = reader.ReadUInt32();
			uint timeRequired = reader.ReadUInt32();
			double yearsRequired = timeRequired / 10.0; // 100 = 10y
		}
		
		static void ReadDiplomacy( PrimitiveReader reader ) {
			DiplomacyStance[][] stances = new DiplomacyStance[playersCount][];
			for( int i = 0; i < stances.Length; i++ ) {
				DiplomacyStance[] playerStances = new DiplomacyStance[playersCount];
				for( int j = 0; j < playerStances.Length; j++ ) {
					playerStances[j] = (DiplomacyStance)reader.ReadUInt32();
				}
				stances[i] = playerStances;
			}
			if( minorVersion < 18 ) { // For AOE
				IndividualVictory[][] customVictoryConditions = new IndividualVictory[playersCount][];
				for( int i = 0; i < customVictoryConditions.Length; i++ ) {
					IndividualVictory[] conditions = new IndividualVictory[12];
					for( int j = 0; j < conditions.Length; j++ ) {
						conditions[j].Unknown = reader.ReadBytes( 60 );
					}
					customVictoryConditions[i] = conditions;
				}
				System.Diagnostics.Debugger.Break();
			} else {
				byte[] unused = reader.ReadBytes( 11520 ); // Always unused.. 11520 bytes.
			}
			uint separator = reader.ReadUInt32();
			if( separator != 0xFFFFFF9D ) {
				throw new InvalidDataException();
			}
			uint[] alliedVictory = new uint[playersCount]; // Apparently ignored.
			for( int i = 0; i < alliedVictory.Length; i++ ) {
				alliedVictory[i] = reader.ReadUInt32();
			}
		}
		
		static void ReadDisables( PrimitiveReader reader ) {
			if( minorVersion < 18 ) {
				System.Diagnostics.Debugger.Break();
				// HACK: Skip unknown data.
				Console.WriteLine( "Skipping unknown data!" );
				while( reader.ReadUInt8() != 0x9D );
				reader.Stream.Seek( -1, SeekOrigin.Current );
				return;
			}
			Console.WriteLine( reader.Stream.Position );
			uint[] disabledTechCounts = reader.ReadUInt32Array( playersCount );
			uint[][] disabledTechIds = ReadDisabledSection( reader, 30 );
			
			uint[] disabledUnitsCount = reader.ReadUInt32Array( playersCount );
			uint[][] disabledUnits = ReadDisabledSection( reader, 30 );
			
			uint[] disabledBuildingsCount = reader.ReadUInt32Array( playersCount );
			uint[][] disabledBuildings = ReadDisabledSection( reader, 20 );
			
			uint unknown1 = reader.ReadUInt32();
			uint unknown2 = reader.ReadUInt32();
			bool allTechs = reader.ReadUInt32() != 0;
			StartingAge[] startingAges = new StartingAge[playersCount];
			for( int i = 0; i < startingAges.Length; i++ ) {
				startingAges[i] = (StartingAge)reader.ReadUInt32();
			}
		}
		
		static uint[][] ReadDisabledSection( PrimitiveReader reader, int sectionCount ) {
			uint[][] disabledIds = new uint[playersCount][];
			for( int i = 0; i < disabledIds.Length; i++ ) {
				disabledIds[i] = reader.ReadUInt32Array( sectionCount );
			}
			return disabledIds;
		}
		
		static void ReadMap( PrimitiveReader reader ) {
			uint separator = reader.ReadUInt32();
			if( separator != 0xFFFFFF9D ) {
				throw new InvalidDataException();
			}
			if( minorVersion >= 18 ) {
				int cameraY = reader.ReadInt32();
				int cameraX = reader.ReadInt32();
			}
			if( minorVersion >= 21 ) {
				AiMapType mapAi = (AiMapType)reader.ReadInt32();
			}
			uint mapWidth = reader.ReadUInt32();
			uint mapHeight = reader.ReadUInt32();
			TerrainElement[] mapData = new TerrainElement[(int)mapWidth * (int)mapHeight];
			for( int i = 0; i < mapData.Length; i++ ) {
				TerrainElement element;
				element.TerrainId = reader.ReadUInt8();
				element.Elevation = reader.ReadUInt8();
				element.Unknown = reader.ReadUInt8();
				mapData[i] = element;
			}
			//System.Diagnostics.Debugger.Break();
		}
		
		static void ReadUnits( PrimitiveReader reader ) {
			uint unitSections = reader.ReadUInt32();
			Console.WriteLine( "UNit POS" + reader.Stream.Position );
			//System.Diagnostics.Debugger.Break();
			PlayerData4[] duplicateData = new PlayerData4[8]; // TODO: Check this number.
			for( int i = 0; i < duplicateData.Length; i++ ) {
				duplicateData[i] = ReadPlayerData4( reader );
			}
			System.Diagnostics.Debugger.Break();
			Console.WriteLine( "UNITS DATS POS" + reader.Stream.Position );
			//System.Diagnostics.Debugger.Break();
			
			Unit[][] units = new Unit[(int)unitSections][];
			for( int i = 0; i < units.Length; i++ ) {
				uint unitCount = reader.ReadUInt32();
				//System.Diagnostics.Debugger.Break();
				Console.WriteLine( "COUT:" + unitCount );
				Unit[] playerUnits = new Unit[(int)unitCount];
				for( int j = 0; j < playerUnits.Length; j++ ) {
					playerUnits[j] = Unit.ReadFrom( reader, vv1, vv2 );
				}
				units[i] = playerUnits;
			}
		}
		
		static PlayerData4 ReadPlayerData4( PrimitiveReader reader ) {
			PlayerData4 value = new PlayerData4();
			value.FoodCount = reader.ReadFloat32();
			value.WoodCount = reader.ReadFloat32();
			value.GoldCount = reader.ReadFloat32();
			value.StoneCount = reader.ReadFloat32();
			if( minorVersion2 >= 17 ) {
				value.OreXCount = reader.ReadFloat32();
				value.OreYCount = reader.ReadFloat32();
			}
			if( minorVersion >= 21 ) {
				value.PopulationLimit = reader.ReadFloat32();
			}
			return value;
		}
		
		static void ReadPlayerData3( PrimitiveReader reader ) {
			//System.Diagnostics.Debugger.Break();
			uint playersCount = reader.ReadUInt32();
			PlayerData3[] players = new PlayerData3[8];
			for( int i = 0; i < players.Length; i++ ) {
				#if DEBUG_ALPHA
				players[i] = PlayerData3.ReadFrom2( reader, vv1, vv2 );
				#else
				players[i] = PlayerData3.ReadFrom( reader, vv1, vv2 );
				#endif
			}
			//System.Diagnostics.Debugger.Break();
			if( minorVersion >= 18 ) {
				double unknown = reader.ReadFloat64();
			}
		}
		
		static string ReadUInt16LengthPrefixedString( PrimitiveReader reader ) {
			ushort length = reader.ReadUInt16();
			return reader.ReadASCIIString( length );
		}
		
		static string ReadUInt32LengthPrefixedString( PrimitiveReader reader ) {
			uint length = reader.ReadUInt32();
			return reader.ReadASCIIString( (int)length );
		}
		
		public static void CreateHexScenario( string file ) {
			using( FileStream output = File.Create( "debug.scx" ) ) {
				BinaryWriter writer = new BinaryWriter( output );
				writer.Write( GetAsciiBytes( "1.21" ) );
				writer.Write( (uint)0 );
				writer.Write( 2 );
				writer.Write( 0xFFFF );
				string value = "HEX TESTS";
				writer.Write( value.Length );
				writer.Write( GetAsciiBytes( value ) );
				
				writer.Write( 2 );
				writer.Write( 8 );
				long pos = writer.BaseStream.Position;
				writer.Seek( 4, SeekOrigin.Begin );
				writer.Write( (uint)( output.Length - 8 ) );
				writer.Seek( (int)pos, SeekOrigin.Begin );
				byte[] data = Ionic.Zlib.DeflateStream.CompressBuffer( File.ReadAllBytes( file ) );
				writer.Write( data );
			}
		}
		
		static byte[] GetAsciiBytes( string value ) {
			return System.Text.Encoding.ASCII.GetBytes( value );
		}
	}

	struct PlayerData1 {
		public bool Active;
		public bool Human;
		public uint Civilization;
		public uint Unknown;
		
		public override string ToString() {
			return Active + "," + Human + "," + Civilization + "," + Unknown;
		}
	}

	public struct TerrainElement {
		public byte TerrainId;
		public byte Elevation;
		public byte Unknown;
		
		public override string ToString() {
			return String.Format( "Id={0}, Height={1}, {2}", TerrainId, Elevation, Unknown );
		}
	}

	struct AiFile {
		public string StrategyFile;
		public string CityPlanFile;
		public string PersonalityFile;
		
		public void ReadData( PrimitiveReader reader, Version version1, Version version2 ) {
			uint strategyLength = version1.Minor >= 9 ? reader.ReadUInt32() : reader.ReadUInt16();
			uint cityLength = version1.Minor >= 9 ? reader.ReadUInt32() : reader.ReadUInt16();
			uint personalityLength = version1.Minor >= 9 ? reader.ReadUInt32() : reader.ReadUInt16();
			StrategyFile = reader.ReadASCIIString( (int)strategyLength );
			CityPlanFile = reader.ReadASCIIString( (int)cityLength );
			PersonalityFile = reader.ReadASCIIString( (int)personalityLength );
		}
		
		public override string ToString() {
			int strategyLength = StrategyFile == null ? 0 : StrategyFile.Length;
			int cityLength = CityPlanFile == null ? 0 : CityPlanFile.Length;
			int personalityLength = PersonalityFile == null ? 0: PersonalityFile.Length;
			return strategyLength + "," + cityLength + "," + personalityLength;
		}
	}

	public enum StartingAge : int {
		NoneSelected = -1,
		DarkAge = 0,
		FeudalAge = 1,
		CastleAge = 2,
		ImperialAge = 3,
		PostImperialAge = 4,
	}

	public enum DiplomacyStance : uint {
		Allied = 0,
		Neutral = 1,
		Enemy = 3,
	}

	public enum AiType : byte {
		Custom = 0,
		Standard = 1,
		None = 2,
	}

	struct IndividualVictory {
		public byte[] Unknown;
	}

	public enum AiMapType {
		Arabia = 0x09,
		Archipelago = 0x0A,
		Baltic = 0x0B,
		Coastal = 0x0D,
		Continental = 0x0E,
		CraterLake = 0x0F,
		Fortress = 0x10,
		GoldRush = 0x11,
		Highland = 0x12,
		Islands = 0x13,
		Mediterranean = 0x14,
		Migration = 0x15,
		Rivers = 0x16,
		TeamIslands = 0x17,
		Unknown1 = 0x18,
		Scandinavia = 0x19,
		Unknown2 = 0x1A,
		Yucatan = 0x1B,
		SaltMarsh = 0x1C,
		Unknown3 = 0x1D,
		KingOfTheHill = 0x1E,
		Oasis = 0x1F,
		Unknown4 = 0x20,
		Nomad = 0x21,
	}

	enum VictoryMode : uint {
		Standard = 0,
		Conquest = 1,
		Score = 2,
		Timed = 3,
		Custom = 4,
	}

	struct PlayerResources {
		public uint GoldCount;
		public uint WoodCount;
		public uint FoodCount;
		public uint StoneCount;
		public uint OreXCount;
		public uint OreYCount; // Only appears to be used in AoE2 Alpha
		
		public override string ToString() {
			return String.Format( "F={0}, W={1}, G={2}, S={3}, X={4}, O={5}",
			                     FoodCount, WoodCount, GoldCount, StoneCount, OreXCount, OreYCount );
		}
	}

	public struct PlayerData4 {
		public float FoodCount;
		public float WoodCount;
		public float GoldCount;
		public float StoneCount;
		public float OreXCount;
		public float OreYCount; // Only appears to be used in AoE2 Alpha
		public float PopulationLimit; // TODO: Would this be better off as a class?
		
		public override string ToString() {
			return String.Format( "F={0}, W={1}, G={2}, S={3}, X={4}, O={5}, P={6}",
			                     FoodCount, WoodCount, GoldCount, StoneCount, OreXCount, OreYCount, PopulationLimit );
		}
	}

	public class PlayerData3 {
		public string ConstantName;
		public float InitialCameraX;
		public float InitialCameraY;
		public short Unknown1;
		public short Unknown2;
		public byte AlliedVictory;
		public ushort DiplomacyCount;
		public byte[] Diplomacy1;
		public uint[] Diplomacy2;
		public uint ColourId;
		
		public float Unknown3;
		public ushort Unknown4;
		public byte[] Unknown5;
		public byte[][] Unknown6;
		public byte[] Unknown7;
		public int Unknown8;
		
		public static PlayerData3 ReadFrom( PrimitiveReader reader, Version version1, Version version2 ) {
			PlayerData3 data = new PlayerData3();
			data.ConstantName = Utils.ReadUInt16LengthPrefixedString( reader );
			data.InitialCameraX = reader.ReadFloat32();
			data.InitialCameraY = reader.ReadFloat32();
			data.Unknown1 = reader.ReadInt16();
			data.Unknown2 = reader.ReadInt16();
			data.AlliedVictory = reader.ReadUInt8();
			ushort diplomacyCount = reader.ReadUInt16();
			data.DiplomacyCount = diplomacyCount;
			byte[] diplomacy1 = reader.ReadBytes( diplomacyCount );
			uint[] diplomacy2 = new uint[diplomacyCount];
			if( version1.Minor >= 9 ) {
				for( int i = 0; i < diplomacy2.Length; i++ ) {
					diplomacy2[i] = reader.ReadUInt32();
				}
			}
			data.Diplomacy1 = diplomacy1;
			data.Diplomacy2 = diplomacy2;
			if( version1.Minor >= 18 ) {
				data.ColourId = reader.ReadUInt32();
			}
			float unknown3 = reader.ReadFloat32();
			ushort unknown4 = reader.ReadUInt16();
			data.Unknown3 = unknown3;
			data.Unknown4 = unknown4;
			if( unknown3 == 2f ) {
				data.Unknown5 = reader.ReadBytes( 8 );
			}
			byte[][] unknown6 = new byte[unknown4][];
			for( int i = 0; i < unknown6.Length; i++ ) {
				unknown6[i] = reader.ReadBytes( 44 );
			}
			data.Unknown6 = unknown6;
			data.Unknown7 = reader.ReadBytes( 7 );
			data.Unknown8 = reader.ReadInt32();
			return data;
		}
		
		public static PlayerData3 ReadFrom2( PrimitiveReader reader, Version version1, Version version2 ) {
			System.Diagnostics.Debugger.Break();
			byte nameDataLength = reader.ReadUInt8();
			byte[] nameData = reader.ReadBytes( nameDataLength );
			byte bbbb = reader.ReadUInt8();
			float InitialCameraX = reader.ReadFloat32();
			float InitialCameraY = reader.ReadFloat32();
			short Unknown1 = reader.ReadInt16();
			short Unknown2 = reader.ReadInt16();
			byte AlliedVictory = reader.ReadUInt8();
			ushort diplomacyCount = reader.ReadUInt16();
			byte[] diplomacy1 = reader.ReadBytes( diplomacyCount );
			uint[] diplomacy2 = new uint[diplomacyCount];
			if( version1.Minor >= 9 ) {
				for( int i = 0; i < diplomacy2.Length; i++ ) {
					diplomacy2[i] = reader.ReadUInt32();
				}
			}
			byte unknown4 = reader.ReadUInt8();
			float unknown3 = reader.ReadFloat32();
			System.Diagnostics.Debugger.Break();
			return null;
		}
	}

	public struct Unit {
		public float X;
		public float Y;
		public float Unknown;
		public uint Id;
		public ushort UnitType;
		public byte Unknown2;
		public float RotationRadians;
		public ushort InitialFrame;
		public uint GarrisonedIn;
		
		public static Unit ReadFrom( PrimitiveReader reader, Version version1, Version version2 ) {
			Unit unit;
			unit.X = reader.ReadFloat32();
			unit.Y = reader.ReadFloat32();
			unit.Unknown = reader.ReadFloat32();
			unit.Id = reader.ReadUInt32();
			unit.UnitType = reader.ReadUInt16();
			unit.Unknown2 = reader.ReadUInt8();
			unit.RotationRadians = reader.ReadFloat32();
			if( version1.Minor >= 18 ) {
				unit.InitialFrame = reader.ReadUInt16();
				unit.GarrisonedIn = reader.ReadUInt32();
			} else {
				unit.InitialFrame = 0;
				unit.GarrisonedIn = 0xFFFFFFFF;
			}
			return unit;
		}
		
		public override string ToString() {
			return Unknown + "," + Unknown2;
		}

	}
}