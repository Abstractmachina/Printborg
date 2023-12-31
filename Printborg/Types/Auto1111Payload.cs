﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Printborg.Types
{
    public class Auto1111Payload
    {
        private protected string _prompt = "a red apple";
        private protected string _negativePrompt = "bad, worse, low quality, strange, ugly";
        private protected string _samplerName = "Euler";
        private protected int _steps = 20;
        private protected int _cfgScale = 7;
        private protected int _batchSize = 1;
        private protected int _width = 512;
        private protected int _height = 512;
        private protected AlwaysOnScripts _alwaysOnScripts = null;

        [JsonProperty("prompt")]
        public string Prompt { get { return _prompt; } set { _prompt = value; } }
        [JsonProperty("steps")]
        public int Steps { get { return _steps; } set { _steps = value; } }
        [JsonProperty("cfg_scale")]
        public int CfgScale { get { return _cfgScale; } set { _cfgScale = value; } }
        [JsonProperty("negative_prompt")]
        public string NegativePrompt { get { return _negativePrompt; } set { _negativePrompt = value; } }
        [JsonProperty("sampler_name")]
        public string SamplerName { get { return _samplerName; } set { _samplerName = value; } }
        [JsonProperty("batch_size")]
        public int BatchSize { get { return _batchSize; } set { _batchSize = value; } }
        [JsonProperty("width")]
        public int Width { get { return _width; } set { _width = value; } }
        [JsonProperty("height")]
        public int Height { get { return _height; } set { _height = value; } }
        [JsonProperty("alwayson_scripts")]
        public AlwaysOnScripts AlwaysOnScripts { get { return _alwaysOnScripts; } set { _alwaysOnScripts = value; } }

        public Auto1111Payload()
        {

        }

        public Auto1111Payload(string prompt, string negativePrompt, int steps, int cfgScale, int width, int height, AlwaysOnScripts alwaysOnScripts)
        {
            _prompt = prompt;
            _negativePrompt = negativePrompt;
            _steps = steps;
            _cfgScale = cfgScale;
            _width = width;
            _height = height;
            _alwaysOnScripts = alwaysOnScripts;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    public class AlwaysOnScripts
    {
        [JsonProperty("controlnet")]
        private ControlNetSetting _controlnetSettings = null;

        public AlwaysOnScripts() { }
        public AlwaysOnScripts(ControlNetSetting controlNetSettings)
        {
            _controlnetSettings = controlNetSettings;
        }

		public override string ToString() {
            string output = "AlwaysOnScripts:\n{\n";
            if (_controlnetSettings != null) {
                output += "\tcontrolnet: {\n\t\targs: {\n";
                foreach (var a in _controlnetSettings.Arguments) {
                    output += String.Format("\t\t\tmodel: {0}\n", a.Model);
                    output += String.Format("\t\t\tmodule: {0}\n", a.Module);
                    output += String.Format("\t\t\tinputImage: {0}\n", a.inputImage);

                }
                output += "\t\t}\n\t}\n"; // close args, close controlnet

			}
            output += "}"; // close AlwaysOnScripts
            return output;

		}
	}

    /// <summary>
    /// Initiate control net settings with different configurations
    /// </summary>
    public static class ControlNetSettingsFactory
    {

        /// <summary>
        /// Initiate control net settings with different configurations
        /// </summary>
        /// <param name="model">controlnet model</param>
        /// <param name="module">controlnet module</param>
        /// <param name="inputImage">base64 encoded image</param>
        /// <returns></returns>
        public static ControlNetSetting Create(string model, string module, string inputImage)
        {
            var setting = new ControlNetSetting();
            setting.AddArgument(model, module, inputImage);
            return setting;
        }

    }


    public class ControlNetSetting
    {

        private List<ControlNetArgument> _arguments = new List<ControlNetArgument>();

        [JsonProperty("args")]
        internal List<ControlNetArgument> Arguments { get { return _arguments; } }

        public ControlNetSetting() { }

        public void AddArgument(string model, string module, string inputImage)
        {
            _arguments.Add(new ControlNetArgument(model, module, inputImage));
        }
        /// <summary>
        /// added for the sake of json conversion. always nested inside controlnet settings
        /// </summary>
        internal class ControlNetArgument
        {
            private string _model = "";
            private string _module = "";
            private string _inputImage = "";

            [JsonProperty("model")]
            public string Model { get { return _model; } set { _model = value; } }
            [JsonProperty("module")]
            public string Module { get { return _module; } }
            [JsonProperty("input_image")]
            public string inputImage { get { return _inputImage; } set { _inputImage = value; } }

            public ControlNetArgument() { }
            public ControlNetArgument(string model, string module, string inputImage)
            {
                _model = model;
                _module = module;
                _inputImage = inputImage;
            }
        }

    }

}
