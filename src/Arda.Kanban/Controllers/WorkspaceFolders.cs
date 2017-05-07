﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using Arda.Kanban.Models.Repositories;
using Arda.Kanban.ViewModels;
using System.Linq;

namespace Arda.Kanban.Controllers
{
    // default = authenticated user

    [Route("v2/folders")]
    public class WorkspaceFoldersController : Controller
    {
        IWorkloadRepository _repository;
        
        public WorkspaceFoldersController(IWorkloadRepository repository)
        {
            _repository = repository;
        }
        
        [HttpGet("{folderId}")]
        public object Root(string folderId)
        {
            object ret = _repository.GetWorkloadsByUser(folderId);

            return ret;
        }        

    }
}
