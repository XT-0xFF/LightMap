﻿using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Reflection;
using Verse;
using HugsLib;
using System.Collections.Generic;
using System.Collections;

namespace LightMap
{
	[StaticConstructorOnStartup]
	public static class HarmonyPatches
	{
		static HarmonyPatches()
		{
			var harmony = new Harmony("syrus.lightmap");

			var type = Type.GetType("ProgressRenderer.MapComponent_RenderManager, Progress-Renderer");
			if (type != null)
			{
				harmony.Patch(
					type.GetProperty("Rendering", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetSetMethod(true),
					postfix: new HarmonyMethod(typeof(ProgressRenderer_Patch), nameof(ProgressRenderer_Patch.ProgressRenderer_Rendering_Postfix)));
			}
		}
	}

	[HarmonyPatch(typeof(MapInterface), nameof(MapInterface.MapInterfaceUpdate))]
	internal static class MapInterface_MapInterfaceUpdate
	{
		[HarmonyPostfix]
		static void Postfix()
		{
			if (Find.CurrentMap == null 
				|| WorldRendererUtility.WorldRenderedNow)
				return;

			Main.Instance.UpdateMaps();
		}
	}

	[HarmonyPatch(typeof(MapInterface), nameof(MapInterface.Notify_SwitchedMap))]
	internal static class MapInterface_Notify_SwitchedMap
	{
		[HarmonyPostfix]
		static void Postfix()
		{
			Main.Instance.ResetMaps();
		}
	}


	[HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
	internal static class PlaySettings_DoPlaySettingsGlobalControls
	{
		[HarmonyPostfix]
		static void PostFix(WidgetRow row, bool worldView)
		{
			if (worldView || row == null)
				return;

			if (Main.Instance.LightMapIconButtonVisible && Resources.IconLight != null)
				row.ToggleableIcon(
					ref Main.Instance.ShowLightMap, 
					Resources.IconLight,
					"SY_LM.ShowLightMap".Translate(),
					SoundDefOf.Mouseover_ButtonToggle);

			if (Main.Instance.MovementSpeedMapIconButtonVisible && Resources.IconPath != null)
				row.ToggleableIcon(
					ref Main.Instance.ShowPathMap, 
					Resources.IconPath,
					"SY_LM.ShowMovementSpeedMap".Translate(),
					SoundDefOf.Mouseover_ButtonToggle);

			if (Main.Instance.BeautyMapIconButtonVisible && Resources.IconBeauty != null)
				row.ToggleableIcon(
					ref Main.Instance.ShowBeautyMap,
					Resources.IconBeauty,
					"SY_LM.ShowBeautyMap".Translate(),
					SoundDefOf.Mouseover_ButtonToggle);
		}
	}

	internal static class ProgressRenderer_Patch
	{
		internal static bool Light;
		internal static bool Path;
		internal static bool Beauty;

		internal static void ProgressRenderer_Rendering_Postfix(bool value)
		{
			var instance = Main.Instance;
			if (value)
			{
				Light = instance.ShowLightMap;
				Path = instance.ShowPathMap;
				Beauty = instance.ShowBeautyMap;

				if (!ProgressRenderer.PRModSettings.renderOverlays)
				{
					instance.ShowLightMap = false;
					instance.ShowPathMap = false;
					instance.ShowBeautyMap = false;
				}
			}
			else
			{
				instance.ShowLightMap = Light;
				instance.ShowPathMap = Path;
				instance.ShowBeautyMap = Beauty;
			}
		}
	}


	// Was testing to see if I could make pathCost accept negative values, 
	//  but sadly the pathCost also resets to 0+ when there's any object, including filth, on the map tile.
	//[HarmonyPatch(typeof(SnowUtility), nameof(SnowUtility.MovementTicksAddOn))]
	//internal static class SnowUtility_Patch
	//{
	//	[HarmonyPostfix]
	//	static void PostFix(SnowCategory category, ref int __result)
	//	{
	//		if (__result <= 0)
	//			__result = -100;
	//	}
	//}
}