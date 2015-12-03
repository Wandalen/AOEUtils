/*#define DEBUG_TEXTURES
//#define DEBUG_SQUARE_LINES
//#define DEBUG_FILLED_LINES
using System;
using OpenTK.Graphics.OpenGL;

namespace IsometricTests.Renderers {
	
	public class TileRendererLinesGLImmediate {
		
		const float tileWidth = 1;
		const float tileHeight = 1;
		
		float X, Y;
		float height;
		float z1, z2;
		
		public void Start( int terrainTexture ) {
			
		}
		
		public void End() {
			
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
				RenderTopCornerFaceRotated( drawInfo );
				return;
			}
			
			if( leftFrontHigher && !leftHigher && !frontHigher ) {
				z2 += tileHeight;
				RenderTopCornerFace( drawInfo );
				return;
			}
			
			if( rightFrontHigher && !rightHigher && !frontHigher ) {
				z1 += tileHeight;
				RenderTopCornerFaceRotated( drawInfo );
				return;
			}
			
			if( rightBehindHigher && !rightHigher && !backHigher ) {
				z1 += tileHeight;
				RenderTopCornerFace( drawInfo );
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
				z2 += tileHeight;
				RenderTopFaceRotated( drawInfo );
				return;
			}
			
			if( frontHigher ) {
				z1 += tileHeight;
				RenderTopFaceRotated( drawInfo );
				return;
			}
			RenderTopFace( drawInfo );
		}

		void RenderTopFace( TileDrawInfo drawInfo ) {
			#if !DEBUG_TEXTURES
			GL.TexCoord2( 0, 0 );
			GL.Vertex3( X, z1, Y );
			GL.TexCoord2( 1, 0 );
			GL.Vertex3( X + tileWidth, z1, Y );
			GL.TexCoord2( 1, 1 );
			GL.Vertex3( X + tileWidth, z1, Y + tileWidth );
			//GL.Vertex3( X + TileWidth, Height, Y + TileWidth );
			GL.TexCoord2( 0, 1 );
			GL.Vertex3( X, z1, Y + tileWidth );
			//GL.Vertex3( X, Height, Y );
			#endif
			
			#if DEBUG_TEXTURES
			GL.End();
			GL.Enable( EnableCap.Blend );
			GL.BlendFunc( BlendingFactorSrc.OneMinusSrcAlpha, BlendingFactorDest.OneMinusSrcAlpha );
			
			GL.Disable( EnableCap.Texture2D );
			#if DEBUG_FILLED_LINES
			GL.Color4( 1f, 0f, 0f, 0.1f );
			GL.Begin( BeginMode.Quads );
			GL.Vertex3( X, z2, Y + 0.5f );
			GL.Vertex3( X + 0.5f, z1, Y );
			GL.Vertex3( X + tileWidth, z1, Y + 0.5f );
			GL.Vertex3( X + 0.5f, z2, Y + tileWidth );
			GL.End();
			#endif
			
			GL.Color4( 0f, 1f, 0f, 0.1f );
			GL.Begin( BeginMode.LineLoop );
			#if DEBUG_SQAURE_LINES
			GL.Vertex3( X, z1, Y );
			GL.Vertex3( X + 1, z1, Y );
			GL.Vertex3( X + 1, z1, Y + 1 );
			GL.Vertex3( X, z1, Y + 1 );
			#else
			GL.Vertex3( X, z2, Y + 0.5f );
			GL.Vertex3( X + 0.5f, z1, Y );
			GL.Vertex3( X + tileWidth, z1, Y + 0.5f );
			GL.Vertex3( X + 0.5f, z2, Y + tileWidth );
			#endif
			GL.End();
			
			GL.Color4( 1f, 1f, 1f, 1f );
			GL.Enable( EnableCap.Texture2D );
			GL.Disable( EnableCap.Blend );
			GL.Begin( BeginMode.Quads );
			#endif
			
			drawInfo.TotalTriangles += 2;
		}
		
		void RenderTopFaceRotated( TileDrawInfo drawInfo ) {
			#if DEBUG_TEXTURES
			GL.End();
			GL.Enable( EnableCap.Blend );
			GL.BlendFunc( BlendingFactorSrc.OneMinusSrcAlpha, BlendingFactorDest.OneMinusSrcAlpha );
			
			GL.Disable( EnableCap.Texture2D );
			#if DEBUG_FILLED_LINES
			GL.Color4( 1f, 0f, 0f, 0.1f );
			GL.Begin( BeginMode.Quads );
			GL.Vertex3( X, z1, Y + 0.5f );
			GL.Vertex3( X + 0.5f, z1, Y );
			GL.Vertex3( X + tileWidth, z2, Y + 0.5f );
			GL.Vertex3( X + 0.5f, z2, Y + tileWidth );
			GL.End();
			#endif
			
			GL.Color4( 0f, 1f, 0f, 0.1f );
			GL.Begin( BeginMode.LineLoop );
			GL.Vertex3( X, z1, Y + 0.5f );
			GL.Vertex3( X + 0.5f, z1, Y );
			GL.Vertex3( X + tileWidth, z2, Y + 0.5f );
			GL.Vertex3( X + 0.5f, z2, Y + tileWidth );
			GL.End();
			
			GL.Color4( 1f, 1f, 1f, 1f );
			GL.Enable( EnableCap.Texture2D );
			GL.Disable( EnableCap.Blend );
			GL.Begin( BeginMode.Quads );
			#endif
			
			drawInfo.TotalTriangles += 2;
		}
		
		void RenderTopCornerFaceRotated( TileDrawInfo drawInfo ) {
			#if DEBUG_TEXTURES
			GL.End();
			GL.Enable( EnableCap.Blend );
			GL.BlendFunc( BlendingFactorSrc.OneMinusSrcAlpha, BlendingFactorDest.OneMinusSrcAlpha );
			
			GL.Disable( EnableCap.Texture2D );
			GL.Color4( 0f, 1f, 0f, 0.1f );
			
			GL.Begin( BeginMode.LineLoop );
			GL.Vertex3( X, height, Y + 0.5f );
			GL.Vertex3( X + 0.5f, z1, Y );
			GL.Vertex3( X + tileWidth, height, Y + 0.5f );
			GL.End();

			GL.Begin( BeginMode.LineLoop );
			GL.Vertex3( X + tileWidth, height, Y + 0.5f );
			GL.Vertex3( X + 0.5f, z2, Y + tileWidth );
			GL.Vertex3( X, height, Y + 0.5f );
			GL.End();
			
			GL.Color4( 1f, 1f, 1f, 1f );
			GL.Enable( EnableCap.Texture2D );
			GL.Disable( EnableCap.Blend );
			GL.Begin( BeginMode.Quads );
			#endif
			
			drawInfo.TotalTriangles += 2;
		}
		
		void RenderTopCornerFace( TileDrawInfo drawInfo ) {
			#if DEBUG_TEXTURES
			GL.End();
			GL.Enable( EnableCap.Blend );
			GL.BlendFunc( BlendingFactorSrc.OneMinusSrcAlpha, BlendingFactorDest.OneMinusSrcAlpha );
			
			GL.Disable( EnableCap.Texture2D );
			GL.Color4( 0f, 1f, 0f, 0.1f );
			
			GL.Begin( BeginMode.LineLoop );		
			GL.Vertex3( X + 0.5f, height, Y );
			GL.Vertex3( X, z2, Y + 0.5f );
			GL.Vertex3( X + 0.5f, height, Y + tileWidth );
			GL.End();
			
			GL.Begin( BeginMode.LineLoop );
			GL.Vertex3( X + 0.5f, height, Y );
			GL.Vertex3( X + tileWidth, z1, Y + 0.5f );
			GL.Vertex3( X + 0.5f, height, Y + tileWidth );
			GL.End();
			
			GL.Color4( 1f, 1f, 1f, 1f );
			GL.Enable( EnableCap.Texture2D );
			GL.Disable( EnableCap.Blend );
			GL.Begin( BeginMode.Quads );
			#endif
			
			drawInfo.TotalTriangles += 2;
		}
	}
}*/