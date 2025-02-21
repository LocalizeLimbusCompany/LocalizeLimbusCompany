using System;
using System.Collections.Generic;
using System.IO;

namespace LimbusLocalize_Updater;

public static class SimpleConfigLoader
{
	// 配置存储结构：Dictionary<节名称, Dictionary<键, 值>>
	private static readonly Dictionary<string, Dictionary<string, string>> Sections = new();

	public static void Load(string filePath)
	{
		Sections.Clear();

		if (!File.Exists(filePath))
			return;

		var currentSection = "";

		foreach (var rawLine in File.ReadLines(filePath))
		{
			var line = rawLine.Trim();

			// 跳过空行和注释
			if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
				continue;

			// 处理节头
			if (line.StartsWith("[") && line.EndsWith("]"))
			{
				currentSection = line.Substring(1, line.Length - 2).Trim();
				Sections[currentSection] = new Dictionary<string, string>();
				continue;
			}

			// 分割键值对
			var separatorIndex = line.IndexOf('=');
			if (separatorIndex <= 0) continue;

			var key = line.Substring(0, separatorIndex).Trim();
			var value = line.Substring(separatorIndex + 1).Trim();

			// 处理无节情况
			if (string.IsNullOrEmpty(currentSection))
			{
				if (!Sections.ContainsKey(""))
					Sections[""] = new Dictionary<string, string>();
				Sections[""][key] = value;
			}
			else
			{
				Sections[currentSection][key] = value;
			}
		}
	}

	public static T Get<T>(string section, string key, T defaultValue = default)
	{
		if (!Sections.TryGetValue(section, out var sectionDict) ||
		    !sectionDict.TryGetValue(key, out var strValue))
			return defaultValue;

		try
		{
			return (T)Convert.ChangeType(strValue, typeof(T));
		}
		catch
		{
			return defaultValue;
		}
	}

	public static T GetEnum<T>(string section, string key, T defaultValue) where T : struct
	{
		var strValue = Get<string>(section, key);
		return Enum.TryParse(strValue, true, out T result) ? result : defaultValue;
	}
}