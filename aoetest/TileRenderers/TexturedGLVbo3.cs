using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace IsometricTests.Renderers {
	
	public class TileRendererTexturedGLVbo3 : TileRenderer {
		
		public override void Start( int terrainTexture ) {
			GL.Enable( EnableCap.Texture2D );
			GL.BindTexture( TextureTarget.Texture2D, terrainTexture );
		}
		
		public override void End() {
			GL.Disable( EnableCap.Texture2D );
		}
		
		public override void RenderTiles( TerrainMap map, ref int totalTriangles ) {
			BuildTerrain( map, ref totalTriangles );
		}
		
		void BuildTerrain( TerrainMap map, ref int totalTriangles ) {
			TileDrawInfo drawInfo = new TileDrawInfo();
			
			int lengthX = map.Width;
			int lengthY = map.Length;
			int index = 0;
			GL.Begin( BeginMode.Quads );
			
			for( int x = 0; x < 6; x++ ) {
				for( int y = 0; y < 6; y++ ) {
					Tile tile = map[x, y];
					drawInfo.CurrentTile = tile;
					
					drawInfo.MapX = x;
					drawInfo.MapY = y;
					RenderTile( drawInfo );
					index += 4;
				}
			} // end for
			
			GL.End();
			totalTriangles = drawInfo.TotalTriangles;
		}
		
		void VertexPosUv( float x, float y, float z, float u, float v ) {
			GL.Vertex3( x, y, z );
			GL.TexCoord2( u, v );
		}
		
		void RenderTile( TileDrawInfo drawInfo ) {
			Tile tile = drawInfo.CurrentTile;
			
			const int xWidth = 1, yWidth = 1;
			
			float X = tile.X * xWidth;
			float Y = tile.Y * yWidth;
			
			float x1 = X, y1 = Y;
			//X = ( x1 * xWidth / 2f ) + ( y1 * xWidth / 2f );
			//Y = ( y1 * yWidth / 2f ) - ( x1 * yWidth / 2f );
			float z = 1;
			
			/*if( ( tile.X % 2 ) == 0 ) {
				//X /= 2f;
				//X += xWidth / 2f;
				//y += yWidth / 2f;
				//Y /= 2f;
				Y += yWidth * 5;
				Y -= (int)( yWidth / 2f );
				//return;
				//return;
			} else {
				X /= 2f;
			}*/

			RectangleF texRec = TerrainHelper.GetTexRec( tile.TerrainId );
			
			float Width = xWidth;
			float Depth = yWidth;
			
			float xHalf = Width / 2;
			
			GL.TexCoord2( 0f, 0f ); GL.Vertex3(X - xHalf, z, Y + Depth / 4); // Bottom left
			GL.TexCoord2( 1f, 0f ); GL.Vertex3(X + xHalf, z, Y + Depth / 4); // Bottom Right
			GL.TexCoord2( 1f, 1f ); GL.Vertex3(X + xHalf, z, Y - 3 * Depth / 4); // Top Right
			GL.TexCoord2( 0f, 1f ); GL.Vertex3(X - xHalf, z, Y - 3 * Depth / 4); // Top Left
			
			//VertexPosUv( x, height, y, texRec.Left, texRec.Top );
			//VertexPosUv( x + tileWidth, height, y, texRec.Right, texRec.Top );
			//VertexPosUv( x + tileWidth, height, y + tileHeight, texRec.Right, texRec.Bottom );
			//VertexPosUv( x, height, y + tileHeight, texRec.Left, texRec.Bottom );
			
			drawInfo.TotalTriangles += 2;
		}
	}
}