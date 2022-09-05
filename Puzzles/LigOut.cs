using Godot;
using System;

public class LigOut : VBoxContainer
{
	[Signal]
	delegate void win();
	[Signal]
	delegate void Instruct(int tab);

	public Node grid;
	public int length;

	public override void _Ready(){
		Godot.Collections.Dictionary config = 
			this.GetParent().Get("PuzzleData") as Godot.Collections.Dictionary;
		length = config["Length"].ToString().ToInt();
		grid = GetNode("HBox/GBox");
		PuzzleSetup(config["Boxes"] as string);
		GetNode("BufferBottom/ResetBtn").Connect("pressed",this,"ResetBoard",new Godot.Collections.Array {config["Boxes"]});
		GetNode("Title/Button").Connect("pressed",this,"ShowInstructions");
	}

	public void ResetBoard(string boxes){
		for (int i = 0; i < boxes.Length; i++){
			char X = boxes[i];
			Button nxt = grid.GetChild<Button>(i);
			if (X == 'T') nxt.Pressed = false;
			else nxt.Pressed = true;
		}
	}

	public void PuzzleSetup(string boxes){
		var ph = GetNode<Button>("PlaceHolder");

		foreach (char X in boxes){
			Button nxt = ph.Duplicate() as Button;
			grid.AddChild(nxt);
			if (X == 'T') nxt.Pressed = false;
			else nxt.Pressed = true;
			nxt.Connect("pressed",this,"ToggleLight",new Godot.Collections.Array {nxt});
			nxt.Connect("pressed",this,"CheckDeferred");
			nxt.Visible = true;
		}
	}

	public void ToggleLight(Button id){
		int index = id.GetIndex();
		Button[] neighbors = {
			(index >= length) ? 
			grid.GetChild<Button>(index - length) : null,//N
			((index % length) != 4) ? 
			grid.GetChild<Button>(index + 1) : null,//E
			(index < length*(length - 1)) ? 
			grid.GetChild<Button>(index + length) : null,//S
			((index % length) != 0) ? 
			grid.GetChild<Button>(index - 1) : null,//W
		};

		foreach (var neighbor in neighbors){
			if (neighbor != null)
				neighbor.Pressed = !neighbor.Pressed;
		}
	}

	public void CheckDeferred(){CallDeferred("StatusCheck");}
	public void StatusCheck(){
		foreach (Button item in grid.GetChildren()){
			if (item.Pressed) return;
		}
		EmitSignal("win");
		return;
	}

	public void ShowInstructions(){ EmitSignal("Instruct",2);}
}
