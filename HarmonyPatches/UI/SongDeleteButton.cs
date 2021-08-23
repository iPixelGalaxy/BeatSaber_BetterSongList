﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HarmonyLib;
using HMUI;
using Polyglot;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace BetterSongList.HarmonyPatches.UI {
	[HarmonyPatch(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.RefreshContent))]
	static class SongDeleteButton {
		static Button deleteButton = null;

		class DeleteConfirmHandler {
			public static Lazy<DeleteConfirmHandler> instance = new Lazy<DeleteConfirmHandler>(() => new DeleteConfirmHandler());

			[UIParams] readonly BSMLParserParams parserParams = null;

			string songPath = null;
			public void ConfirmDelete(IPreviewBeatmapLevel level, string path) {
				songPath = path;

				parserParams.EmitEvent("Show");
			}

			void Confirm() => SongCore.Loader.Instance.DeleteSong(songPath);
		}

		static void Postfix(StandardLevelDetailView __instance, Button ____practiceButton, IPreviewBeatmapLevel ____level) {
			if(deleteButton == null && ____practiceButton != null) {
				var newButton = GameObject.Instantiate(____practiceButton.gameObject, ____practiceButton.transform.parent);
				deleteButton = newButton.GetComponentInChildren<Button>();
				newButton.GetComponentsInChildren<LayoutElement>().Last().minWidth = 12;
				newButton.transform.SetAsFirstSibling();

				var t = newButton.GetComponentInChildren<CurvedTextMeshPro>();

				var iconG = new GameObject("Icon");
				iconG.transform.SetParent(t.transform.parent, false);
				iconG.transform.localScale = new Vector3(0.69f, 0.69f);
				var icon = iconG.AddComponent<ImageView>();

				icon.color = t.color;
				icon.material = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "UINoGlow");
				icon.SetImage("#DeleteIcon");

				GameObject.DestroyImmediate(t.gameObject);

				BSMLParser.instance.Parse(
					Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "BetterSongList.UI.BSML.SongDeleteConfirm.bsml"),
					__instance.transform.parent.gameObject,
					DeleteConfirmHandler.instance.Value
				);
			}

			if(deleteButton == null)
				return;

			deleteButton.interactable = false;
			deleteButton.onClick.RemoveAllListeners();
			bool isCustomLevel = ____level.levelID.StartsWith("custom_level_");

			if(!isCustomLevel)
				return;

			string songPath = null;

			var allMaps = SongCore.Loader.CustomLevels.AsEnumerable();
			if(Config.Instance.AllowWipDelete)
				allMaps = allMaps.Concat(SongCore.Loader.CustomWIPLevels);

			foreach(var x in allMaps) {
				if(x.Value.levelID == ____level.levelID) {
					songPath = x.Key;
					break;
				}
			}

			if(songPath == null)
				return;

			deleteButton.interactable = true;
			deleteButton.onClick.AddListener(() => DeleteConfirmHandler.instance.Value.ConfirmDelete(____level, songPath));
		}
	}
}
