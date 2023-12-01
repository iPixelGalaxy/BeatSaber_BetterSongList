﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BetterSongList.Util {
	public static class PlaylistsUtil {
		public static bool hasPlaylistLib = false;

		public static void Init() {
			hasPlaylistLib = IPA.Loader.PluginManager.GetPluginFromId("BeatSaberPlaylistsLib") != null;
		}

		public static Dictionary<string, IBeatmapLevelPack> packs = null;

		public static IBeatmapLevelPack GetPack(string packName) {
			if(packName == null)
				return null;

			if(packs == null) {
				packs =
					SongCore.Loader.BeatmapLevelsModelSO.allLoadedBeatmapLevelPackCollection.beatmapLevelPacks
					// There shouldnt be any duplicate name basegame playlists... But better be safe
					.GroupBy(x => x.shortPackName)
					.Select(x => x.First())
					.ToDictionary(x => x.shortPackName, x => x);
			}

			if(packs.TryGetValue(packName, out var p)) {
				return p;
			} else if(hasPlaylistLib) {
				IBeatmapLevelPack wrapper() {
					if(!SongCore.Loader.AreSongsLoaded)
						return null;
					foreach(var x in BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.GetAllPlaylists(true)) {
						if(x.packName == packName)
							return x;
					}
					return null;
				}
				return wrapper();
			}
			return null;
		}

		public static bool IsCollection(IAnnotatedBeatmapLevelCollection levelCollection) {
			return levelCollection is BeatSaberPlaylistsLib.Legacy.LegacyPlaylist || levelCollection is BeatSaberPlaylistsLib.Blist.BlistPlaylist;
		}

		public static IPreviewBeatmapLevel[] GetLevelsForLevelCollection(IAnnotatedBeatmapLevelCollection levelCollection) {
			if(levelCollection is BeatSaberPlaylistsLib.Legacy.LegacyPlaylist legacyPlaylist)
				return legacyPlaylist.BeatmapLevels;
			if(levelCollection is BeatSaberPlaylistsLib.Blist.BlistPlaylist blistPlaylist)
				return blistPlaylist.BeatmapLevels;
			return null;
		}
	}
}
