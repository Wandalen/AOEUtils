/*#define DEBUG_MAP
using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Collections.Generic;

namespace IsometricTests {
	
	public sealed class IsometricTes1t : GameWindow {
		/// <summary>Creates a 800x600 window with the specified title.</summary>
		public IsometricTes1t() : base( 800, 600, GraphicsMode.Default, "Isometric Tests" ) {
			VSync = VSyncMode.On;
		}

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad(e);
			GL.ClearColor( 0.1f, 0.2f, 0.5f, 0.0f );
			GL.Enable( EnableCap.DepthTest );
			Mouse.WheelChanged += MouseZoom;
		}
		
		float zoom = 10f;
		float horizontalOffset, verticalOffset;

		void MouseZoom( object sender, MouseWheelEventArgs e ) {
			zoom -= e.Delta;
			UpdateProjection();
		}
		
		void UpdateProjection() {
			Matrix4 projection = Matrix4.CreateOrthographicOffCenter( -zoom - horizontalOffset, zoom - horizontalOffset,
			                                                         -zoom - verticalOffset, zoom - verticalOffset,
			                                                         -100f, 100f );
			GL.MatrixMode( MatrixMode.Projection );
			GL.LoadMatrix( ref projection );
		}		

		protected override void OnUpdateFrame( FrameEventArgs e ) {
			base.OnUpdateFrame( e );
			if( Keyboard[Key.Escape] )
				Exit();
			
			if( Keyboard[Key.Left] ) {
				horizontalOffset += 0.1f;
				UpdateProjection();
			}
			if( Keyboard[Key.Right] ) {
				horizontalOffset -= 0.1f;
				UpdateProjection();
			}
			if( Keyboard[Key.Up] ) {
				verticalOffset -= 0.1f;
				UpdateProjection();
			}
			if( Keyboard[Key.Down] ) {
				verticalOffset += 0.1f;
				UpdateProjection();
			}
		}
		
		float rotX, rotY, rotZ;
		
		void DrawCube( float x1, float y1, float z1, float x2, float y2, float z2 ) {
			// Render a cube
			GL.Begin( BeginMode.Quads );
			// Top face
			GL.Color3( 0.0f, 1.0f, 0.0f );  // Green
			GL.Vertex3( x2, y2, z1 );  // Top-right of top face
			GL.Vertex3( x1, y2, z1 );  // Top-left of top face
			GL.Vertex3( x1, y2, z2 );  // Bottom-left of top face
			GL.Vertex3( x2, y2, z2 );  // Bottom-right of top face
			
			// Bottom face
			GL.Color3( 1.0f, 0.5f, 0.0f ); // Orange
			GL.Vertex3( x2, y1, z1 ); // Top-right of bottom face
			GL.Vertex3( x1, y1, z1 ); // Top-left of bottom face
			GL.Vertex3( x1, y1, z2 ); // Bottom-left of bottom face
			GL.Vertex3( x2, y1, z2 ); // Bottom-right of bottom face
			
			// Front face
			GL.Color3( 1.0f, 0.0f, 0.0f );  // Red
			GL.Vertex3( x2, y2, z2 );  // Top-Right of front face
			GL.Vertex3( x1, y2, z2 );  // Top-left of front face
			GL.Vertex3( x1, y1, z2 );  // Bottom-left of front face
			GL.Vertex3( x2, y1, z2 );  // Bottom-right of front face
			
			// Back face
			GL.Color3( 1.0f, 1.0f, 0.0f ); // Yellow
			GL.Vertex3( x2, y1, z1 ); // Bottom-Left of back face
			GL.Vertex3( x1, y1, z1 ); // Bottom-Right of back face
			GL.Vertex3( x1, y2, z1 ); // Top-Right of back face
			GL.Vertex3( x2, y2, z1 ); // Top-Left of back face
			
			// Left face
			GL.Color3( 0.0f, 0.0f, 1.0f );  // Blue
			GL.Vertex3( x1, y2, z2 );  // Top-Right of left face
			GL.Vertex3( x1, y2, z1 );  // Top-Left of left face
			GL.Vertex3( x1, y1, z1 );  // Bottom-Left of left face
			GL.Vertex3( x1, y1, z2 );  // Bottom-Right of left face
			
			// Right face
			GL.Color3( 1.0f, 0.0f, 1.0f );  // Violet
			GL.Vertex3( x2, y2, z2 );  // Top-Right of left face
			GL.Vertex3( x2, y2, z1 );  // Top-Left of left face
			GL.Vertex3( x2, y1, z1 );  // Bottom-Left of left face
			GL.Vertex3( x2, y1, z2 );  // Bottom-Right of left face
			GL.End();
		}
		
		protected override void OnResize( EventArgs e ) {
			base.OnResize( e );
			GL.Viewport( ClientRectangle );
			renderer2d.Width = Width;
			renderer2d.Height = Height;
			UpdateProjection();
		}
		
		Renderers.Renderer2D renderer2d = new Renderers.Renderer2D();

		protected override void OnRenderFrame( FrameEventArgs e ) {
			base.OnRenderFrame( e );
			
			GL.ClearColor( 0.0f, 0.0f, 0.0f, 1.0f );
			GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

			GL.MatrixMode( MatrixMode.Modelview );
			GL.LoadIdentity();
			//GL.Rotate( 35.624f, Vector3.UnitX );
			
			GL.Rotate( 45f, Vector3.UnitX );
			GL.Rotate( 45f, Vector3.UnitY );
			
			GL.Rotate( rotX, Vector3.UnitX );
			GL.Rotate( rotY, Vector3.UnitY );
			GL.Rotate( rotZ, Vector3.UnitZ );
			
			/*DrawCube( -1, 0, -1, 0, 2, 0 );
			DrawCube( 0, 0, 0, 1, 2, 1 );
			DrawCube( 1, 0, 1, 2, 2, 2 );
			
			DrawCube( -1, 0, -2, 0, 3, -1 );
			DrawCube( 0, 0, -1, 1, 3, 0 );
			DrawCube( 1, 0, 0, 2, 3, 1 );*/
			
			//DrawGridOfCubes( -3, -3, 3, 3 );
			//DrawDiagonalLine( -5, 3, 0, 2 );
			//DrawDiagonalLine( -5, 12, 1, 4 );
			//DrawDiagonalLine( 3, 3, 0, 2 );
			
			//DrawDiagonalLine( -3, 2, -5, 1 );
			//DrawLine( -1, 3, 0, 2 );
			//DrawLine( -1, 4, 1, 3 );
			//DrawLine( -1, 5, 2, 4 );
			//DrawGrid( -5, -5, 5, 5, 3 );

			//GL.End();
			
			//RenderTestTiles();
			/*#if !DEBUG_MAP
			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Line );
			RenderTestTiles();
			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Fill );
			#else
			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Line );
			RenderTestMap();
			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Fill );
			#endif
			
			GL.Color3( 1f, 1f, 1f );
			renderer2d.Render();
			
			UpdateTitleFps( e );
			SwapBuffers();
		}
		
		public long Triangles { get; private set; }
		
		DateTime lasttitleupdate;
		int fpscount = 0;
		float longestframedt = 0;
		int fpsTexture = -1;
		
		private void UpdateTitleFps( FrameEventArgs e ) {
			fpscount++;
			longestframedt = (float)Math.Max( longestframedt, e.Time );
			TimeSpan elapsed = DateTime.UtcNow - lasttitleupdate;
			if( elapsed.TotalSeconds >= 1 )  {
				lasttitleupdate = DateTime.UtcNow;
				const string formatString = "FPS: {0} (min {1}), Triangles: {2}";
				string fpstext = String.Format( formatString,
				                               (int)( fpscount / elapsed.TotalSeconds ),
				                               (int)( 1f / longestframedt ),
				                               Triangles );
				longestframedt = 0;
				fpscount = 0;
				if( fpsTexture != -1 ) {
					renderer2d.DeleteTexture( fpsTexture );
				}
				Console.WriteLine( fpstext );
				fpsTexture = renderer2d.MakeTextTexture( fpstext, 10, 5, 5 );
			}
		}
		
		Map map;
		
		void RenderTestMap() {
			if( map == null ) {
				map = new Map();
				map.SetMapSizes( 20, 20 );
				TerrainBuilder builder = new TerrainBuilder( map );
				builder.RaiseMountain( 10, 10, 4.5f );
			}
			map.Render();
			Triangles = map.TotalTriangles;
		}

		// <><><><><><><><><><><><>
		// ||||||||||||||||||||||||
		// <><><><><><><><><><><><>
		void DrawDiagonalLine( int x1, int count, int yOffset, int height ) {
			for( int x = x1; x < x1 + count; x++ ) {
				DrawCube( x, 0, x - yOffset, x + 1, height, x + 1 - yOffset );
			}
		}

		void DrawLine( int x1, int count, int yOffset, int height ) {
			for( int x = x1; x < x1 + count; x++ ) {
				DrawCube( x, 0, 0 - yOffset, x + 1, height, 0 + 1 - yOffset );
			}
		}

		void DrawGrid( int x1, int y1, int x2, int y2, int height ) {
			for( int x = x1; x < x2; x++ ) {
				for( int y = y1; y < y2; y++ ) {
					DrawCube( x, 0, y, x + 1, height, y + 1 );
				}
			}
		}

		/// <summary> The main entry point for the application. </summary>
		[STAThread]
		static void Main() {
			using( IsometricTest game = new IsometricTest() ) {
				game.Run(30.0);
			}
		}
	}
}*/