using Godot;
using System;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<Button>("PlayButton").Pressed += OnPlayButtonPressed;
		GetNode<Button>("QuitButton").Pressed += OnQuitButtonPressed;
	}
	
	private void OnPlayButtonPressed()
	{
		GD.Print("starting game");
		GetTree().ChangeSceneToFile("res://Scenes/battle_1.tscn");
	}
	
	private void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}
}
