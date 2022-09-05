using Godot;
using System;

public class Sudoku : VBoxContainer
{
	[Signal]
	delegate void win();
	[Signal]
	delegate void Instruct(int tab);

	public Node grid;
	public int length;
	public string islandGroups;
	public Button focus;

	public override void _Ready(){
		Godot.Collections.Dictionary config = 
			this.GetParent().Get("PuzzleData") as Godot.Collections.Dictionary;
		length = config["Length"].ToString().ToInt();
		focus = null;
		islandGroups = "";
		grid = GetNode("HBox/GBox");
		PuzzleSetup(config["Boxes"] as string, config["Islands"] as string);
		SudokuSetup(config["Keys"] as string);
		GetNode("BufferMiddle/CenterContainer/ResetBtn").Connect("pressed",this,"ResetBoard",new Godot.Collections.Array {config["Boxes"]});
		// GetNode("Title/Button").Connect("pressed",this,"ShowInstructions");
	}

	public void ResetBoard(string boxes){
		for (int i = 0; i < boxes.Length; i++){
			char X = boxes[i];
			Button nxt = grid.GetChild<Button>(i);
			if (X != '_') nxt.Text = X.ToString();
			else nxt.Text = ""; 
		}
	}

	public void PuzzleSetup(string boxes, string islands = ""){
		var ph = GetNode<Button>("ButtonWBorders");

		for (int i = 0; i < boxes.Length; i++){
			char X = boxes[i];
			char Y = '\0';
			if(islands.Length > i) Y = islands[i];
			Button nxt = ph.Duplicate() as Button;
			if (Y != '\0') nxt.AddToGroup(Y.ToString());
			if (!islandGroups.Contains(Y.ToString()))
				islandGroups += Y.ToString();
			char[] neighbors = {
				(i >= length) ? 
				islands[i - length] : '_',//N
				((i % length) != 4) ? 
				islands[i + 1] : '_',//E
				(i < length*(length - 1)) ? 
				islands[i + length] : '_',//S
				((i % length) != 0) ? 
				islands[i - 1] : '_',//W
			};
			int bbit = 0;
			for (int j = 0; j < 4; j++){
				if(neighbors[j] != Y && neighbors[j] != '_')
					bbit |= 1 << j;
			}
				nxt.Set("borderBits",bbit);
			grid.AddChild(nxt);
			if (X != '_'){
				nxt.Text = X.ToString();
				nxt.Disabled = true;
			}
			nxt.Connect("pressed",this,"FocusIsMe",new Godot.Collections.Array {nxt});
			nxt.Visible = true;
		}
	}

	public void FocusIsMe(Button nxt){
		if (nxt.Pressed){
			if (focus != null) focus.Pressed = false;
			focus = nxt;
		}
		else focus = null;
	}

	public void SudokuSetup(string keys){
		Button ph = GetNode<Button>("PHButton");
		Node grid = GetNode("HBox2");

		foreach (char X in keys){
			Button nxt = ph.Duplicate() as Button;
			grid.AddChild(nxt);
			nxt.Text = X.ToString();
			nxt.Connect("pressed",this,"PlaceNumber",new Godot.Collections.Array {nxt.Text});
			nxt.Connect("pressed",this,"CheckDeferred");
			nxt.Visible = true;
		}
		grid.AddChild(
			GetNode("HBox2/Buffer").Duplicate());
	}

	public void PlaceNumber(string number){
		if (focus == null) return;
		focus.Text = number;
	}

	public void CheckDeferred(){CallDeferred("StatusCheck");}
	public void StatusCheck(){
		var Checker = new Godot.Collections.Dictionary();
		//Check all Rows for Duplicates
		foreach (Button item in grid.GetChildren()){
			if((item.GetIndex() % length) == 0) Checker.Clear();
			if(item.Text == "") return;
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
			if(!Checker.Contains(item.Text)){
				Checker[item.Text] = 1;
			}
			else return;
		}
		//Check all Islands for Duplicates
		foreach (char group in islandGroups){
			Checker.Clear();
			foreach (Button item in GetTree().GetNodesInGroup(group.ToString())){
				if(!Checker.Contains(item.Text))
					Checker[item.Text] = 1;
				else return;
		}}
		EmitSignal("win");
		return;
	}

	public void ShowInstructions(){GD.Print("Hello");}
}
