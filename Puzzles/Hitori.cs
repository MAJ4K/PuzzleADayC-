using Godot;
using System;

public class Hitori : VBoxContainer
{
	[Signal]
	delegate void win();
	[Signal]
	delegate void Instruct(int tab);
	public Node grid;
	public int length;
	public Godot.Collections.Dictionary Checker;
	public override void _Ready(){
		Godot.Collections.Dictionary config = 
			this.GetParent().Get("PuzzleData") as Godot.Collections.Dictionary;
		length = config["Length"].ToString().ToInt();
		grid = GetNode("HBox/GBox");
		Checker = new Godot.Collections.Dictionary();
		PuzzleSetup(config["Boxes"] as string);
		GetNode("BufferBottom/ResetBtn").Connect("pressed",this,"ResetBoard");
		GetNode("Title/Button").Connect("pressed",this,"ShowInstructions");
	}

	public void ResetBoard(){
		foreach (Button item in grid.GetChildren()){
			item.Pressed = false;
		}
	}

	public void PuzzleSetup(string boxes){
		var ph = GetNode<Button>("PlaceHolder");

		foreach (char X in boxes){
			Button nxt = ph.Duplicate() as Button;
			nxt.Connect("pressed",this,"CheckDeferred");
			grid.AddChild(nxt);
			nxt.Text = X.ToString();
			nxt.Visible = true;
		}
	}

	public void CheckDeferred(){CallDeferred("StatusCheck");}
	public void StatusCheck(){
		//Check all Rows for Duplicates
		foreach (Button item in grid.GetChildren()){
			if((item.GetIndex() % length) == 0) Checker.Clear();
			if(item.Pressed) continue;
			if(!Checker.Contains(item.Text)){
				Checker[item.Text] = 1;
			}
			else return;
		}
		//Check all Columns for Duplicates
		for (int i = 0; i < length; i++)
		for (int j = 0; j < length; j++){
			int index = i + j*length;
			Button item = grid.GetChild<Button>(index);
			if(index < length) Checker.Clear();
			if(item.Pressed) continue;
			if(!Checker.Contains(item.Text)){
				Checker[item.Text] = 1;
			}
			else return;
		}
		//check pressed buttons for no neighbors
		foreach (Button item in grid.GetChildren()){
			if (!item.Pressed) continue;
			//collect all 4 neighbors
			int index = item.GetIndex();
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
			// if neigbor is pressed return false
			foreach (Button neighbor in neighbors){
				if(neighbor == null) continue;
				if(neighbor.Pressed) return;
			}
		}
		// flood fill to get all unpressed buttons
		Checker.Clear();
		int childCount = 0;
		foreach (Button item in grid.GetChildren()){
			if (!item.Pressed) childCount++;
		}
		if (grid.GetChild<Button>(0).Pressed)
			childCount -= FloodFill(grid.GetChild<Button>(1));
		else
			childCount -= FloodFill(grid.GetChild<Button>(0));
		if (childCount != 0)return;
		EmitSignal("win");
		return;
	}

	public int FloodFill(Button id){
		if (Checker.Contains(id.Name)) return 0;
		Checker[id.Name] = 1;
		int idx = id.GetIndex();
		Button[] neighbors = {
			(idx >= length) ?
			grid.GetChild<Button>(idx - length) : null,//N
			((idx % length) != 4) ?
			grid.GetChild<Button>(idx + 1) : null,//E
			(idx < length*(length - 1)) ?
			grid.GetChild<Button>(idx + length) : null,//S
			((idx % length) != 0) ?
			grid.GetChild<Button>(idx - 1) : null,//W
		};
		return 1 +
			((neighbors[0] != null && !neighbors[0].Pressed) ? //Top is avaliable
				FloodFill(neighbors[0]) : 0) +
			((neighbors[2] != null && !neighbors[2].Pressed) ? //Bottom is avaliable
				FloodFill(neighbors[2]) : 0) +
			((neighbors[3] != null && !neighbors[3].Pressed) ? //Left is avaliable
				FloodFill(neighbors[3]) : 0) +
			((neighbors[1] != null && !neighbors[1].Pressed) ? //Right is avaliable 
				FloodFill(neighbors[1]) : 0)
		;
	}

	public void ShowInstructions(){ EmitSignal("Instruct",0);}
}
