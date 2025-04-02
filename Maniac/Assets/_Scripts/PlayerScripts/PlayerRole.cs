using System;
using UnityEditor;
using UnityEngine;

namespace _Scripts.PlayerScripts
{
    public enum PlayerRoleEnum
    {
        Murder,
        Victim
    }
    
    public class PlayerRole : MonoBehaviour
    {
        // set in PlayerSetup.cs using SetRole(...)
        private PlayerRoleEnum _role = PlayerRoleEnum.Victim; 

        public void SetRole(PlayerRoleEnum newRole) => this._role = newRole;
        public PlayerRoleEnum GetRole() => _role;
    }
}