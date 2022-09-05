using Godot;
using System;

public class TodaysPuzzle : Control
{
	[Export(PropertyHint.Enum,"Wood,Crystal,FSim,Ocean,Galaxy")]
	private Panel mBackground;
	private Panel mLoading;
	private Panel mError;
	private HTTPRequest httpRequest;

	private Node adMob;
	private AnimationPlayer mAnim;

	private Control Game;
	private Godot.Collections.Dictionary PuzzleData;
	public override void _Ready(){
		GD.Randomize();//get ridd of when done
		mBackground = GetNode<Panel>("Background");
		mLoading = GetNode<Panel>("LoadingGraphic");
		mError = GetNode<Panel>("ErrorGraphic");
		httpRequest = GetNode<HTTPRequest>("HTTPRequest");
		mAnim = GetNode<AnimationPlayer>("AnimationPlayer");
		
		GetNode("AdMob").Connect("interstitial_loaded",this,"AdLoaded");
		httpRequest.Connect("request_completed",this,"_HttpGetRequestCompleted");
		try
		{
			httpRequest.Request("https://6tha92ai7i.execute-api.us-east-1.amazonaws.com/Test");
		}
		catch (System.Exception)
		{
			_CriticalError("Request Incomplete");
			throw;
		}
	}

	public void menuSelection(){
		GD.Print("hello");
	}
	
	public void _HttpGetRequestCompleted
	(int result,int responseType,String[] headers,Byte[] body){
		if (result != 0 || responseType < 200 || responseType > 299){
			_CriticalError(
				"Https Response\n" +
				"Result: " + result + 
				"\nResponseType: " + responseType);
			return;
		}
		Godot.Collections.Dictionary response = 
			JSON.Parse(body.GetStringFromUTF8()).Result
			as Godot.Collections.Dictionary;
		// find out what the response is for
		var test = response.ToString();

		_SetBackground(Convert.ToInt32(response["Theme"]));
		Button TPbtn = GetNode<Button>("TitleMenu/CBox/Menu/Button");
		TPbtn.Connect("pressed", this, "menuSelection", new Godot.Collections.Array {response["Today"]});
		Godot.Collections.Array puzzles = response["puzzles"] as Godot.Collections.Array;
		foreach (Godot.Collections.Dictionary puzzle in puzzles){
			Button nxt = TPbtn.Duplicate() as Button;
			nxt.Connect("pressed", this, "menuSelection", new Godot.Collections.Array {puzzle});
			GetNode<Control>("TitleMenu/CBox/Menu").AddChild(nxt);
			switch (puzzle["Puzzle"].ToString()){
				case "LigOut": nxt.Text = "Lights Out";
					break;
				default: nxt.Text = puzzle["Puzzle"].ToString();
					break;
			}
			nxt.Visible = true;
		}
		GetNode<Control>("TitleMenu").Visible = true;
	}

	public void menuSelection(Godot.Collections.Dictionary puzzle){
		PuzzleData = puzzle["Configuration"] as Godot.Collections.Dictionary;
		_SetPuzzle(puzzle["Puzzle"].ToString());
	}
	public void _SetBackground(int bgtheme = 0){
		gameManager.Bg = bgtheme;
		ShaderMaterial bMaterial = new ShaderMaterial{};
		Vector2 resolution = GetGlobalRect().Size;
		float aspect = GetGlobalRect().Size.Aspect();
		float scale = 1.0f;
		switch (bgtheme)
		{
			case 0:
				this.Theme = GD.Load<Theme>("res://Themes/Wood.theme");
				bMaterial.Shader = GD.Load<Shader>("res://Themes/Wood.gdshader");
				mBackground.AddChild(new ColorRect());
				mBackground.GetChild<ColorRect>(0).ShowBehindParent = true;
				mBackground.GetChild<ColorRect>(0).SelfModulate = new Color(0.4f,0.2f,0.04f);
				mBackground.GetChild<ColorRect>(0).RectSize = this.RectSize;
				mBackground.SelfModulate = new Color(0.5f,0.25f,0.04f);
				scale = 2.0f;
				break;
			case 1:
				this.Theme = GD.Load<Theme>("res://Themes/Crystal.theme");
				bMaterial.Shader = GD.Load<Shader>("res://Themes/Crystal.gdshader");
				scale = 10.0f;
				break;
			case 2:
				this.Theme = GD.Load<Theme>("res://Themes/FSim.theme");
				bMaterial.Shader = GD.Load<Shader>("res://Themes/FSim.gdshader");
				break;
			case 3:
				this.Theme = GD.Load<Theme>("res://Themes/Sea.theme");
				bMaterial.Shader = GD.Load<Shader>("res://Themes/Sea.gdshader");
				scale = 2.0f;
				break;
			case 4:
				this.Theme = GD.Load<Theme>("res://Themes/Galaxy.theme");
				bMaterial.Shader = GD.Load<Shader>("res://Themes/Galaxy.gdshader");
				resolution *= 3.5f;
				break;
			default:break; //something went wrong
		}
		bMaterial.SetShaderParam("RESOLUTION",resolution);
		bMaterial.SetShaderParam("ASPECT",aspect);
		bMaterial.SetShaderParam("SCALE", scale);
		mBackground.Material = bMaterial;
	}

	public void _SetPuzzle(string puzzle){
		GetNode<Control>("TitleMenu").Visible = false;
		switch (puzzle)
		{
			case "Hitori":
				Game = GD.Load<PackedScene>("res://Puzzles/Hitori.tscn").Instance() as Control;
				break;
			case "Sudoku":
				Game = GD.Load<PackedScene>("res://Puzzles/Sudoku.tscn").Instance() as Control;
				break;
			case "LigOut":
				Game = GD.Load<PackedScene>("res://Puzzles/LigOut.tscn").Instance() as Control;
				break;
			default:
				_CriticalError("Couldn't find Puzzle Try Updating App");
				break;
		}
		this.AddChildBelowNode(mBackground,Game);
		Game.Connect("win",this,"_InputData");
		Game.Connect("Instruct",this,"_ShowInstructions");
		GetNode("AdMob").CallDeferred("load_interstitial");
		_ToggleLoading(false);
	}

	public void _ShowInstructions(int tab){
		GetNode<PopupPanel>("PopupPanel2").Show();
		GetNode<TabContainer>("PopupPanel2/TabContainer").CurrentTab = tab;
	}

	public void _InputData(){
		Game.QueueFree();
		GetNode<Control>("NameEdit").Visible = true;
		GetNode<LineEdit>("NameEdit/LineEdit").GrabFocus();
		GetNode<LineEdit>("NameEdit/LineEdit").Connect("text_entered",this,"_uploadData");
	}

	public void _uploadData(string name){
		GetNode("NameEdit").QueueFree();
		var body = new Godot.Collections.Dictionary {};
		body.Add("Name",name);
		body.Add("Font",3);
		body.Add("RequestID","123445");
		body.Add("vData",PuzzleData);

		
		httpRequest.Disconnect("request_completed",this,"_HttpGetRequestCompleted");
		httpRequest.Connect("request_completed",this,"_HttpPostRequestCompleted");
		try {
			httpRequest.Request(
				"https://6tha92ai7i.execute-api.us-east-1.amazonaws.com/Test",
				null,true,HTTPClient.Method.Post,JSON.Print(body));
		}
		catch (System.Exception)
		{
			_CriticalError("Request Incomplete");
			throw;
		}
		GetNode("AdMob").CallDeferred("show_interstitial");
	}

	public void _HttpPostRequestCompleted
	(int result,int responseType,String[] headers,Byte[] body){
		if (result != 0 || responseType < 200 || responseType > 299){
			_CriticalError(
				"Https Response\n" +
				"Result: " + result + 
				"\nResponseType: " + responseType);
			return;
		}
		Godot.Collections.Dictionary response = 
			JSON.Parse(body.GetStringFromUTF8()).Result
			as Godot.Collections.Dictionary;
		// find out what the response is for
		var test = response.ToString();

		// gameManager.Leaders = response["body"] as Godot.Collections.Array;
		GetTree().ChangeScene("res://TodaysLeader.tscn");
	}

	public void AdLoaded(){
		GD.Print("AdLoaded!!!");
	}

	public void _ToggleLoading(bool show){
		if(!show){
			mAnim.Play("load_toggle",-1,5);
		} else {
			mLoading.Visible = true;
			mAnim.PlayBackwards("load_toggle");
		}
	}

	public void _CriticalError(string message){
		mError.Visible = true;
		GetNode<Label>("ErrorGraphic/Label2").Text = message;
		GD.Print(message);
	}
}

public static class gameManager{
	public static int Bg {
		get; set;
	} = 1;
	public static Godot.Collections.Array Leaders = null;
}