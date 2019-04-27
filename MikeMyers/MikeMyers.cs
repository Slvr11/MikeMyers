using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using InfinityScript;
using static InfinityScript.GSCFunctions;

namespace MikeMyers
{
    public class MikeMyers : BaseScript
    {
      private HudElem info;
      public MikeMyers()
        {
            PlayerConnected += Mike_PlayerConnected;
            SetDvar("g_TeamName_Allies", "Michael");
            SetDvar("g_TeamName_Axis", "Runners");
            deleteBombsites();
        }

        private void Mike_PlayerConnected(Entity player)
        {
            showAlive();
            player.SpawnedPlayer += () => OnPlayerSpawned(player);
            player.OnNotify("joined_team", entity =>
            {
                entity.CloseInGameMenu();
                entity.Notify("menuresponse", "changeclass", "class1");
                if (player.SessionTeam == "axis")
                    otherSet(entity);
                else if (player.SessionTeam == "allies")
                    checkMike(entity);
            });
        }
        private static void deleteBombsites()
        {
            Entity bomb = getBombs("bombzone");
            Entity bomb1 = getBombTarget(getBombTarget(bomb));//Trigger
            if (bomb1 != null) bomb1.Delete();
            bomb1 = getBombTarget(getBombs("bombzone"));//model
            if (bomb1 != null) bomb1.Delete();
            bomb1 = getBombs("bombzone");//plant trigger
            if (bomb1 != null) bomb1.Delete();

            Entity bomb2 = getBombTarget(getBombTarget(getBombs("bombzone")));//Trigger
            if (bomb2 != null) bomb2.Delete();
            bomb2 = getBombTarget(getBombs("bombzone"));//model
            if (bomb2 != null) bomb2.Delete();
            bomb2 = getBombs("bombzone");//plant trigger
            if (bomb2 != null) bomb2.Delete();

            deleteBombCol();//Collision

            getBombs("sd_bomb_pickup_trig").Delete();//Bomb pickup trigger
            getBombs("sd_bomb").Delete();//bomb pickup model
        }
        private static Entity getBombs(string name)
            => GetEnt(name, "targetname");
        private static Entity getBombTarget(Entity bomb)
            => GetEnt(bomb.Target, "targetname");
        private static void deleteBombCol()
        {
            Entity col = null;
            for (int i = 18; i < 100; i++)
            {
                Entity ent = Entity.GetEntity(i);
                if (ent == null) continue;

                if (ent.Classname == "script_brushmodel")
                {
                    if (ent.SpawnFlags == 1)
                    {
                        col = ent;
                        break;
                    }
                }
            }
            if (col != null) col.Delete();
        }

        public void OnPlayerSpawned(Entity player)
        {
            if (player.SessionTeam == "axis")
                otherSet(player);
            else if (player.SessionTeam == "allies")
                checkMike(player);
        }
        public void otherSet(Entity ent)
        {
            //other
                ent.SessionTeam = "axis";
                ent.TakeAllWeapons();
            ent.GiveWeapon("portable_radar_mp");
            ent.SetWeaponAmmoClip("portable_radar_mp", 0);
            ent.SetWeaponAmmoStock("portable_radar_mp", 0);
            OnInterval(500, () =>
            {
                if (ent.CurrentWeapon != "portable_radar_mp") ent.SwitchToWeaponImmediate("portable_radar_mp");
                if (!ent.IsAlive || !ent.HasWeapon("portable_radar_mp")) return false;
                return true;
            });
                ent.SetPerk("specialty_marathon", true, true);
            OnNotify("last_alive", () =>
            {
                permKill();
                ent.TakeWeapon("portable_radar_mp");
                ent.GiveWeapon("iw5_usp45jugg_mp_tactical");
                AfterDelay(200, () => ent.SwitchToWeaponImmediate("iw5_usp45jugg_mp"));
                ent.SetWeaponAmmoClip("iw5_usp45jugg_mp_tactical", 0);
                ent.SetWeaponAmmoStock("iw5_usp45jugg_mp_tactical", 0);
                IPrintLnBold("^1One enemy left! Finish this.");
                ent.SetPerk("specialty_radarjuggernaut", true, true);
            });
        }
        public void checkMike(Entity ent)
        {
            if (GetTeamPlayersAlive("allies") == 2)
            {
                otherSet(ent);
            }
            else
            {
                ent.TakeAllWeapons();
                ent.SetPerk("specialty_marathon", true, true);
                ent.GiveWeapon("iw5_usp45jugg_mp_tactical");
                ent.SetWeaponAmmoClip("iw5_usp45jugg_mp_tactical", 0);
                ent.SetWeaponAmmoStock("iw5_usp45jugg_mp_tactical", 0);
                AfterDelay(200, () => ent.SwitchToWeapon("iw5_usp45jugg_mp_tactical"));
                OnNotify("last_alive", () =>
                         ent.SetPerk("specialty_radarjuggernaut", true, true));
            }
        }
        private void showAlive()
        {
            HudElem SeekersAlive = HudElem.CreateServerFontString(HudElem.Fonts.Default, 1.4f);
            SeekersAlive.Foreground = true;
            SeekersAlive.HideWhenInMenu = true;
            SeekersAlive.Archived = true;
            SeekersAlive.SetPoint("BOTTOM", "BOTTOM", 0, -20);
            int Seekers = -1;

            OnInterval(500, () =>
            {
                int sAlive = GetTeamPlayersAlive("axis");
                if (sAlive != Seekers)
                {
                    Seekers = sAlive;
                    SeekersAlive.SetText("People alive: " + Seekers);
                }
                return true;
            });
        }
        private void permKill()
        {
            info = HudElem.CreateServerFontString(HudElem.Fonts.HudBig, 1.4f);
            info.SetPoint("TOPCENTER", "TOPCENTER", 0, -15);
            info.HideWhenInMenu = true;
            info.SetText("^1Kill Mike!");
        }
    }
}
