﻿using GovITHub.Auth.Common.Infrastructure.Localization;
using Localization.SqlLocalizer.DbStringLocalizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace GovITHub.Auth.Identity.Controllers.Api
{
    [Authorize]
    [Route("api/LocalizationImportExport")]
    public class LocalizationImportExportController : ControllerBase
    {
        private IStringExtendedLocalizerFactory _stringExtendedLocalizerFactory;

        public LocalizationImportExportController(IStringExtendedLocalizerFactory stringExtendedLocalizerFactory)
        {
            _stringExtendedLocalizerFactory = stringExtendedLocalizerFactory;
        }

        [HttpGet]
        [Route("localizedData.csv")]
        [Produces("text/csv")]
        public IActionResult GetDataAsCsv()
        {
            return Ok(_stringExtendedLocalizerFactory.GetLocalizationData());
        }

        [Route("update")]
        [HttpPost]
        [ServiceFilter(typeof(ValidateMimeMultipartContentFilter))]
        public IActionResult ImportCsvFileForExistingData(CsvImportDescription csvImportDescription)
        {
            // TODO validate that data is a csv file.
            var contentTypes = new List<string>();

            if (ModelState.IsValid)
            {
                foreach (var file in csvImportDescription.File)
                {
                    if (file.Length > 0)
                    {
                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        contentTypes.Add(file.ContentType);

                        var inputStream = file.OpenReadStream();
                        var items = readStream(file.OpenReadStream());
                        _stringExtendedLocalizerFactory.UpdatetLocalizationData(items, csvImportDescription.Information);
                    }
                }
            }

            return RedirectToAction("Localization", "Home");
        }

        [Route("new")]
        [HttpPost]
        [ServiceFilter(typeof(ValidateMimeMultipartContentFilter))]
        public IActionResult ImportCsvFileForNewData(CsvImportDescription csvImportDescription)
        {
            // TODO validate that data is a csv file.
            var contentTypes = new List<string>();

            if (ModelState.IsValid)
            {
                foreach (var file in csvImportDescription.File)
                {
                    if (file.Length > 0)
                    {
                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        contentTypes.Add(file.ContentType);

                        var inputStream = file.OpenReadStream();
                        var items = readStream(file.OpenReadStream());
                        _stringExtendedLocalizerFactory.AddNewLocalizationData(items, csvImportDescription.Information);
                    }
                }
            }

            return RedirectToAction("Localization", "Home");
        }

        private List<LocalizationRecord> readStream(Stream stream)
        {
            bool skipFirstLine = true;
            string csvDelimiter = ";";

            List<LocalizationRecord> list = new List<LocalizationRecord>();
            var reader = new StreamReader(stream);


            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(csvDelimiter.ToCharArray());
                if (skipFirstLine)
                {
                    skipFirstLine = false;
                }
                else
                {
                    var itemTypeInGeneric = list.GetType().GetTypeInfo().GenericTypeArguments[0];
                    var item = new LocalizationRecord();
                    var properties = item.GetType().GetProperties();
                    for (int i = 0; i < values.Length; i++)
                    {
                        properties[i].SetValue(item, Convert.ChangeType(values[i], properties[i].PropertyType), null);
                    }

                    list.Add(item);
                }
            }
            return list;
        }
    }
}

