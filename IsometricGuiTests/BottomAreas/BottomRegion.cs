using System;
using System.Windows.Forms;

namespace IsometricGuiTests {
	
	public abstract class BottomRegion {
		
		protected MainForm form;
		public BottomRegion( MainForm form ) {
			this.form = form;
		}
		
		public abstract void Display();
		
		public abstract void Dispose();	
		
		protected void DisposeElements( params Control[] elements ) {
			for( int i = 0; i < elements.Length; i++ ) {
				Control element = elements[i];
				if( element != null ) {
					element.Dispose();
					form.Controls.Remove( element );
				}
			}
		}
		
		protected static void DecheckCheckboxes( params CheckBox[] checkboxes ) {
			for( int i = 0; i < checkboxes.Length; i++ ) {
				CheckBox checkbox = checkboxes[i];
				if( checkbox == null ) continue;
				checkbox.Checked = false;
			}
		}
	}
}
