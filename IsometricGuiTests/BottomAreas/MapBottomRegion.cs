using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AoeUtils;
using System.Collections.Generic;
using AokTerrain = IsometricTests.TerrainHelper;

namespace IsometricGuiTests {
	
	public sealed class MapBottomRegion : BottomRegion {
		
		public MapBottomRegion( MainForm form ) : base( form ) {
		}
		
		ComboBox sizeSelections, defaultSelections;
		Label sizeLabel, defaultLabel, mapLabel, customSizeLabel;
		CheckBox blank, random, seed;
		Button generateButton;
		TextBox customSizeBox;
		
		public override void Display() {
			CreateElement( "size", "Map size", 160, 680, ref sizeSelections, ref sizeLabel );
			CreateElement( "default", "Default terrain", 160, 740, ref defaultSelections, ref defaultLabel );
			generateButton = AddButton( "generateButton", "Generate map", 160, 800, 120, 30 );
			
			mapLabel = AddLabel( "mapLabel", "Map type", 20, 680, 120 );
			blank = CreateCheckbox( "blankCheckbox", "Blank Map", 20, 700 );
			random = CreateCheckbox( "randomCheckbox", "Random Map", 20, 720 );
			seed = CreateCheckbox( "seedCheckbox", "Seed Map", 20, 740 );
			
			customSizeLabel = AddLabel( "customSizeLabel", "Custom map size", 20, 780, 100 );
			customSizeBox = AddTextbox( "customSizeBox", 20, 800, 100, 3 );
			
			SetComboboxItems( sizeSelections, "Tiny (120 x 120)", "Small (144 x 144)", "Medium (168 x 168)", "Normal (200 x 200)",
			                 "Large (220 x 220)", "Giant (240 x 240)", "Custom" );
			SetComboboxItems( defaultSelections, AokTerrain.AokTerrainNames );
			
			sizeSelections.SelectedIndex = 1;
			generateButton.Click += GenerateHandler;
			blank.Checked = true;
			seed.Enabled = false;
			random.Enabled = false;
		}

		void GenerateHandler( object sender, EventArgs e ) {
			System.Diagnostics.Debug.WriteLine( "CLICK" );
			GenerateNewMap();
		}
		
		void GenerateNewMap() {
			int index = sizeSelections.SelectedIndex;
			if( index == -1 ) return; // No type selected.
			
			int terrainIndex = defaultSelections.SelectedIndex;
			byte terrainType = 0;
			if( terrainIndex >= 0 ) {
				terrainType = (byte)terrainIndex;
			}
			
			if( index < defaultMapSizes.Length ) {
				int axisSize = defaultMapSizes[index];	
				form.map.GenerateNewMap( axisSize, axisSize, terrainType, 1 );
				form.RedrawGlControl();
			} else {
				ushort axisSize = 0;
				if( UInt16.TryParse( customSizeBox.Text, out axisSize ) ) {
					if( axisSize > 255 ) return; // TODO: What should this map size limit be?
					form.map.GenerateNewMap( axisSize, axisSize, terrainType, 1 );
					form.RedrawGlControl();
				}
			}
		}
		
		public double gRoughness;
		public double gBigSize;
		Random rnd = new Random( 0xFFF );
		
		public double[,] Generate( int width, int length, double iRoughness ) {
			double c1, c2, c3, c4;
			double[,] points = new double[width + 1, length + 1];
			
			//Assign the four corners of the intial grid random color values
			//These will end up being the colors of the four corners
			c1 = rnd.NextDouble();
			c2 = rnd.NextDouble();
			c3 = rnd.NextDouble();
			c4 = rnd.NextDouble();
			gRoughness = iRoughness;
			gBigSize = width + length;
			DivideGrid( ref points, 0, 0, width, length, c1, c2, c3, c4 );
			return points;
		}
		
		public void DivideGrid( ref double[,] points, double x, double y, double width, double height,
		                       double c1, double c2, double c3, double c4 ) {
			double Edge1, Edge2, Edge3, Edge4, Middle;

			double newWidth = Math.Floor( width / 2 );
			double newHeight = Math.Floor( height / 2 );

			if( width > 1 || height > 1 ) {
				Middle = ( ( c1 + c2 + c3 + c4 ) / 4 ) + Displace( newWidth + newHeight );	//Randomly displace the midpoint!
				Edge1 = ( c1 + c2 ) / 2;	//Calculate the edges by averaging the two corners of each edge.
				Edge2 = ( c2 + c3 ) / 2;
				Edge3 = ( c3 + c4 ) / 2;
				Edge4 = ( c4 + c1 ) / 2;
				//Make sure that the midpoint doesn't accidentally "randomly displaced" past the boundaries!
				Middle= Rectify(Middle);
				Edge1 = Rectify(Edge1);
				Edge2 = Rectify(Edge2);
				Edge3 = Rectify(Edge3);
				Edge4 = Rectify(Edge4);
				//Do the operation over again for each of the four new grids.
				DivideGrid( ref points, x, y, newWidth, newHeight, c1, Edge1, Middle, Edge4 );
				DivideGrid( ref points, x + newWidth, y, width - newWidth, newHeight, Edge1, c2, Edge2, Middle );
				DivideGrid( ref points, x + newWidth, y + newHeight, width - newWidth, height - newHeight, Middle, Edge2, c3, Edge3 );
				DivideGrid( ref points, x, y + newHeight, newWidth, height - newHeight, Edge4, Middle, Edge3, c4 );
			}
			else	//This is the "base case," where each grid piece is less than the size of a pixel.
			{
				//The four corners of the grid piece will be averaged and drawn as a single pixel.
				double c = ( c1 + c2 + c3 + c4 ) / 4;

				points[(int)x, (int)y] = c;
				if( width == 2 ) {
					points[(int)( x + 1 ), (int)y] = c;
				}
				if ( height == 2 ) {
					points[(int)x, (int)( y + 1 )] = c;
				}
				if( width == 2 && height == 2 ) {
					points[(int)( x + 1 ), (int)( y + 1 )] = c;
				}
			}
		}
		
		private double Rectify(double iNum) {
			if( iNum < 0 ) {
				iNum = 0;
			} else if( iNum > 1 ) {
				iNum = 1;
			}
			return iNum;
		}

		private double Displace( double SmallSize ) {
			double Max = SmallSize / gBigSize * gRoughness;
			return ( rnd.NextDouble() - 0.5 ) * Max;
		}

		
		int[] defaultMapSizes = new int[] {
			120, // tiny
			144, // small
			168, // medium
			200, // normal
			220, // large
			240, // giant
		};
		
		static void SetComboboxItems<T>( ComboBox combobox, params T[] items ) where T : class {
			combobox.Items.AddRange( items );
		}
		
		Button AddButton( string name, string text, int x, int y, int width, int height ) {
			Button button = new Button();
			button.Location = new Point( x, y );
			button.Name = name;
			button.Size = new Size( width, height );
			button.Text = text;
			//button.TextAlign = ContentAlignment.MiddleLeft;
			button.UseVisualStyleBackColor = true;
			form.Controls.Add( button );
			return button;
		}
		
		Label AddLabel( string name, string text, int x, int y, int width ) {
			Label label = new Label();
			label.Location = new Point( x, y );
			label.Name = name;
			label.Size = new Size( 100, 20 );
			label.Text = text;
			label.TextAlign = ContentAlignment.MiddleLeft;
			form.Controls.Add( label );
			return label;
		}
		
		TextBox AddTextbox( string name, int x, int y, int width, int textLength ) {
			TextBox textbox = new TextBox();
			textbox.Location = new Point( x, y );
			textbox.Name = name + "Textbox";
			textbox.Size = new Size( width, 20 );
			textbox.MaxLength = textLength;
			form.Controls.Add( textbox );
			return textbox;
		}
		
		CheckBox CreateCheckbox( string name, string text, int x, int y ) {
			CheckBox checkbox = new CheckBox();
			checkbox.Location = new Point( x, y );
			checkbox.Name = name;
			checkbox.Size = new Size( 100, 20 );
			checkbox.Text = text;
			checkbox.UseVisualStyleBackColor = true;
			form.Controls.Add( checkbox );
			return checkbox;
		}
		
		void CreateElement( string name, string text, int x, int y, ref ComboBox combobox, ref Label label ) {
			label = AddLabel( name + "Label", text, x, y, 120 );
			
			combobox = new ComboBox();
			combobox.Location = new Point( x, y + 20 );
			combobox.Name = name + "Combobox";
			combobox.Size = new Size( 100, 21 );
			combobox.DropDownStyle = ComboBoxStyle.DropDownList;
			form.Controls.Add( combobox );
		}
		
		public override void Dispose() {
			DisposeElements( sizeSelections, defaultSelections, sizeLabel, defaultLabel, mapLabel,
			                blank, random, seed, customSizeLabel, customSizeBox, generateButton );
		}
	}
}
