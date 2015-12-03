using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace IsometricTests.Renderers {
	
	public class TileRendererTexturedGLVbo2 : TileRenderer {
		
		const float tileWidth = 1;
		const float tileHeight = 1;
		
		[StructLayoutAttribute( LayoutKind.Sequential, Pack = 1 )]
		struct VertexPos3Col4 {
			public float X, Y, Z;
			public byte R, G, B, A;
			
			public VertexPos3Col4( float x, float y, float z, FastColour col ) {
				X = x; Y = y; Z = z;
				R = col.R; G = col.G; B = col.B; A = col.A;
			}
			
			public static readonly int Stride = Marshal.SizeOf( default( VertexPos3Col4 ) );
		}
		
		[StructLayoutAttribute( LayoutKind.Sequential, Pack = 1 )]
		struct VertexPos3Tex2 {
			public float X, Y, Z;
			public float U, V;
			
			public VertexPos3Tex2( float x, float y, float z, float u, float v ) {
				X = x; Y = y; Z = z;
				U = u; V = v;
			}
			
			public static readonly int Stride = Marshal.SizeOf( default( VertexPos3Tex2 ) );
		}
		
		float X, Y;
		float height;
		float z1, z2;
		
		public override void Start( int terrainTexture ) {
			GL.Enable( EnableCap.Texture2D );
			GL.BindTexture( TextureTarget.Texture2D, terrainTexture );
		}
		
		public override void End() {
			GL.Disable( EnableCap.Texture2D );
		}
		
		int triangles;
		int fullVboId = -1;
		int linesVboId = -1;
		int fullVerticesCount = -1;
		int lineVerticesCount = -1;
		
		public override void RenderTiles( TerrainMap map, ref int totalTriangles ) {
			if( fullVboId == -1 ) {
				Console.WriteLine( "Generating with textures!" );
				fullVboId = GLUtils.GenBuffer();
				linesVboId = GLUtils.GenBuffer();
				var sw = Stopwatch.StartNew();
				VertexPos3Tex2[] vertices = BuildTerrain( map, ref triangles );
				VertexPos3Col4[] lineVertices = BuildTerrainBorders( map, ref triangles );
				long elapsed = sw.ElapsedMilliseconds;
				sw.Stop();
				Console.WriteLine( "Took " + elapsed + " ms to generate the map." );
				int size = vertices.Length * VertexPos3Tex2.Stride;
				GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, fullVboId );
				GL.Arb.BufferData( BufferTargetArb.ArrayBuffer, new IntPtr( size ), vertices, BufferUsageArb.StaticDraw );
				
				size = lineVertices.Length * VertexPos3Col4.Stride;
				GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, linesVboId );
				GL.Arb.BufferData( BufferTargetArb.ArrayBuffer, new IntPtr( size ), lineVertices, BufferUsageArb.StaticDraw );
				GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, 0 );
				
				fullVerticesCount = vertices.Length;
				lineVerticesCount = lineVertices.Length;
				GL.LineWidth( 5 );
			}
			
			GL.EnableClientState( ArrayCap.VertexArray );
			GL.EnableClientState( ArrayCap.TextureCoordArray );
					
			GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, fullVboId );
			GL.VertexPointer( 3, VertexPointerType.Float, VertexPos3Tex2.Stride, new IntPtr( 0 ) );
			GL.TexCoordPointer( 2, TexCoordPointerType.Float, VertexPos3Tex2.Stride, new IntPtr( 12 ) );
			GL.DrawArrays( BeginMode.Quads, 0, fullVerticesCount );
			
			GL.DisableClientState( ArrayCap.TextureCoordArray );
			/*GL.EnableClientState( ArrayCap.ColorArray );
			
			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Line );
			GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, linesVboId );
			GL.VertexPointer( 3, VertexPointerType.Float, VertexPos3Col4.Stride, new IntPtr( 0 ) );
			GL.ColorPointer( 4, ColorPointerType.UnsignedByte, VertexPos3Col4.Stride, new IntPtr( 12 ) );
			GL.DrawArrays( BeginMode.Quads, 0, lineVerticesCount );
			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Fill );
			
			GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, 0 );
			GL.DisableClientState( ArrayCap.ColorArray );*/
			GL.DisableClientState( ArrayCap.VertexArray );
			totalTriangles = triangles;
		}
		
		VertexPos3Tex2[] BuildTerrain( TerrainMap map, ref int totalTriangles ) {
			TileDrawInfo drawInfo = new TileDrawInfo();
			
			int lengthX = map.Width;
			int lengthY = map.Length;
			VertexPos3Tex2[] vertices = new VertexPos3Tex2[lengthX * lengthY * 4];
			int index = 0;
			
			for( int x = 0; x < lengthX; x++ ) {
				for( int y = 0; y < lengthY; y++ ) {
					Tile tile = map[x, y];
					drawInfo.CurrentTile = tile;
					
					drawInfo.MapX = x;
					drawInfo.MapY = y;
					RenderTile( drawInfo, vertices, index );
					index += 4;
				}
			} // end for
			totalTriangles = drawInfo.TotalTriangles;
			return vertices;
		}
		
		VertexPos3Col4[] BuildTerrainBorders( TerrainMap map, ref int totalTriangles ) {
			TileDrawInfo drawInfo = new TileDrawInfo();
			
			int lengthX = map.Width;
			int lengthY = map.Length;
			VertexPos3Col4[] vertices = new VertexPos3Col4[lengthX * lengthY * 4];
			int index = 0;
			
			for( int x = 0; x < lengthX; x++ ) {
				for( int y = 0; y < lengthY; y++ ) {
					Tile tile = map[x, y];
					drawInfo.CurrentTile = tile;
					
					drawInfo.MapX = x;
					drawInfo.MapY = y;
					RenderTileBorders( drawInfo, vertices, index );
					index += 4;
				}
			} // end for
			totalTriangles += drawInfo.TotalTriangles;
			return vertices;
		}
		
		void RenderTile( TileDrawInfo drawInfo, VertexPos3Tex2[] vertices, int index ) {
			Tile tile = drawInfo.CurrentTile;
			X = tile.X;
			Y = tile.Y;
			height = tile.Height;

			RectangleF texRec = TerrainHelper.GetTexRec( tile.TerrainId );
			vertices[index] = new VertexPos3Tex2( X, height, Y, texRec.Left, texRec.Top );
			vertices[index + 1] = new VertexPos3Tex2( X + tileWidth, height, Y, texRec.Right, texRec.Top );
			vertices[index + 2] = new VertexPos3Tex2( X + tileWidth, height, Y + tileWidth, texRec.Right, texRec.Bottom );
			vertices[index + 3] = new VertexPos3Tex2( X, height, Y + tileWidth, texRec.Left, texRec.Bottom );
			drawInfo.TotalTriangles += 2;
		}
		
		void RenderTileBorders( TileDrawInfo drawInfo, VertexPos3Col4[] vertices, int index ) {
			Tile tile = drawInfo.CurrentTile;
			float x = tile.X;
			float y = tile.Y;
			float height = tile.Height;
			
			FastColour colour = new FastColour( 150, 150, 150 );
			vertices[index] = new VertexPos3Col4( x, height, y + 0.5f, colour );
			vertices[index + 1] = new VertexPos3Col4( x + 0.5f, height, y, colour );
			vertices[index + 2] = new VertexPos3Col4( x + tileWidth, height, y + 0.5f, colour );
			vertices[index + 3] = new VertexPos3Col4( x + 0.5f, height, y + tileWidth, colour );
			drawInfo.TotalTriangles += 2;
		}
	}
}