using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraBorder
{
    public delegate void UpdateStage(string stage, int process);

    public delegate void GenerateFail(string filename, string reason);

    public delegate void GenerateSuccess(string filename);
    public enum WatermarkLayout
    {
        Left,
        Right,
        Middle,
        None,
    }

    internal class CharacterConfig
    {
        public string? Format { get; set; }
        public int FontSize { get; set; } = 1;
        public bool FontBold { get; set; } = false;
        public Color FontColor { get; set; } = Color.Black;
        public string? FontFamily { get; set; } = "微软雅黑";

    }

    internal class WatermarkConfig
    {
        public WatermarkLayout Layout { get; set; }
        public List<CharacterConfig> LeftCharacterConfigs { get; set; } = new();
        public List<CharacterConfig> MiddleCharacterConfigs { get; set; } = new();
        public List<CharacterConfig> RightCharacterConfigs { get; set; } = new();
        public string? CopyrightInfo { get; set; } = "";
        public string? ArtistName { get; set; } = "";
        public bool OverwriteCopyright { get; set; } = false;
        public bool OverwriteArtistName { get; set; } = false;
        public string? OutputPath { get; set; } = "./output/";
        public bool WhiteFill { get; set; } = true;
    }
    internal class WatermarkGenerator
    {
        private List<string>? fileList;
        private WatermarkConfig? config;
        public WatermarkGenerator(List<string> fileList, WatermarkConfig config)
        {
            this.fileList = fileList;
            this.config = config;
        }

        private bool GenerateWatermark(string filename, ref string reason)
        {
            return false;
        }

        public void StartGenerateAsync(GenerateSuccess success, GenerateFail fail, UpdateStage update)
        {
            
        }
        

    }
}
