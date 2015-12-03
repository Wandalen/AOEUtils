using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AoeUtils;

namespace IsometricGuiTests {
	
	public sealed class CinematicBottomRegion : BottomRegion {
		
		public CinematicBottomRegion( MainForm form ) : base( form ) {
		}
		
		ComboBox sizeSelections, defaultSelections, lossSelections, instructionsSelections;
		
		Label sizeLabel, defaultLabel, lossLabel, instructionsLabel;
		
		public override void Display() {
			CreateElement( "pregame", "Pregame cinematic", 20, 660, ref sizeSelections, ref sizeLabel );
			CreateElement( "victory", "Victory cinematic", 20, 720, ref defaultSelections, ref defaultLabel );
			CreateElement( "loss", "Loss cinematic", 200, 660, ref lossSelections, ref lossLabel );
			CreateElement( "instructions", "Scenario instructions map", 200, 720, ref instructionsSelections, ref instructionsLabel );
			
			string path = form.AgeOfEmpires2Path;
			if( path == null ) {
				sizeSelections.Enabled = false;
				defaultSelections.Enabled = false;
				lossSelections.Enabled = false;
				return;
			}
			
			AddCinematicsSelectionsElement( " <None> " );
			string cinematicsDirectory = Path.Combine( path, "avi" );
			
			// TODO: When changing this for .NET 2.0, change this to "Directory.GetFiles"
			foreach( string file in Directory.EnumerateFiles( cinematicsDirectory, "*.avi" ) ) {
				string value = Path.GetFileNameWithoutExtension( file );
				AddCinematicsSelectionsElement( value );
			}
			
			SelectDefaultCinematic( sizeSelections, form.LoadedScenario.PregameCinematicFile );
			SelectDefaultCinematic( defaultSelections, form.LoadedScenario.VictoryCinematicFile );
			SelectDefaultCinematic( lossSelections, form.LoadedScenario.LossCinematicFile );
		}
		
		void AddCinematicsSelectionsElement( string value ) {
			sizeSelections.Items.Add( value );
			defaultSelections.Items.Add( value );
			lossSelections.Items.Add( value );
		}
		
		void SelectDefaultCinematic( ComboBox box, string defaultValue ) {
			if( defaultValue == String.Empty ) return;
			
			for( int i = 0; i < box.Items.Count; i++ ) {
				string value = (string)box.Items[i];
				if( value.Equals( defaultValue, StringComparison.InvariantCultureIgnoreCase ) ) {
					box.SelectedIndex = i;
					break;
				}
			}
		}
		
		void CreateElement( string name, string text, int x, int y, ref ComboBox combobox, ref Label label ) {
			label = new Label();
			label.Location = new Point( x, y );
			label.Name = name + "Label";
			label.Size = new Size( 140, 20 );
			label.Text = text;
			label.TextAlign = ContentAlignment.MiddleLeft;
			form.Controls.Add( label );
			
			combobox = new ComboBox();
			combobox.Location = new Point( x, y + 20 );
			combobox.Name = name + "Combobox";
			combobox.Size = new Size( 140, 21 );
			combobox.DropDownStyle = ComboBoxStyle.DropDownList;
			form.Controls.Add( combobox );
		}
		
		public override void Dispose() {
			DisposeElements( sizeSelections, defaultSelections, lossSelections, instructionsSelections,
			                sizeLabel, defaultLabel, lossLabel, instructionsLabel );
		}
	}
}
