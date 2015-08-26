using System;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
	public Spawn SpawnLeftTop;
	public Spawn SpawnLeftBottom;
	public Spawn SpawnRightTop;
	public Spawn SpawnRightBottom;
	
	public void PlayerChangedTeam(Player player, Team team)
	{
		DissociatePlayerFromSpawn(player);
		
		Spawn targetSpawn;
		if (team == Team.Red)
		{
			targetSpawn = SpawnLeftTop.Player == null ? SpawnLeftTop : SpawnLeftBottom;
		}
		else
		{
			targetSpawn = SpawnRightTop.Player == null ? SpawnRightTop : SpawnRightBottom;
		}
		targetSpawn.Player = player;
		
		MovePlayerToSpawn(player, targetSpawn);
	}

	public void MovePlayerToHisSpawn(Player player)
	{
		var spawn = GetPlayerSpawn(player);
		MovePlayerToSpawn(player, spawn);
	}
	
	public void MovePlayerToSpawn(Player player, Spawn spawn)
	{
		//Debug.Log(string.Format("Moving player '{0}' of team '{1}' to spawn '{2}'", player.name, player.team, spawn.name));
		player.transform.position = new Vector3(spawn.transform.position.x, spawn.transform.position.y, player.transform.position.z); 
	}
	
	private void DissociatePlayerFromSpawn(Player player)
	{
		// Find the Spawn that contains the given player and remove him from the spawn
		var spawn = GetPlayerSpawn (player);

		if (spawn != null)
		{
			spawn.Player = null;
		}
	}

	private Spawn GetPlayerSpawn(Player player)
	{
		Spawn[] spawns = (Spawn[]) GameObject.FindObjectsOfType(typeof(Spawn));
		foreach (var spawn in spawns)
		{
			if (spawn.Player == player)
			{
				return spawn;
			}
		}

		return null;
	}
}
