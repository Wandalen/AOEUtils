using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AoeUtils {
	
	public class Campaign {
		
		const int versionLength = 4;
		const int campaignNameLength = 256;
		const int scenarioNameLength = 255;
		const int scenarioFilenameLength = 257;
		
		string _campaignName;
		List<CampaignScenario> _scenarios;
		
		public string CampaignName {
			get { return _campaignName; }
			set {
				Utils.CheckFixedString( value, campaignNameLength );
				_campaignName = value;
			}
		}
		
		public List<CampaignScenario> Scenarios {
			get { return _scenarios; }
		}	
		
		public void WriteToStream( Stream output ) {
			if( output == null ) throw new ArgumentNullException( "output" );
			
			Utils.WriteFixedString( output, "1.00", versionLength ); // There isn't really much point
			// in allowing users to edit this version field, since all genie engine games use '1.00'.
			Utils.WriteFixedString( output, _campaignName, campaignNameLength );
			BinaryWriter writer = new BinaryWriter( output );
			writer.Write( _scenarios.Count );
			
			// Since the header is a fixed size, the offset can be calculated
			// in the same pass as writing the scenarios.
			int offset = CalculateFirstOffset();
			
			foreach( CampaignScenario scenario in _scenarios ) {
				int size = scenario.Data.Length;
				writer.Write( size );
				writer.Write( offset );
				Utils.WriteFixedString( output, scenario.Name, scenarioNameLength );
				Utils.WriteFixedString( output, scenario.Filename, scenarioFilenameLength );
				offset += size;
			}
			
			_scenarios.ForEach( s => writer.Write( s.Data ) );
		}
		
		public override string ToString() {
			return String.Format( "{0} ({1} scenarios)", _campaignName, _scenarios.Count );
		}
		
		int CalculateFirstOffset() {
			return versionLength + campaignNameLength + 4 +
				_scenarios.Count * ( 4 + 4 + scenarioNameLength + scenarioFilenameLength );
		}
		
		public static Campaign FromFile( string file ) {
			using( FileStream fs = File.OpenRead( file ) ) {
				return FromStream( fs );
			}
		}
		
		public static Campaign FromStream( Stream stream ) {
			Campaign campaign = new Campaign();
			PrimitiveReader reader = new PrimitiveReader( stream );
			
			string rawVersion = reader.ReadASCIIString( versionLength );
			campaign._campaignName = reader.ReadFixedString( campaignNameLength, Utils.TextEncoding );
			int scenariosCount = (int)reader.ReadUInt32();
			List<CampaignScenario> scenarios = new List<CampaignScenario>( scenariosCount );
			uint[] offsets = new uint[scenariosCount];
			uint[] sizes = new uint[scenariosCount];
			
			// Scenarios layout
			// | All metadata      |
			// | All raw scenarios |
			for( int i = 0; i < scenariosCount; i++ ) {
				CampaignScenario element = new CampaignScenario();
				sizes[i] = reader.ReadUInt32();
				offsets[i] = reader.ReadUInt32();
				element.Name = reader.ReadFixedString( scenarioNameLength, Utils.TextEncoding );
				element.Filename = reader.ReadFixedString( scenarioFilenameLength, Utils.TextEncoding );
				scenarios.Add( element );
			}
			
			// TODO: This could probably be simplified to just reading
			// the scenarios without seeking, since the scenarios are
			// almost always stored in order.
			for( int i = 0; i < scenarios.Count; i++ ) {
				CampaignScenario element = scenarios[i];
				uint offset = offsets[i];
				int length = (int)sizes[i];
				stream.Seek( offset, SeekOrigin.Begin );
				element.Data = reader.ReadBytes( length );
			}
			campaign._scenarios = scenarios;
			return campaign;
		}
	}
	
	public class CampaignScenario {
		// TODO: Make these into a fixed size string
		public string Name;
		
		public string Filename;
		
		public byte[] Data;
		
		public override string ToString() {
			return Name;
		}
	}
}
