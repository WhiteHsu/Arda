﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Arda.Kanban.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace IntegrationTests
{
    public class ArdaTestMgr
    {
        public static bool AllowCreateResultFile = true;
        private static IConfigurationRoot Configuration;

        static ArdaTestMgr()
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("local-secret.json", true)
                .AddUserSecrets("IntegrationTests.Arda.Kanban.Repositories-20170226092351")
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        static string GetConnectionString()
        {
            return Configuration["Storage_SqlServer_Kanban_ConnectionString"];
        }

        public static TransactionalKanbanContext GetTransactionContext()
        {
            string connectionString = GetConnectionString();

            return TransactionalKanbanContext.Create(connectionString);
        }

        public static string SerializeObject(object obj)
        {
            if (obj == null)
                return "(null)";

            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        public static string ReadFile(string filename)
        {
            StreamReader reader = null;

            try
            {
                reader = System.IO.File.OpenText(filename);

                string body = reader.ReadToEnd();

                return body.Replace("\r", "").Replace("\n","");
            }
            catch(FileNotFoundException)
            {
                throw new TestFileNotFoundException(filename);
            }
            finally
            {
                if(reader != null)
                    reader.Dispose();
            }
        }

        public static void WriteFile(string filename, string text)
        {
            using (var writer = System.IO.File.CreateText(filename))
            {
                writer.Write(text);
            }
        }

        public static void Validate<R>(ISupportSnapshot<R> testClass, 
                                            string testName, 
                                            Func<R[],KanbanContext,object> testFunction, 
                                            [System.Runtime.CompilerServices.CallerMemberName] string member = "")
        {
            using (var context = ArdaTestMgr.GetTransactionContext())
            {
                var before = testClass.GetSnapshot(context).ToArray();
                string beforeText = SerializeObject(before);

                var returnObject = testFunction(before, context);

                string returnObjectText = SerializeObject(returnObject);

                string result = $"BEFORE:\n=======\n{beforeText}\n\nCOMMAND: {testName}\n\nAFTER:\n======\n{returnObjectText}";
                
                CompareResults(result, member);
            }
        }

        public static void CompareResults(string result, string member)
        {
            string filename = $"files/{member}.json";

            try
            {
                string expected = ReadFile(filename);

                string cleanResult = result.Replace("\r", "").Replace("\n", "");
                string cleanExpected = expected.Replace("\r", "").Replace("\n", "");

                if (cleanResult != cleanExpected)
                {
                    throw new FailedTestException(member, null, result, expected);
                }
            }
            catch (IntegrationTestException)
            {
                if (AllowCreateResultFile)
                {
                    WriteFile(filename + ".result", result);
                }

                throw;
            }
        }
        
    }
}
