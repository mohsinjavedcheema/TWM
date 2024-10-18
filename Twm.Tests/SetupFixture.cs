using System;
using System.IO;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using NUnit.Framework;

namespace Twm.Tests
{
    [SetUpFixture]
    public class SetupFixture
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            try
            {
                var solutionDirectory = GetSolutionDirectory();
                Environment.CurrentDirectory = solutionDirectory;

                Session.Instance.InitTestEnvironment();
            }
            catch (Exception e)
            {
                Assert.Fail("Session test environment fail: " + e.Message);
            }

            try
            {
                SystemOptions.Instance.InitTestOptions();
                SystemOptions.Instance.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            }
            catch (Exception e)
            {
                Assert.Fail("SystemOptions test environment fail: " + e.Message);
            }

            BuildController.Instance.LoadCustomAssembly();
        }

        private string GetSolutionDirectory()
        {
            var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory);

            //while (currentDirectory != null && !currentDirectory.Name.Equals("Twm.Tests"))
            //{
            //    currentDirectory = currentDirectory.Parent;
            //}
            //return currentDirectory?.FullName ?? throw new Exception("Test project directory not found");

            while (currentDirectory != null && !Directory.Exists(Path.Combine(currentDirectory.FullName, ".git")))
            {
                currentDirectory = currentDirectory.Parent;
            }
            return currentDirectory?.FullName ?? throw new Exception("Solution directory not found");
        }
    }
}
