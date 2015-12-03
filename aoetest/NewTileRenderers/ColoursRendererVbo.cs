#define OPTIMISED_COLOURS
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace IsometricTests.Renderers {
	
	#if OPTIMISED_COLOURS
	public class ColoursRendererVbo : OptimisedTileRendererVbo
		#else
		public class ColoursRendererVbo : TileRendererVbo
		#endif
	{
		
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
		
		protected override int PrimitiveElementSize {
			get { return lineMode ? 4 : 6; }
		}
		
		protected override void CleanupResources() {
			if( fullVboId != -1 ) {
				GL.Arb.DeleteBuffers( 1, ref fullVboId );
			}
			if( linesVboId != -1 ) {
				GL.Arb.DeleteBuffers( 1, ref linesVboId );
			}
		}
		
		int fullVboId = -1, linesVboId = -1;
		int fullVerticesCount;
		bool lineMode = false;
		
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
		
		bool linesEnabled = false;		
		Vertex[] vertices;
		
		protected override void BuildTerrainVbo( TerrainMap map ) {
			fullVboId = GLUtils.GenBuffer();
			Vertex[] fullVertices = new Vertex[map.Width * map.Length * PrimitiveElementSize];
			vertices = fullVertices;
			BuildTerrain( map, ref triangles );
			fullVerticesCount = verticesCount;
			
			int size = fullVerticesCount * 16;
			GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, fullVboId );
			GL.Arb.BufferData( BufferTargetArb.ArrayBuffer, new IntPtr( size ), fullVertices, BufferUsageArb.StaticDraw );
			
			if( linesEnabled ) {
				linesVboId = GLUtils.GenBuffer();
				lineMode = true;
				Vertex[] lineVertices = new Vertex[map.Width * map.Length * PrimitiveElementSize];
				vertices = lineVertices;
				BuildTerrain( map, ref triangles );
				
				size = verticesCount * 16;
				GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, linesVboId );
				GL.Arb.BufferData( BufferTargetArb.ArrayBuffer, new IntPtr( size ), lineVertices, BufferUsageArb.StaticDraw );
				GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, 0 );
				GL.LineWidth( 2 );
			}
		}
		
		protected override void RenderTerrainVbo() {
			GL.EnableClientState( ArrayCap.VertexArray );
			GL.EnableClientState( ArrayCap.ColorArray );
			
			GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, fullVboId );
			GL.VertexPointer( 3, VertexPointerType.Float, 16, new IntPtr( 0 ) );
			GL.ColorPointer( 4, ColorPointerType.UnsignedByte, 16, new IntPtr( 12 ) );
			GL.DrawArrays( BeginMode.Triangles, 0, fullVerticesCount );
			
			if( linesEnabled ) {
				GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Line );
				GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, linesVboId );
				GL.VertexPointer( 3, VertexPointerType.Float, 16, new IntPtr( 0 ) );
				GL.ColorPointer( 4, ColorPointerType.UnsignedByte, 16, new IntPtr( 12 ) );
				GL.DrawArrays( BeginMode.Quads, 0, verticesCount );
				GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Fill );
			}
			
			GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, 0 );
			GL.DisableClientState( ArrayCap.VertexArray );
			GL.DisableClientState( ArrayCap.ColorArray );
		}
		
		#if OPTIMISED_COLOURS
		protected override void RenderStretchedFlatFace( TileDrawInfo drawInfo, int index ) {
			FastColour colour = lineMode ? new FastColour( 150, 150, 150 ) :
				TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );
			
			if( !lineMode ) {
				vertices[index] = new Vertex( x, z1, y + 0.5f, colour );
				vertices[index + 1] = new Vertex( x + 0.5f, z1, y, colour );
				vertices[index + 2] = new Vertex( x + stretchY, z2, y + stretchY - halfTileLength, colour );
				
				vertices[index + 3] = new Vertex( x + stretchY, z2, y + stretchY - halfTileLength, colour );
				vertices[index + 4] = new Vertex( x + stretchY - halfTileWidth, z2, y + stretchY, colour );
				vertices[index + 5] = new Vertex( x, z1, y + 0.5f, colour );
			} else {
				vertices[index] = new Vertex( x, z1, y + 0.5f, colour );
				vertices[index + 1] = new Vertex( x + 0.5f, z1, y, colour );
				vertices[index + 2] = new Vertex( x + stretchY, z2, y + stretchY - halfTileLength, colour );
				vertices[index + 3] = new Vertex( x + stretchY - halfTileWidth, z2, y + stretchY, colour );
			}
			
			drawInfo.TotalTriangles += 2;
		}
		#endif	

		protected override void RenderTopFace( TileDrawInfo drawInfo, int index ) {
			FastColour colour = lineMode ? new FastColour( 150, 150, 150 ) :
				TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );
			
			if( !lineMode ) {
				vertices[index    ] = new Vertex( x, z2, y + halfTileLength, colour );
				vertices[index + 1] = new Vertex( x + halfTileWidth, z1, y, colour );
				vertices[index + 2] = new Vertex( x + tileWidth, z1, y + halfTileLength, colour );
				
				vertices[index + 3] = new Vertex( x + tileWidth, z1, y + halfTileLength, colour );
				vertices[index + 4] = new Vertex( x + halfTileWidth, z2, y + tileWidth, colour );
				vertices[index + 5] = new Vertex( x, z2, y + halfTileLength, colour );
			} else {
				vertices[index    ] = new Vertex( x, z2, y + halfTileLength, colour );
				vertices[index + 1] = new Vertex( x + halfTileWidth, z1, y, colour );
				vertices[index + 2] = new Vertex( x + tileWidth, z1, y + halfTileLength, colour );
				vertices[index + 3] = new Vertex( x + halfTileWidth, z2, y + tileWidth, colour );
			}
			drawInfo.TotalTriangles += 2;
		}
		
		protected override void RenderTopFaceRotated( TileDrawInfo drawInfo, int index ) {
			FastColour colour = lineMode ? new FastColour( 150, 150, 150 ) :
				TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );
			
			if( !lineMode ) {
				vertices[index    ] = new Vertex( x, z1, y + halfTileLength, colour );
				vertices[index + 1] = new Vertex( x + halfTileWidth, z1, y, colour );
				vertices[index + 2] = new Vertex( x + tileWidth, z2, y + halfTileLength, colour );
				
				vertices[index + 3] = new Vertex( x + tileWidth, z2, y + halfTileLength, colour );
				vertices[index + 4] = new Vertex( x + halfTileWidth, z2, y + tileWidth, colour );
				vertices[index + 5] = new Vertex( x, z1, y + halfTileLength, colour );
			} else {
				vertices[index    ] = new Vertex( x, z1, y + halfTileLength, colour );
				vertices[index + 1] = new Vertex( x + halfTileWidth, z1, y, colour );
				vertices[index + 2] = new Vertex( x + tileWidth, z2, y + halfTileLength, colour );
				vertices[index + 3] = new Vertex( x + halfTileWidth, z2, y + tileWidth, colour );
			}
			drawInfo.TotalTriangles += 2;
		}
		
		protected override void RenderBottomCornerFaceRotated( TileDrawInfo drawInfo, int index ) {
			FastColour colour = lineMode ? new FastColour( 150, 150, 150 ) :
				TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );
			
			if( !lineMode ) {
				vertices[index    ] = new Vertex( x, height, y + halfTileLength, colour );
				vertices[index + 1] = new Vertex( x + halfTileWidth, z1, y, colour );
				vertices[index + 2] = new Vertex( x + tileWidth, height, y + halfTileLength, colour );

				vertices[index + 3] = new Vertex( x + tileWidth, height, y + halfTileLength, colour );
				vertices[index + 4] = new Vertex( x + halfTileWidth, z2, y + tileWidth, colour );
				vertices[index + 5] = new Vertex( x, height, y + halfTileLength, colour );
			} else {
				vertices[index    ] = new Vertex( x, height, y + halfTileLength, colour );
				vertices[index + 1] = new Vertex( x + halfTileWidth, z1, y, colour );
				vertices[index + 2] = new Vertex( x + tileWidth, height, y + halfTileLength, colour );
				vertices[index + 3] = new Vertex( x + halfTileWidth, z2, y + tileWidth, colour );
			}
			drawInfo.TotalTriangles += 2;
		}
		
		protected override void RenderBottomCornerFace( TileDrawInfo drawInfo, int index ) {
			FastColour colour = lineMode ? new FastColour( 150, 150, 150 ) :
				TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );
			
			if( !lineMode ) {
				vertices[index    ] = new Vertex( x + halfTileWidth, height, y, colour );
				vertices[index + 1] = new Vertex( x, z2, y + halfTileLength, colour );
				vertices[index + 2] = new Vertex( x + halfTileWidth, height, y + tileWidth, colour );
				
				vertices[index + 3] = new Vertex( x + halfTileWidth, height, y, colour );
				vertices[index + 4] = new Vertex( x + tileWidth, z1, y + halfTileLength, colour );
				vertices[index + 5] = new Vertex( x + halfTileWidth, height, y + tileWidth, colour );
			} else {
				vertices[index    ] = new Vertex( x + halfTileWidth, height, y, colour );
				vertices[index + 1] = new Vertex( x, z2, y + halfTileLength, colour );
				vertices[index + 2] = new Vertex( x + halfTileWidth, height, y + tileWidth, colour );
				vertices[index + 3] = new Vertex( x + tileWidth, z1, y + halfTileLength, colour );
			}
			drawInfo.TotalTriangles += 2;
		}
		
		protected override void RenderTopCornerFaceRotated( TileDrawInfo drawInfo, int index ) {
			float height = this.height + tileHeight;
			FastColour colour = lineMode ? new FastColour( 150, 150, 150 ) :
				TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );
			
			if( !lineMode ) {
				vertices[index    ] = new Vertex( x, height, y + halfTileLength, colour );
				vertices[index + 1] = new Vertex( x + halfTileWidth, z1, y, colour );
				vertices[index + 2] = new Vertex( x + tileWidth, height, y + halfTileLength, colour );

				vertices[index + 3] = new Vertex( x + tileWidth, height, y + halfTileLength, colour );
				vertices[index + 4] = new Vertex( x + halfTileWidth, z2, y + tileWidth, colour );
				vertices[index + 5] = new Vertex( x, height, y + halfTileLength, colour );
			} else {
				vertices[index    ] = new Vertex( x, height, y + halfTileLength, colour );
				vertices[index + 1] = new Vertex( x + halfTileWidth, z1, y, colour );
				vertices[index + 2] = new Vertex( x + tileWidth, height, y + halfTileLength, colour );
				vertices[index + 3] = new Vertex( x + halfTileWidth, z2, y + tileWidth, colour );
			}
			drawInfo.TotalTriangles += 2;
		}
		
		protected override void RenderTopCornerFace( TileDrawInfo drawInfo, int index ) {
			float height = this.height + tileHeight;
			FastColour colour = lineMode ? new FastColour( 150, 150, 150 ) :
				TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );
			
			if( !lineMode ) {
				vertices[index    ] = new Vertex( x + halfTileWidth, height, y, colour );
				vertices[index + 1] = new Vertex( x, z2, y + halfTileLength, colour );
				vertices[index + 2] = new Vertex( x + halfTileWidth, height, y + tileWidth, colour );
				
				vertices[index + 3] = new Vertex( x + halfTileWidth, height, y, colour );
				vertices[index + 4] = new Vertex( x + tileWidth, z1, y + halfTileLength, colour );
				vertices[index + 5] = new Vertex( x + halfTileWidth, height, y + tileWidth, colour );
			} else {
				vertices[index    ] = new Vertex( x + halfTileWidth, height, y, colour );
				vertices[index + 1] = new Vertex( x, z2, y + halfTileLength, colour );
				vertices[index + 2] = new Vertex( x + halfTileWidth, height, y + tileWidth, colour );
				vertices[index + 3] = new Vertex( x + tileWidth, z1, y + halfTileLength, colour );
			}
			drawInfo.TotalTriangles += 2;
		}
	}
}