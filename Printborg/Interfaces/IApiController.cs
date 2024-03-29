﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Printborg.Interfaces
{
    public interface IApiController
    {
        string BaseAddress { get; set; }
        int Timeout { get; set; }
        Task<string> POST_Batch(string payload);
        Task<string> DELETE_Batch(string id);
        Task<string> GET_Job(string id);
        Task<string> GET_Jobs();
        Task<List<string>> DELETE_Batches();
        Task<string> GET_Batch(string id);
        Task<string> GET_Batches();
        Task<double> GET_Progress(string id);
    }
}
