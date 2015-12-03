using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace IsometricTests.Renderers {
	
	public class TileRendererColoursGLVbo2 : TileRenderer {
		
		const float tileWidth = 1;
		const float tileHeight = 1;
		
		[StructLayoutAttribute( LayoutKind.Sequential, Pack = 1 )]
		struct Vertex {
			public float X, Y, Z;
			public byte R, G, B, A;
			
			public Vertex( float x, float y, float z, FastColour col ) {
				X = x; Y = y; Z = z;
				R = col.R; G = col.G; B = col.B; A = col.A;
			}
			
			public static readonly int Stride = Marshal.SizeOf( default( Vertex ) );
		}
		
		float X, Y;
		float height;
		float z1, z2;
		
		int triangles;
		int fullVboId = -1;
		int linesVboId = -1;
		int fullVerticesCount = -1;
		int lineVerticesCount = -1;
		
		void RenderColoursInfo( TerrainMap map ) {
			Renderer2D renderer = map.Renderer2d;
			
			int yOffset = 80;
			const int textXOffset = 20;
			const int colourOffset = 4;
			const int colourSize = 16;
			FastColour[] terrainColours = TerrainHelper.AokTerrainColours;
			string[] terrainNames = TerrainHelper.AokTerrainNames;
			for( int i = 0; i < terrainColours.Length; i++ ) {
				renderer.MakeTextTexture( terrainNames[i], 12, textXOffset, yOffset );
				renderer.MakeColourTexture( terrainColours[i], colourOffset,
				                           yOffset + colourOffset, colourSize, colourSize );
				yOffset += 20;
			}
		}
		
		public override void RenderTiles( TerrainMap map, ref int totalTriangles ) {
			if( fullVboId == -1 ) {
				Console.WriteLine( "Generating with *full* colours!" );
				fullVboId = GLUtils.GenBuffer();
				linesVboId = GLUtils.GenBuffer();
				var sw = Stopwatch.StartNew();
				Vertex[] vertices = BuildTerrain( map, ref triangles );
				Vertex[] lineVertices = BuildTerrainBorders( map, ref triangles );
				long elapsed = sw.ElapsedMilliseconds;
				sw.Stop();
				Console.WriteLine( "Took " + elapsed + " ms to generate the map." );
				int size = vertices.Length * 16;
				GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, fullVboId );
				GL.Arb.BufferData( BufferTargetArb.ArrayBuffer, new IntPtr( size ), vertices, BufferUsageArb.StaticDraw );
				
				size = lineVertices.Length * 16;
				GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, linesVboId );
				GL.Arb.BufferData( BufferTargetArb.ArrayBuffer, new IntPtr( size ), lineVertices, BufferUsageArb.StaticDraw );
				GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, 0 );
				
				fullVerticesCount = vertices.Length;
				lineVerticesCount = lineVertices.Length;
				GL.LineWidth( 5 );
				RenderColoursInfo( map );
			}
			
			GL.EnableClientState( ArrayCap.VertexArray );
			GL.EnableClientState( ArrayCap.ColorArray );
			
			GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, fullVboId );
			GL.VertexPointer( 3, VertexPointerType.Float, 16, new IntPtr( 0 ) );
			GL.ColorPointer( 4, ColorPointerType.UnsignedByte, 16, new IntPtr( 12 ) );
			GL.DrawArrays( BeginMode.Triangles, 0, fullVerticesCount );
			
			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Line );
			GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, linesVboId );
			GL.VertexPointer( 3, VertexPointerType.Float, 16, new IntPtr( 0 ) );
			GL.ColorPointer( 4, ColorPointerType.UnsignedByte, 16, new IntPtr( 12 ) );
			GL.DrawArrays( BeginMode.Quads, 0, lineVerticesCount );
			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Fill );
			
			
			GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, 0 );
			GL.DisableClientState( ArrayCap.VertexArray );
			GL.DisableClientState( ArrayCap.ColorArray );
			totalTriangles = triangles;
		}
		
		Vertex[] BuildTerrain( TerrainMap map, ref int totalTriangles ) {
			TileDrawInfo drawInfo = new TileDrawInfo();
			
			int lengthX = map.Width;
			int lengthY = map.Length;
			Vertex[] vertices = new Vertex[lengthX * lengthY * 6];
			int index = 0;
			
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
					RenderTile( drawInfo, vertices, index );
					index += 6;
				}
			} // end for
			totalTriangles = drawInfo.TotalTriangles;
			return vertices;
		}
		
		Vertex[] BuildTerrainBorders( TerrainMap map, ref int totalTriangles ) {
			TileDrawInfo drawInfo = new TileDrawInfo();
			
			int lengthX = map.Width;
			int lengthY = map.Length;
			Vertex[] vertices = new Vertex[lengthX * lengthY * 4];
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
		
		void RenderTile( TileDrawInfo drawInfo, Vertex[] vertices, int index ) {
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
				RenderTopCornerFace( drawInfo, vertices, index );
				return;
			}
			
			if( leftFrontHigher && !leftHigher && !frontHigher ) {
				z2 += tileHeight;
				RenderTopCornerFaceRotated( drawInfo, vertices, index );
				return;
			}
			
			if( rightFrontHigher && !rightHigher && !frontHigher ) {
				z1 += tileHeight;
				RenderTopCornerFace( drawInfo, vertices, index );
				return;
			}
			
			if( rightBehindHigher && !rightHigher && !backHigher ) {
				z1 += tileHeight;
				RenderTopCornerFaceRotated( drawInfo, vertices, index );
				return;
			}
			
			if( leftHigher ) {
				z2 += tileHeight;
				RenderTopFace( drawInfo, vertices, index );
				return;
			}
			
			if( rightHigher ) {
				z1 += tileHeight;
				RenderTopFace( drawInfo, vertices, index );
				return;
			}
			
			if( backHigher ) {
				z1 += tileHeight;
				RenderTopFaceRotated( drawInfo, vertices, index );
				return;
			}
			
			if( frontHigher ) {
				z2 += tileHeight;
				RenderTopFaceRotated( drawInfo, vertices, index );
				return;
			}
			RenderTopFace( drawInfo, vertices, index );
		}
		
		void RenderTileBorders( TileDrawInfo drawInfo, Vertex[] vertices, int index ) {
			Tile tile = drawInfo.CurrentTile;
			float x = tile.X;
			float y = tile.Y;
			float height = tile.Height;
			
			FastColour colour = new FastColour( 150, 150, 150 );
			vertices[index] = new Vertex( x, height, y + 0.5f, colour );
			vertices[index + 1] = new Vertex( x + 0.5f, height, y, colour );
			vertices[index + 2] = new Vertex( x + tileWidth, height, y + 0.5f, colour );
			vertices[index + 3] = new Vertex( x + 0.5f, height, y + tileWidth, colour );
			drawInfo.TotalTriangles += 2;
		}
		

		void RenderTopFace( TileDrawInfo drawInfo, Vertex[] vertices, int index ) {
			FastColour colour = TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );
			vertices[index] = new Vertex( X, z2, Y + 0.5f, colour );
			vertices[index + 1] = new Vertex( X + 0.5f, z1, Y, colour );
			vertices[index + 2] = new Vertex( X + tileWidth, z1, Y + 0.5f, colour );
			
			vertices[index + 3] = new Vertex( X + tileWidth, z1, Y + 0.5f, colour );
			vertices[index + 4] = new Vertex( X + 0.5f, z2, Y + tileWidth, colour );
			vertices[index + 5] = new Vertex( X, z2, Y + 0.5f, colour );
			drawInfo.TotalTriangles += 2;
		}
		
		void RenderTopFaceRotated( TileDrawInfo drawInfo, Vertex[] vertices, int index ) {
			FastColour colour = TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );
			vertices[index] = new Vertex( X, z1, Y + 0.5f, colour );
			vertices[index + 1] = new Vertex( X + 0.5f, z1, Y, colour );
			vertices[index + 2] = new Vertex( X + tileWidth, z2, Y + 0.5f, colour );
			
			vertices[index + 3] = new Vertex( X + tileWidth, z2, Y + 0.5f, colour );
			vertices[index + 4] = new Vertex( X + 0.5f, z2, Y + tileWidth, colour );
			vertices[index + 5] = new Vertex( X, z1, Y + 0.5f, colour );
			drawInfo.TotalTriangles += 2;
		}
		
		void RenderTopCornerFaceRotated( TileDrawInfo drawInfo, Vertex[] vertices, int index ) {
			FastColour colour = TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );
			vertices[index] = new Vertex( X, height, Y + 0.5f, colour );
			vertices[index + 1] = new Vertex( X + 0.5f, z1, Y, colour );
			vertices[index + 2] = new Vertex( X + tileWidth, height, Y + 0.5f, colour );

			vertices[index + 3] = new Vertex( X + tileWidth, height, Y + 0.5f, colour );
			vertices[index + 4] = new Vertex( X + 0.5f, z2, Y + tileWidth, colour );
			vertices[index + 5] = new Vertex( X, height, Y + 0.5f, colour );
			drawInfo.TotalTriangles += 2;
		}
		
		void RenderTopCornerFace( TileDrawInfo drawInfo, Vertex[] vertices, int index ) {
			FastColour colour = TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );
			vertices[index] = new Vertex( X + 0.5f, height, Y, colour );
			vertices[index + 1] = new Vertex( X, z2, Y + 0.5f, colour );
			vertices[index + 2] = new Vertex( X + 0.5f, height, Y + tileWidth, colour );
			
			vertices[index + 3] = new Vertex( X + 0.5f, height, Y, colour );
			vertices[index + 4] = new Vertex( X + tileWidth, z1, Y + 0.5f, colour );
			vertices[index + 5] = new Vertex( X + 0.5f, height, Y + tileWidth, colour );
			drawInfo.TotalTriangles += 2;
		}
	}
}