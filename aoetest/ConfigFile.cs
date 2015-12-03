using System;
using System.Collections.Generic;
using System.IO;

namespace IsometricTests {
	
	// Hashcode and comparisons ignore offsets
	sealed class ConfigNode : IEquatable<ConfigNode>, IEquatable<string> {
		public string Value;
		public int Offset;
		
		public override int GetHashCode() {
			return Value.GetHashCode();
		}
		
		public bool Equals( ConfigNode value ) {
			return Value == value.Value;
		}
		
		public bool Equals( string value ) {
			return Value == value;
		}
		
		public static implicit operator ConfigNode( string value ) {
			return new ConfigNode() { Value = value };
		}
		
		private ConfigNode() { }
		
		public ConfigNode( string value, int position ) {
			Value = value;
			Offset = position;
		}

	}
	
	/// <summary> Manages the configuration file for a ManicDigger instance. </summary>
	public sealed class ConfigFile {
		
		int lastOffset = 0;
		
		/// <summary> Gets the relative file path of the configuration file. </summary>
		public string RelativeConfigFile { get; private set; }
		
		/// <summary> The separator between keys and values. </summary>
		public char Separator { get; set; }
		
		private Dictionary<ConfigNode, string> config = new Dictionary<ConfigNode, string>();
		private List<ConfigNode> comments = new List<ConfigNode>();
		
		public ConfigFile( string file ) {
			RelativeConfigFile = file;
			Separator = ':';
		}
		
		/// <summary> Checks whether the configuration file exists. </summary>
		/// <returns> true if the file exists, otherwise false. </returns>
		public bool Exists() {
			return File.Exists( RelativeConfigFile );
		}
		
		static readonly string[] defaultconfig = new string[] {
			"# The colours to use for .",
		};
		
		/// <summary> Writes default config settings. </summary>
		public void WriteDefaultConfig() {
			using( StreamWriter sw = new StreamWriter( RelativeConfigFile ) ) {
				for( int i = 0; i < defaultconfig.Length; i++ ) {
					sw.WriteLine( defaultconfig[i] );
				}
			}
		}
		
		/// <summary> Attempts to load configuration from the specified file. </summary>
		/// <returns> true on success, false on failure. </returns>
		public bool Load() {
			if( !Exists() ) return false;
			
			try {
				string line;
				using( FileStream fs = File.OpenRead( RelativeConfigFile ) ) {
					using( StreamReader reader = new StreamReader( fs ) ) {
						while( ( line = reader.ReadLine() ) != null ) {
							if( line.Length == 0 ) continue; // Comment or empty line.
							
							if( line.StartsWith( "#" ) ) {
								comments.Add( new ConfigNode( line, lastOffset ) );
								lastOffset++;
								continue;
							}
							
							string[] keypair = line.Split( new [] { Separator }, 2 ); // key:value
							string key = keypair[0].ToLowerInvariant(); // Case insensitive keys.
							string value = keypair[1].Trim();
							config.Add( new ConfigNode( key, lastOffset ), value );
							lastOffset++;
						}
					}
				}
				return true;
			} catch( Exception e ) {
				Console.WriteLine( "Error while loading config: " + e.ToString() );
				return false;
			}
		}
		
		/// <summary> Gets the raw value associated with the specified configuration key. Throws on error. </summary>
		/// <param name="configKey"> The key of the configuration option. </param>
		/// <exception cref="System.ArgumentNullException"> configKey is null. </exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException"> No value was found for the specified key. </exception>
		/// <returns> The value of the specified key. </returns>
		public string GetRawValue( string configKey ) {
			if( configKey == null ) throw new ArgumentNullException( "configKey" );
			string key = configKey.ToLowerInvariant();
			return config[key];
		}
		
		/// <summary> Attempts to get the raw value associated with the specified configuration key. </summary>
		/// <param name="configKey"> The key of the configuration option. </param>
		/// <param name="value"> The value of the config key, assigned by the method. </param>
		/// <exception cref="System.ArgumentNullException"> configKey is null. </exception>
		/// <returns> true if the key was found, assigning the value parameter to the found value.
		/// Otherwise returns false, and value is assigned to null. </returns>
		public bool TryGetRawValue( string configKey, out string value ) {
			if( configKey == null ) throw new ArgumentNullException( "configKey" );
			string key = configKey.ToLowerInvariant();
			return config.TryGetValue( key, out value );
		}
		
		/// <summary> Adds or updates the raw value associated with the specified configuration key. </summary>
		/// <param name="configKey"> The key of the configuration option. </param>
		/// <param name="value"> The value of the config key. </param>
		/// <exception cref="System.ArgumentNullException"> configKey is null. </exception>
		/// <exception cref="System.ArgumentException"> configKey is either an empty string, or starts with a # symbol. </exception>
		public void AddOrUpdateValue( string configKey, string value ) {
			if( configKey == null ) throw new ArgumentNullException( "configKey" );
			if( configKey.Length == 0 || configKey.StartsWith( "#" ) ) throw new ArgumentException( "Invalid configKey given." );
			string key = configKey.ToLowerInvariant();
			if( !config.ContainsKey( configKey ) ) {
				ConfigNode node = new ConfigNode( key, lastOffset++ );
				config[node] = value;
			} else {
				config[key] = value;
			}
		}
		
		/// <summary> Saves the configuration file, or creates it if doesn't exist. </summary>
		public void Save() {
			using( StreamWriter writer = new StreamWriter( RelativeConfigFile ) ) {
				string[] lines = new string[lastOffset + 1];
				foreach( var keypair in config ) {
					//writer.WriteLine( keypair.Key.Key + ":" + keypair.Value );
					lines[keypair.Key.Offset] = keypair.Key.Value + Separator + keypair.Value;
				}
				for( int i = 0; i < comments.Count; i++ ) {
					lines[comments[i].Offset] = comments[i].Value;
				}
				for( int i = 0; i < lines.Length; i++ ) {
					writer.WriteLine( lines[i] );
				}
			}
		}
		
		/// <summary> Clears all stored keys. Does not effect the file, unless Save() is later called. </summary>
		public void Clear() {
			config.Clear();
		}
		
		/// <summary> Attempts to parse the value of the given configuration key as the specified type. </summary>
		/// <param name="configKey"> The key of the configuration option. </param>
		/// <param name="defaultValue"> The default value to assign if parsing fails. </param>
		/// <param name="value"> The configuration value assigned by the method. </param>
		/// <returns> true on success, false on failure. The method can fail if the key is null,
		/// the key is not found, or if the value could not be parsed. </returns>
		/// <remarks> T can be one of the following types: bool, char, sbyte, short, int, long, byte,
		/// ushort, uint, ulong, float, double, decimal, DateTime, Enum. </remarks>
		public bool TryParseValueOrDefault<T>( string configKey, T defaultValue, out T value ) where T : struct, IConvertible {
			if( configKey == null ) {
				value = defaultValue;
				return false;
			} else {
				Type type = typeof( T );
				bool parsingWorked = false;
				string rawValue;
				if( !config.TryGetValue( configKey, out rawValue ) ) {
					value = defaultValue;
					Console.WriteLine( "Couldn't find key {0} in config. Setting to default value of {1}.", configKey, defaultValue );
					return false;
				}
				
				// Switch case doesn't work, and we can't do straight casts in an out parameter.
				// Ugly if else chain it is.
				if( type == typeof( bool ) ) {
					bool result;
					parsingWorked = Boolean.TryParse( rawValue, out result );
					value = (T)(ValueType)result;
				} else if( type == typeof( char ) ) {
					char result;
					parsingWorked = Char.TryParse( rawValue, out result );
					value = (T)(ValueType)result;
				} else if( type == typeof( sbyte ) ) {
					sbyte result;
					parsingWorked = SByte.TryParse( rawValue, out result );
					value = (T)(ValueType)result;
				} else if( type == typeof( short ) ) {
					short result;
					parsingWorked = Int16.TryParse( rawValue, out result );
					value = (T)(ValueType)result;
				} else if( type == typeof( int ) ) {
					int result;
					parsingWorked = Int32.TryParse( rawValue, out result );
					value = (T)(ValueType)result;
				} else if( type == typeof( long ) ) {
					long result;
					parsingWorked = Int64.TryParse( rawValue, out result );
					value = (T)(ValueType)result;
				} else if( type == typeof( byte ) ) {
					byte result;
					parsingWorked = Byte.TryParse( rawValue, out result );
					value = (T)(ValueType)result;
				} else if( type == typeof( ushort ) ) {
					ushort result;
					parsingWorked = UInt16.TryParse( rawValue, out result );
					value = (T)(ValueType)result;
				} else if( type == typeof( uint ) ) {
					uint result;
					parsingWorked = UInt32.TryParse( rawValue, out result );
					value = (T)(ValueType)result;
				} else if( type == typeof( ulong ) ) {
					ulong result;
					parsingWorked = UInt64.TryParse( rawValue, out result );
					value = (T)(ValueType)result;
				} else if( type == typeof( float ) ) {
					float result;
					parsingWorked = Single.TryParse( rawValue, out result );
					value = (T)(ValueType)result;
				} else if( type == typeof( double ) ) {
					double result;
					parsingWorked = Double.TryParse( rawValue, out result );
					value = (T)(ValueType)result;
				} else if( type == typeof( decimal ) ) {
					decimal result;
					parsingWorked = Decimal.TryParse( rawValue, out result );
					value = (T)(ValueType)result;
				} else if( type == typeof( DateTime ) ) {
					DateTime result;
					parsingWorked = DateTime.TryParse( rawValue, out result );
					value = (T)(ValueType)result;
				} else if( type == typeof( FastColour ) ) {
					FastColour result;
					parsingWorked = FastColour.TryParse( rawValue, out result );
					value = (T)(ValueType)result;
				} else if( type.IsEnum ) {
					try {
						value = (T)(ValueType)Enum.Parse( type, rawValue, true ); // Case insensitive.
						parsingWorked = true;
					} catch( ArgumentException ) {
						value = defaultValue; // Stops compilation errors.
						parsingWorked = false;
					}
				} else {
					value = defaultValue;
					throw new InvalidCastException( "Cannot parse type " + type.Name );
				}
				if( !parsingWorked ) {
					Console.WriteLine( "Couldn't parse value for {0} from config. Setting to default value of {1}.", configKey, defaultValue );
					value = defaultValue;
				}
				return parsingWorked;
			}
		}
	}
}