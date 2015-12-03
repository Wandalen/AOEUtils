using System;
using OpenTK.Graphics.OpenGL;

namespace IsometricTests.Renderers {
	
	public class TileRendererLinesGLImmediate : TileRenderer {
		
		const float tileWidth = 1;
		const float tileHeight = 1;
		
		float X, Y;
		float height;
		float z1, z2;
		
		public override void Start( int terrainTexture ) {
			GL.Enable( EnableCap.Blend );
			GL.BlendFunc( BlendingFactorSrc.OneMinusSrcAlpha, BlendingFactorDest.OneMinusSrcAlpha );
			GL.Color4( 0f, 1f, 0f, 0.1f );
			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Line );
		}
		
		public override void RenderTiles( TerrainMap map, ref int totalTriangles ) {
			BuildTerrain( map, ref totalTriangles );
		}
		
		void BuildTerrain( TerrainMap map, ref int totalTriangles ) {
			TileDrawInfo drawInfo = new TileDrawInfo();
			
			int lengthX = map.Width;
			int lengthY = map.Length;
			GL.Begin( BeginMode.Triangles );
			
			for( int x = 0; x < lengthX; x++ ) {
				for( int y = 0; y < lengthY; y++ ) {
					Tile tile = map[x, y];
					drawInfo.CurrentTile = tile;
					bool leftExists = x > 0;
					bool behindExists = y > 0;
					bool rightExists = x < lengthX - 1;
					bool frontExists = y < lengthY - 1;
					
					if( leftExists && behindExists ) {
						drawInfo.LeftBehind = map[x - 1, y - 1];
					}
					if( leftExists && frontExists ) {
						drawInfo.LeftFront = map[x - 1, y + 1];
					}
					if( rightExists && behindExists ) {
						drawInfo.RightBehind = map[x + 1, y - 1];
					}
					if( rightExists && frontExists ) {
						drawInfo.RightFront = map[x + 1, y + 1];
					}
					
					if( leftExists ) {
						drawInfo.Left = map[x - 1, y];
					}
					if( behindExists ) {
						drawInfo.Behind = map[x, y - 1];
					}
					if( rightExists ) {
						drawInfo.Right = map[x + 1, y];
					}
					if( frontExists ) {
						drawInfo.Front = map[x, y + 1];
					}
					
					drawInfo.MapX = x;
					drawInfo.MapY = y;
					RenderTile( drawInfo );
				}
			} // end for
			GL.End();
			totalTriangles = drawInfo.TotalTriangles;
		}
		
		
		public override void End() {
			GL.Color4( 1f, 1f, 1f, 1f );
			GL.Disable( EnableCap.Blend );
			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Fill );
		}
		
		public void RenderTile( TileDrawInfo drawInfo ) {
			Tile tile = drawInfo.CurrentTile;
			X = tile.X;
			Y = tile.Y;
			height = tile.Height;
			z1 = height;
			z2 = height;
			
			Tile? left = drawInfo.Left;
			Tile? right = drawInfo.Right;
			Tile? back = drawInfo.Behind;
			Tile? front = drawInfo.Front;
			
			Tile? leftBehind = drawInfo.LeftBehind;
			Tile? leftFront = drawInfo.LeftFront;
			Tile? rightBehind = drawInfo.RightBehind;
			Tile? rightFront = drawInfo.RightFront;
			
			bool leftHigher = left != null && left.Value.Height > z2;
			bool rightHigher = right != null && right.Value.Height > z1;
			bool backHigher = back != null && back.Value.Height > z2;
			bool frontHigher = front != null && front.Value.Height > z1;
			
			bool leftBehindHigher = leftBehind != null && leftBehind.Value.Height > height;
			bool leftFrontHigher = leftFront != null && leftFront.Value.Height > height;
			bool rightBehindHigher = rightBehind != null && rightBehind.Value.Height > height;
			bool rightFrontHigher = rightFront != null && rightFront.Value.Height > height;
			
			if( leftBehindHigher && !leftHigher && !backHigher ) {
				z2 += tileHeight;
				RenderTopCornerFace( drawInfo );
				return;
			}
			
			if( leftFrontHigher && !leftHigher && !frontHigher ) {
				z2 += tileHeight;
				RenderTopCornerFaceRotated( drawInfo );
				return;
			}
			
			if( rightFrontHigher && !rightHigher && !frontHigher ) {
				z1 += tileHeight;
				RenderTopCornerFace( drawInfo );
				return;
			}
			
			if( rightBehindHigher && !rightHigher && !backHigher ) {
				z1 += tileHeight;
				RenderTopCornerFaceRotated( drawInfo );
				return;
			}
			
			if( leftHigher ) {
				z2 += tileHeight;
				RenderTopFace( drawInfo );
				return;
			}
			
			if( rightHigher ) {
				z1 += tileHeight;
				RenderTopFace( drawInfo );
				return;
			}
			
			if( backHigher ) {
				z1 += tileHeight;
				RenderTopFaceRotated( drawInfo );
				return;
			}
			
			if( frontHigher ) {
				z2 += tileHeight;
				RenderTopFaceRotated( drawInfo );
				return;
			}
			RenderTopFace( drawInfo );
		}

		void RenderTopFace( TileDrawInfo drawInfo ) {
			GL.Vertex3( X, z2, Y + 0.5f );
			GL.Vertex3( X + 0.5f, z1, Y );
			GL.Vertex3( X + tileWidth, z1, Y + 0.5f );
			
			GL.Vertex3( X + tileWidth, z1, Y + 0.5f );
			GL.Vertex3( X + 0.5f, z2, Y + tileWidth );
			GL.Vertex3( X, z2, Y + 0.5f );
			drawInfo.TotalTriangles += 2;
		}
		
		void RenderTopFaceRotated( TileDrawInfo drawInfo ) {
			GL.Vertex3( X, z1, Y + 0.5f );
			GL.Vertex3( X + 0.5f, z1, Y );
			GL.Vertex3( X + tileWidth, z2, Y + 0.5f );
			
			GL.Vertex3( X + tileWidth, z2, Y + 0.5f );
			GL.Vertex3( X + 0.5f, z2, Y + tileWidth );
			GL.Vertex3( X, z1, Y + 0.5f );
			drawInfo.TotalTriangles += 2;
		}
		
		void RenderTopCornerFaceRotated( TileDrawInfo drawInfo ) {
			GL.Vertex3( X, height, Y + 0.5f );
			GL.Vertex3( X + 0.5f, z1, Y );
			GL.Vertex3( X + tileWidth, height, Y + 0.5f );

			GL.Vertex3( X + tileWidth, height, Y + 0.5f );
			GL.Vertex3( X + 0.5f, z2, Y + tileWidth );
			GL.Vertex3( X, height, Y + 0.5f );
			drawInfo.TotalTriangles += 2;
		}
		
		void RenderTopCornerFace( TileDrawInfo drawInfo ) {
			GL.Vertex3( X + 0.5f, height, Y );
			GL.Vertex3( X, z2, Y + 0.5f );
			GL.Vertex3( X + 0.5f, height, Y + tileWidth );

			
			GL.Vertex3( X + 0.5f, height, Y );
			GL.Vertex3( X + tileWidth, z1, Y + 0.5f );
			GL.Vertex3( X + 0.5f, height, Y + tileWidth );
			drawInfo.TotalTriangles += 2;
		}
	}
}