using BorderlessGaming.Logic.Core;
using BorderlessGaming.Logic.Models;
using BorderlessGaming.Logic.System.Utilities;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using Newtonsoft.Json;
using BorderlessGaming.Logic.Properties;
using System.Net.Http;
using System.ServiceModel;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BorderlessGaming.Logic.System
{
  public static class Tools
  {
    public static void Setup()
    {
      if (!Directory.Exists(AppEnvironment.DataPath))
      {
        Directory.CreateDirectory(AppEnvironment.DataPath);
      }

      if (!Debugger.IsAttached)
      {
        ExceptionHandler.AddGlobalHandlers();
      }

      Config.Load();
      LanguageManager.Load();
    }

    public static Rectangle GetContainingRectangle(Rectangle a, Rectangle b)
    {
      var amin = new Point(a.X, a.Y);
      var amax = new Point(a.X + a.Width, a.Y + a.Height);
      var bmin = new Point(b.X, b.Y);
      var bmax = new Point(b.X + b.Width, b.Y + b.Height);
      var nmin = new Point(0, 0);
      var nmax = new Point(0, 0);

      nmin.X = amin.X < bmin.X ? amin.X : bmin.X;
      nmin.Y = amin.Y < bmin.Y ? amin.Y : bmin.Y;
      nmax.X = amax.X > bmax.X ? amax.X : bmax.X;
      nmax.Y = amax.Y > bmax.Y ? amax.Y : bmax.Y;

      return new Rectangle(nmin, new Size(nmax.X - nmin.X, nmax.Y - nmin.Y));
    }


    public static void GotoSite(string url)
    {
      try
      {
        Process.Start(url);
      }
      catch
      {
        // ignored
      }
    }

    public static void ExtractZipFile(string archiveFilenameIn, string password, string outFolder)
    {
      using (ZipArchive archive = ZipFile.Open(archiveFilenameIn, ZipArchiveMode.Read, Encoding.UTF8))
      {
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
          string destinationPath = Path.Combine(outFolder, entry.FullName);
          string destinationDirectory = Path.GetDirectoryName(destinationPath);

          if (!Directory.Exists(destinationDirectory))
          {
            Directory.CreateDirectory(destinationDirectory);
          }

          if (!string.IsNullOrEmpty(Path.GetFileName(destinationPath))) // Skip directories
          {
            entry.ExtractToFile(destinationPath, overwrite: true);
          }
        }
      }
    }

    public static async Task CheckForUpdates()
    {
      try
      {
        var releasePageUrl = "https://github.com/Venipa/Borderless-Gaming/releases/latest";
        const string apiUrl = "https://api.github.com/repos/Venipa/Borderless-Gaming/releases/latest";
        var http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        http.DefaultRequestHeaders.Add("Accept", "application/json");
        http.DefaultRequestHeaders.Add("Accept-Language", "en-US");
        http.DefaultRequestHeaders.Add("Referer", "https://github.com/");
        http.DefaultRequestHeaders.Add("Origin", "https://github.com/");
        var response = await http.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        JObject release = JsonConvert.DeserializeObject<JObject>(content);
        var latestVersion = new Version(release.Value<string>("tag_name").Replace("v", ""));
        var appVersion = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0, 0, 0);
        if (appVersion.CompareTo(latestVersion) < 0)
        {
          if (MessageBox.Show(Resources.InfoUpdateAvailable, Resources.InfoUpdatesHeader,
                MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
          {
            GotoSite(releasePageUrl);
          }
        }

      }
      catch (Exception)
      {
        MessageBox.Show(Resources.ErrorUpdates, Resources.ErrorHeader, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }
  }
}