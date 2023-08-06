﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grasshopper.Kernel;
using GrasshopperAsyncComponent;
using Newtonsoft.Json;
using Printborg.API;
using Rhino.Geometry;

namespace PrintborgGH.GH_Components.ImageAi
{
    public class C_GetControlnetModules : GH_AsyncComponent
    {
        /// <summary>
        /// Initializes a new instance of the C_GetControlnetModules class.
        /// </summary>
        public C_GetControlnetModules()
          : base("Get Controlnet Modules", "Modules",
              "Get currently installed modules from server" ,
              Labels.PluginName, Labels.Category_AI)
        {
            BaseWorker = new fetchAuto1111ModuleWorker();
        }

        private class fetchAuto1111ModuleWorker : WorkerInstance
        {
            public List<string> _debug = new List<string>();


            public fetchAuto1111ModuleWorker() : base(null) { }

            public override async void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                // Checking for cancellation
                if (CancellationToken.IsCancellationRequested) { return; }

                if (!_processRequest) { return; }

                _debug.Clear();

                try
                {
                    if (_baseAddress == "") throw new Exception("base address is empty");

                    _debug.Add("baseAddress: " + _baseAddress);

                    _resultString = await Auto1111Controller.GetControlnetModules(_baseAddress, ReportProgress);

                    //using (HttpClient client = new HttpClient())
                    //{
                    //    client.BaseAddress = new Uri(_baseAddress);
                    //    client.Timeout = TimeSpan.FromSeconds(30d);
                    //    string uri = "/controlnet/module_list";


                    //    //var content = new StringContent(null, System.Text.Encoding.UTF8, "application/json");

                    //    var rawResponse = client.GetAsync(uri);

                    //    while (!rawResponse.IsCompleted)
                    //    {
                    //        ReportProgress("Fetching", 0.5d);
                    //        await Task.Delay(2000);
                    //    }


                    //    rawResponse.Result.EnsureSuccessStatusCode();

                    //    string responseContent = await rawResponse.Result.Content.ReadAsStringAsync();

                    //    _debug.Add(responseContent);
                    //    _resultString = responseContent;
                    //}
                }
                catch (Exception ex)
                {
                    _debug.Add(ex.ToString());
                }

                _processRequest = false;
                Done();

            }
            public override WorkerInstance Duplicate() => new fetchAuto1111ModuleWorker();



            private bool _processRequest = false;
            private string _baseAddress = "";
            private string _resultString = "";

            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                if (CancellationToken.IsCancellationRequested) return;

                //bool startProcess = false;
                if (!_processRequest)
                {
                    if (!DA.GetData("Send", ref _processRequest))
                    {
                        Parent.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input required");
                        return;
                    }

                    if (!DA.GetData("API Address", ref _baseAddress))
                    {
                        Parent.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "API base address required.");
                        return;
                    }
                }

                //if (startProcess)
                //{
                //    _processRequest = true;
                //    startProcess = false;
                //}
            }

            public override void SetData(IGH_DataAccess DA)
            {
                if (CancellationToken.IsCancellationRequested) return;


                if (_resultString == "") DA.SetData(0, $"Response not received.");
                else
                {
                    var modules = JsonConvert.DeserializeObject<ModuleList>(_resultString).Modules;
                    DA.SetDataList(0, modules);

                }

                DA.SetDataList(1, _debug);

            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Send", "S", "Send out API request", GH_ParamAccess.item);
            pManager.AddTextParameter("API Address", "A", "API address of hosted or local Auto1111 server", GH_ParamAccess.item, "");

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Models", "M", "Currently installed models", GH_ParamAccess.list);
            pManager.AddTextParameter("Debug", "D", "Debug Log", GH_ParamAccess.list);

        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendItem(menu, "Cancel", (s, e) =>
            {
                RequestCancellation();
            });
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("AE287DCB-200F-448C-9BE1-F75E2F203E7B"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;


        private class ModuleList
        {
            [JsonProperty("module_list")]
            public List<string> Modules { get; set; }

            public ModuleList() { }


        }
    }
}