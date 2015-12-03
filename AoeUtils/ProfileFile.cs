using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AoeUtils {
	
	public sealed class ProfileFile {
		
		public Version Version1;
		public Version Version2;
		public uint LastPlayerId;
		public uint SelectedPlayerIndex;
		
		public List<ProfilePlayer> Players;
		
		public static ProfileFile FromStream( PrimitiveReader reader ) {
			ProfileFile profile = new ProfileFile();
			profile.Version1 = Version.Parse( reader.ReadASCIIString( 4 ) );
			if( profile.Version1.Minor >= 6 ) {
				profile.Version2 = Version.Parse( reader.ReadFloat32().ToString( "0.00" ) );
			}
			profile.LastPlayerId = reader.ReadUInt32();
			profile.SelectedPlayerIndex = reader.ReadUInt32();
			
			int playersCount = reader.ReadInt32();
			List<ProfilePlayer> players = new List<ProfilePlayer>( playersCount );
			for( int i = 0; i < playersCount; i++ ) {
				players.Add( ProfilePlayer.ReadFrom( reader ) );
			}
			profile.Players = players;
			return profile;
		}
		
		public static ProfileFile FromStream( Stream stream ) {
			PrimitiveReader reader = new PrimitiveReader( stream );
			return FromStream( reader );
		}
		
		public override string ToString() {
			if( Version2 != null )
				return Version1.ToString( 2 ) + ", " + Version2.ToString( 2 ) + ", " + Players.Count;
			return Version1.ToString( 2 ) + ", " + Players.Count;
		}
	}
	
	public sealed class ProfilePlayer {
		public string Name;
		public uint PlayerId;
		public uint Unknown;
		public List<ProfileCampaign> Campaigns;
		public byte[] Options;
		
		sealed class ProfilePlayerOptions {
			byte[] rawData;
			
			ushort GetU16( int index ) {
				return (ushort)( rawData[index] | rawData[index + 1] << 8 );
			}
			
			short GetI16( int index ) {
				return (short)( rawData[index] | rawData[index + 1] << 8 );
			}
			
			uint GetU32( int index ) {
				return (uint)( rawData[index] | ( rawData[index + 1] << 8 ) |
				              ( rawData[index + 2 ] << 16 ) | rawData[index + 3] << 24 );
			}
			
			int GetI32( int index ) {
				return rawData[index] | ( rawData[index + 1] << 8 ) |
					( rawData[index + 2 ] << 16 ) | rawData[index + 3] << 24;
			}
			
			public ProfilePlayerOptions( byte[] options ) {
				rawData = options;
			}
			
			public ushort DisplayWidth {
				get { return GetU16( 510 ); }
			}
			
			public int DisplayHeight {
				get { return GetU16( 512 ); }
			}
			
			public bool AudioTauntsEnabled {
				get { return rawData[514] != 0; }
			}
			
			public bool OneClickGarisoning {
				get { return rawData[515] != 0; }
			}
			
			public byte TwoMouseMode {
				get { return rawData[516]; }
			}
			
			public bool FriendOrFoeColours {
				get { return rawData[517] != 0; }
			}
			
			public bool UnknownBool {
				get { return rawData[518] != 0; }
			}
			
			public bool StatisticsDisplayed {
				get { return rawData[519] != 0; }
			}
			
			public bool TimeCounterDisplayed {
				get { return rawData[520] != 0; }
			}
			
			public bool ChosenDifficultyMode {
				get { return rawData[521] != 0; }
			}
			
			public string StandardGameMapName {
				get { return Utils.GetFixedString( rawData, 43, 128 ); }
			}
			
			public string MultiplayerGameMapName {
				get { return Utils.GetFixedString( rawData, 299, 128 ); }
			}
			
			public byte[] Un1 {
				get {
					byte[] test = new byte[8 * 9];
					Buffer.BlockCopy( rawData, 171, test, 0, 8 * 9 );
					return test;
				}
			}
			
			public byte[] Un2 {
				get {
					byte[] test = new byte[7 * 9];
					Buffer.BlockCopy( rawData, 427, test, 0, 7 * 9 );
					return test;
				}
			}
		}
		
		const int nameLength = 255;
		
		public static ProfilePlayer ReadFrom( PrimitiveReader reader ) {
			ProfilePlayer player = new ProfilePlayer();
			player.Name = reader.ReadFixedString( nameLength, Utils.TextEncoding );
			player.PlayerId = reader.ReadUInt32();
			player.Unknown = reader.ReadUInt32();
			
			int campaignsCount = reader.ReadInt32();
			List<ProfileCampaign> campaigns = new List<ProfileCampaign>( campaignsCount );
			for( int i = 0; i < campaignsCount; i++ ) {
				campaigns.Add( ProfileCampaign.ReadFrom( reader ) );
			}
			player.Campaigns = campaigns;		
			player.Options = reader.ReadBytes( 522 );
			var options2 = new ProfilePlayerOptions( player.Options );
			return player;
		}
		
		public override string ToString() {
			return Name + ", " + Campaigns.Count;
		}
	}
	
	public sealed class ProfileCampaign {
		public string Name;
		public uint Count1; // not sure of purpose
		public uint ScenariosCount;
		public uint Count2; // not sure of purpose
		public uint Unknown1;
		public uint Unknown2;
		public ScenarioStatus[] Statuses;
		
		const int nameLength = 255;
		
		public static ProfileCampaign ReadFrom( PrimitiveReader reader ) {
			ProfileCampaign campaign = new ProfileCampaign();
			campaign.Name = reader.ReadFixedString( nameLength, Utils.TextEncoding );
			campaign.Count1 = reader.ReadUInt32();
			campaign.ScenariosCount = reader.ReadUInt32();
			campaign.Count2 = reader.ReadUInt32();
			campaign.Unknown1 = reader.ReadUInt32();
			campaign.Unknown2 = reader.ReadUInt32();
			ScenarioStatus[] statuses = new ScenarioStatus[(int)campaign.ScenariosCount];
			for( int i = 0; i < statuses.Length; i++ ) {
				statuses[i] = (ScenarioStatus)reader.ReadUInt8();
			}
			campaign.Statuses = statuses;
			return campaign;
		}
		
		public override string ToString() {
			return Name + ", " + ScenariosCount;
		}

	}
	
	public enum ScenarioStatus : byte {
		NotUnlocked = 0x00,
		Completed = 0x01,
		Unlocked = 0x02,
	}
}
