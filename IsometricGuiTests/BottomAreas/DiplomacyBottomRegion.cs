using System;
using System.Drawing;
using System.Windows.Forms;
using AoeUtils;

namespace IsometricGuiTests {
	
	public sealed class DiplomacyBottomRegion : BottomRegion {
		
		public DiplomacyBottomRegion( MainForm form ) : base( form ) {
		}
		
		ComboBox playersBox;
		Label allyLabel, neutralLabel, enemyLabel, alliedVictoryLabel;
		CheckBox alliedVictoryCheckbox;
		CheckBox[] allyCheckboxes;
		CheckBox[] neutralCheckboxes;
		CheckBox[] enemyCheckboxes;
		Label[] playerLabels;
		
		void ResetDiplomacyCheckboxes( int playerIndex ) {
			DisposeElements( allyCheckboxes );
			DisposeElements( enemyCheckboxes );
			DisposeElements( neutralCheckboxes );
			DisposeElements( alliedVictoryCheckbox );
			DisposeElements( playerLabels );	
			
			if( playerIndex >= 0 ) {
				PlayerInfo info = form.LoadedScenario.PlayersInfo[playerIndex];
				int x = 160;
				int count = form.LoadedScenario.PlayersCount;

				for( int i = 0; i < count; i++ ) {
					int index = i;
					playerLabels[i] = AddLabel( "player" + i + "Label", ( i + 1 ).ToString(), x, 700, 20 );
					if( i == playerIndex ) {
						alliedVictoryCheckbox = CreateCheckbox( "alliedVictoryCheckbox", "", x, 795 );
						alliedVictoryCheckbox.Checked = info.AlliedVictory;
					} else {
						// TODO: Rewrite these to avoid the use of index variable?
						allyCheckboxes[i] = CreateCheckbox( "ally" + i + "Checkbox", "", x, 720 );
						allyCheckboxes[i].CheckedChanged += delegate {
							if( allyCheckboxes[index].Checked ) {
								neutralCheckboxes[index].Checked = false;
								enemyCheckboxes[index].Checked = false;
							}
						};
						neutralCheckboxes[i] = CreateCheckbox( "neutral" + i + "Checkbox", "", x, 745 );
						neutralCheckboxes[i].CheckedChanged += delegate {
							if( neutralCheckboxes[index].Checked ) {
								allyCheckboxes[index].Checked = false;
								enemyCheckboxes[index].Checked = false;
							}
						};
						
						enemyCheckboxes[i] = CreateCheckbox( "enemy" + i + "Checkbox", "", x, 770 );
						enemyCheckboxes[i].CheckedChanged += delegate {
							if( enemyCheckboxes[index].Checked ) {
								allyCheckboxes[index].Checked = false;
								neutralCheckboxes[index].Checked = false;
							}
						};
						SetStance( i, info.Diplomacy[i] );
					}
					x += 40;
				}
			}
		}
		
		void SetStance( int index, DiplomacyStance stance ) {
			if( stance == DiplomacyStance.Allied ) {
				allyCheckboxes[index].Checked = true;
			} else if( stance == DiplomacyStance.Neutral ) {
				neutralCheckboxes[index].Checked = true;
			} else {
				enemyCheckboxes[index].Checked = true;
			}
		}
		
		public override void Display() {
			playersBox = CreateCombobox( "playerCombobox", 20, 680 );
			allyLabel = AddLabel( "allyLabel", "Ally", 20, 720, 100 );
			neutralLabel = AddLabel( "neutralLabel", "Neutral", 20, 745, 100 );
			enemyLabel = AddLabel( "enemyLabel", "Enemy", 20, 770, 100 );
			alliedVictoryLabel = AddLabel( "victoryLabel", "Allied victory", 20, 795, 100 );
			
			int count = form.LoadedScenario.PlayersCount;
			//160, 720
			allyCheckboxes = new CheckBox[count];
			neutralCheckboxes = new CheckBox[count];
			enemyCheckboxes = new CheckBox[count];
			playerLabels = new Label[count];
			
			for( int i = 0; i < count; i++ ) {
				playersBox.Items.Add( "Player " + ( i + 1 ) );
			}
			
			playersBox.SelectedIndexChanged += PlayersBoxIndexChanged;
			playersBox.SelectedIndex = 0;
		}

		void PlayersBoxIndexChanged( object sender, EventArgs e ) {
			ResetDiplomacyCheckboxes( playersBox.SelectedIndex );
		}

		
		static void SetComboboxItems<T>( ComboBox combobox, params T[] items ) where T : class {
			combobox.Items.AddRange( items );
		}
		
		Label AddLabel( string name, string text, int x, int y, int width ) {
			Label label = new Label();
			label.Location = new Point( x, y );
			label.Name = name;
			label.Size = new Size( width, 20 );
			label.Text = text;
			label.TextAlign = ContentAlignment.MiddleLeft;
			form.Controls.Add( label );
			return label;
		}
		
		CheckBox CreateCheckbox( string name, string text, int x, int y ) {
			CheckBox checkbox = new CheckBox();
			checkbox.Location = new Point( x, y );
			checkbox.Name = name;
			checkbox.Size = new Size( 20, 20 );
			checkbox.Text = text;
			checkbox.UseVisualStyleBackColor = true;
			form.Controls.Add( checkbox );
			return checkbox;
		}
		
		ComboBox CreateCombobox( string name, int x, int y ) {
			ComboBox combobox = new ComboBox();
			combobox.Location = new Point( x, y );
			combobox.Name = name;
			combobox.Size = new Size( 100, 21 );
			combobox.DropDownStyle = ComboBoxStyle.DropDownList;
			form.Controls.Add( combobox );
			return combobox;
		}
		
		public override void Dispose() {
			DisposeElements( playersBox, alliedVictoryLabel, allyLabel, neutralLabel, enemyLabel );
			ResetDiplomacyCheckboxes( -1 );
		}
	}
}
