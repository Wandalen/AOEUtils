using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace AoeUtils {
	
	public sealed class SlpFile {
		
		public static SlpFile FromStream( Stream stream ) {
			PrimitiveReader reader = new PrimitiveReader( stream );
			string version = reader.ReadASCIIString( 3 );
			string unknown = reader.ReadASCIIString( 1 );
			
			int framesCount = reader.ReadInt32();
			// Either 'ArtDesk SLP 1.00 writer' or
			// 'RGE RLE shape file'
			string comment = reader.ReadASCIIString( 24 );
			
			SlpFrameInfo[] frames = new SlpFrameInfo[framesCount];
			for( int i = 0; i < frames.Length; i++ ) {
				frames[i] = SlpFrameInfo.ReadFrom( reader );
				SlpFrameInfo info = frames[i];
			}
			ReadFrameData( reader, frames[0] );
			throw new NotImplementedException();
			return null;
		}
		
		public static SlpFile FromFile( string file ) {
			using( FileStream fs = File.OpenRead( file ) ) {
				return FromStream( fs );
			}
		}
		
		static void ReadFrameData( PrimitiveReader reader, SlpFrameInfo frame ) {
			long pos1 = reader.Stream.Position;
			int height = frame.Height;
			int rowCount = Math.Abs( height );
			LineMask[] masks = new LineMask[rowCount];
			for( int i = 0; i < rowCount; i++ ) {
				masks[i] = LineMask.ReadFrom( reader );
			}
			
			long pos2 = reader.Stream.Position;
			uint[] commandOffsets = new uint[rowCount];
			for( int i = 0; i < commandOffsets.Length; i++ ) {
				commandOffsets[i] = reader.ReadUInt32();
			}
			System.Diagnostics.Debugger.Break();
			
			SlpGraphic graphic = new SlpGraphic( frame.Width, height );
			
			Console.WriteLine( "POS :" + reader.Stream.Position );
			for( int i = 0; i < masks.Length; i++ ) {
				Console.WriteLine( "CHECK:" + reader.Stream.Position + "," + commandOffsets[i] );
				ProcessRow( reader, i, graphic, masks[i] );
			}
			Console.WriteLine( "POS2:" + reader.Stream.Position );
			
			long ttt = 0;
			using( StreamWriter tempWriter = new StreamWriter( Path.Combine( "tests", "log.txt" ) ) ) {
				foreach( string file in Directory.EnumerateFiles( "pals" ) ) {
					Palette palette = Palette.FromFile( file );
					using( Bitmap bmp = graphic.CreateBitmap( palette ) ) {
						bmp.Save( Path.Combine( "tests", ttt + ".bmp" ) );
						//bmp.Save( Path.Combine( "tests", ttt + ".png" ), ImageFormat.Png );
					}
					ttt++;
					tempWriter.WriteLine( ttt + " : " + file );
				}
			}			
			throw new Exception();
		}
		
		class SlpGraphic {
			public short[] Indices;
			
			public readonly int Width;
			public readonly int Height;
			
			public short this[int x, int y] {
				get { return Indices[( Width * y ) + x]; }
				set { Indices[( Width * y ) + x] = value; }
			}
			
			GCHandle handle;
			
			public IntPtr LockBits() {
				if( handle.IsAllocated ) {
					throw new InvalidOperationException( "Bits already locked." );
				}
				handle = GCHandle.Alloc( Indices, GCHandleType.Pinned );
				return handle.AddrOfPinnedObject();
			}
			
			public void UnlockBits() {
				if( !handle.IsAllocated ) {
					throw new InvalidOperationException( "Bits not locked" );
				}
				handle.Free();
			}
			
			public SlpGraphic( int width, int height ) {
				Indices = new short[width * height];
				Height = height;
				Width = width;
			}
			
			public unsafe Bitmap CreateBitmap( Palette palette ) {
				Bitmap bmp = new Bitmap( Width, Height );
				BitmapData bmd = bmp.LockBits( new Rectangle( 0, 0, Width, Height ), ImageLockMode.WriteOnly, bmp.PixelFormat );
				
				// Sequential scan
				short[] indices = Indices;
				for( int i = 0; i < indices.Length; i++ ) {
					int* ptr = (int*)( (byte*)bmd.Scan0 );
					short index = indices[i];
					if( index == -1 ) {
						ptr[i] = 0;
					} else {
						ptr[i] = palette.coloursRaw[index];
					}
				}
				bmp.UnlockBits( bmd );
				return bmp;
			}
		}
		
		static void ProcessRow( PrimitiveReader reader, int row, SlpGraphic graphic, LineMask mask ) {
			int i = 0;
			
			// Some fully transparent rows just have '-32768' for left and the command '15'.
			// Thus the entire row must be filled in here.
			if( mask.Left == short.MinValue )
				mask.Left = (short)graphic.Width;
				
			for( ; i < mask.Left; i++ ) {
				graphic[i, row] = -1;
			}
			while( ProcessCommand( reader, row, graphic, ref i ) );
			
			int max = i + mask.Right;
			for( ; i < max; i++ ) {
				graphic[i, row] = -1;
			}
		}
		
		static bool ProcessCommand( PrimitiveReader reader, int row, SlpGraphic graphic, ref int index ) {
			// Command type       | Binary
			// Colour list        | xxxx xx00
			// Skip               | xxxx xx01
			// Big colour list    | xxxx 0010 | xxxx xxxx
			// Big skip           | xxxx 0011 | xxxx xxxx
			// Player colour list | xxxx 0110
			// Fill               | xxxx 0111
			// Player colour fill | xxxx 1010
			// Shadow transparent | xxxx 1011
			// Shadow player      | 0000 1110 | xxxx xxxx
			// End of row         | 0000 1111
			// Outline            | 0100 1110
			// Outline span       | 0101 1110 | xxxx xxxx
			
			int command = reader.ReadUInt8();
			if( ( command & 0x01 ) == 0 ) { // Commands of xxxx xxx0
				if( ( command & 0x02 ) == 0 ) { // Colour list
					int pixelsCount = command >> 2;
					for( int i = 0; i < pixelsCount; i++ ) {
						graphic[index + i, row] = reader.ReadUInt8();
					}
					index += pixelsCount;
				} else {
					int commandType = command & 0x0F;
					if( commandType == 0x02 ) { // Big colour list
						int pixelsCount =  ( ( command >> 4 ) << 8 ) + reader.ReadUInt8();
						for( int i = 0; i < pixelsCount; i++ ) {
							graphic[index + i, row] = reader.ReadUInt8();
						}
						index += pixelsCount;
					} else if( commandType == 0x06 ) { // Player colour list
						int pixelsCount = ( command >> 4 );
						if( pixelsCount == 0 ) pixelsCount = reader.ReadUInt8();
						
						for( int i = 0; i < pixelsCount; i++ ) {
							graphic[index + i, row] = (short)( reader.ReadUInt8() + ( 1 << 4 ) + 16 );
						}
						index += pixelsCount;
					} else if( commandType == 0x0A ) { // Player colour fill
						int pixelsCount = ( command >> 4 );
						if( pixelsCount == 0 ) pixelsCount = reader.ReadUInt8();
						short paletteIndex = (short)( reader.ReadUInt8() + ( 1 << 4 ) + 16 );
						for( int i = 0; i < pixelsCount; i++ ) {
							graphic[index + i, row] = paletteIndex;
						}
						index += pixelsCount;
					} else if( commandType == 0x0E ) {
						if( command == 0x0E ) { // Shadow player
							throw new NotSupportedException();
						} else if( command == 0x4E ) { // Outline
							throw new NotSupportedException();
						} else if( command == 0x5E ) { // Outline span
							throw new NotSupportedException();
						} else {
							throw new NotSupportedException( "Unsupported command: 0x" + command.ToString( "X2" ) );
						}
					} else {
						throw new NotSupportedException( "Unsupported command: 0x" + command.ToString( "X2" ) );
					}
				}
			} else { // Commands of xxxx xxx1
				if( ( command & 0x02 ) == 0 ) { // Skip
					int pixelsCount = ( command >> 2 );
					if( pixelsCount == 0 ) pixelsCount = reader.ReadUInt8();
					for( int i = 0; i < pixelsCount; i++ ) {
						graphic[index + i, row] = -1;
					}
					index += pixelsCount;
				} else {
					int commandType = command & 0x0F;
					if( commandType == 0x03 ) { // Big skip
						int pixelsCount =  ( ( command >> 4 ) << 8 ) + reader.ReadUInt8();
						for( int i = 0; i < pixelsCount; i++ ) {
							graphic[index + i, row] = -1;
						}
						index += pixelsCount;
					} else if( commandType == 0x07 ) { // Fill
						int pixelsCount = ( command >> 4 );
						if( pixelsCount == 0 ) pixelsCount = reader.ReadUInt8();
						byte paletteIndex = reader.ReadUInt8();
						for( int i = 0; i < pixelsCount; i++ ) {
							graphic[index + i, row] = paletteIndex;
						}
						index += pixelsCount;
					} else if( commandType == 0x0B ) { // Shadow transparent
						throw new NotSupportedException();
					} else if( commandType == 0x0F ) { // End of row
						Console.WriteLine( "STOP " + reader.Stream.Position );
						return false;
					} else {
						throw new NotSupportedException( "Unsupported command: 0x" + command.ToString( "X2" ) );
					}
				}
			}
			return true;
		}
		
		struct SlpFrameInfo {
			
			/// <summary> Specified the position of the first command offset. </summary>
			public uint CommandTableOffset;
			
			/// <summary> Specifies the location of the first line mask. </summary>
			public uint OutlineTableOffset;
			
			public uint PaletteOffset;
			public uint Properties;
			
			public int Width;
			public int Height;
			public int HotspotX;
			public int HotspotY;
			
			public static SlpFrameInfo ReadFrom( PrimitiveReader reader ) {
				SlpFrameInfo info;
				info.CommandTableOffset = reader.ReadUInt32();
				info.OutlineTableOffset = reader.ReadUInt32();
				info.PaletteOffset = reader.ReadUInt32();
				info.Properties = reader.ReadUInt32();
				
				info.Width = reader.ReadInt32();
				info.Height = reader.ReadInt32();
				info.HotspotX = reader.ReadInt32();
				info.HotspotY = reader.ReadInt32();
				return info;
			}
		}
		
		struct LineMask {
			public short Left, Right;
			
			public static LineMask ReadFrom( PrimitiveReader reader ) {
				LineMask lineMask;
				lineMask.Left = reader.ReadInt16();
				lineMask.Right = reader.ReadInt16();
				return lineMask;
			}
			
			public override string ToString() {
				return Left + "," + Right;
			}

		}
	}
}
