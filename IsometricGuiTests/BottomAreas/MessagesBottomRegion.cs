using System;
using System.Drawing;
using System.Windows.Forms;
using AoeUtils;

namespace IsometricGuiTests {
	
	public sealed class MessagesBottomRegion : BottomRegion {
		
		public MessagesBottomRegion( MainForm form ) : base( form ) {
		}
		
		CheckBox instructions, hints, victory, loss, history, scouts;
		TextBox messagesTextbox;
		
		public override void Display() {
			instructions = CreateCheckbox( "instuctionsCheckbox", "Instructions", 660 );
			hints = CreateCheckbox( "hintsCheckbox", "Hints", 680 );
			victory = CreateCheckbox( "victoryCheckbox", "Victory", 700 );
			loss = CreateCheckbox( "lossCheckbox", "Loss", 720 );
			history = CreateCheckbox( "historyCheckbox", "History", 740 );
			if( form.LoadedScenario.IsTheConquerorsScenario ) {
				scouts = CreateCheckbox( "scoutsCheckbox", "Scouts", 760 );
			} else {
				scouts = null;
			}
			
			messagesTextbox = new TextBox();
			messagesTextbox.Name = "messagesTextbox";
			messagesTextbox.Multiline = true;
			messagesTextbox.ScrollBars = ScrollBars.Vertical;
			messagesTextbox.Location = new Point( 240, 660 );
			messagesTextbox.Size = new Size( 550, 120 );
			form.Controls.Add( messagesTextbox );
		}
		
		CheckBox CreateCheckbox( string name, string text, int y ) {
			CheckBox checkbox = new CheckBox();
			checkbox.Location = new Point( 20, y );
			checkbox.Name = name;
			checkbox.Size = new Size( 100, 20 );
			checkbox.Text = text;
			checkbox.UseVisualStyleBackColor = true;
			form.Controls.Add( checkbox );
			checkbox.Click += CheckedChangedHandler;
			return checkbox;
		}

		void CheckedChangedHandler( object sender, EventArgs e ) {
			if( sender == instructions ) {
				DecheckCheckboxes( hints, victory, loss, history, scouts );
				messagesTextbox.Text = form.LoadedScenario.Instructions.Value;
				instructions.Checked = true;
			} else if( sender == hints ) {
				DecheckCheckboxes( instructions, victory, loss, history, scouts );
				messagesTextbox.Text = form.LoadedScenario.Hints.Value;
				hints.Checked = true;
			} else if( sender == victory ) {
				DecheckCheckboxes( instructions, hints, loss, history, scouts );
				messagesTextbox.Text = form.LoadedScenario.Victory.Value;
				victory.Checked = true;
			} else if( sender == loss ) {
				DecheckCheckboxes( instructions, hints, victory, history, scouts );
				messagesTextbox.Text = form.LoadedScenario.Loss.Value;
				loss.Checked = true;
			} else if( sender == history ) {
				DecheckCheckboxes( instructions, hints, victory, loss, scouts );
				messagesTextbox.Text = form.LoadedScenario.History.Value;
				history.Checked = true;
			} else if( scouts != null && sender == scouts ) {
				DecheckCheckboxes( instructions, hints, victory, loss, history );
				messagesTextbox.Text = ( (AokTcScenario) form.LoadedScenario ).Scouts.Value;
				scouts.Checked = true;
			}
		}
		
		public override void Dispose() {
			DisposeElements( instructions, hints, victory, loss,
			                history, scouts, messagesTextbox );
		}
	}
}
