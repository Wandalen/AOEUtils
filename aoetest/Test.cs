#define DEBUG_MAP
using System;
using System.Drawing;
using System.IO;
using AoeUtils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace IsometricTests {
	
	public sealed class IsometricTest : GameWindow, IRenderTarget {
		/// <summary>Creates a 800x600 window with the specified title.</summary>
		public IsometricTest() : base( 800, 600, GraphicsMode.Default, "Isometric Tests" ) {
			VSync = VSyncMode.On;
		}

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );
			GL.ClearColor( 0.1f, 0.2f, 0.5f, 0.0f );
			GL.Enable( EnableCap.DepthTest );
			Mouse.WheelChanged += MouseZoom;
			Mouse.ButtonDown += MouseButtonDown;
			CreateCachedMatrices();
			GL.PointSize( 10 );
			
			GL.Enable( EnableCap.AlphaTest );
			GL.AlphaFunc( AlphaFunction.Greater, 0.0f );
			
			
			gui = new GuiRenderer();
			gui.DefineDefaultGui();
			Keyboard.KeyDown += KeyDown;
		}

		void KeyDown( object sender, KeyboardKeyEventArgs e ) {
			if( e.Key == Key.F1 ) {
				if( gui != null ) {
					gui.Disabled = !gui.Disabled;
				}
			}
			
			if( e.Key == Key.F11 ) {
				if( WindowState == WindowState.Fullscreen ) {
					WindowState = WindowState.Normal;
				} else {
					WindowState = WindowState.Fullscreen;
				}
			}
			
			if( e.Key == Key.F10 ) {
				if( VSync == VSyncMode.Off ) {
					VSync = VSyncMode.On;
				} else {
					VSync = VSyncMode.Off;
				}
			}
			if( e.Key == Key.W ) {
				renderer2d.MoveUp( t2 );
			}
			if( e.Key == Key.A ) {
				renderer2d.MoveLeft( t2 );
			}
			if( e.Key == Key.S ) {
				renderer2d.MoveDown( t2 );
			}
			if( e.Key == Key.D ) {
				renderer2d.MoveRight( t2 );
			}
		}

		void MouseButtonDown( object sender, MouseButtonEventArgs e ) {
			if( gui != null && gui.ClickIsInInterface( e.X, e.Y ) ) {
				gui.OnGuiMouseClick( e );
			} else {
				// TODO: Clicking on the game map.
				Vector2 mousePos = new Vector2( Mouse.X, Mouse.Y );
				GLUtils.UnProject( Projection, ModelView, this, mousePos );
			}
		}
		
		// IRenderTarget implementation
		public float Zoom {
			get { return zoom; }
		}
		
		public float HorizontalViewOffset {
			get { return horizontalOffset; }
		}
		
		public float VerticalViewOffset {
			get { return verticalOffset; }
		}
		
		public float DisplayWidth {
			get { return width; }
		}
		
		public float DisplayHeight {
			get { return height; }
		}
		
		float zoom = 10f; // 200f
		float horizontalOffset, verticalOffset;
		GuiRenderer gui;

		void MouseZoom( object sender, MouseWheelEventArgs e ) {
			zoom -= e.Delta;
			UpdateProjection();
		}
		
		void UpdateProjection() {
			Matrix4 projection = Matrix4.CreateOrthographicOffCenter(
				-zoom - horizontalOffset, zoom - horizontalOffset,
				-zoom - verticalOffset, zoom - verticalOffset,
				-1000f, 1000f );
			
			GL.MatrixMode( MatrixMode.Projection );
			GL.LoadMatrix( ref projection );
			Projection = projection;
			//UpdateCulling();
		}
		float rotX, rotY, rotZ;
		
		void UpdateCulling() {
			if( map != null ) {
				map.Culling.CalcFrustumEquations();
			}
		}

		protected override void OnUpdateFrame( FrameEventArgs e ) {
			base.OnUpdateFrame( e );
			if( Keyboard[Key.Escape] )
				Exit();
			
			if( Keyboard[Key.Number1] ) {
				rotX += 1f;
				//UpdateCulling();
			}
			if( Keyboard[Key.Number2] ) {
				rotX -= 1f;
				//UpdateCulling();
			}
			if( Keyboard[Key.Number3] ) {
				rotY += 1f;
				//UpdateCulling();
			}
			if( Keyboard[Key.Number4] ) {
				rotY -= 1f;
				//UpdateCulling();
			}
			if( Keyboard[Key.Number5] ) {
				rotZ += 1f;
				//UpdateCulling();
			}
			if( Keyboard[Key.Number6] ) {
				rotZ -= 1f;
				//UpdateCulling();
			}
			
			if( Keyboard[Key.Left] ) {
				horizontalOffset += 1f;
				UpdateProjection();
			}
			if( Keyboard[Key.Right] ) {
				horizontalOffset -= 1f;
				UpdateProjection();
			}
			if( Keyboard[Key.Up] ) {
				verticalOffset -= 1f;
				UpdateProjection();
			}
			if( Keyboard[Key.Down] ) {
				verticalOffset += 1f;
				UpdateProjection();
			}
		}
		
		float width, height;
		
		protected override void OnResize( EventArgs e ) {
			base.OnResize( e );
			GL.Viewport( ClientRectangle );
			width = Width;
			height = Height;
			gui.OnResize( width, height );
			UpdateProjection();
		}
		
		Renderer2D renderer2d = new Renderer2D();
		
		//static Matrix4 modelView;
		static void CreateCachedMatrices() {
			//modelView = Matrix4.Identity;
			//modelView *= Matrix4.CreateRotationX( ToRadians( 60f ) );
		}
		
		static float ToRadians( float angle ) {
			return (float)( Math.PI * angle / 180.0 );
		}
		
		public Matrix4 Projection, ModelView;

		protected override void OnRenderFrame( FrameEventArgs e ) {
			base.OnRenderFrame( e );
			GL.ClearColor( 0.0f, 0.0f, 0.0f, 1.0f );
			GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

			GL.MatrixMode( MatrixMode.Modelview );
			Matrix4 modelView = Matrix4.Identity;
			modelView *= Matrix4.CreateRotationZ( ToRadians( rotZ ) );
			modelView *= Matrix4.CreateRotationY( ToRadians( rotY ) );
			modelView *= Matrix4.CreateRotationX( ToRadians( rotX ) );
			
			//modelView *= Matrix4.CreateRotationY( ToRadians( 45f ) ); // 35f
			modelView *= Matrix4.CreateRotationX( ToRadians( 45f ) ); // was 60
			ModelView = modelView;
			
			GL.LoadMatrix( ref modelView );
			UpdateCulling();
			
			//GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Line );
			//GL.Disable( EnableCap.DepthTest );
			RenderTestMap();
			//GL.Enable( EnableCap.DepthTest );
			//GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Fill );
			
			
			GL.Color3( 1f, 1f, 1f );
			GLUtils.Mode2D( width, height );
			renderer2d.Render();
			//gui.Render();
			GLUtils.Mode3D();
			
			/*GL.Color3( 1f, 0f, 0f );
			GL.LineWidth( 3 );
			GL.Begin( BeginMode.LineStrip );
			
			Vector2 mousePos = new Vector2( Mouse.X, Mouse.Y );
			foreach( var pos in  GLUtils.UnProject( Projection, ModelView, this, mousePos ) ) {
				GL.Vertex3( pos.X, pos.Y, pos.Z );
				//GL.Vertex3( pos.Pos2.X, pos.Pos2.Y, pos.Pos2.Z );
				
			}
			//GL.Vertex3( pos.X, pos.Y, pos.Z );
			//GL.Vertex3( pos.X + 1, pos.Y, pos.Z );
			//GL.Vertex3( pos.X + 1, pos.Y, pos.Z + 1 );
			//GL.Vertex3( pos.X, pos.Y, pos.Z + 1 );
			GL.End();*/

			GL.Color3( 1f, 1f, 1f );
			
			UpdateTitleFps( e );
			SwapBuffers();
		}
		
		void Mode3D() {
			GL.MatrixMode( MatrixMode.Projection );
			// Get rid of orthographic 2D matrix.
			GL.PopMatrix();
			GL.MatrixMode( MatrixMode.Modelview );
			GL.PopMatrix();
		}
		
		public long Triangles { get; private set; }
		
		DateTime lasttitleupdate;
		int fpscount = 0;
		double longestframedt = 0;
		int fpsTexture = -1;
		
		private void UpdateTitleFps( FrameEventArgs e ) {
			fpscount++;
			longestframedt = Math.Max( longestframedt, e.Time );
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
				fpsTexture = renderer2d.MakeShadowedTextTexture( fpstext, 10, 5, 60 );
				//Console.WriteLine( "FFFFF:" + zoom );
				//Console.WriteLine( Vector3.Transform( new Vector3( 0, 0, 0 ), ModelView ) );
				//Console.WriteLine( Vector3.Transform( new Vector3( 0, 1, 0 ), ModelView ) );
				//Console.WriteLine( "--" );
				//Console.WriteLine( Vector3.Transform( new Vector3( 1, 0, 1 ), ModelView ) );
				//Console.WriteLine( Vector3.Transform( new Vector3( 1, 3, 1 ), ModelView ) );
			}
		}
		TerrainMap map;
		
		protected override void OnClosed( EventArgs e ) {
			renderer2d.Dispose();
			map.Dispose();
			base.OnClosed( e );
		}
		
		int t1 = -1, t2 = -1;
		
		void RenderTestMap() {
			if( map == null ) {
				//DrsFile.FromFile( "interfac.drs" );
				//SlpFile.FromFile( "2540.slp" );
				//ScenarioReader.ReadFile( "AOECOMB.scn" );
				
				using( FileStream fs = File.OpenRead( "hastings.scx" ) ) {
					//ScenarioReader.CreateHexScenario( "rawbinary.bin" );
					var scenario = (AokScenario)Scenario.FromStream( fs );
					
					map = TerrainMap.FromMap( scenario.GameMap );
					map.Culling = new FrustumCulling() { Window = this };
				}
				map.Renderer2d = renderer2d;
				map.RenderTarget = this;
				//t1 = renderer2d.Make2DTexture( "tfttt.bmp", 0, 0, 97 * 8, 49 * 8 );
				//t2 = renderer2d.Make2DTexture( "tfttt.bmp", 97 * 4, 49 * 4, 97 * 8, 49 * 8 );
				Console.WriteLine( "W:" + map.Width + ", L:" + map.Length );
			}
			map.Render();
			Triangles = map.TotalTriangles;
		}

		/// <summary> The main entry point for the application. </summary>
		[STAThread]
		static void Main() {
			using( IsometricTest game = new IsometricTest() ) {
				game.Run( 30.0 );
			}
		}
	}
}