using Godot;
using System;

public class ButtonWBorders : Button
{
	public byte BBit;

	public override void _Ready(){
	}

	[Export]
	public byte borderBits{
		get {return BBit;}
		set {
			BBit = value;
			CallDeferred("UpdateBorders");
		}
	}

	public void UpdateBorders(){
		setclipper();
		Node bds = GetChild(0);
		if ((BBit & 1) == 1) 
			bds.GetChild<Control>(0).Visible = true;
		else
			bds.GetChild<Control>(0).Visible = false;
		if ((BBit & 2) == 2) 
			bds.GetChild<Control>(1).Visible = true;
		else
			bds.GetChild<Control>(1).Visible = false;
		if ((BBit & 4) == 4) 
			bds.GetChild<Control>(2).Visible = true;
		else
			bds.GetChild<Control>(2).Visible = false;
		if ((BBit & 8) == 8) 
			bds.GetChild<Control>(3).Visible = true;
		else
			bds.GetChild<Control>(3).Visible = false;
	}

	public void setclipper(){
		Control bds = GetNode<Control>("Control");
		Vector2 adden = new Vector2(10,10);
		bds.RectSize = this.RectSize + adden;
		bds.SetPosition(new Vector2(-5,-5));
	}
}
