using System;
using System.Collections.Generic;
using System.Text;

namespace Barotrauma.Networking
{
	partial class Client
	{
		public void SetClientCharacter(Character character)
		{
			GameMain.Server.SetClientCharacter(this, character);
		}

		public void Kick(string reason = "")
		{
			GameMain.Server.KickClient(this.Connection, reason);
		}

		public void Ban(string reason = "", float seconds = -1)
		{
			if (seconds == -1)
			{
				GameMain.Server.BanClient(this, reason, null);
			}
			else
			{
				GameMain.Server.BanClient(this, reason, TimeSpan.FromSeconds(seconds));
			}
		}

		public static void UnbanPlayer(string playerName)
		{
			GameMain.Server.UnbanPlayer(playerName);
		}

		public static void BanPlayer(string player, string reason, bool range = false, float seconds = -1)
		{
			if (seconds == -1)
			{
				GameMain.Server.BanPlayer(player, reason, null);
			}
			else
			{
				GameMain.Server.BanPlayer(player, reason, TimeSpan.FromSeconds(seconds));
			}
		}

		public bool CheckPermission(ClientPermissions permissions)
		{
			return this.Permissions.HasFlag(permissions);
		}
	}
}

namespace Barotrauma
{
	using Microsoft.Xna.Framework;
	using System.Reflection;

	partial class Item
	{
		public object CreateServerEventString(string component)
		{
			var comp = GetComponentString(component);

			if (comp == null)
				return null;

			MethodInfo method = typeof(Item).GetMethod(nameof(Item.CreateServerEvent), new Type[]{ Type.MakeGenericMethodParameter(0) });
			MethodInfo generic = method.MakeGenericMethod(comp.GetType());
			return generic.Invoke(this, new object[]{ comp });
		}

		public object CreateServerEventString(string component, object[] extraData)
		{
			var comp = GetComponentString(component);

			if (comp == null)
				return null;

			MethodInfo method = typeof(Item).GetMethod(nameof(Item.CreateServerEvent), new Type[]{ Type.MakeGenericMethodParameter(0), typeof(object[]) });
			MethodInfo generic = method.MakeGenericMethod(comp.GetType());
			return generic.Invoke(this, new object[]{comp, extraData });
		}
	}
}

namespace Barotrauma.Items.Components
{
	using Barotrauma.Networking;

	partial struct Signal
	{
		public static Signal Create(string value, int stepsTaken = 0, Character sender = null, Item source = null, float power = 0.0f, float strength = 1.0f)
		{
			return new Signal(value, stepsTaken, sender, source, power, strength);
		}
	}

	partial class Quality
	{
		public void SetValue(StatType statType, float value)
		{
			statValues[statType] = value;
		}
	}
}
