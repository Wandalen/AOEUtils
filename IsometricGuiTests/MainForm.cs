using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using IsometricTests;
using AoeUtils;

namespace IsometricGuiTests {
	
	public partial class MainForm : Form {
		bool openglLoaded = false;
		
		public MainForm() {
			InitializeComponent();
		}
		
		void GlControl1Load( object sender, EventArgs e ) {
			System.Diagnostics.Debug.WriteLine( "LOAD!" );
			File.WriteAllBytes( "savegame.bin", Ionic.Zlib.DeflateStream.UncompressBuffer( File.ReadAllBytes( "bjc15.gax" ) ) );
			openglLoaded = true;
			
			GL.ClearColor( 0, 0, 0, 1 );
			GL.Enable( EnableCap.DepthTest );
			GL.Enable( EnableCap.AlphaTest );
			GL.AlphaFunc( AlphaFunction.Greater, 0 );
			SetupViewport();
			MouseWheel += MouseWheelHandler;
		}

		void MouseWheelHandler( object sender, MouseEventArgs e ) {
			zoom -= e.Delta / 120f; // Match OpenTK GameWindow.
			UpdateProjection();
			glControl1.Invalidate();
		}
		
		protected override bool ProcessCmdKey( ref Message msg, Keys keyData ) {
			if( keyData == Keys.Left ) {
				horizontalOffset += 1f;
				UpdateProjection();
				glControl1.Invalidate();
				return true;
			} else if( keyData == Keys.Up ) {
				verticalOffset -= 1f;
				UpdateProjection();
				glControl1.Invalidate();
				return true;
			} else if( keyData == Keys.Down ) {
				verticalOffset += 1f;
				UpdateProjection();
				glControl1.Invalidate();
				return true;
			} else if( keyData == Keys.Right ) {
				horizontalOffset -= 1f;
				UpdateProjection();
				glControl1.Invalidate();
				return true;
			}
			return base.ProcessCmdKey( ref msg, keyData );
		}
		
		float zoom = 10f;
		float horizontalOffset, verticalOffset;

		void UpdateProjection() {
			Matrix4 projection = Matrix4.CreateOrthographicOffCenter(
				-zoom - horizontalOffset, zoom - horizontalOffset,
				-zoom - verticalOffset, zoom - verticalOffset,
				-1000f, 1000f );
			GL.MatrixMode( MatrixMode.Projection );
			GL.LoadMatrix( ref projection );
		}
		
		private void SetupViewport() {
			int width = glControl1.Width;
			int height = glControl1.Height;
			GL.Viewport( 0, 0, width, height );
			UpdateProjection();
		}
		
		void GlControl1Resize( object sender, EventArgs e ) {
			if( !openglLoaded ) return;
			SetupViewport();
			glControl1.Invalidate();
		}
		
		static float ToRadians( float angle ) {
			return (float)( Math.PI * angle / 180.0 );
		}
		
		void GlControl1Paint( object sender, PaintEventArgs e ) {
			if( !openglLoaded ) return;
			
			try {
				GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );
				GL.MatrixMode( MatrixMode.Modelview );
				
				Matrix4 modelView = Matrix4.Identity;
				modelView *= Matrix4.CreateRotationX( ToRadians( 60f ) );
				GL.LoadMatrix( ref modelView );
				
				RenderTestMap();
				glControl1.SwapBuffers();
			} catch ( Exception ex ) { System.Diagnostics.Debugger.Break(); }
		}
		
		public void RedrawGlControl() {
			glControl1.Refresh();
		}
		
		public TerrainMap map;
		public AokScenario LoadedScenario;
		
		static void TestAll() {
			//System.Diagnostics.Debugger.Break();
			string path = Path.Combine( "D:", "Age of Empires 2", "Scenario" );
			foreach( string file in Directory.GetFiles( path, "*.sc*", SearchOption.AllDirectories ) ) {
				//System.Diagnostics.Debug.WriteLine( file );
				try {
					Scenario.FromFile( file );
				} catch {
				}
			}
			System.Diagnostics.Debugger.Break();
		}
		
		void RenderTestMap() {
			//TestAll();
			
			if( map == null ) {
				using( FileStream fs = File.OpenRead( "Barbarossa_scn1.scx" ) ) {
					LoadedScenario = (AokScenario)Scenario.FromStream( fs );
					map = TerrainMap.FromMap( LoadedScenario.GameMap );
				}
			}
			map.Render();
		}
		
		protected override void OnClosed( EventArgs e ) {
			if( bottomState != null ) {
				bottomState.Dispose();
			}
		}
		
		bool requestedPath = false;
		public string AgeOfEmpires2Path {
			get {
				if( !requestedPath ) {
					RequestAgeOfEmpiresPath();
					requestedPath = true;
				}
				return aoe2Path;
			}
		}
		
		const string message =
			@"Some features require you to specify the directory Age of Empires 2 is located in.
You can choose not to provide the directory by clicking 'cancel',
but this will result in some features being disabled.

This message box will not be displayed again for the rest of this session.
";
		
		public void RequestAgeOfEmpiresPath() {
			if( MessageBox.Show( message, "", MessageBoxButtons.OKCancel ) == DialogResult.Cancel ) {
				return;
			}
			
			using( FolderBrowserDialog dialog = new FolderBrowserDialog() ) {
				dialog.Description = "Select your Age of Empires 2 directory.";
				DialogResult folderResult = dialog.ShowDialog();
				if( folderResult == DialogResult.OK ) {
					aoe2Path = dialog.SelectedPath;
				}
			}
		}
		
		string aoe2Path = null;
		
		BottomRegion bottomState;
		void SetBottomRegion( BottomRegion newState ) {
			if( bottomState != null ) {
				bottomState.Dispose();
			}
			if( newState != null ) {
				newState.Display();
			}
			bottomState = newState;
		}
		
		void CinematicsButtonClick( object sender, EventArgs e ) {
			SetBottomRegion( new CinematicBottomRegion( this ) );
		}
		
		void MessagesButtonClick( object sender, EventArgs e ) {
			SetBottomRegion( new MessagesBottomRegion( this ) );
		}
		
		void TriggersButtonClick( object sender, EventArgs e ) {
			SetBottomRegion( null );
		}
		
		void UnitsButtonClick( object sender, EventArgs e ) {
			SetBottomRegion( null );
		}
		
		void PlayersButtonClick( object sender, EventArgs e ) {
			SetBottomRegion( new PlayersBottomRegion( this ) );
		}
		
		void MapButtonClick( object sender, EventArgs e ) {
			SetBottomRegion( new MapBottomRegion( this ) );
		}
		
		void TerrainButtonClick( object sender, EventArgs e ) {
			SetBottomRegion( null );
		}
		
		void DiplomacyButtonClick( object sender, EventArgs e ) {
			SetBottomRegion( new DiplomacyBottomRegion( this ) );
		}
		
		void VictoryButtonClick( object sender, EventArgs e ) {
			SetBottomRegion( null );
		}
		
		void OptionsButtonClick( object sender, EventArgs e ) {
			SetBottomRegion( new OptionsBottomRegion( this ) );
		}
		
		void SaveButtonClick( object sender, EventArgs e ) {
			using( SaveFileDialog dialog = new SaveFileDialog() ) {
				dialog.Filter = "AoE II scenarios (*.scn, *.scx)|*.scn;*.scx";
				dialog.Title = "Save scenario..";
				DialogResult result = dialog.ShowDialog();
				
				if( result == DialogResult.OK ) {
					using( FileStream fs = File.Create( dialog.FileName ) ) {
						LoadedScenario.WriteToStream( fs );
					}
				}
			}
		}
		
		void LoadButtonClick( object sender, EventArgs e ) {
			using( OpenFileDialog dialog = new OpenFileDialog() ) {
				dialog.CheckPathExists = true;
				dialog.CheckFileExists = true;
				dialog.Filter = "AoE II scenarios (*.scn, *.scx)|*.scn;*.scx";
				dialog.Title = "Load scenario..";
				DialogResult result = dialog.ShowDialog();
				
				if( result == DialogResult.OK ) {
					map.Dispose();
					using( FileStream fs = File.OpenRead( dialog.FileName ) ) {
						LoadedScenario = (AokScenario)Scenario.FromStream( fs );
						map = TerrainMap.FromMap( LoadedScenario.GameMap );
					}
				}
			}
		}
	}
}