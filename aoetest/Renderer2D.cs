using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace IsometricTests {
	
	public sealed class Renderer2D : IDisposable {
		Dictionary<int, Texture> textures = new Dictionary<int, Texture>();
		
		public void MoveLeft( int id ) {
			Texture value = textures[id];
			value.X1--;
			textures[id] = value;
		}
		
		public void MoveRight( int id ) {
			Texture value = textures[id];
			value.X1++;
			textures[id] = value;
		}
		
		public void MoveUp( int id ) {
			Texture value = textures[id];
			value.Y1--;
			textures[id] = value;
		}
		
		public void MoveDown( int id ) {
			Texture value = textures[id];
			value.Y1++;
			textures[id] = value;
		}
		
		public void Render() {
			if( textures.Count == 0 ) return;
			
			GL.Enable( EnableCap.Texture2D );
			GL.Disable( EnableCap.DepthTest );
			//GL.Enable( EnableCap.AlphaTest );
			//GL.AlphaFunc( AlphaFunction.Greater, 0.0f );
			
			foreach( var kvp in textures ) {
				Texture texture = kvp.Value;
				GL.BindTexture( TextureTarget.Texture2D, texture.ID );
				float x1 = texture.X1, y1 = texture.Y1;
				float x2 = x1 + texture.Width, y2 = y1 + texture.Height;
				
				GL.Begin( BeginMode.Quads );
				// TODO: Configurable texture coordinates.
				// This will probably be required for graphics cards that don't
				// support non power of two textures. (Like my laptop)
				GL.TexCoord2( 1, 1 ); GL.Vertex2( x2, y2 );
				GL.TexCoord2( 1, 0 ); GL.Vertex2( x2, y1 );
				GL.TexCoord2( 0, 0 ); GL.Vertex2( x1, y1 );
				GL.TexCoord2( 0, 1 ); GL.Vertex2( x1, y2 );
				GL.End();
			}
			
			//GL.Disable( EnableCap.AlphaTest );
			GL.Enable( EnableCap.DepthTest );
			GL.Disable( EnableCap.Texture2D );
		}
		
		public void Dispose() {
			foreach( var texture in textures ) {
				GL.DeleteTexture( texture.Key );
			}
			textures.Clear();
		}
		
		public int MakeShadowedTextTexture( string text, float fontSize, float x1, float y1 ) {
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
			Brush shadowBrush = new SolidBrush( Color.Black );
			Bitmap bmp = new Bitmap( (int)totalwidth, (int)totalheight );
			using( Graphics g2 = Graphics.FromImage( bmp ) ) {
				g2.DrawString( text, font, shadowBrush, 1.3f, 1.3f );
				g2.DrawString( text, font, textBrush, 0, 0 );
			}
			textBrush.Dispose();
			shadowBrush.Dispose();
			font.Dispose();
			return Make2DTexture( bmp, x1, y1 );
		}
		
		public int MakeTextTexture( string text, float fontSize, int x1, int y1 ) {
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
		
		public int MakeColourTexture( FastColour colour, int x1, int y1, int width, int height ) {
			Bitmap bmp = new Bitmap( 1, 1 );
			bmp.SetPixel( 0, 0, Color.FromArgb( colour.A, colour.R, colour.G, colour.B ) );
			return Make2DTexture( bmp, x1, y1, width, height );
		}
		
		public int Make2DTexture( Bitmap bmp, float x1, float y1 ) {
			using( bmp ) {
				int textureID = GLUtils.LoadTexture( bmp );
				Texture texture = new Texture( textureID, x1, y1, bmp.Width, bmp.Height );
				textures.Add( textureID, texture );
				return textureID;
			}
		}
		
		public int Make2DTexture( Bitmap bmp, float x1, float y1, float width, float height ) {
			using( bmp ) {
				int textureID = GLUtils.LoadTexture( bmp );
				Texture texture = new Texture( textureID, x1, y1, width, height );
				textures.Add( textureID, texture );
				return textureID;
			}
		}
		
		public int Make2DTexture( string file, float x1, float y1 ) {
			using( Bitmap bmp = new Bitmap( file ) ) {
				return Make2DTexture( bmp, x1, y1 );
			}
		}
		
		public int Make2DTexture( string file, float x1, float y1, float width, float height ) {
			using( Bitmap bmp = new Bitmap( file ) ) {
				return Make2DTexture( bmp, x1, y1, width, height );
			}
		}
		
		public bool DeleteTexture( int id ) {
			bool removed = textures.Remove( id );
			// If the texture ID was found, delete from OpenGL as well to avoid a memory leak.
			if( removed ) {
				GL.DeleteTexture( id );
			}
			return removed;
		}
		
		
	}
	
	public struct Texture {
		public int ID;
		public float X1, Y1;
		public float Width, Height;
		
		public Texture( int id, float x1, float y1, float width, float height ) {
			ID = id;
			X1 = x1;
			Y1 = y1;
			Width = width;
			Height = height;
		}
		
		public override int GetHashCode() {
			return ID;
		}

	}
}
