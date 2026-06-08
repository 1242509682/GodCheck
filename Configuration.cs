using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Terraria.ID;
using TShockAPI;

namespace Plugin;

internal class Configuration
{
    #region 配置项成员
    [JsonProperty("插件开关", Order = 0)]
    public bool Enabled { get; set; } = true;
    [JsonProperty("违规提醒", Order = 1)]
    public bool SendMess { get; set; } = true;
    [JsonProperty("检测冷却", Order = 2)]
    public int CoolDown { get; set; } = 1;
    [JsonProperty("违规次数", Order = 3)]
    public int PunCount { get; set; } = 3;
    [JsonProperty("自动踢出", Order = 4)]
    public bool Kick { get; set; } = false;
    [JsonProperty("惩罚名单", Order = 5)]
    public List<string> PunList { get; set; } = new();
    #endregion

    #region 预设参数方法
    public void SetDefault()
    {

    }
    #endregion

    #region 读取与创建配置文件方法
    public static readonly string CfgPath = Path.Combine(TShock.SavePath, $"{Plugin.PluginName}.json"); // 配置文件路径
    public void Write()
    {
        string json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(CfgPath, json);
    }
    public static Configuration Read()
    {
        if (!File.Exists(CfgPath))
        {
            var json = new Configuration();
            json.SetDefault();
            json.Write();
            return json;
        }
        else
        {
            try
            {
                string json = File.ReadAllText(CfgPath);
                var config = JsonConvert.DeserializeObject<Configuration>(json)!;
                return config;
            }
            catch (JsonReaderException ex)
            {
                string json = File.ReadAllText(CfgPath);
                string[] lines = json.Split('\n');
                int line = ex.LineNumber;
                int idx = Math.Max(0, Math.Min(line - 2, lines.Length - 1));
                string text = lines[idx].Trim();
                throw new Exception($"位置: 第 {line - 1} 行\n" +
                                    $"内容: {text ?? string.Empty}\n" +
                                    $"路径: {FormatPath(ex.Path ?? string.Empty)}", ex);
            }
        }
    }

    public static string FormatPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        // 使用正则表达式匹配 "[数字]"
        return Regex.Replace(path, @"\[(\d+)\]", match =>
        {
            int index = int.Parse(match.Groups[1].Value);
            return $":第{index + 1}项";
        });
    }
    #endregion
}