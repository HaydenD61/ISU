using Godot;
using System;
using System.Threading.Tasks;

public partial class Battle1 : Node2D
{
	[Export] public AnimationPlayer AnimPlayer;
	[Export] public Label InfoLabel;
	[Export] public ProgressBar PlayerHPBar;
	[Export] public ProgressBar EnemyHPBar;

	private int playerHP = 20;
	private int playerMaxHP = 20;
	private int enemyHP = 10;
	private int enemyMaxHP = 10;
	private int currentBattle = 1;
	private bool isPlayerTurn = true;
	private bool hasUsedHeal = false;

	public override void _Ready()
	{
		if (AnimPlayer == null)
			GD.Print("AnimationPlayer not assigned!");
		
		StartBattle(currentBattle);
	}

	private void StartBattle(int battleNumber)
	{
		playerHP = playerMaxHP;
		enemyMaxHP = 10 + (battleNumber - 1) * 5;
		enemyHP = enemyMaxHP;
		hasUsedHeal = false;
		isPlayerTurn = true;
		InfoLabel.Text = $"Battle {battleNumber} begins!";
		UpdateHUD();
	}

	private void UpdateHUD()
	{
		PlayerHPBar.Value = playerHP;
		EnemyHPBar.Value = enemyHP;
	}

	private async Task PlayAnimAsync(string name)
	{
		if (AnimPlayer != null && AnimPlayer.HasAnimation(name))
		{
			AnimPlayer.Play(name);
			await ToSignal(AnimPlayer, AnimationPlayer.SignalName.AnimationFinished);
			GD.Print($"Finished animation: {name}");
		}
		else
		{
			GD.Print($"Animation '{name}' not found or AnimationPlayer not assigned.");
		}
	}

	public async void OnBasicAttackPressed()
	{
		if (!isPlayerTurn) return;

		await PlayAnimAsync("player_attack");

		int damage = 5 + currentBattle;
		enemyHP -= damage;
		InfoLabel.Text = $"You attack! Enemy takes {damage} damage.";
		UpdateHUD();

		if (enemyHP <= 0)
		{
			await PlayAnimAsync("enemy_death");
			InfoLabel.Text = "Enemy defeated!";
			await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
			LoadNextBattle();
			return;
		}

		EndPlayerTurn();
	}

	public async void OnSpecialAttackPressed()
	{
		if (!isPlayerTurn) return;

		await PlayAnimAsync("player_attack");

		int damage = 8 + GD.RandRange(0, currentBattle);
		int chance = (int)(GD.Randi() % 100);

		if (chance < 50)
		{
			InfoLabel.Text = "Special attack missed!";
		}
		else
		{
			enemyHP -= damage;
			InfoLabel.Text = $"Special attack hit! Enemy takes {damage} damage.";
		}

		UpdateHUD();

		if (enemyHP <= 0)
		{
			await PlayAnimAsync("enemy_death");
			InfoLabel.Text = "Enemy defeated!";
			await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
			LoadNextBattle();
			return;
		}

		EndPlayerTurn();
	}

	public async void OnHealSpellPressed()
	{
		if (!isPlayerTurn) return;

		if (hasUsedHeal)
		{
			InfoLabel.Text = "Heal already used!";
			return;
		}

		hasUsedHeal = true;
		int healAmount = 6;
		playerHP += healAmount;
		if (playerHP > playerMaxHP)
			playerHP = playerMaxHP;

		InfoLabel.Text = $"You cast Heal and recover {healAmount} HP!";
		UpdateHUD();

		EndPlayerTurn();
	}

	private async void EndPlayerTurn()
	{
		isPlayerTurn = false;
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		ExecuteEnemyTurn();
	}

	private async void ExecuteEnemyTurn()
	{
		await PlayAnimAsync("enemy_attack");

		int damage = 3 + currentBattle;
		playerHP -= damage;
		InfoLabel.Text = $"Enemy attacks! You take {damage} damage.";
		UpdateHUD();

		if (playerHP <= 0)
		{
			await PlayAnimAsync("player_death");
			InfoLabel.Text = "You were defeated...";
			await ToSignal(GetTree().CreateTimer(1.5f), "timeout");
			GetTree().ChangeSceneToFile("res://Scenes/GameOver.tscn");
			return;
		}


		isPlayerTurn = true;
	}

	private async void LoadNextBattle()
{
	currentBattle ++;
	await PlayAnimAsync("enemy_enter");

	if (currentBattle > 5) // Change this to match your total number of battles
	{
		GetTree().ChangeSceneToFile("res://Scenes/VictoryScreen.tscn");
		return;
	}

	StartBattle(currentBattle);
}

}
