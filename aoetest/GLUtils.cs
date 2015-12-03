using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace IsometricTests {

	public static class GLUtils {
		
		public static Vector3 UnProject( Matrix4 projection, Matrix4 view, GameWindow window, Vector2 mouse ) {
			Vector4 vector;
			//System.Diagnostics.Debugger.Break();
			
			Size viewport = window.ClientSize;
			vector.X = 2f * mouse.X / (float)viewport.Width - 1;
			vector.Y = -( 2f * mouse.Y / (float)viewport.Height - 1 );
			vector.Z = 0;
			vector.W = 1;
			
			Matrix4 viewInv = Matrix4.Invert( view );
			Matrix4 projInv = Matrix4.Invert( projection );
			
			Vector4.Transform( ref vector, ref projInv, out vector );
			Console.WriteLine( vector.X + "," + vector.Y + "," + vector.Z );
			Vector4.Transform( ref vector, ref viewInv, out vector );
			Console.WriteLine( vector.X + "," + vector.Y + "," + vector.Z );
			
			Console.WriteLine( "---" );
			
			//double xDir = 1;//Math.Cos( ToRadians( 60f ) );
			//double yDir = 1;//Math.Sin( ToRadians( 60f ) );
			//for( int i = 0; i < 10; i++ ) {
			//	yield return new Vector3( vector.X, vector.Y, vector.Z - 1 * i );
			//}
			return new Vector3( vector.X, vector.Y, vector.Z );
		}
		
		static float ToRadians( float angle ) {
			return (float)( Math.PI * angle / 180.0 );
		}
		
		static int textureDimensions = -1;
		public static int MaxTextureDimensions {
			get {
				if( textureDimensions == -1 )
					GL.GetInteger( GetPName.MaxTextureSize, out textureDimensions );
				return textureDimensions;
			}
		}
		
		public static int LoadTexture( Bitmap bmp ) {
			int id = GL.GenTexture();
			GL.Enable( EnableCap.Texture2D );
			GL.BindTexture( TextureTarget.Texture2D, id );
			GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest );
			GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest );
			
			//GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge );
			//GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge );

			BitmapData data = bmp.LockBits( new Rectangle( 0, 0, bmp.Width, bmp.Height),
			                               ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb );

			GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
			              OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0 );
			bmp.UnlockBits( data );
			GL.Disable( EnableCap.Texture2D );
			return id;
		}
		
		public static int LoadTexture( string path ) {
			if( String.IsNullOrEmpty( path ) ) throw new ArgumentException( "path is empty or null." );
			if( !File.Exists( path ) ) throw new FileNotFoundException( "Path not found." );
			
			using( Bitmap bmp = new Bitmap( path ) ) {
				return LoadTexture( bmp );
			}
		}
		
		public static int LoadTextureFile( string file ) {
			string path = Path.Combine( "textures", file );
			return LoadTexture( path );
		}
		
		public static int GenBuffer() {
			int[] temp = new int[1];
			GL.Arb.GenBuffers( 1, temp );
			return temp[0];
		}
		
		public static void Mode2D( float width, float height ) {
			GL.MatrixMode( MatrixMode.Projection );
			GL.PushMatrix();
			GL.LoadIdentity();
			GL.Ortho( 0, width, height, 0, 0, 1 );
			GL.MatrixMode( MatrixMode.Modelview );
			GL.PushMatrix();
			GL.LoadIdentity();
		}
		
		public static void Mode3D() {
			GL.MatrixMode( MatrixMode.Projection );
			// Get rid of orthographic 2D matrix.
			GL.PopMatrix();
			GL.MatrixMode( MatrixMode.Modelview );
			GL.PopMatrix();
		}
	}
}
