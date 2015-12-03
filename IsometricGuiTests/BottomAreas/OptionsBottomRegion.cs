using System;
using System.Drawing;
using System.Windows.Forms;
using AoeUtils;

namespace IsometricGuiTests {
	
	public sealed class OptionsBottomRegion : BottomRegion {
		
		public OptionsBottomRegion( MainForm form ) : base( form ) {
		}
		
		ListBox box;
		
		public override void Display() {
			box = CreateListbox( "box", 20, 680 );
			uint[][] disabledBuildings = form.LoadedScenario.DisabledBuildings;
			uint[] ids = disabledBuildings[0];
			for( int i = 0; i < ids.Length; i++ ) {
				box.Items.Add( ids[i].ToString() );
			}
		}
		
		ListBox CreateListbox( string name, int x, int y ) {
			ListBox listbox = new ListBox();
			listbox.Location = new Point( x, y );
			listbox.Name = name;
			listbox.Size = new Size( 120, 95 );
			form.Controls.Add( listbox );
			return listbox;
		}
		
		public override void Dispose() {
			DisposeElements( box );
		}
	}
}
