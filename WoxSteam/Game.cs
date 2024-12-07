using System;
using System.IO;
using Microsoft.Win32;
using NeXt.Vdf;
using WoxSteam.BinaryVdf;
using VdfInteger = NeXt.Vdf.VdfInteger;
using VdfString = NeXt.Vdf.VdfString;

namespace WoxSteam
{
	/// <summary>
	/// Represents single installed game and its details.
	/// </summary>
	public class Game
	{
		/// <summary>
		/// Contains informations stored in app manifest file.
		/// </summary>
		private readonly VdfTable manifest;
		/// <summary>
		/// Path where resources, like icons, should be stored when downloaded.
		/// </summary>
		private readonly Steam steam;

		/// <summary>
		/// Application id. Used to identify game on steam.
		/// </summary>
		public int Appid => ((VdfInteger) manifest.GetByName("appid")).Content;

		/// <summary>
		/// Game name.
		/// </summary>
		public string Name => ((VdfString) manifest.GetByName("name")).Content;

		/// <summary>
		/// Path to game icon. If the icon isn't already downloaded, it will be downloaded upon requesting this.
		/// </summary>
		public string Icon => LoadIcon();

		/// <summary>
		/// Game details loaded from appinfo.vdf
		/// </summary>
		public BinaryVdfItem Details { get; set; }

		public Game(string pathToManifest, Steam steam)
		{
			manifest = (VdfTable) VdfDeserializer.FromFile(pathToManifest).Deserialize();
			this.steam = steam;
		}

		/// <summary>
		/// Downloads game icon if present and not already downloaded.
		/// </summary>
		/// <returns>local path to cached game icon</returns>
		private string LoadIcon()
		{
			var icon = GetGameIconPathFromLibraryCache(Appid);

			if (icon != string.Empty)
			{
				return icon;
			}

			icon = GetGameIconPathRegistry(Appid);

            if (icon != string.Empty)
            {
                return icon;
            }

            return null;
		}

		private string GetGameIconPathFromLibraryCache(int gameId)
		{
			var iconPath = Path.Combine(this.steam.RootPath, "appcache", "librarycache", gameId + "_icon.jpg");

            if (File.Exists(iconPath))
			{
				return iconPath;
			}

			return String.Empty;
		}


        private static string GetGameIconPathRegistry(int gameId)
        {
            try
            {

                RegistryKey regKeyGame = Registry.LocalMachine.OpenSubKey($"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App {gameId}", false);

                if (regKeyGame != null)
                {
                    return regKeyGame.GetValue("DisplayIcon", string.Empty).ToString();
                }

                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
