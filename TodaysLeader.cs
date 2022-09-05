using Godot;
using System;

public class TodaysLeader : Control
{
	private HTTPRequest httpRequest;
	public override void _Ready(){
		_SetBackground(gameManager.Bg);
		GetNode<Button>("Panel/HBoxContainer/Button").Connect("pressed",this,"restartApp");
		httpRequest = GetNode<HTTPRequest>("HTTPRequest");

		httpRequest.Connect("request_completed",this,"_HttpGetRequestCompleted");
		try
		{
			httpRequest.Request("https://6tha92ai7i.execute-api.us-east-1.amazonaws.com/Test/leader");
		}
		catch (System.Exception)
		{
			_CriticalError("Request Incomplete");
			throw;
		}
	}

	public void restartApp(){
		GetTree().ChangeScene("res://TodaysPuzzle.tscn");
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
		var list = GetNode("SCont/VBox");
		var ph = GetNode("SCont/VBox/EPH");
		foreach (Godot.Collections.Dictionary X in response["items"] as Godot.Collections.Array){
			Control nxt = ph.Duplicate() as Control;
			nxt.GetChild<Label>(1).Text = X["Place"].ToString() + ": ";
			nxt.GetChild<Label>(2).Text = X["Name"].ToString();
			nxt.Visible = true;
			list.AddChildBelowNode(ph,nxt);
		}
		httpRequest.Disconnect("request_completed",this,"_HttpGetRequestCompleted");
	}

	public void _SetBackground(int bgtheme = 0){
		Panel mBackground = GetNode<Panel>("Background");
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

	public void _CriticalError(string err){
		GD.Print(err);
	}
}
