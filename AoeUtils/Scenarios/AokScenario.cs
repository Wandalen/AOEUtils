//#define UNKNOWN_DOUBLE_TEST
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace AoeUtils {
	
	/// <summary> Age of Empires 2 final version scenario. </summary>
	public class AokScenario : Scenario {
		
		const int maxPlayersCount = 16;
		const int maxUsablePlayersCount = 8;
		const int playerNameLength = 256;
		protected const uint separatorValue = 0xFFFFFF9D;
		
		/// <summary> Gets the array of names of the players. </summary>
		/// <remarks> You can change the individual elements in
		/// the array, but you cannot reassign the array. </remarks>
		public IndexedString[] PlayerNames { get; protected set; }
		
		/// <summary> Gets the array of information about the players. </summary>
		/// <remarks> You can change the individual elements in
		/// the array, but you cannot reassign the array. </remarks>
		public PlayerInfo[] PlayersInfo { get; protected set; }
		
		protected uint chUnknown1;
		protected byte chUnknown2;
		protected float chUnknown3;
		
		public string OriginalFilename { get; protected set; }

		public IndexedString Instructions { get; protected set; }
		
		public IndexedString Hints { get; protected set; }
		
		public IndexedString Victory { get; protected set; }
		
		public IndexedString Loss { get; protected set; }
		
		public IndexedString History { get; protected set; }
		
		public string PregameCinematicFile { get; set; }
		
		public string VictoryCinematicFile { get; set; }
		
		public string LossCinematicFile { get; set; }
		
		public string BackgroundFile { get; set; }
		
		public Bitmap PregameBitmap { get; protected set; }
		
		public AiTextData[] AiPlayersInfo { get; protected set; }
		
		public Map GameMap { get; protected set; }
		
		public Point2I CameraPosition { get; set; }
		
		public bool IsTheConquerorsScenario {
			get {
				return Version1.Minor >= 21 && Version2.Minor >= 22;
			}
		}
		
		protected override void ReadCompressedData( PrimitiveReader reader ) {
			ReadCompressedHeaderData( reader );
			ReadMessages( reader );
			ReadCinematics( reader );
			ReadPlayerData2( reader );
			ReadGlobalVictory( reader );
			ReadDiplomacy( reader );
			ReadDisables( reader );
			ReadMap( reader );
			ReadPlayerUnits( reader );
			ReadPlayerData3( reader );
			//System.Diagnostics.Debugger.Break();
			ReadTriggers( reader );
			ReadIncludedFiles( reader );
		}
		
		protected virtual void ReadCompressedHeaderData( PrimitiveReader reader ) {
			IndexedString[] names = new IndexedString[maxPlayersCount];
			for( int i = 0; i < names.Length; i++ ) {
				string asciiName = reader.ReadAsciiFixedString( playerNameLength );
				names[i] = new IndexedString( asciiName );
			}
			for( int i = 0; i < names.Length; i++ ) {
				names[i].Index = reader.ReadUInt32();
			}
			PlayerNames = names;
			
			PlayerInfo[] playersData = new PlayerInfo[maxPlayersCount];
			for( int i = 0; i < playersData.Length; i++ ) {
				PlayerInfo info = new PlayerInfo();
				info.Active = reader.ReadUInt32() != 0;
				info.Human = reader.ReadUInt32() != 0;
				info.CivilizationId = reader.ReadUInt32();
				info.Unknown = reader.ReadUInt32();
				playersData[i] = info;
			}
			PlayersInfo = playersData;
			
			chUnknown1 = reader.ReadUInt32();
			chUnknown2 = reader.ReadUInt8();
			chUnknown3 = reader.ReadUInt32();
			OriginalFilename = Utils.ReadUInt16LengthPrefixedString( reader );
		}
		
		protected virtual void ReadMessages( PrimitiveReader reader ) {
			Instructions = new IndexedString();
			Hints = new IndexedString();
			Victory = new IndexedString();
			Loss = new IndexedString();
			History = new IndexedString();
			
			Instructions.Index = reader.ReadUInt32();
			Hints.Index = reader.ReadUInt32();
			Victory.Index = reader.ReadUInt32();
			Loss.Index = reader.ReadUInt32();
			History.Index = reader.ReadUInt32();
			
			Instructions.rawValue = Utils.ReadUInt16LengthPrefixedString( reader );
			Hints.rawValue = Utils.ReadUInt16LengthPrefixedString( reader );
			Victory.rawValue = Utils.ReadUInt16LengthPrefixedString( reader );
			Loss.rawValue = Utils.ReadUInt16LengthPrefixedString( reader );
			History.rawValue = Utils.ReadUInt16LengthPrefixedString( reader );
		}
		
		protected virtual void ReadCinematics( PrimitiveReader reader ) {
			PregameCinematicFile = Utils.ReadUInt16LengthPrefixedString( reader );
			VictoryCinematicFile = Utils.ReadUInt16LengthPrefixedString( reader );
			LossCinematicFile = Utils.ReadUInt16LengthPrefixedString( reader );
			BackgroundFile = Utils.ReadUInt16LengthPrefixedString( reader );
			
			bool bitmapIncluded = reader.ReadUInt32() != 0;
			uint bitmapWidth = reader.ReadUInt32();
			int bitmapHeight = reader.ReadInt32();
			short unknown1 = reader.ReadInt16(); // -1 when bitmap, 1 otherwise.
			if( bitmapIncluded ) {
				PregameBitmap = ReadBitmap( reader );
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
			// Test case: World Conquest has size not equal to (width * height)
			long bytesLeft = (long)bmpSize - ( bmpWidth * bmpHeight );
			if( bytesLeft > 0 ) {
				Console.WriteLine( "Skipping " + bytesLeft + " bytes.." );
				reader.SkipData( bytesLeft );
			}
			bmp.UnlockBits( data );
			bmp.RotateFlip( RotateFlipType.RotateNoneFlipY );
			return bmp;
		}
		
		protected virtual void ReadPlayerData2( PrimitiveReader reader ) {
			AiTextData[] ais = new AiTextData[maxPlayersCount];
			for( int i = 0; i < ais.Length; i++ ) {
				ais[i] = new AiTextData();
			}
			
			for( int i = 0; i < ais.Length; i++ ) {
				ais[i].AiStrategyFile = Utils.ReadUInt16LengthPrefixedString( reader );
			}
			
			for( int i = 0; i < ais.Length; i++ ) {
				ais[i].AiCityPlanFile = Utils.ReadUInt16LengthPrefixedString( reader );
			}
			
			for( int i = 0; i < ais.Length; i++ ) {
				ais[i].AiPersonalityFile = Utils.ReadUInt16LengthPrefixedString( reader );
			}
			
			for( int i = 0; i < ais.Length; i++ ) {
				ais[i].ReadAiTextData( reader );
			}
			
			for( int i = 0; i < ais.Length; i++ ) {
				ais[i].AiType = (AiType)reader.ReadUInt8();
			}
			AiPlayersInfo = ais;
			
			uint separator = reader.ReadUInt32();
			if( separator != separatorValue ) {
				throw new InvalidDataException( "Expected separator value." );
			}
			
			PlayerInfo[] info = PlayersInfo;
			for( int i = 0; i < info.Length; i++ ) {
				info[i].ReadResources( reader );
			}
		}
		
		protected virtual void ReadGlobalVictory( PrimitiveReader reader ) {
			uint separator = reader.ReadUInt32();
			if( separator != separatorValue ) {
				throw new InvalidDataException( "Expected separator value." );
			}
			bool conquestRequired = reader.ReadUInt32() != 0;
			uint ruinsRequired = reader.ReadUInt32(); // Only used in AoE?
			uint relicsRequired = reader.ReadUInt32();
			uint discoveriesRequired = reader.ReadUInt32(); // Only used in AoE?
			uint exploredPercentageRequired = reader.ReadUInt32();
			uint unknown3 = reader.ReadUInt32();
			bool allCustomConditionsRequired = reader.ReadUInt32() != 0;
			VictoryMode mode = (VictoryMode)reader.ReadUInt32();
			uint requiredScore = reader.ReadUInt32();
			uint timeRequired = reader.ReadUInt32();
			double yearsRequired = timeRequired / 10.0;
		}
		
		protected virtual void ReadDiplomacy( PrimitiveReader reader ) {
			PlayerInfo[] infos = PlayersInfo;
			for( int i = 0; i < infos.Length; i++ ) {
				DiplomacyStance[] stances = new DiplomacyStance[maxPlayersCount];
				for( int j = 0; j < stances.Length; j++ ) {
					stances[j] = (DiplomacyStance)reader.ReadUInt32();
				}
				infos[i].Diplomacy = stances;
			}
			
			// This is individual victory in AoE.
			// Not used in AoE2.
			byte[] aoeIndividualVictory = reader.ReadBytes( 11520 );
			
			uint separator = reader.ReadUInt32();
			if( separator != separatorValue ) {
				throw new InvalidDataException( "Expected separator value." );
			}
			for( int i = 0; i < infos.Length; i++ ) {
				infos[i].AlliedVictory = reader.ReadUInt32() != 0;
			}
		}
		
		public uint[][] DisabledTechs;
		public uint[][] DisabledUnits;
		public uint[][] DisabledBuildings;
		
		protected virtual void ReadDisables( PrimitiveReader reader ) {
			uint[] disabledTechCount = reader.ReadUInt32Array( maxPlayersCount );
			uint[][] disabledTechIds = ReadDisabledSection( reader, 30 );
			
			uint[] disabledUnitsCount = reader.ReadUInt32Array( maxPlayersCount );
			uint[][] disabledUnits = ReadDisabledSection( reader, 30 );
			
			uint[] disabledBuildingsCount = reader.ReadUInt32Array( maxPlayersCount );
			uint[][] disabledBuildings = ReadDisabledSection( reader, 20 );
			
			uint unknown1 = reader.ReadUInt32();
			uint unknown2 = reader.ReadUInt32();
			bool allTechs = reader.ReadUInt32() != 0;
			
			PlayerInfo[] players = PlayersInfo;
			for( int i = 0; i < players.Length; i++ ) {
				players[i].Age = (StartingAge)reader.ReadUInt32();
			}
			DisabledTechs = disabledTechIds;
			DisabledUnits = disabledUnits;
			DisabledBuildings = disabledBuildings;
		}
		
		static uint[][] ReadDisabledSection( PrimitiveReader reader, int sectionCount ) {
			uint[][] disabledIds = new uint[maxPlayersCount][];
			for( int i = 0; i < disabledIds.Length; i++ ) {
				disabledIds[i] = reader.ReadUInt32Array( sectionCount );
			}
			return disabledIds;
		}
		
		protected virtual void ReadMap( PrimitiveReader reader ) {
			uint separator = reader.ReadUInt32();
			if( separator != separatorValue ) {
				throw new InvalidDataException( "Expected separator value." );
			}
			int editorCameraY = reader.ReadInt32();
			int editorCameraX = reader.ReadInt32();
			CameraPosition = new Point2I( editorCameraX, editorCameraY );
			GameMap = Map.ReadFrom( reader );
		}
		
		protected virtual void ReadPlayerUnits( PrimitiveReader reader ) {
			uint unitSections = reader.ReadUInt32();
			PlayerData4[] duplicateData = new PlayerData4[8]; // TODO: Check this number.
			for( int i = 0; i < duplicateData.Length; i++ ) {
				duplicateData[i] = ReadPlayerData4( reader );
			}
			//System.Diagnostics.Debugger.Break();
			
			Version version1 = Version1;
			Version version2 = Version2;
			
			Unit[][] units = new Unit[(int)unitSections][];
			for( int i = 0; i < units.Length; i++ ) {
				uint unitCount = reader.ReadUInt32();
				Unit[] playerUnits = new Unit[(int)unitCount];
				for( int j = 0; j < playerUnits.Length; j++ ) {
					playerUnits[j] = Unit.ReadFrom( reader, version1, version2 );
				}
				units[i] = playerUnits;
			}
			//System.Diagnostics.Debugger.Break();
		}
		
		protected virtual PlayerData4 ReadPlayerData4( PrimitiveReader reader ) {
			PlayerData4 value = new PlayerData4();
			value.PopulationLimit = 75f;
			value.FoodCount = reader.ReadFloat32();
			value.WoodCount = reader.ReadFloat32();
			value.GoldCount = reader.ReadFloat32();
			value.StoneCount = reader.ReadFloat32();
			value.OreXCount = reader.ReadFloat32();
			value.OreYCount = reader.ReadFloat32();
			return value;
		}
		
		protected virtual void ReadPlayerData3( PrimitiveReader reader ) {
			uint playersCount = reader.ReadUInt32();
			PlayerData3[] players = new PlayerData3[8];
			for( int i = 0; i < players.Length; i++ ) {
				players[i] = PlayerData3.ReadFrom( reader, Version1, Version2 );
			}
		}
		
		protected virtual void ReadTriggers( PrimitiveReader reader ) {
			double triggerVersion = reader.ReadFloat64();
			sbyte unknown = reader.ReadInt8();
			int triggersCount = reader.ReadInt32();
			//System.Diagnostics.Debugger.Break();
			
			Trigger[] triggers = new Trigger[triggersCount];
			for( int i = 0; i < triggers.Length; i++ ) {
				triggers[i] = Trigger.ReadFrom( reader );
			}
			for( int i = 0; i < triggers.Length; i++ ) {
				triggers[i].DisplayOrder = reader.ReadInt32();
			}
			//System.Diagnostics.Debugger.Break();
		}
		
		protected virtual void ReadIncludedFiles( PrimitiveReader reader ) {
			bool filesIncluded = reader.ReadUInt32() != 0;
			bool esOnlyDataIncluded = reader.ReadUInt32() != 0;
			if( esOnlyDataIncluded ) {
				byte[] esdata = reader.ReadBytes( 396 );
				string path = OriginalFilename;
				path = Path.GetFileName( path );
				path = Path.ChangeExtension( path, "esbin" );
				path = Path.Combine( "ESDATA", path );
				if( !Directory.Exists( "ESDATA" ) ) {
					Directory.CreateDirectory( "ESDATA" );
				}
				File.WriteAllBytes( path, esdata );
			}
			if( filesIncluded ) {
				uint filesCount = reader.ReadUInt32();
				IncludedFile[] files = new IncludedFile[(int)filesCount];
				for( int i = 0; i < filesCount; i++ ) {
					IncludedFile file = new IncludedFile();
					file.Filename = Utils.ReadUInt32LengthPrefixedString( reader );
					file.Text = Utils.ReadUInt32LengthPrefixedString( reader );
					files[i] = file;
				}
			}
			if( esOnlyDataIncluded ) {
				//System.Diagnostics.Debug.WriteLine( reader.Stream.Position + "," + reader.Stream.Length );
			}
		}
		
		class IncludedFile {
			public string Filename;
			public string Text;
		}
		
		protected override void WriteCompressedData( BinaryWriter writer ) {
			WriteCompressedHeaderData( writer );
			WriteMessages( writer );
			WriteCinematics( writer );
			WritePlayerData2( writer );
			WriteGlobalVictory( writer );
			WriteDiplomacy( writer );
			WriteDisables( writer );
			WriteMap( writer );
		}
		
		protected virtual void WriteCompressedHeaderData( BinaryWriter writer ) {
			IndexedString[] names = PlayerNames;
			for( int i = 0; i < names.Length; i++ ) {
				Utils.WriteFixedString( writer, names[i].rawValue, playerNameLength );
			}
			for( int i = 0; i < names.Length; i++ ) {
				writer.Write( names[i].Index );
			}
			
			PlayerInfo[] info = PlayersInfo;
			for( int i = 0; i < info.Length; i++ ) {
				PlayerInfo playerInfo = info[i];
				writer.Write( playerInfo.Active ? 1 : 0 );
				writer.Write( playerInfo.Human ? 1 : 0 );
				writer.Write( playerInfo.CivilizationId );
				writer.Write( playerInfo.Unknown );
			}
		}
		
		protected virtual void WriteMessages( BinaryWriter writer ) {
			writer.Write( Instructions.Index );
			writer.Write( Hints.Index );
			writer.Write( Victory.Index );
			writer.Write( Loss.Index );
			writer.Write( History.Index );
			
			Utils.WriteUInt16LengthPrefixedString( writer, Instructions.rawValue );
			Utils.WriteUInt16LengthPrefixedString( writer, Hints.rawValue );
			Utils.WriteUInt16LengthPrefixedString( writer, Victory.rawValue );
			Utils.WriteUInt16LengthPrefixedString( writer, Loss.rawValue );
			Utils.WriteUInt16LengthPrefixedString( writer, History.rawValue );
		}
		
		protected virtual void WriteCinematics( BinaryWriter writer ) {
			Utils.WriteUInt16LengthPrefixedString( writer, PregameCinematicFile );
			Utils.WriteUInt16LengthPrefixedString( writer, VictoryCinematicFile );
			Utils.WriteUInt16LengthPrefixedString( writer, LossCinematicFile );
			Utils.WriteUInt16LengthPrefixedString( writer, BackgroundFile );
			
			// TODO: Bitmaps
			writer.Write( 0 ); // Bitmap included (false)
			writer.Write( 0 ); // Bitmap width
			writer.Write( 0 ); // Bitmap height
			writer.Write( (short)1 );
		}
		
		protected virtual void WritePlayerData2( BinaryWriter writer ) {
			AiTextData[] ais = AiPlayersInfo;
			for( int i = 0; i < ais.Length; i++ ) {
				Utils.WriteUInt16LengthPrefixedString( writer, ais[i].AiStrategyFile );
			}
			for( int i = 0; i < ais.Length; i++ ) {
				Utils.WriteUInt16LengthPrefixedString( writer, ais[i].AiCityPlanFile );
			}
			for( int i = 0; i < ais.Length; i++ ) {
				Utils.WriteUInt16LengthPrefixedString( writer, ais[i].AiPersonalityFile );
			}
			
			for( int i = 0; i < ais.Length; i++ ) {
				ais[i].WriteAiTextData( writer );
			}
			for( int i = 0; i < ais.Length; i++ ) {
				writer.Write( (byte)ais[i].AiType );
			}
			writer.Write( separatorValue );
			
			PlayerInfo[] infos = PlayersInfo;
			for( int i = 0; i < infos.Length; i++ ) {
				infos[i].WriteResources( writer );
			}
		}
		
		protected virtual void WriteGlobalVictory( BinaryWriter writer ) {
			writer.Write( separatorValue );
			// Eh I'm kinda not sure about this section.
			throw new NotImplementedException();
			
		}
		
		protected virtual void WriteDiplomacy( BinaryWriter writer ) {
			PlayerInfo[] infos = PlayersInfo;
			for( int i = 0; i < infos.Length; i++ ) {
				DiplomacyStance[] stances = infos[i].Diplomacy;
				for( int j = 0; i < stances.Length; j++ ) {
					writer.Write( (uint)stances[j] );
				}
			}
			writer.Write( new byte[11520] ); // TODO: Should individual victory conditions be preserved?
			writer.Write( separatorValue );
			for( int i = 0; i < infos.Length; i++ ) {
				writer.Write( infos[i].AlliedVictory ? 1 : 0 );
			}
		}
		
		protected virtual void WriteDisables( BinaryWriter writer ) {
			throw new NotImplementedException();
		}
		
		protected virtual void WriteMap( BinaryWriter writer ) {
			writer.Write( separatorValue );
			writer.Write( CameraPosition.Y );
			writer.Write( CameraPosition.X );
			GameMap.WriteData( writer );
		}
	}
	
	/// <summary> Represents information about a player. </summary>
	public sealed class PlayerInfo {
		public bool Active;
		public bool Human;
		public uint CivilizationId;
		public uint Unknown; // Seems to always be 4?
		public StartingAge Age;
		
		public DiplomacyStance[] Diplomacy;
		public bool AlliedVictory;
		
		// Note that these cannot be greater than 16777216.
		public uint Food, Gold, Wood, Stone, OreX, Unknown2;
		
		public void ReadResources( PrimitiveReader reader ) {
			Gold = reader.ReadUInt32();
			Wood = reader.ReadUInt32();
			Food = reader.ReadUInt32();
			Stone = reader.ReadUInt32();
			OreX = reader.ReadUInt32();
			Unknown2 = reader.ReadUInt32(); // Padding?
		}
		
		public void WriteResources( BinaryWriter writer ) {
			writer.Write( Gold );
			writer.Write( Wood );
			writer.Write( Food );
			writer.Write( Stone );
			writer.Write( OreX );
			writer.Write( Unknown2 ); // Padding?
		}
	}
	
	public sealed class Map {
		public uint Width;
		public uint Length;
		
		internal TerrainElement[] mapData;
		
		public TerrainElement[] MapData {
			get { return mapData; }
		}
		
		private Map() {	}
		
		public Map( uint width, uint length ) {
			Width = width;
			Length = length;
			int size = (int)Width * (int)Length;
			mapData = new TerrainElement[size];
		}
		
		public static Map GenerateBlank( uint width, uint length, byte terrainType, byte elevation ) {
			Map map = new Map( width, length );
			TerrainElement[] mapData = map.mapData;
			for( int i = 0; i < mapData.Length; i++ ) {
				mapData[i].TerrainId = terrainType;
				mapData[i].Elevation = elevation;
			}
			return map;
		}
		
		public TerrainElement this[int x, int y] {
			get { return mapData[( y * Width ) + x]; }
			set { mapData[( y * Width ) + x] = value; }
		}
		
		public static Map ReadFrom( PrimitiveReader reader ) {
			Map map = new Map();
			map.Width = reader.ReadUInt32();
			map.Length = reader.ReadUInt32();
			int size = (int)map.Width * (int)map.Length;
			TerrainElement[] mapData = new TerrainElement[size];
			for( int i = 0; i < mapData.Length; i++ ) {
				TerrainElement element;
				element.TerrainId = reader.ReadUInt8();
				element.Elevation = reader.ReadUInt8();
				element.Unknown = reader.ReadUInt8();
				mapData[i] = element;
			}
			map.mapData = mapData;
			return map;
		}
		
		public override string ToString() {
			return Width + "," + Length;
		}
		
		public void WriteData( BinaryWriter writer ) {
			writer.Write( Width );
			writer.Write( Length );
			for( int i = 0; i < mapData.Length; i++ ) {
				TerrainElement element = mapData[i];
				writer.Write( element.TerrainId );
				writer.Write( element.Elevation );
				writer.Write( element.Unknown );
			}
		}
	}
	
	public sealed class AiTextData {
		static readonly Encoding asciiEncoding = Encoding.ASCII;
		
		public string AiStrategyFile;
		public string AiCityPlanFile;
		public string AiPersonalityFile;
		
		public string AiStrategyText;
		public string AiCityPlanText;
		public string AiPersonalityText;
		
		public AiType AiType;
		
		public void ReadAiTextData( PrimitiveReader reader ) {
			uint strategyLength = reader.ReadUInt32();
			uint cityLength = reader.ReadUInt32();
			uint personalityLength = reader.ReadUInt32();
			AiStrategyText = reader.ReadASCIIString( (int)strategyLength );
			AiCityPlanText = reader.ReadASCIIString( (int)cityLength );
			AiPersonalityText = reader.ReadASCIIString( (int)personalityLength );
		}
		
		public void WriteAiTextData( BinaryWriter writer ) {
			int strategyLength = AiStrategyText == null ? 0 : AiStrategyText.Length;
			int cityPlanLength = AiCityPlanText == null ? 0 : AiCityPlanText.Length;
			int personalityLength = AiPersonalityText == null ? 0 : AiPersonalityText.Length;
			writer.Write( strategyLength );
			writer.Write( cityPlanLength );
			writer.Write( personalityLength );
			
			if( AiStrategyText != null )
				writer.Write( asciiEncoding.GetBytes( AiStrategyText ) );
			if( AiCityPlanText != null )
				writer.Write( asciiEncoding.GetBytes( AiCityPlanText ) );
			if( AiPersonalityText != null )
				writer.Write( asciiEncoding.GetBytes( AiPersonalityText ) );
		}
	}
	
	/// <summary> Represents a point in 2D space. </summary>
	public struct Point2I : IEquatable<Point2I> {
		
		/// <summary> 2D point at origin. (0, 0) </summary>
		public static Point2I Zero = new Point2I( 0, 0 );
		public int X, Y;
		
		public Point2I( int x, int y ) {
			X = x;
			Y = y;
		}
		
		public override int GetHashCode() {
			return unchecked( 1000000007 * X + 1000000009 * Y );
		}
		
		public override bool Equals( object obj ) {
			return ( obj is Point2I ) && Equals( (Point2I)obj );
		}

		public bool Equals( Point2I other ) {
			return X == other.X && Y == other.Y;
		}
		
		public static bool operator == ( Point2I a, Point2I b ) {
			return a.Equals( b );
		}
		
		public static bool operator != ( Point2I a, Point2I b ) {
			return !( a == b );
		}
		
		public override string ToString() {
			return X + "," + Y;
		}
	}
	
	public class Trigger {
		public int DisplayOrder;
		
		public bool Enabled;
		public bool Looping;
		public byte Unknown1;
		public bool DisplayAsObjective;
		public uint ObjectivesDescriptionOrder;
		public uint Unknown2;
		public string Description;
		public string Name;
		public TriggerEffect[] Effects;
		public TriggerCondition[] Conditions;
		
		public static Trigger ReadFrom( PrimitiveReader reader ) {
			Trigger trigger = new Trigger();
			//System.Diagnostics.Debugger.Break();
			
			trigger.Enabled = reader.ReadUInt32() != 0;
			trigger.Looping = reader.ReadUInt32() != 0;
			trigger.Unknown1 = reader.ReadUInt8();
			trigger.DisplayAsObjective = reader.ReadUInt8() != 0;
			trigger.ObjectivesDescriptionOrder = reader.ReadUInt32();
			trigger.Unknown2 = reader.ReadUInt32();
			trigger.Description = Utils.ReadUInt32LengthPrefixedString( reader );
			trigger.Name = Utils.ReadUInt32LengthPrefixedString( reader );
			
			//System.Diagnostics.Debugger.Break();
			
			int effectsCount = reader.ReadInt32();
			if( effectsCount < 0 ) effectsCount = 0;
			TriggerEffect[] effects = new TriggerEffect[effectsCount];
			for( int i = 0; i < effects.Length; i++ ) {
				effects[i] = TriggerEffect.ReadFrom( reader );
			}
			for( int i = 0; i < effects.Length; i++ ) {
				effects[i].DisplayOrder = reader.ReadInt32();
			}
			trigger.Effects = effects;
			
			int conditionsCount = reader.ReadInt32();
			if( conditionsCount < 0 ) conditionsCount = 0;
			TriggerCondition[] conditions = new TriggerCondition[conditionsCount];
			for( int i = 0; i < conditions.Length; i++ ) {
				conditions[i] = TriggerCondition.ReadFrom( reader );
			}
			for( int i = 0; i < conditions.Length; i++ ) {
				conditions[i].DisplayOrder = reader.ReadInt32();
			}
			trigger.Conditions = conditions;
			//System.Diagnostics.Debugger.Break();
			return trigger;
		}
		
		public override string ToString() {
			return Name;
		}

	}
	
	public class TriggerEffect {
		public int DisplayOrder;
		
		public int Type;
		public int Check;
		public int AiGoal;
		public int Amount;
		public int ResourceType;
		public int Diplomacy;
		public int UnitLocation;
		public int UnitType;
		
		public int PlayerSource;
		public int PlayerTarget;
		public int Technology;
		public int StringTableIndex;
		public int Unknown;
		public int DisplayTime;
		public int TriggerIndex;
		public Point2I Location;
		public Point2I BottomLeft;
		public Point2I UpperRight;
		public int UnitGroup;
		public int UnitGroupType;
		public int InstructionPanel;
		public string Text;
		public string SoundFile;
		public uint[] SelectedUnits;
		
		public static TriggerEffect ReadFrom( PrimitiveReader reader ) {
			TriggerEffect effect = new TriggerEffect();
			effect.Type = reader.ReadInt32();
			effect.Check = reader.ReadInt32();
			if( effect.Check != 0x17 )
				throw new InvalidDataException( "Expected 0x17 for check." );
			
			effect.AiGoal = reader.ReadInt32();
			effect.Amount = reader.ReadInt32();
			effect.ResourceType = reader.ReadInt32();
			effect.Diplomacy = reader.ReadInt32();
			int selectedUnitsCount = reader.ReadInt32();
			effect.UnitLocation = reader.ReadInt32();
			effect.UnitType = reader.ReadInt32();
			effect.PlayerSource = reader.ReadInt32();
			effect.PlayerTarget = reader.ReadInt32();
			effect.Technology = reader.ReadInt32();
			effect.StringTableIndex = reader.ReadInt32();
			effect.Unknown = reader.ReadInt32();
			effect.DisplayTime = reader.ReadInt32();
			effect.TriggerIndex = reader.ReadInt32();
			effect.Location = new Point2I( reader.ReadInt32(), reader.ReadInt32() );
			effect.BottomLeft = new Point2I( reader.ReadInt32(), reader.ReadInt32() );
			effect.UpperRight = new Point2I( reader.ReadInt32(), reader.ReadInt32() );
			effect.UnitGroup = reader.ReadInt32();
			effect.UnitGroupType = reader.ReadInt32();
			effect.InstructionPanel = reader.ReadInt32();
			
			effect.Text = Utils.ReadUInt32LengthPrefixedString( reader );
			effect.SoundFile = Utils.ReadUInt32LengthPrefixedString( reader );
			
			if( selectedUnitsCount < 0 ) selectedUnitsCount = 0;
			uint[] selectedUnits = new uint[selectedUnitsCount];
			for( int i = 0; i < selectedUnits.Length; i++ ) {
				selectedUnits[i] = reader.ReadUInt32();
			}
			effect.SelectedUnits = selectedUnits;
			return effect;
		}
		
	}
	
	public class TriggerCondition {
		public int DisplayOrder;
		
		public int Type;
		public int Check; // Always 0x10?
		public int Amount;
		public int ResourceType;
		
		public int UnitObject;
		public int UnitLocation;
		public int UnitType;
		
		public int Player;
		public int Technology;
		public int Timer;
		public int Unknown;
		
		public Point2I BottomLeft;
		public Point2I UpperRight;
		
		public int UnitGroup;
		public int UnitGroupType;
		
		public int AiSignal;
		
		public static TriggerCondition ReadFrom( PrimitiveReader reader ) {
			TriggerCondition condition = new TriggerCondition();
			
			condition.Type = reader.ReadInt32();
			condition.Check = reader.ReadInt32();
			if( condition.Check != 0x10 )
				throw new InvalidDataException( "Expected 0x10 for check." );
			
			condition.Amount = reader.ReadInt32();
			condition.ResourceType = reader.ReadInt32();
			
			condition.UnitObject = reader.ReadInt32();
			condition.UnitLocation = reader.ReadInt32();
			condition.UnitType = reader.ReadInt32();
			
			condition.Player = reader.ReadInt32();
			condition.Technology = reader.ReadInt32();
			condition.Timer = reader.ReadInt32();
			condition.Unknown = reader.ReadInt32();
			
			condition.BottomLeft = new Point2I( reader.ReadInt32(), reader.ReadInt32() );
			condition.UpperRight = new Point2I( reader.ReadInt32(), reader.ReadInt32() );
			
			condition.UnitGroup = reader.ReadInt32();
			condition.UnitGroupType = reader.ReadInt32();
			condition.AiSignal = reader.ReadInt32();
			return condition;
		}
	}
	
	/// <summary> Represented a set of disabled unit/building/technology ids.
	/// Note that due to the unfortunate fact that the genie engine
	/// uses fixed sized arrays, disabled sections cannot use
	/// more than a certain number of elements.  </summary>
	public struct DisabledSection {
		
		/// <summary> Number of used elements in the Values array. </summary>
		/// <remarks> This cannot be greater than the length of the values array. </remarks>
		public uint UsedCount;
		
		/// <summary> List of disabled unit/building/technology ids. </summary>
		public uint[] Values;
	}
	
	
	// TODO: This probably needs to be cleaned up more.
	/// <summary> Represents a string that may or may not be indexed by the language.dll string table. </summary>
	public sealed class IndexedString {
		
		const uint notInTable = 0xFFFFFFFF;
		
		public IndexedString() {
			rawValue = String.Empty;
		}
		
		public IndexedString( string value ) {
			rawValue = value;
		}
		
		public IndexedString( uint index ) {
			Index = index;
			// TODO: String table
			throw new NotImplementedException( "String table not yet supported." );
		}
		
		/// <summary> Index of the value of this indexed string in the string table. </summary>
		/// <remarks> 0xFFFFFFFF or -1 means that the indexed string is not
		/// in the table. </remarks>
		public uint Index = notInTable;
		
		/// <summary> Gets whether this indexed string is in the string table or not. </summary>
		public bool InStringTable {
			get { return Index != notInTable; }
		}
		
		internal string rawValue;
		
		public string Value {
			get {
				if( Index == notInTable ) {
					return rawValue;
				} else {
					throw new NotImplementedException( "String table not yet supported." );
				}
			}
			set {
				if( Index == notInTable ) {
					rawValue = value;
				} else {
					throw new InvalidOperationException( "Cannot modify an indexed string in a string table." );
				}
			}
		}
		
		/// <summary> Unindexes this string so that it no longer depends on the string table. </summary>
		/// <remarks> This method means that the string value will change
		/// to whatever is in the current string table, so users
		/// with different string tables will not see the
		/// different string. </remarks>
		public void Unindex() {
			if( Index == notInTable ) return;
			throw new NotImplementedException( "String table not yet supported." );
		}
		
		
		public static implicit operator string( IndexedString value ) {
			return value.Value;
		}
		
		public static explicit operator IndexedString( uint index ) {
			return new IndexedString( index );
		}
		
		public static implicit operator IndexedString( string value ) {
			return new IndexedString( value );
		}
	}
}