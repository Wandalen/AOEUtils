#define OPTIMISED_LINES
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace IsometricTests.Renderers {
	
	#if OPTIMISED_LINES
	public class LinesRendererVbo : OptimisedTileRendererVbo
		#else
		public class LinesRendererVbo : TileRendererVbo
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
			get { return 6; }
		}
		
		public override void Start( int terrainTexture ) {
			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Line );
		}
		
		protected override void CleanupResources() {
			if( vboId != -1 ) {
				GL.Arb.DeleteBuffers( 1, ref vboId );
			}
		}
		
		int vboId = -1;
		
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
		Vertex[] vertices;
		
		protected override void BuildTerrainVbo( TerrainMap map ) {
			vboId = GLUtils.GenBuffer();
			vertices = new Vertex[map.Width * map.Length * PrimitiveElementSize];
			BuildTerrain( map, ref triangles );
			Console.WriteLine( "Triangles:" + triangles + "," + verticesCount );
			
			Console.WriteLine( GC.GetTotalMemory( false ) / 1024 / 1024 );
			Console.WriteLine( vertices.Length * Marshal.SizeOf( default( Vertex ) ) / 1024 / 1024 );
			
			int size = verticesCount * 16;
			GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, vboId );
			GL.Arb.BufferData( BufferTargetArb.ArrayBuffer, new IntPtr( size ), vertices, BufferUsageArb.StaticDraw );
			GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, 0 );
			
			// Yes this is not nice having to force manual garbage collection.
			
			// TODO: Consider using unmanaged memory for this process.
			// ptr = Marshal.AllocHGlobal( width * length * 6 * Vertex.Stride )
		}
		
		protected override void RenderTerrainVbo() {
			GL.EnableClientState( ArrayCap.VertexArray );
			GL.EnableClientState( ArrayCap.ColorArray );
			
			GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, vboId );
			GL.VertexPointer( 3, VertexPointerType.Float, 16, new IntPtr( 0 ) );
			GL.ColorPointer( 4, ColorPointerType.UnsignedByte, 16, new IntPtr( 12 ) );
			GL.DrawArrays( BeginMode.Triangles, 0, verticesCount );
			GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, 0 );
			
			GL.DisableClientState( ArrayCap.VertexArray );
			GL.DisableClientState( ArrayCap.ColorArray );
		}
		
		
		public override void End() {
			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Fill );
		}

		protected override void RenderTopFace( TileDrawInfo drawInfo, int index ) {
			FastColour colour = TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );
			vertices[index] = new Vertex( x, z2, y + halfTileLength, colour );
			vertices[index + 1] = new Vertex( x + halfTileWidth, z1, y, colour );
			vertices[index + 2] = new Vertex( x + tileWidth, z1, y + halfTileLength, colour );
			
			vertices[index + 3] = new Vertex( x + tileWidth, z1, y + halfTileLength, colour );
			vertices[index + 4] = new Vertex( x + halfTileWidth, z2, y + tileLength, colour );
			vertices[index + 5] = new Vertex( x, z2, y + halfTileLength, colour );
			drawInfo.TotalTriangles += 2;
		}
		
		#if OPTIMISED_LINES
		protected override void RenderStretchedFlatFace( TileDrawInfo drawInfo, int index ) {
			FastColour colour = TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );	
			
			vertices[index] = new Vertex( x, z1, y + 0.5f, colour );
			vertices[index + 1] = new Vertex( x + 0.5f, z1, y, colour );
			vertices[index + 2] = new Vertex( x + stretchY, z2, y + stretchY - halfTileLength, colour );
			
			vertices[index + 3] = new Vertex( x + stretchY, z2, y + stretchY - halfTileLength, colour );
			vertices[index + 4] = new Vertex( x + stretchY - halfTileWidth, z2, y + stretchY, colour );
			vertices[index + 5] = new Vertex( x, z1, y + 0.5f, colour );
			drawInfo.TotalTriangles += 2;
		}
		#endif
		
		protected override void RenderTopFaceRotated( TileDrawInfo drawInfo, int index ) {
			FastColour colour = TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );
			vertices[index] = new Vertex( x, z1, y + 0.5f, colour );
			vertices[index + 1] = new Vertex( x + 0.5f, z1, y, colour );
			vertices[index + 2] = new Vertex( x + tileWidth, z2, y + 0.5f, colour );
			
			vertices[index + 3] = new Vertex( x + tileWidth, z2, y + 0.5f, colour );
			vertices[index + 4] = new Vertex( x + 0.5f, z2, y + tileWidth, colour );
			vertices[index + 5] = new Vertex( x, z1, y + 0.5f, colour );
			drawInfo.TotalTriangles += 2;
		}
		
		protected override void RenderBottomCornerFaceRotated( TileDrawInfo drawInfo, int index ) {
			FastColour colour = TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );
			vertices[index] = new Vertex( x, height, y + 0.5f, colour );
			vertices[index + 1] = new Vertex( x + 0.5f, z1, y, colour );
			vertices[index + 2] = new Vertex( x + tileWidth, height, y + 0.5f, colour );

			vertices[index + 3] = new Vertex( x + tileWidth, height, y + 0.5f, colour );
			vertices[index + 4] = new Vertex( x + 0.5f, z2, y + tileWidth, colour );
			vertices[index + 5] = new Vertex( x, height, y + 0.5f, colour );
			drawInfo.TotalTriangles += 2;
		}
		
		protected override void RenderBottomCornerFace( TileDrawInfo drawInfo, int index ) {
			FastColour colour = TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );
			vertices[index] = new Vertex( x + 0.5f, height, y, colour );
			vertices[index + 1] = new Vertex( x, z2, y + 0.5f, colour );
			vertices[index + 2] = new Vertex( x + 0.5f, height, y + tileWidth, colour );
			
			vertices[index + 3] = new Vertex( x + 0.5f, height, y, colour );
			vertices[index + 4] = new Vertex( x + tileWidth, z1, y + 0.5f, colour );
			vertices[index + 5] = new Vertex( x + 0.5f, height, y + tileWidth, colour );
			drawInfo.TotalTriangles += 2;
		}
		
		protected override void RenderTopCornerFaceRotated( TileDrawInfo drawInfo, int index ) {
			float height = this.height + tileHeight;
			FastColour colour = TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );
			vertices[index] = new Vertex( x, height, y + 0.5f, colour );
			vertices[index + 1] = new Vertex( x + 0.5f, z1, y, colour );
			vertices[index + 2] = new Vertex( x + tileWidth, height, y + 0.5f, colour );

			vertices[index + 3] = new Vertex( x + tileWidth, height, y + 0.5f, colour );
			vertices[index + 4] = new Vertex( x + 0.5f, z2, y + tileWidth, colour );
			vertices[index + 5] = new Vertex( x, height, y + 0.5f, colour );
			drawInfo.TotalTriangles += 2;
		}
		
		protected override void RenderTopCornerFace( TileDrawInfo drawInfo, int index ) {
			float height = this.height + tileHeight;
			FastColour colour = TerrainHelper.GetColour( drawInfo.CurrentTile.TerrainId );
			vertices[index] = new Vertex( x + 0.5f, height, y, colour );
			vertices[index + 1] = new Vertex( x, z2, y + 0.5f, colour );
			vertices[index + 2] = new Vertex( x + 0.5f, height, y + tileWidth, colour );
			
			vertices[index + 3] = new Vertex( x + 0.5f, height, y, colour );
			vertices[index + 4] = new Vertex( x + tileWidth, z1, y + 0.5f, colour );
			vertices[index + 5] = new Vertex( x + 0.5f, height, y + tileWidth, colour );
			drawInfo.TotalTriangles += 2;
		}
	}
}