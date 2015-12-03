using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace IsometricTests.Renderers {
	
	public class TileRendererTexturedGLVbo4 : TileRenderer {
		
		
		
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
		int fullVerticesCount = -1;
		
		public override void RenderTiles( TerrainMap map, ref int totalTriangles ) {
			if( fullVboId == -1 ) {
				Console.WriteLine( "Generating with textures!" );
				fullVboId = GLUtils.GenBuffer();
				var sw = Stopwatch.StartNew();
				VertexPos3Tex2[] vertices = BuildTerrain( map, ref triangles );
				long elapsed = sw.ElapsedMilliseconds;
				sw.Stop();
				Console.WriteLine( "Took " + elapsed + " ms to generate the map." );
				int size = vertices.Length * VertexPos3Tex2.Stride;
				GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, fullVboId );
				GL.Arb.BufferData( BufferTargetArb.ArrayBuffer, new IntPtr( size ), vertices, BufferUsageArb.StaticDraw );
				
				fullVerticesCount = vertices.Length;
			}
			
			GL.EnableClientState( ArrayCap.VertexArray );
			GL.EnableClientState( ArrayCap.TextureCoordArray );
			
			GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, fullVboId );
			GL.VertexPointer( 3, VertexPointerType.Float, VertexPos3Tex2.Stride, new IntPtr( 0 ) );
			GL.TexCoordPointer( 2, TexCoordPointerType.Float, VertexPos3Tex2.Stride, new IntPtr( 12 ) );
			GL.DrawArrays( BeginMode.Quads, 0, fullVerticesCount );
			
			GL.DisableClientState( ArrayCap.TextureCoordArray );
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
		
		const float tileWidth = 20;//97;
		const float tileHeight = 20;//49;
		
		static void TranslatePoint( ref float x, ref float y ) {
			float yHalf = ( y * tileHeight ) / 2f;
			float xHalf = ( x * tileWidth ) / 2f;
			
			//http://stackoverflow.com/questions/892811/drawing-isometric-game-worlds
			x = yHalf + xHalf;
			y = yHalf - xHalf;
		}
		
		void RenderTile( TileDrawInfo drawInfo, VertexPos3Tex2[] vertices, int index ) {
			Tile tile = drawInfo.CurrentTile;
			X = tile.X;
			Y = tile.Y;
			height = 1;
			
			X += ( X + 1 ) / tileWidth;
			
			TranslatePoint( ref X, ref Y );

			RectangleF texRec = TerrainHelper.GetTexRec( tile.TerrainId );
			vertices[index] = new VertexPos3Tex2( X, height, Y, texRec.Left, texRec.Top );
			vertices[index + 1] = new VertexPos3Tex2( X + tileWidth, height, Y, texRec.Right, texRec.Top );
			vertices[index + 2] = new VertexPos3Tex2( X + tileWidth, height, Y + tileHeight, texRec.Right, texRec.Bottom );
			vertices[index + 3] = new VertexPos3Tex2( X, height, Y + tileHeight, texRec.Left, texRec.Bottom );
			drawInfo.TotalTriangles += 2;
		}
	}
}