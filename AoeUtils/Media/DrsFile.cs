using System;
using System.IO;
using System.Text;

namespace AoeUtils {
	
	public sealed class DrsFile {
		
		public string CopyrightInfo { get; private set; }
		
		public string FileVersion { get; private set; }
		
		public string FileType { get; private set; }
		
		public Table[] Tables { get; set; }
		
		const int copyrightLength = 40;
		const int versionLength = 4;
		const int typeLength = 12;
		
		public static DrsFile FromFile( string path ) {
			using( FileStream fs = File.OpenRead( path ) ) {
				return FromStream( fs );
			}
		}
		
		public static DrsFile FromStream( Stream stream ) {
			if( stream == null ) throw new ArgumentNullException( "stream" );
			
			DrsFile file = new DrsFile();
			PrimitiveReader reader = new PrimitiveReader( stream );
			file.CopyrightInfo = reader.ReadASCIIString( copyrightLength );
			file.FileVersion = reader.ReadASCIIString( versionLength );
			file.FileType = reader.ReadASCIIString( typeLength );
			
			uint tablesCount = reader.ReadUInt32();
			uint firstFileOffset = reader.ReadUInt32();
			
			Table[] tables = new Table[(int)tablesCount];
			for( int i = 0; i < tables.Length; i++ ) {
				Table table = new Table();
				table.Unknown = reader.ReadUInt8();
				byte[] extension = reader.ReadBytes( 3 );
				extension = new [] { extension[2], extension[1], extension[0] }; // Stored in reversed form in DRS file.
				table.Extension = Encoding.ASCII.GetString( extension );
				table.TableOffset = reader.ReadUInt32();
				table.FilesCount = reader.ReadUInt32();
				Console.WriteLine( "Table {0}: {1} elements of {2}", i, table.FilesCount, table.Extension );
				tables[i] = table;
			}
			
			System.Diagnostics.Debugger.Break();
			for( int i = 0; i < tables.Length; i++ ) {
				Table table = tables[i];
				stream.Seek( table.TableOffset, SeekOrigin.Begin );
				TableFile[] files = new TableFile[(int)table.FilesCount];
				string directory = "table_" + i;
				Directory.CreateDirectory( directory );
				string extension = "." + table.Extension;
				
				for( int j = 0; j < files.Length; j++ ) {
					TableFile element = new TableFile();
					element.FileId = reader.ReadUInt32();
					uint offset = reader.ReadUInt32();
					uint size = reader.ReadUInt32();
					
					long position = stream.Position;
					stream.Seek( offset, SeekOrigin.Begin );
					element.Data = reader.ReadBytes( (int)size );
					stream.Seek( position, SeekOrigin.Begin );
					files[j] = element;
					
					string path = Path.Combine( directory, element.FileId + extension );
					File.WriteAllBytes( path, element.Data );
					
				}
				table.Files = files;
			}
			file.Tables = tables;
			System.Diagnostics.Debugger.Break();
			return file;
		}
		
		public void WriteTo( Stream stream ) {
			BinaryWriter writer = new BinaryWriter( stream );
			Utils.WriteAsciiString( stream, CopyrightInfo );
			Utils.WriteAsciiString( stream, FileVersion );
			Utils.WriteAsciiString( stream, FileType );
			
			Table[] tables = Tables;
			writer.Write( tables.Length );
			long firstOffsetPosition = stream.Position;
			writer.Write( 1 ); // Come back to write this later.
			throw new NotImplementedException();
		}
	}
	
	public class Table {
		public byte Unknown;
		
		public string Extension;
		
		internal uint TableOffset;
		
		internal uint FilesCount;
		
		public TableFile[] Files;
	}
	
	public struct TableFile {
		public uint FileId;
		public byte[] Data;
	}
}