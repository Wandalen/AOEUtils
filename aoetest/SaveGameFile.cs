using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AoeUtils;

namespace aoetest {

	public class SaveGameFile {
		
		public sealed class AiInfo {
			public ushort Unknown;
			public uint Unknown2;
			public List<string> Resources;
			public byte[] Unknown3;
			public AiData[] Data;
			public byte[] Unknown4;
			public AiTimer[] Timers;
			public uint[] SharedGoals;
			public uint[] Unknown5;
			
			public static AiInfo ReadFrom( PrimitiveReader reader ) {
				AiInfo info = new AiInfo();
				info.Unknown = reader.ReadUInt16();
				ushort stringsCount = reader.ReadUInt16();
				System.Diagnostics.Debugger.Break();
				info.Unknown2 = reader.ReadUInt32();
				List<string> resources = new List<string>( stringsCount );
				for( int i = 0; i < stringsCount; i++ ) {
					resources.Add( Utils.ReadUInt32LengthPrefixedString( reader ) );
				}
				info.Resources = resources;
				info.Unknown3 = reader.ReadBytes( 6 );
				AiData[] data = new AiData[8];
				for( int i = 0; i < data.Length; i++ ) {
					data[i] = AiData.ReadFrom( reader );
				}
				info.Data = data;
				
				info.Unknown4 = reader.ReadBytes( 104 );
				AiTimer[] timers = new AiTimer[8];
				for( int i = 0; i < timers.Length; i++ ) {
					timers[i] = AiTimer.ReadFrom( reader );
				}
				info.Timers = timers;
				info.SharedGoals = reader.ReadUInt32Array( 256 );
				info.Unknown5 = reader.ReadUInt32Array( 1024 );
				return info;
			}
		}
		
		public sealed class AiData {
			public uint Unknown;
			public uint Unknown2;
			public ushort MaximumRules;
			public uint Unknown3;
			public List<AiRule> Rules;
			
			public static AiData ReadFrom( PrimitiveReader reader ) {
				AiData data = new AiData();
				data.Unknown = reader.ReadUInt32();
				data.Unknown2 = reader.ReadUInt32();
				data.MaximumRules = reader.ReadUInt16();
				ushort rulesCount = reader.ReadUInt16();
				data.Unknown3 = reader.ReadUInt32();
				List<AiRule> rules = new List<AiRule>( rulesCount );
				for( int i = 0; i < rulesCount; i++ ) {
					rules.Add( AiRule.ReadFrom( reader ) );
				}
				data.Rules = rules;
				return data;
			}
		}
		
		public sealed class AiRule {
			public byte[] Unknown;
			public byte FactsCount;
			public byte FactsCountAction;
			public ushort Unknown2;
			public AiRuleData[] RuleData;
			
			public static AiRule ReadFrom( PrimitiveReader reader ) {
				AiRule rule = new AiRule();
				rule.Unknown = reader.ReadBytes( 8 );
				rule.FactsCount = reader.ReadUInt8();
				rule.FactsCountAction = reader.ReadUInt8();
				rule.Unknown2 = reader.ReadUInt16();
				AiRuleData[] rulesData = new AiRuleData[8];
				for( int i = 0; i < rulesData.Length; i++ ) {
					rulesData[i] = AiRuleData.ReadFrom( reader );
				}
				return rule;
			}
		}
		
		
		public sealed class AiTimer {
			public uint[] Timers;
			
			public static AiTimer ReadFrom( PrimitiveReader reader ) {
				AiTimer timer = new AiTimer();
				timer.Timers = reader.ReadUInt32Array( 10 );
				return timer;
			}
		}
		
		public sealed class AiRuleData {
			public uint Type;
			public ushort Id;
			public ushort Unknown;
			public uint Param1;
			public uint Param2;
			public uint Param3;
			public uint Param4;
			
			public static AiRuleData ReadFrom( PrimitiveReader reader ) {
				AiRuleData data = new AiRuleData();
				data.Type = reader.ReadUInt32();
				data.Id = reader.ReadUInt16();
				data.Unknown = reader.ReadUInt16();
				data.Param1 = reader.ReadUInt32();
				data.Param2 = reader.ReadUInt32();
				data.Param2 = reader.ReadUInt32();
				data.Param4 = reader.ReadUInt32();
				return data;
			}
		}
		
		public sealed class SaveGameMap {
			
			public struct SaveGameMapTile {
				public byte TerrainType;
				public byte Elevation;
			}
			
			public SaveGameMapTile[] Tiles;
			
			
			public static SaveGameMap ReadFrom( PrimitiveReader reader, int w, int l ) {
				var tiles = new SaveGameMapTile[w * l];
				for( int i = 0; i < tiles.Length; i++ ) {
					tiles[i].TerrainType = reader.ReadUInt8();
					tiles[i].Elevation = reader.ReadUInt8();
				}
				SaveGameMap map = new SaveGameMap();
				map.Tiles = tiles;
				return map;
			}
		}
		
		public sealed class SavePlayerInfo {
			
			sealed class FloatTest {
				float[] elements;
				
				public FloatTest( float[] values ) {
					elements = values;
				}
				
				public float Food {
					get { return elements[0]; }
				}
				
				public float Wood {
					get { return elements[1]; }
				}
				
				public float Gold {
					get { return elements[2]; }
				}
				
				public float Stone {
					get { return elements[3]; }
				}
				
				public float Headroom {
					get { return elements[4]; }
				}
			}
			
			sealed class ResearchStat {
				public short Status;
				public uint Unknown1;
				public uint Unknown2;
				public uint Unknown3;
				
				public static ResearchStat ReadFrom( PrimitiveReader reader ) {
					ResearchStat s = new ResearchStat();
					s.Status = reader.ReadInt16();
					s.Unknown1 = reader.ReadUInt32();
					s.Unknown2 = reader.ReadUInt32();
					s.Unknown3 = reader.ReadUInt32();
					return s;
				}
			}
			
			public sealed class MasterObject {
				
				public byte ObjectType;
				public ushort UnitId;
				public ushort Unknown1;
				public ushort Unknown2;
				public ushort UnitClass;
				public byte[] Unknown3;
				
				public byte[] UnitData;
				
				struct ResourceCost {
					public short Type;
					public short Amount;
					public short Unknown;
					
					public static ResourceCost ReadFrom( PrimitiveReader reader ) {
						ResourceCost value;
						value.Type = reader.ReadInt16();
						value.Amount = reader.ReadInt16();
						value.Unknown = reader.ReadInt16();
						return value;
					}
					
					public override string ToString() {
						return Type + ", " + Amount + ", " + Unknown;
					}

				}
				
				struct AttackStrength {
					public short GroupType;
					public short Amount;
					
					public static AttackStrength ReadFrom( PrimitiveReader reader ) {
						AttackStrength value;
						value.GroupType = reader.ReadInt16();
						value.Amount = reader.ReadInt16();
						return value;
					}
					
					public override string ToString() {
						return GroupType + ", " + Amount;
					}
				}
				
				struct ArmourStrength {
					public short GroupType;
					public short Amount;
					
					public static ArmourStrength ReadFrom( PrimitiveReader reader ) {
						ArmourStrength value;
						value.GroupType = reader.ReadInt16();
						value.Amount = reader.ReadInt16();
						return value;
					}
					
					public override string ToString() {
						return GroupType + ", " + Amount;
					}
				}
				
				public static MasterObject ReadFrom( PrimitiveReader reader, int i ) {
					MasterObject value = new MasterObject();
					
					byte objectType = reader.ReadUInt8();
					value.ObjectType = objectType;
					value.UnitId = reader.ReadUInt16();
					value.Unknown1 = reader.ReadUInt16();
					value.Unknown2 = reader.ReadUInt16();
					value.UnitClass = reader.ReadUInt16();
					value.Unknown3 = reader.ReadBytes( 6 );
					
					const int breakId = -1;
					
					switch( objectType ) {
						default:
							throw new NotImplementedException( "Unsupported object type: " + objectType );
							
						case 10: // Basic units
							{
								short hitpoints = reader.ReadInt16();
								float lengthOfSight = reader.ReadFloat32();
								byte garrisonCapacity = reader.ReadUInt8();
								float sizeRadiusX = reader.ReadFloat32();
								float sizeRadiusY = reader.ReadFloat32();
								ushort resourceCarriage = reader.ReadUInt16();
								float resourceAmount = reader.ReadFloat32();
								
								// Constants?
								ushort unknown12 = reader.ReadUInt16();
								byte unknown13 = reader.ReadUInt8();
								
							}
							break;
							
						case 25: // Same as 20, only DOPL uses this.
						case 20: // Extended basic units (FLAGX)
							{
								value.UnitData = reader.ReadBytes( 28 );
								PrimitiveReader r2 = new PrimitiveReader( value.UnitData );
								short hitpoints = r2.ReadInt16();
								float lengthOfSight = r2.ReadFloat32();
								byte garrisonCapacity = r2.ReadUInt8();
								float sizeRadiusX = r2.ReadFloat32();
								float sizeRadiusY = r2.ReadFloat32();
								
								ulong unknown1 = r2.ReadUInt64();
								ushort unknown2 = r2.ReadUInt16();
								
								if( unknown1 != 0 || unknown2 != 0 ) {
									File.WriteAllBytes( "unitextended.bin", value.UnitData );
									System.Diagnostics.Debugger.Break();
								}
								
								// Constants?
								ushort unknown12 = r2.ReadUInt16();
								byte unknown13 = r2.ReadUInt8();
								
								if( i == breakId ) System.Diagnostics.Debugger.Break();
							} break;
							
						case 30: // Dead units
							{
								value.UnitData = reader.ReadBytes( 32 );			
								if( i == breakId ) System.Diagnostics.Debugger.Break();
							}
							break;
							
						case 60: // Projectiles
							{
								short hitpoints = reader.ReadInt16();
								float lengthOfSight = reader.ReadFloat32();
								byte garrisonCapacity = reader.ReadUInt8();
								float sizeRadiusX = reader.ReadFloat32();
								float sizeRadiusY = reader.ReadFloat32();
								short resourceCarriage = reader.ReadInt16();
								float unknownFloat = reader.ReadFloat32();
								byte unknown2 = reader.ReadUInt8();
								float movementRate = reader.ReadFloat32();
								
								float extraRotationSpeed = reader.ReadFloat32();
								float searchRadius = reader.ReadFloat32();
								float workRate = reader.ReadFloat32();
								short unknown4 = reader.ReadInt16();
								
								ushort attackStrengthsCount = reader.ReadUInt16();
								AttackStrength[] strengths = new AttackStrength[attackStrengthsCount];
								for( int j = 0; j < strengths.Length; j++ ) {
									strengths[j] = AttackStrength.ReadFrom( reader );
								}
								
								ushort armourStrengthsCount = reader.ReadUInt16();
								ArmourStrength[] astrengths = new ArmourStrength[armourStrengthsCount];
								for( int j = 0; j < astrengths.Length; j++ ) {
									astrengths[j] = ArmourStrength.ReadFrom( reader );
								}
								float reloadTime = reader.ReadFloat32();
								float maximumRange = reader.ReadFloat32();
								
								short accuracyPercentage = reader.ReadInt16();
								short projectileUnitId = reader.ReadInt16();
								
								short unknown7 = reader.ReadInt16();
								float displayedAttackRange = reader.ReadFloat32();
								float blastRadius = reader.ReadFloat32();
								float minimumRange = reader.ReadFloat32();
								
								// Constants?
								ushort unknown12 = reader.ReadUInt16();
								byte unknown13 = reader.ReadUInt8();
								
								if( unknown2 != 0 ) System.Diagnostics.Debugger.Break();
								if( i == breakId ) System.Diagnostics.Debugger.Break();
							} break;
							
						case 70: // Living units
							{
								short hitpoints = reader.ReadInt16();
								float lengthOfSight = reader.ReadFloat32();
								byte garrisonCapacity = reader.ReadUInt8();
								float sizeRadiusX = reader.ReadFloat32();
								float sizeRadiusY = reader.ReadFloat32();
								short resourceCarriage = reader.ReadInt16();
								float storageAmount = reader.ReadFloat32(); // e.g. iron boar = 700
								
								byte unknown2 = reader.ReadUInt8();
								float movementRate = reader.ReadFloat32();
								float extraRotationSpeed = reader.ReadFloat32(); // Rotation speed for extra parts - sails for ships.
								
								float searchRadius = reader.ReadFloat32();
								float workRate = reader.ReadFloat32();
								short unknown4 = reader.ReadInt16();
								
								ushort attackStrengthsCount = reader.ReadUInt16();
								AttackStrength[] strengths = new AttackStrength[attackStrengthsCount];
								for( int j = 0; j < strengths.Length; j++ ) {
									strengths[j] = AttackStrength.ReadFrom( reader );
								}
								
								ushort armourStrengthsCount = reader.ReadUInt16();
								ArmourStrength[] astrengths = new ArmourStrength[armourStrengthsCount];
								for( int j = 0; j < astrengths.Length; j++ ) {
									astrengths[j] = ArmourStrength.ReadFrom( reader );
								}
								float reloadTime = reader.ReadFloat32();
								float maximumRange = reader.ReadFloat32();
								
								short accuracyPercentage = reader.ReadInt16();
								short projectileUnitId = reader.ReadInt16();
								
								short unknown7 = reader.ReadInt16();				
								float displayedAttackRange = reader.ReadFloat32();
								float blastRadius = reader.ReadFloat32();
								float minimumRange = reader.ReadFloat32();
								
								ResourceCost res1 = ResourceCost.ReadFrom( reader );
								ResourceCost res2 = ResourceCost.ReadFrom( reader );
								ResourceCost res3 = ResourceCost.ReadFrom( reader );
								short trainingTime = reader.ReadInt16();
								float missileDuplicationAmount = reader.ReadFloat32();
								
								// Constants?
								ushort unknown12 = reader.ReadUInt16();
								byte unknown13 = reader.ReadUInt8();
								
								if( i == breakId ) System.Diagnostics.Debugger.Break();
							}
							break;
							
						case 80: // Buildings
							{
								short hitpoints = reader.ReadInt16();
								float lengthOfSight = reader.ReadFloat32();
								byte garrisonCapacity = reader.ReadUInt8();
								float sizeRadius1 = reader.ReadFloat32();
								float sizeRadius2 = reader.ReadFloat32();
								short resourceCarriage = reader.ReadInt16();
								float storageAmount = reader.ReadFloat32();
								
								byte unknown2 = reader.ReadUInt8();
								float movementRate = reader.ReadFloat32();
								
								float extraRotationSpeed = reader.ReadFloat32();// Rotation speed for extra parts - something for trebuchets.
								float searchRadius = reader.ReadFloat32();
								float workRate = reader.ReadFloat32();
								short unknown4 = reader.ReadInt16();
								
								ushort attackStrengthsCount = reader.ReadUInt16();
								AttackStrength[] strengths = new AttackStrength[attackStrengthsCount];
								for( int j = 0; j < strengths.Length; j++ ) {
									strengths[j] = AttackStrength.ReadFrom( reader );
								}
								
								ushort armourStrengthsCount = reader.ReadUInt16();
								ArmourStrength[] astrengths = new ArmourStrength[armourStrengthsCount];
								for( int j = 0; j < astrengths.Length; j++ ) {
									astrengths[j] = ArmourStrength.ReadFrom( reader );
								}
								float reloadTime = reader.ReadFloat32();
								float maximumRange = reader.ReadFloat32();
								
								short accuracyPercentage = reader.ReadInt16();
								short projectileUnitId = reader.ReadInt16();
								
								short unknown7 = reader.ReadInt16(); // Seems to have something to do with buildings					
								float displayedAttackRange = reader.ReadFloat32();
								float blastRadius = reader.ReadFloat32();
								float minimumRange = reader.ReadFloat32();
								
								ResourceCost res1 = ResourceCost.ReadFrom( reader );
								ResourceCost res2 = ResourceCost.ReadFrom( reader );
								ResourceCost res3 = ResourceCost.ReadFrom( reader );
								short trainingTime = reader.ReadInt16();
								float missileDuplicationAmount = reader.ReadFloat32(); 
								ushort unknown14 = reader.ReadUInt16(); // No clue at all.
								
								// Constants?
								ushort unknown12 = reader.ReadUInt16();
								byte unknown13 = reader.ReadUInt8();
								
								if( i == breakId ) System.Diagnostics.Debugger.Break();
							}
							break;
					}
					return value;
				}
			}
			
			public static SavePlayerInfo ReadFrom( PrimitiveReader reader, int pCount ) {
				SavePlayerInfo p = new SavePlayerInfo();
				byte[] diplomacyFrom = reader.ReadBytes( pCount );
				uint[] diplomacyTo = reader.ReadUInt32Array( 9 );
				uint unknown1 = reader.ReadUInt32();
				byte unknown2 = reader.ReadUInt8();
				string name = Utils.ReadUInt16LengthPrefixedString( reader );
				byte unknown3 = reader.ReadUInt8();
				int floatsCount = reader.ReadInt32();
				byte unknown4 = reader.ReadUInt8();
				float[] civHeader = reader.ReadFloat32Array( floatsCount );
				FloatTest t = new FloatTest( civHeader );
				
				byte unknown5 = reader.ReadUInt8();
				float[] unknown6 = reader.ReadFloat32Array( 2 );
				byte[] unknown7 = reader.ReadBytes( 9 );
				byte civilization = reader.ReadUInt8();
				byte[] unknown8 = reader.ReadBytes( 3 );
				
				byte colour = reader.ReadUInt8();
				byte[] unknown9 = reader.ReadBytes( 4183 );
				float unknown10 = reader.ReadFloat32();
				
				int researchCount = reader.ReadInt32();
				ushort unknown11 = reader.ReadUInt16();
				
				ResearchStat[] stats = new ResearchStat[researchCount];
				for( int i = 0; i < stats.Length; i++ ) {
					stats[i] = ResearchStat.ReadFrom( reader );
				}
				
				Console.WriteLine( "LAST KNOWN POS:" + reader.Position );
				
				
				reader.SeekAbsolute( 128358 );
				
				int masterObjectsCount = reader.ReadInt32();
				
				bool[] objectExistsFlags = new bool[masterObjectsCount];
				for( int i = 0; i < objectExistsFlags.Length; i++ ) {
					objectExistsFlags[i] = reader.ReadUInt32() != 0;
				}
				//System.Diagnostics.Debugger.Break();
				ushort unknownObjects = reader.ReadUInt16();
				
				Console.WriteLine( "BEGIN READ OBJECTS.." + reader.Position );
				
				MasterObject[] masterObjects = new MasterObject[masterObjectsCount];
				for( int i = 0; i < masterObjects.Length; i++ ) {
					if( objectExistsFlags[i] ) {
						masterObjects[i] = MasterObject.ReadFrom( reader, i );
					}
				}
				
				System.Diagnostics.Debugger.Break();
				return p;
			}
		}
		
		public string Version;
		public float Unknown1;
		public AiInfo AiInfo2;
		
		public uint Unknown2;
		public uint GameSpeed1;
		public uint Unknown3;
		public uint GameSpeed2;
		public float Unknown4;
		public uint Unknown5;
		public byte[] Unknown6;
		public ushort RecordedGamePlayerNumber;
		public byte PlayersCount;
		public uint Unknown7;
		public byte[] Unknown8;
		public byte[] Unknown9;
		public uint[] Unknown10;
		
		public void ReadData( string path ) {
			PrimitiveReader reader = new PrimitiveReader( path );
			SaveGameFile file = new SaveGameFile();
			file.Version = reader.ReadASCIIString( 8 );
			file.Unknown1 = reader.ReadFloat32();
			
			bool aiDataIncluded = reader.ReadUInt32() != 0;
			if( aiDataIncluded ) {
				file.AiInfo2 = AiInfo.ReadFrom( reader );
			}
			file.Unknown2 = reader.ReadUInt32();
			file.GameSpeed1 = reader.ReadUInt32();
			file.Unknown3 = reader.ReadUInt32();
			file.GameSpeed2 = reader.ReadUInt32();
			file.Unknown4 = reader.ReadFloat32();
			file.Unknown5 = reader.ReadUInt32();
			file.Unknown6 = reader.ReadBytes( 17 );
			file.RecordedGamePlayerNumber = reader.ReadUInt16();
			//System.Diagnostics.Debugger.Break();
			file.PlayersCount = reader.ReadUInt8();
			file.Unknown7 = reader.ReadUInt32();
			file.Unknown8 = reader.ReadBytes( 12 );
			file.Unknown9 = reader.ReadBytes( 14 );
			file.Unknown10 = reader.ReadUInt32Array( 8 );
			
			//System.Diagnostics.Debugger.Break();
			
			reader.SeekAbsolute( 126 );
			int mapWidth = reader.ReadInt32();
			int mapLength = reader.ReadInt32();
			
			uint unknownDataCount = reader.ReadUInt32();
			
			ushort unknown2 = reader.ReadUInt16();
			
			SaveGameMap map = SaveGameMap.ReadFrom( reader, mapWidth, mapLength );
			
			
			/*int unknownIntsArrayCount = reader.ReadInt32();
			uint[][] unknowns = new uint[unknownIntsArrayCount][];
			for( int i = 0; i < unknowns.Length; i++ ) {
				int intsCount = reader.ReadInt32() - 1;
				if( intsCount < 0 ) throw new Exception();
				unknowns[i] = reader.ReadUInt32Array( intsCount );
			}*/
			reader.SeekAbsolute( 29132 );
			
			int mapWidth2 = reader.ReadInt32();
			int mapLength2 = reader.ReadInt32();
			
			uint[] unknownMap2 = reader.ReadUInt32Array( mapWidth2 * mapLength2 );
			
			//byte unknownIntsCount1 = reader.ReadUInt8();
			//uint[] unknownInts = reader.ReadUInt32Array( unknownIntsCount1 );
			
			reader.SeekAbsolute( 88779 );
			SavePlayerInfo f = SavePlayerInfo.ReadFrom( reader, 2 );
			System.Diagnostics.Debugger.Break();
		}
	}
}
