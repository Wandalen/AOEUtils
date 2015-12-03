using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace IsometricTests {
	
	public sealed class GuiRenderer : IDisposable {
		
		float width, height;
		bool _disabled;
		bool renderBottomSection = false;
		
		const int lineElementHeight = 30;
		const int topAreaHeight = 60;
		
		/// <summary> Whether the gui is disabled or not. </summary>
		public bool Disabled {
			get { return _disabled; }
			set { _disabled = value; }
		}
		
		void RenderTop() {
			RenderTopBackground();
			RenderTopLine( topAreaLine0 );
			RenderTopLine( topAreaLine1 );
		}
		
		void RenderTopLine( Element[] line ) {
			for( int i = 0; i < line.Length; i++ ) {
				Element element = line[i];
				if( element != null ) {
					element.Render();
				}
			}
		}
		
		void RenderBottom() {
			GL.Color3( (byte)147, (byte)73, (byte)0 );
			GL.Begin( BeginMode.Quads );
			GL.Vertex2( 0, height );
			GL.Vertex2( width, height );
			GL.Vertex2( width, height - 60 );
			GL.Vertex2( 0, height - 60 );
			GL.End();
			GL.Color3( 1f, 1f, 1f );
			
			bottomElements.ForEach( e => e.Render() );
		}
		
		void RenderTopBackground() {
			GL.Color3( (byte)147, (byte)73, (byte)0 );
			GL.Begin( BeginMode.Quads );
			GL.Vertex2( 0, 0 );
			GL.Vertex2( width, 0 );
			GL.Vertex2( width, 60 );
			GL.Vertex2( 0, 60 );
			GL.End();
			GL.Color3( 1f, 1f, 1f );
		}
		
		public void OnResize( float width, float height ) {
			this.width = width;
			this.height = height;
		}
		
		public bool ClickIsInInterface( int x, int y ) {
			return !_disabled && y <= topAreaHeight;
		}
		
		public void OnGuiMouseClick( MouseEventArgs e ) {
			int x = e.X;
			renderBottomSection = FindTopClickedElement( x, e.Y >= lineElementHeight );
		}
		
		sealed class TextElement : Element {
			
			public string Text;
			
			public bool ShouldRenderBackground = true;
			
			public TextElement( int id, float x, float y, float width, float height, string text ) :
				base( id, x, y, width, height )
			{
				Text = text;
			}
			
			public override void Render() {
				if( ShouldRenderBackground ) {
					RenderBackground();
				}
				RenderText();
			}
			
			public void RenderBackground() {
				GL.Begin( BeginMode.Quads );
				GL.Color3( 0f, 0f, 0f );
				
				GL.Vertex2( X, Y );
				GL.Vertex2( X + 2, Y );
				GL.Vertex2( X + 2, Y + lineElementHeight );
				GL.Vertex2( X, Y + lineElementHeight );
				
				float x2 = X + ElementWidth + 10;
				GL.Vertex2( x2, Y );
				GL.Vertex2( x2 - 2, Y );
				GL.Vertex2( x2 - 2, Y + lineElementHeight );
				GL.Vertex2( x2, Y + lineElementHeight );
				
				GL.Color3( (byte)128, (byte)64, (byte)0 );
				GL.Vertex2( X + 2, Y );
				GL.Vertex2( x2 - 2, Y );
				GL.Vertex2( x2 - 2, Y + lineElementHeight );
				GL.Vertex2( X + 2, Y + lineElementHeight );
				GL.End();
				GL.Color3( 1f, 1f, 1f );
			}
			
			public void RenderText() {
				GL.Enable( EnableCap.Texture2D );
				GL.BindTexture( TextureTarget.Texture2D, TextureId );
				float x1 = X + 5, y1 = Y;
				float x2 = x1 + Width, y2 = y1 + Height;
				
				GL.Begin( BeginMode.Quads );
				GL.TexCoord2( 1, 1 ); GL.Vertex2( x2, y2 );
				GL.TexCoord2( 1, 0 ); GL.Vertex2( x2, y1 );
				GL.TexCoord2( 0, 0 ); GL.Vertex2( x1, y1 );
				GL.TexCoord2( 0, 1 ); GL.Vertex2( x1, y2 );
				GL.End();
				GL.Disable( EnableCap.Texture2D );
			}
		}
		
		List<Element> bottomElements = new List<Element>();
		
		sealed class ButtonElement : Element {
			static int selectedTexture = -1, notSelectedTexture = -1;
			
			TextElement textElement;
			const int buttonSize = 20;
			
			public bool Selected = true;
			
			public ButtonElement( int id, float x, float y, string text ) :
				base( id, x, y, buttonSize, buttonSize )
			{
				if( text != null ) {
					textElement = MakeTextElement( text, 12, (int)( x + buttonSize * 1.5f ), (int)y );
					textElement.ShouldRenderBackground = false;
					Width += textElement.Width + buttonSize;
					ElementWidth = Width;
				}
				if( selectedTexture == -1 ) {
					selectedTexture = GLUtils.LoadTextureFile( "buttonpressed.png" );
					notSelectedTexture = GLUtils.LoadTextureFile( "buttonnotpressed.png" );
				}
			}
			
			public override void Render() {
				GL.Enable( EnableCap.Texture2D );
				if( Selected ) {
					GL.BindTexture( TextureTarget.Texture2D, selectedTexture );
				} else {
					GL.BindTexture( TextureTarget.Texture2D, notSelectedTexture );
				}
				float x1 = X + 10, y1 = Y;
				float x2 = x1 + buttonSize, y2 = y1 + buttonSize;
				
				GL.Begin( BeginMode.Quads );
				GL.TexCoord2( 1, 1 ); GL.Vertex2( x2, y2 );
				GL.TexCoord2( 1, 0 ); GL.Vertex2( x2, y1 );
				GL.TexCoord2( 0, 0 ); GL.Vertex2( x1, y1 );
				GL.TexCoord2( 0, 1 ); GL.Vertex2( x1, y2 );
				GL.End();
				GL.Disable( EnableCap.Texture2D );
				
				if( textElement != null ) {
					textElement.Render();
				}
			}
		}
		
		bool FindTopClickedElement( int x, bool line1 ) {
			Element[] lineElements = line1 ? topAreaLine1 : topAreaLine0;
			// Scan starting at the right of the screen.
			bool furthestElement = true;
			for( int i = lineElements.Length - 1; i >= 0; i-- ) {
				Element element = topAreaLine0[i];
				if( element == null ) continue;
				
				// Check if the user is clicking past the buttons.
				if( furthestElement ) {
					if( x >= element.X + element.ElementWidth + 10 ) {
						return false;
					}
					furthestElement = false;
				}
				
				if( x >= element.X ) {
					Console.WriteLine( "CLICKED LINE " + ( line1 ? "1" : "0" ) + ": " + i );
					// TODO: OnClick
					return true;
				}
			}
			return false;
		}
		
		public void Render() {
			if( _disabled ) return;
			
			GL.Disable( EnableCap.DepthTest );
			RenderTop();
			if( renderBottomSection ) {
				RenderBottom();
			}
			GL.Enable( EnableCap.DepthTest );
		}
		
		class Element {
			public int TextureId;
			public float X, Y;
			public float Width, Height;
			
			public float ElementWidth;
			
			public Element( int id, float x1, float y1, float width, float height ) {
				TextureId = id;
				X = x1;
				Y = y1;
				Width = width;
				Height = height;
			}
			
			public override int GetHashCode() {
				return TextureId;
			}
			
			public virtual void OnMouseHover( int x, int y ) {
			}
			
			public virtual void OnMouseLeftClick( int x, int y ) {
			}
			
			public virtual void Render() {
			}
		}
		
		public void DefineDefaultGui() {
			DefineTopElement( "Map", 0, 0 );
			DefineTopElement( "Global victory", 0, 1 );
			DefineTopElement( "Terrain", 1, 0 );
			DefineTopElement( "Options", 1, 1 );
			DefineTopElement( "Players", 2, 0 );
			DefineTopElement( "Messages", 2, 1 );
			DefineTopElement( "Units", 3, 0 );
			DefineTopElement( "Cinematics", 3, 1 );
			DefineTopElement( "Diplomacy", 4, 0 );
			DefineTopElement( "Triggers", 4, 1 );
			
			bottomElements.Add( new ButtonElement( 0, 0, 600 - 30, "TEST" ) );
		}
		
		const int elementsPerLine = 5;
		Element[] topAreaLine0 = new Element[elementsPerLine];
		Element[] topAreaLine1 = new Element[elementsPerLine];
		
		public void DefineTopElement( string value, int index, int line ) {
			if( line > 1 ) throw new ArgumentOutOfRangeException( "Top area only has two lines." );
			
			if( line == 1 ) {
				topAreaLine1[index] = MakeTextElement( value, 13, 0, lineElementHeight );
			} else {
				topAreaLine0[index] = MakeTextElement( value, 13, 0, 0 );
			}
			UpdateTopPositions();
		}
		
		void UpdateTopPositions() {
			float linePosition = 0;
			for( int i = 0; i < topAreaLine0.Length; i++ ) {
				Element element0 = topAreaLine0[i];
				Element element1 = topAreaLine1[i];
				float element0Width = element0 == null ? 0 : element0.Width + 10;
				float element1Width = element1 == null ? 0 : element1.Width + 10;
				float maxWidth = Math.Max( element0Width, element1Width );
				if( element0 != null ) {
					element0.X = linePosition;
					element0.ElementWidth = maxWidth;
				}
				if( element1 != null ) {
					element1.X = linePosition;
					element1.ElementWidth = maxWidth;
				}
				linePosition += maxWidth;
			}
		}
		
		void DeleteTopLineTextures( Element[] line ) {
			for( int i = 0; i < line.Length; i++ ) {
				Element element = line[i];
				if( element != null ) {
					GL.DeleteTexture( element.TextureId );
				}
			}
		}
		
		public void Dispose() {
			DeleteTopLineTextures( topAreaLine0 );
			DeleteTopLineTextures( topAreaLine1 );
		}
		
		static TextElement MakeTextElement( string text, float fontSize, int x1, int y1 ) {
			Font font = new Font( "Times New Roman", fontSize );
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
			using( bmp ) {
				int textureID = GLUtils.LoadTexture( bmp );
				return new TextElement( textureID, x1, y1, bmp.Width, bmp.Height, text );
			}
		}
		
		static Element Make2DTexture( Bitmap bmp, float x1, float y1 ) {
			using( bmp ) {
				int textureID = GLUtils.LoadTexture( bmp );
				return new Element( textureID, x1, y1, bmp.Width, bmp.Height );
			}
		}
		
		static Element Make2DTexture( Bitmap bmp, float x1, float y1, float width, float height ) {
			using( bmp ) {
				int textureID = GLUtils.LoadTexture( bmp );
				return new Element( textureID, x1, y1, width, height );
			}
		}
	}
}
