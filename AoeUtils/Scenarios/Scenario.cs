//#define DUMP_RAW
using System;
using System.IO;

namespace AoeUtils {
	
	/// <summary> Represents a scenario created by the Genie Engine. </summary>
	/// <remarks> Please note that I do not have Star Wars: Galatic Backgrounds,
	/// so I don't know how compatible the decoding routine is with that game. </remarks>
	public abstract class Scenario {
		
		public Version Version1 { get; protected set; }
		
		public Version Version2 { get; protected set; }
		
		/// <summary> Gets or sets the number of players in the scenario. </summary>
		/// <remarks> Values &gt; 8 are not supported.
		/// Note that this excludes 'gaia'.	</remarks>
		public int PlayersCount { get; protected set; }
		// TODO: Make a function that sets this value.
		
		/// <summary> Gets the time of when the scenario was last edited in UTC. </summary>
		public DateTime UtcLastEdited { get; protected set; }
		
		/// <summary> Gets the time of when the scenario was last edited in local time. </summary>
		public DateTime LocalLastEdited {
			get { return UtcLastEdited.ToLocalTime(); }
		}
		
		public uint NextUnitId { get; protected set; }
		
		/// <summary> Gets or sets the text that is displayed at the scenario introduction screen. </summary>
		public string ScenarioIntroductionText { get; set; }
		
		// Start of compressed data.
		static readonly DateTime unixEpoch = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
		const int versionLength = 4;
		
		public static string tesTFile;
		public uint Unknown2222;
		
		public static Scenario FromFile( string path ) {
			tesTFile = path;
			using( FileStream fs = File.OpenRead( path ) ) {
				return FromStream( fs );
			}
		}
		
		public static Scenario FromStream( Stream stream ) {
			if( stream == null ) throw new ArgumentNullException( "stream" );
			PrimitiveReader reader = new PrimitiveReader( stream );
			string version1 = reader.ReadASCIIString( versionLength );
			if( version1[1] != '.' ) {
				throw new InvalidDataException( "Expected period for the second character of the version1 string." );
			}
			
			uint dataOffset = reader.ReadUInt32();
			uint headerVersion = reader.ReadUInt32();
			if( headerVersion > 2 ) {
				throw new InvalidDataException( "Header version cannot be greater than 2." );
			}
			
			DateTime utcTime = unixEpoch;
			if( headerVersion >= 2 ) {
				uint secondsSinceEpoch = reader.ReadUInt32();
				utcTime = unixEpoch.AddSeconds( secondsSinceEpoch );
			}
			
			string scenarioIntroduction = Utils.ReadUInt32LengthPrefixedString( reader );
			uint unknown = reader.ReadUInt32();
			uint playersCount = reader.ReadUInt32();
			if( playersCount > 16 ) { // TODO: Should this be 8 instead?
				throw new InvalidDataException( "The genie engine only supports up to 16 characters at maximum." );
			}
			
			// Read the compressed header
			int compressedLength = (int)( stream.Length - stream.Position );
			byte[] buffer = reader.ReadBytes( compressedLength );
			byte[] data = Ionic.Zlib.DeflateStream.UncompressBuffer( buffer );
			
			#if DUMP_RAW
			string file = Path.GetFileNameWithoutExtension( ( stream as FileStream ).Name );
			Console.WriteLine( "FILE:" + file );
			File.WriteAllBytes( file + ".bin", data );
			#endif
			reader.Stream = new MemoryStream( data );
			
			uint nextUnitId = reader.ReadUInt32();
			float version2Raw = reader.ReadFloat32();
			string version2 = version2Raw.ToString( "0.00" );
			
			Scenario scenario;
			if( version1 == "1.18" & version2 == "1.20" ) {
				scenario = new AokScenario();
			} else if( version1 == "1.21" && version2 == "1.22" ) {
				scenario = new AokTcScenario();
			} else {
				string message = String.Format( "Processing versions: {0}, {1} is not supported.", version1, version2 );
				throw new NotImplementedException( message );
			}
			scenario.Version1 = Version.Parse( version1 );
			scenario.Version2 = Version.Parse( version2 );
			scenario.UtcLastEdited = utcTime;
			scenario.ScenarioIntroductionText = scenarioIntroduction;
			scenario.PlayersCount = (int)playersCount;
			scenario.Unknown2222 = unknown;
			scenario.NextUnitId = nextUnitId;
			scenario.ReadCompressedData( reader );
			
			reader.Stream = stream;
			return scenario;
		}
		
		protected virtual void ReadCompressedData( PrimitiveReader reader ) {
		}
		
		public void WriteToStream( Stream stream ) {
			BinaryWriter writer = new BinaryWriter( stream );
			Utils.WriteFixedString( writer, Version1.ToString( 2 ), 4 ); // Version1
			writer.Write( uint.MaxValue ); // Data offset / length of header
			writer.Write( (uint)2 ); // Header version
			writer.Write( Utils.GetCurrentUnixTime() ); // Timestamp
			Utils.WriteUInt32LengthPrefixedString( writer, ScenarioIntroductionText );
			writer.Write( 0 );
			writer.Write( PlayersCount );
			
			// Come back and write the data offset.
			long position = stream.Position;
			stream.Seek( 4, SeekOrigin.Begin );
			writer.Write( (uint)( position - 8 ) );
			stream.Seek( position, SeekOrigin.Begin );
			
			MemoryStream tempBuffer = new MemoryStream();
			BinaryWriter tempWriter = new BinaryWriter( tempBuffer );
			WriteCompressedHeader( tempWriter );
			WriteCompressedData( tempWriter );
			byte[] compressedData = Ionic.Zlib.DeflateStream.CompressBuffer( tempBuffer.ToArray() );
			writer.Write( compressedData );
		}
		
		void WriteCompressedHeader( BinaryWriter writer ) {
			writer.Write( NextUnitId );
			float version2 = Single.Parse( Version2.ToString( 2 ) );
			writer.Write( version2 );
		}
		
		
		protected virtual void WriteCompressedData( BinaryWriter writer ) {
		}
		
	}
}
