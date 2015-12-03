using System;
using System.IO;

namespace AoeUtils {
	
	/// <summary> Age of Empires 2: The Conquerors final version scenario. </summary>
	public class AokTcScenario : AokScenario {
		
		public IndexedString Scouts { get; protected set; }
		
		public AiMapType MapAiType { get; set; }
		
		protected override void ReadMessages( PrimitiveReader reader ) {
			Instructions = new IndexedString();
			Hints = new IndexedString();
			Victory = new IndexedString();
			Loss = new IndexedString();
			History = new IndexedString();
			Scouts = new IndexedString();
			
			Instructions.Index = reader.ReadUInt32();
			Hints.Index = reader.ReadUInt32();
			Victory.Index = reader.ReadUInt32();
			Loss.Index = reader.ReadUInt32();
			History.Index = reader.ReadUInt32();
			Scouts.Index = reader.ReadUInt32();
			
			Instructions.rawValue = Utils.ReadUInt16LengthPrefixedString( reader );
			Hints.rawValue = Utils.ReadUInt16LengthPrefixedString( reader );
			Victory.rawValue = Utils.ReadUInt16LengthPrefixedString( reader );
			Loss.rawValue = Utils.ReadUInt16LengthPrefixedString( reader );
			History.rawValue = Utils.ReadUInt16LengthPrefixedString( reader );
			Scouts.rawValue = Utils.ReadUInt16LengthPrefixedString( reader );
		}
		
		protected override void ReadMap( PrimitiveReader reader ) {
			uint separator = reader.ReadUInt32();
			if( separator != separatorValue ) {
				throw new InvalidDataException( "Expected separator value." );
			}
			int editorCameraY = reader.ReadInt32();
			int editorCameraX = reader.ReadInt32();
			CameraPosition = new Point2I( editorCameraX, editorCameraY );
			AiMapType aiType = (AiMapType)reader.ReadInt32();
			MapAiType = aiType;
			GameMap = Map.ReadFrom( reader );
		}
		
		protected override PlayerData4 ReadPlayerData4( PrimitiveReader reader ) {
			PlayerData4 value = new PlayerData4();
			value.FoodCount = reader.ReadFloat32();
			value.WoodCount = reader.ReadFloat32();
			value.GoldCount = reader.ReadFloat32();
			value.StoneCount = reader.ReadFloat32();
			value.OreXCount = reader.ReadFloat32();
			value.OreYCount = reader.ReadFloat32();
			value.PopulationLimit = reader.ReadFloat32();
			return value;
		}
		
		protected override void WriteMap( BinaryWriter writer ) {
			writer.Write( separatorValue );
			writer.Write( CameraPosition.Y );
			writer.Write( CameraPosition.X );
			writer.Write( (int)MapAiType );
			GameMap.WriteData( writer );
		}
	}
}
