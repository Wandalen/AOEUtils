using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace IsometricTests.Renderers {
	
	public abstract class OptimisedTileRendererVbo : TileRendererVbo {
		
		protected float stretchY = 1;
		
		//    N
		//   / \
		//  /   \
		// W     E
		//  \   /
		//   \ /
		//    S
		protected enum SlopeMode : byte {
			Flat,
			NERaised,
			NWRaised,
			SWRaised,
			SERaised,
			
			ERaised,
			WRaised,
			SRaised,
			NRaised,
			
			NWERaised,
			NSERaised,
			NSWRaised,
			SWERaised,
		}
		
		protected override void BuildTerrain( TerrainMap map, ref int totalTriangles ) {
			verticesCount = 0;
			TileDrawInfo drawInfo = new TileDrawInfo();
			SlopeMode[] slopes = new SlopeMode[map.Width * map.Length];
			
			int verticesPerElement = PrimitiveElementSize;
			int lengthX = map.Width;
			int lengthY = map.Length;
			index = 0;
			
			// First pass - calculate the slope map.
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
					
					slopes[( y * map.Width ) + x] = CalculateSlope( drawInfo );
				}
			}
			
			int width = map.Width;
			// Second pass - actually render the tile
			bool[] shouldDrawTiles = new bool[map.Width * map.Length];
			for( int i = 0; i < shouldDrawTiles.Length; i++ ) {
				shouldDrawTiles[i] = true;
			}
			
			for( int y = 0; y < lengthY; y++ ) {
				for( int x = 0; x < lengthX; x++ ) {
					Tile tile = map[x, y];
					drawInfo.CurrentTile = tile;
					int arrayIndex = ( y * width ) + x;
					tileSlopeMode = slopes[arrayIndex];
					stretchY = 1;
					if( shouldDrawTiles[arrayIndex] ) {
						PerformTileStretching( x, y, width, shouldDrawTiles, slopes, map );
						RenderTile( drawInfo, index );
						verticesCount += verticesPerElement;
						index += verticesPerElement;
					}
				}
			}
			totalTriangles += drawInfo.TotalTriangles;
		}
		SlopeMode tileSlopeMode;
		
		// Example map Y axis (example tiles given as [id, height]
		// [5, 1]
		// [5, 1]
		// [5, 1]
		// The unoptimised renderer will render 6 triangles, while the optimised renderer only renders 2
		// triangles by "stretching" one tile across the three tiles on the Y axis.

		// Obviously, the optimised renderer performs best with large flat maps of the same terrain type.
		// The optimised renderer generally increases performance on nearly all maps, and in theory,
		// never gives worse performance than the unoptimised renderer. (most maps have at least some flat terrain of the same type)
		
		// Unfortunately, the optimised renderer does take longer to build the map terrain,
		// and this will probably need to be addressed when editable terrain is suported.
		
		// But believe me kid, it makes a difference.
		void PerformTileStretching( int x, int y, int width, bool[] shouldDrawTiles, SlopeMode[] slopes, TerrainMap map ) {
			SlopeMode currentSlope = tileSlopeMode;
			byte terrainId = map[x, y].TerrainId;
			if( currentSlope == SlopeMode.Flat ) {
				int index = ( y * width ) + x;
				int stretch = 1;
				while( y < width && slopes[index] == SlopeMode.Flat && map[x, y].TerrainId == terrainId ) {
					// Don't set the first tile to 'don't draw'.
					if( stretch > 1 ) {
						shouldDrawTiles[index] = false;
					}
					y++;
					index += width;
					stretch++;
				}
				stretchY = ( stretch / 2f );
			}
		}
		
		static SlopeMode CalculateSlope( TileDrawInfo drawInfo ) {
			Tile tile = drawInfo.CurrentTile;
			float height = tile.Height;
			float z1 = height;
			float z2 = height;
			
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
				return SlopeMode.WRaised;
			}
			
			if( leftHigher && backHigher ) {
				return SlopeMode.NSWRaised;
			}
			
			if( leftFrontHigher && !leftHigher && !frontHigher ) {
				return SlopeMode.SRaised;
			}
			
			if( leftHigher && frontHigher ) {
				return SlopeMode.SWERaised;
			}
			
			if( rightFrontHigher && !rightHigher && !frontHigher ) {
				return SlopeMode.ERaised;
			}
			
			if( rightHigher && frontHigher ) {
				return SlopeMode.NSERaised;
			}
			
			if( rightBehindHigher && !rightHigher && !backHigher ) {
				return SlopeMode.NRaised;
			}
			
			if( rightHigher && backHigher ) {
				return SlopeMode.NWERaised;
			}
			
			if( leftHigher ) {
				return SlopeMode.SWRaised;
			}
			
			if( rightHigher ) {
				return SlopeMode.NERaised;
			}
			
			if( backHigher ) {
				return SlopeMode.NWRaised;
			}
			
			if( frontHigher ) {
				return SlopeMode.SERaised;
			}
			return SlopeMode.Flat;
		}
		
		protected abstract void RenderStretchedFlatFace( TileDrawInfo drawInfo, int index );
		
		// This method does the bulk of determining how the terrain is drawn.
		// Note that this is also the cause of any terrain drawing differences compared to Age of Empires.
		// I have tried my best to check that the terrain is drawn the same, but I'm not perfect. K.
		protected override void RenderTile( TileDrawInfo drawInfo, int index ) {
			Tile tile = drawInfo.CurrentTile;
			x = tile.X;
			y = tile.Y;
			height = tile.Height;
			z1 = height;
			z2 = height;
			
			if( stretchY != 1 ) {
				RenderStretchedFlatFace( drawInfo, index );
				return;
			}
			
			switch( tileSlopeMode ) {
				case SlopeMode.WRaised:
					z2 += tileHeight;
					RenderBottomCornerFace( drawInfo, index );
					return;
					
				case SlopeMode.NSWRaised:
					z2 += tileHeight;
					RenderTopCornerFace( drawInfo, index );
					return;
					
				case SlopeMode.SRaised:
					z2 += tileHeight;
					RenderBottomCornerFaceRotated( drawInfo, index );
					return;
					
				case SlopeMode.SWERaised:
					z2 += tileHeight;
					RenderTopCornerFaceRotated( drawInfo, index );
					return;
					
				case SlopeMode.ERaised:
					z1 += tileHeight;
					RenderBottomCornerFace( drawInfo, index );
					return;
					
				case SlopeMode.NSERaised:
					z1 += tileHeight;
					RenderTopCornerFace( drawInfo, index );
					return;
					
				case SlopeMode.NRaised:
					z1 += tileHeight;
					RenderBottomCornerFaceRotated( drawInfo, index );
					return;
					
				case SlopeMode.NWERaised:
					z1 += tileHeight;
					RenderTopCornerFaceRotated( drawInfo, index );
					return;
					
				case SlopeMode.SWRaised:
					z2 += tileHeight;
					RenderTopFace( drawInfo, index );
					return;
					
				case SlopeMode.NERaised:
					z1 += tileHeight;
					RenderTopFace( drawInfo, index );
					return;
					
				case SlopeMode.NWRaised:
					z1 += tileHeight;
					RenderTopFaceRotated( drawInfo, index );
					return;
					
				case SlopeMode.SERaised:
					z2 += tileHeight;
					RenderTopFaceRotated( drawInfo, index );
					return;
					
				default:
					RenderTopFace( drawInfo, index );
					return;
			}
		}
	}
}