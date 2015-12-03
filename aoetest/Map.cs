using System;
using System.Collections.Generic;
using IsometricTests.Renderers;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using AoeUtils;

namespace IsometricTests {
	
	public sealed class TerrainMap : IDisposable {
		
		public Tile[] Tiles {
			get { return tiles; }
		}
		
		public FrustumCulling Culling;
		
		Tile[] tiles;
		
		public int Width { get; private set; }
		public int Length { get; private set; }
		
		/// <summary> The size of individual chunks in the terrain. </summary>
		public int ChunkSize;
		
		public int TotalTriangles = -1;
		
		public void SetMapSizes( int width, int length ) {
			Width = width;
			Length = length;
			tiles = new Tile[Width * Length];
		}
		
		public void GenerateNewMap( int width, int length, byte terrainType, byte elevation ) {
			FromMapInternal( Map.GenerateBlank( (uint)width, (uint)length, terrainType, elevation ) );
		}
		
		public void FromMapInternal( Map gameMap ) {
			if( renderer != null ) {
				renderer.Dispose();
			}
			renderer = new ColoursRendererVbo();
			int width = (int)gameMap.Width;
			int length = (int)gameMap.Length;
			SetMapSizes( width, length );
			
			for( int y = 0; y < length; y++ ) {
				for( int x = 0; x < width; x++ ) {
					TerrainElement tile = gameMap[x, y];
					Tile mapTile = new Tile( x, y, tile.Elevation, tile.TerrainId );
					this[x, y] = mapTile;
				}
			}
		}
		
		//TileRendererTexturedGLVbo3 renderer = new TileRendererTexturedGLVbo3();
		TileRenderer renderer;
		public Renderer2D Renderer2d;
		//MapRenderer mapRenderer;
		
		int terrainTexture = -1;
		
		public void Render() {
			if( terrainTexture == -1 ) {
				terrainTexture = TerrainHelper.MakeTerrainTexture();
			}
			//RenderPointGrid();
			
			//GL.Enable( EnableCap.Texture2D );
			//GL.BindTexture( TextureTarget.Texture2D, terrainTexture );
			//GL.Begin( BeginMode.Quads );
			
			renderer.Start( terrainTexture );
			renderer.RenderTiles( this, ref TotalTriangles );
			renderer.End();
			
			//RenderBarracks();
			
			//GL.End();
			//GL.BindTexture( TextureTarget.Texture2D, 0 );
			//GL.Disable( EnableCap.Texture2D );
		}
		public IRenderTarget RenderTarget;
		
		
		void Mode2D() {
			GL.MatrixMode( MatrixMode.Projection );
			GL.PushMatrix();
			GL.LoadIdentity();
			float zoom = RenderTarget.Zoom;
			float width = RenderTarget.DisplayWidth;
			float height = RenderTarget.DisplayHeight;
			float horOffset = RenderTarget.HorizontalViewOffset;
			float verOffset = RenderTarget.VerticalViewOffset;
			
			GL.Ortho( -zoom - horOffset, zoom - horOffset, -zoom - verOffset, zoom - verOffset, -1000f, 1000f );
			GL.MatrixMode( MatrixMode.Modelview );
			GL.PushMatrix();
			GL.LoadIdentity();
			
			/*Matrix4 projection = Matrix4.CreateOrthographicOffCenter(
				-zoom - horizontalOffset, zoom - horizontalOffset,
				-zoom - verticalOffset, zoom - verticalOffset,
				-1000f, 1000f );*/
		}
		
		int keepId = -1, barracksId = -1, keep2Id = -1;
		Texture keepData, barracksData, keep2Data;
		
		static Texture Make2DTexture( Bitmap bmp, float x1, float y1 ) {
			using( bmp ) {
				int textureID = GLUtils.LoadTexture( bmp );
				return new Texture( textureID, x1, y1, bmp.Width, bmp.Height );
			}
		}
		
		
		void RenderSprite( float x1, float y1, float width, float height ) {
			Console.WriteLine( x1 + "," + y1 + "," + width + "," + height );
			width /= 100; height /= 100;
			float x2 = x1 + width, y2 = y1 + height;
			
			GL.TexCoord2( 1, 0 ); GL.Vertex2( x2, y2 );
			GL.TexCoord2( 1, 1 ); GL.Vertex2( x2, y1 );
			GL.TexCoord2( 0, 1 ); GL.Vertex2( x1, y1 );
			GL.TexCoord2( 0, 0 ); GL.Vertex2( x1, y2 );	
		}
		
		public void RenderBarracks() {
			if( keepId == -1 ) {
				keepData = Make2DTexture( new Bitmap( "5b.bmp" ), 0, 0 );
				barracksData = Make2DTexture( new Bitmap( "5.bmp" ), 0, 0 );
				keep2Data = Make2DTexture( new Bitmap( "13.bmp" ), 0, 0 );
				keepId = keepData.ID;
				barracksId = barracksData.ID;
				keep2Id = keep2Data.ID;
			}
			
			Mode2D();
			GL.Enable( EnableCap.Texture2D );
			GL.Disable( EnableCap.DepthTest );
			
			GL.BindTexture( TextureTarget.Texture2D, keepId );
			Texture texture = keepData;
			GL.Begin( BeginMode.Quads );
			RenderSprite( texture.X1, texture.Y1, texture.Width, texture.Height );
			//RenderSprite( texture.X1 + 2, texture.Y1, texture.Width, texture.Height );
			GL.End();
			
			//GL.BindTexture( TextureTarget.Texture2D, barracksId );
			//texture = barracksData;
			//GL.Begin( BeginMode.Quads );
			//RenderSprite( texture.X1, texture.Y1, texture.Width, texture.Height );
			//GL.End();
			
			GL.BindTexture( TextureTarget.Texture2D, keep2Id );
			texture = keep2Data;
			GL.Begin( BeginMode.Quads );
			RenderSprite( texture.X1 + 2, texture.Y1, texture.Width, texture.Height );
			GL.End();
			
			GL.Enable( EnableCap.DepthTest );
			GL.Disable( EnableCap.Texture2D );
			
			GLUtils.Mode3D();
			/*const float horSize = 3, verSize = 2f;
			float height = -1.5f;
			float x1 = 0, y1 = 0,
			x2 = horSize, y2 = 0,
			x3 = horSize, y3 = verSize,
			x4 = 0, y4 = verSize;
			
			
			GL.Disable( EnableCap.DepthTest );
			TranslatePoint( ref x1, ref y1 );
			TranslatePoint( ref x2, ref y2 );
			TranslatePoint( ref x3, ref y3 );
			TranslatePoint( ref x4, ref y4 );
			
			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Line );
			GL.Begin( BeginMode.Quads );
			GL.Vertex3( x1, height + 1, y1 );
			GL.Vertex3( x2, height + 1, y2 );
			GL.Vertex3( x3, height + 1, y3 );
			GL.Vertex3( x4, height + 1, y4 );
			GL.End();
			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Fill );
			GL.Enable( EnableCap.DepthTest );*/
		}
		
		void VertexUV( float x, float y, float z, float u, float v ) {
			GL.Vertex3( x, y, -z );
			GL.TexCoord2( u, v );
		}
		
		/*int textTexture = -1;
		Texture[] pointTextures;
		
		void RenderPointGrid() {
			GL.Color3( 0f, 0f, 1f );
			GL.Begin( BeginMode.Points );
			int counter = 0;
			for( float x = -2; x < 5; x += 0.5f ) {
				for( float y = -2; y < 10; y += 0.5f ) {
					GL.Vertex3( x, 1, -y );
					counter++;
				}
				
			}
			GL.End();
			GL.Color3( 1f, 1f, 1f );
			
			if( textTexture == -1 ) {
				pointTextures = new Texture[counter];
				counter = 0;
				
				for( float x = -2; x < 5; x += 0.5f ) {
					for( float y = -2; y < 10; y += 0.5f ) {
						pointTextures[counter] = MakeTextTexture( x + "," + y, 10, x, y );
						counter++;
					}
				}
				textTexture = 1;
			}
			
			GL.Enable( EnableCap.Texture2D );
			for( int i = 0; i < pointTextures.Length; i++ ) {
				Texture texture = pointTextures[i];
				if( texture.ID == 0 ) break;
				
				GL.BindTexture( TextureTarget.Texture2D, texture.ID );
				float x1 = texture.X1, y1 = texture.Y1;
				float x2 = x1 + 0.3f, y2 = y1 + 0.3f;
				
				GL.Begin( BeginMode.Quads );
				GL.TexCoord2( 1, 0 ); GL.Vertex3( x2, 1, -y2 );
				GL.TexCoord2( 1, 1 ); GL.Vertex3( x2, 1, -y1 );
				GL.TexCoord2( 0, 1 ); GL.Vertex3( x1, 1, -y1 );
				GL.TexCoord2( 0, 0 ); GL.Vertex3( x1, 1, -y2 );
				GL.End();
			}
			GL.Disable( EnableCap.Texture2D );
		}
		
		static Texture MakeTextTexture( string text, float fontSize, float x1, float y1 ) {
			Font font = new Font( "Arial", fontSize );
			float totalwidth = 0;
			float totalheight = 0;
			using( Bitmap measuringBmp = new Bitmap( 1, 1 ) ) {
				using( Graphics g = Graphics.FromImage( measuringBmp ) ) {
					SizeF size = g.MeasureString( text, font );
					totalwidth = size.Width;
					totalheight = size.Height;
				}
			}
			Brush textBrush = new SolidBrush( Color.White );
			Bitmap bmp = new Bitmap( (int)totalwidth, (int)totalheight );
			using( Graphics g2 = Graphics.FromImage( bmp ) ) {
				g2.DrawString( text, font, textBrush, 0, 0 );
			}
			textBrush.Dispose();
			font.Dispose();
			return Make2DTexture( bmp, x1, y1 );
		}
		
		static Texture Make2DTexture( Bitmap bmp, float x1, float y1 ) {
			using( bmp ) {
				int textureID = Renderer2D.LoadTexture( bmp );
				return new Texture( textureID, x1, y1, bmp.Width, bmp.Height );
			}
		}*/
		
		public Tile this[int x, int y] {
			get { return tiles[( y * Width ) + x]; }
			set { SetTile( x, y, value ); }
		}
		
		void SetTile( int x, int y, Tile tile ) {
			tile.X = x;
			tile.Y = y;
			TranslatePoint( ref tile.X, ref tile.Y );
			tiles[( y * Width ) + x] = tile;
		}
		
		static void TranslatePoint( ref float x, ref float y ) {
			float yHalf = y / 2f;
			float xHalf = x / 2f;
			
			//x = (y * 97 / 2f) + (x * 97 / 2f)
			//y = (x * 49 / 2f) - (y * tile_height / 2f)
			//http://stackoverflow.com/questions/892811/drawing-isometric-game-worlds
			x = yHalf + xHalf;
			y = yHalf - xHalf;
		}
		
		public static TerrainMap FromMap( Map gameMap ) {
			TerrainMap map = new TerrainMap();
			map.FromMapInternal( gameMap );
			return map;
		}
		
		public void Dispose() {
			if( renderer != null ) {
				renderer.Dispose();
			}
		}
	}
	
	/// <summary> Represents a tile in the world. </summary>
	public struct Tile {
		
		public float X;
		public float Y;
		public float Height;
		public byte TerrainId; // Note that each genie engine game has different terrain types,
		// so this can't be an enumeration.
		
		public Tile( float x, float y, float height, byte id ) {
			X = x; Y = y; Height = height;
			TerrainId = id;
		}
		
		public override string ToString() {
			return String.Format( "{0},{1},{2} (Id {3})", X, Y, Height, TerrainId );
		}
	}
	
	public sealed class TileDrawInfo {
		public Tile CurrentTile;
		
		// Diagonals
		public Tile? LeftBehind;
		public Tile? RightBehind;
		public Tile? LeftFront;
		public Tile? RightFront;
		
		// Horizontal / Verritcal
		public Tile? Left;
		public Tile? Right;
		public Tile? Front;
		public Tile? Behind;
		
		public int MapX;
		public int MapY;
		
		public int TotalTriangles;
	}
}