using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace IsometricTests.Renderers {
	
	public abstract class TileRendererVbo : TileRenderer {
		
		protected const float tileWidth = 1;
		protected const float tileLength = 1;
		protected const float tileHeight = 1f;
		
		protected const float halfTileWidth = tileWidth / 2f;
		protected const float halfTileLength = tileLength / 2f;
		
		protected float x, y;
		protected float height;
		protected float z1, z2;
		protected int verticesCount;
		
		protected int triangles;
		bool builtVbo = false;
		
		protected abstract void BuildTerrainVbo( TerrainMap map );
		
		protected abstract void RenderTerrainVbo();
		
		public override void RenderTiles( TerrainMap map, ref int totalTriangles ) {
			if( !builtVbo ) {
				Console.WriteLine( "Building map.." );
				#if DEBUG
				var sw = Stopwatch.StartNew();
				#endif
				BuildTerrainVbo( map );
				#if DEBUG
				long elapsed = sw.ElapsedMilliseconds;
				sw.Stop();
				Console.WriteLine( "Took " + elapsed + " ms to build the map." );
				#endif
				builtVbo = true;
			}
			
			RenderTerrainVbo();
			totalTriangles = triangles;
		}
		
		protected abstract int PrimitiveElementSize { get; }
		
		protected int index;
		
		protected virtual void BuildTerrain( TerrainMap map, ref int totalTriangles ) {
			TileDrawInfo drawInfo = new TileDrawInfo();
			verticesCount = 0;
			
			int verticesPerElement = PrimitiveElementSize;
			int lengthX = map.Width;
			int lengthY = map.Length;
			index = 0;
			
			for( int y = 0; y < lengthY; y++ ) {
				for( int x = 0; x < lengthX; x++ ) {
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
					RenderTile( drawInfo, index );
					index += verticesPerElement;
					verticesCount += verticesPerElement;
				}
			}
			
			totalTriangles += drawInfo.TotalTriangles;
		}
		
		// This method does the bulk of determining how the terrain is drawn.
		// Note that this is also the cause of any terrain drawing differences compared to Age of Empires.
		// I have tried my best to check that the terrain is drawn the same, but I'm not perfect. K.
		protected virtual void RenderTile( TileDrawInfo drawInfo, int index ) {
			Tile tile = drawInfo.CurrentTile;
			x = tile.X;
			y = tile.Y;
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
			
			bool leftHigher = left != null && left.Value.Height > height;
			bool rightHigher = right != null && right.Value.Height > height;
			bool backHigher = back != null && back.Value.Height > height;
			bool frontHigher = front != null && front.Value.Height > height;
			
			bool leftBehindHigher = leftBehind != null && leftBehind.Value.Height > height;
			bool leftFrontHigher = leftFront != null && leftFront.Value.Height > height;
			bool rightBehindHigher = rightBehind != null && rightBehind.Value.Height > height;
			bool rightFrontHigher = rightFront != null && rightFront.Value.Height > height;
			
			if( leftBehindHigher && !leftHigher && !backHigher ) {
				z2 += tileHeight;
				RenderBottomCornerFace( drawInfo, index );
				return;
			}
			
			if( leftHigher && backHigher ) {
				z2 += tileHeight;
				RenderTopCornerFace( drawInfo, index );
				return;
			}
			
			if( leftFrontHigher && !leftHigher && !frontHigher ) {
				z2 += tileHeight;
				RenderBottomCornerFaceRotated( drawInfo, index );
				return;
			}
			
			if( leftHigher && frontHigher ) {
				z2 += tileHeight;
				RenderTopCornerFaceRotated( drawInfo, index );
				return;
			}
			
			if( rightFrontHigher && !rightHigher && !frontHigher ) {
				z1 += tileHeight;
				RenderBottomCornerFace( drawInfo, index );
				return;
			}
			
			if( rightHigher && frontHigher ) {
				z1 += tileHeight;
				RenderTopCornerFace( drawInfo, index );
				return;
			}
			
			if( rightBehindHigher && !rightHigher && !backHigher ) {
				z1 += tileHeight;
				RenderBottomCornerFaceRotated( drawInfo, index );
				return;
			}
			
			if( rightHigher && backHigher ) {
				z1 += tileHeight;
				RenderTopCornerFaceRotated( drawInfo, index );
				return;
			}
			
			if( leftHigher ) {
				z2 += tileHeight;
				RenderTopFace( drawInfo, index );
				return;
			}
			
			if( rightHigher ) {
				z1 += tileHeight;
				RenderTopFace( drawInfo, index );
				return;
			}
			
			if( backHigher ) {
				z1 += tileHeight;
				RenderTopFaceRotated( drawInfo, index );
				return;
			}
			
			if( frontHigher ) {
				z2 += tileHeight;
				RenderTopFaceRotated( drawInfo, index );
				return;
			}
			RenderTopFace( drawInfo, index );
		}

		protected abstract void RenderTopFace( TileDrawInfo drawInfo, int index );
		
		protected abstract void RenderTopFaceRotated( TileDrawInfo drawInfo, int index );
		
		protected abstract void RenderBottomCornerFaceRotated( TileDrawInfo drawInfo, int index );
		
		protected abstract void RenderBottomCornerFace( TileDrawInfo drawInfo, int index );
		
		protected abstract void RenderTopCornerFaceRotated( TileDrawInfo drawInfo, int index );
		
		protected abstract void RenderTopCornerFace( TileDrawInfo drawInfo, int index );
	}
}