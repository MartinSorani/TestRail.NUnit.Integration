using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using TestRail.Service.Base.Concrete;
using TestRail.Service.Base.Entities;

public class TestRailConfig
{
    private TestContext _fixtureContext;
    private TestRailApi _testRailApi;
    private string _suiteid, _projectid;
    private bool IgnoreAddResults = false;
    private int _projectIdInt, _suiteIdInt, _caseId;
    private List<Result> _resultsForCases;

    public string testRailUrl { get; }
    public string testRailPassword { get; }
    public string testRailUsername { get; }

    public TestRailConfig(string testRailUrl, string testRailUsername, string testRailPassword, TestContext testContext)
    {
        this.testRailUrl = testRailUrl;
        this.testRailUsername = testRailUsername;
        this.testRailPassword = testRailPassword;
        this._fixtureContext = testContext;
    }

    public void InitTestRail()
    {
        try
        {
            InitTestRailConfig(testRailUrl, testRailUsername, testRailPassword);
            ValidateSuiteIdAndProjectId();
            _resultsForCases = new List<Result>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            IgnoreAddResults = true;
        }
    }

    public void PublishRun()
    {
        if (!IgnoreAddResults)
        {
            if (_resultsForCases.Count > 0)
            {
                var runId = _testRailApi.CreateRun(new Run { project_id = _projectIdInt, suite_id = _suiteIdInt, include_all = false });
                if (runId > 0) _testRailApi.AddResultsForCases(runId, _resultsForCases);
            }
        }
    }

    public void AddResults()
    {
        if (!IgnoreAddResults)
        {
            var caseid = _fixtureContext.Test.Properties.Get("caseid")?.ToString();
            if (Int32.TryParse(caseid, out _caseId))
            {
                var result = new Result { case_id = _caseId, comment = _fixtureContext.Result.Message };
                var resultState = _fixtureContext.Result.Outcome;

                if (resultState == ResultState.Success) result.status_id = 1;
                else if (resultState == ResultState.Inconclusive) result.status_id = 4;
                else result.status_id = 5;

                _resultsForCases.Add(result);
            }
        }
    }

    private void InitTestRailConfig(string testRailUrl, string testRailUsername, string testRailPassword)
    {

        if (string.IsNullOrEmpty(testRailUrl)) throw new Exception("Invalid testrail url");
        if (string.IsNullOrEmpty(testRailUsername)) throw new Exception("Invalid testrail username");
        if (string.IsNullOrEmpty(testRailPassword)) throw new Exception("Invalid testrail password");

        _testRailApi = new TestRailApi(testRailUrl, testRailUsername, testRailPassword);
    }

    private void ValidateSuiteIdAndProjectId()
    {
        _suiteid = _fixtureContext.Test.Properties.Get("suiteid")?.ToString();
        _projectid = _fixtureContext.Test.Properties.Get("projectid")?.ToString();

        if (string.IsNullOrEmpty(_suiteid)) throw new Exception("Invalid suite id");
        if (string.IsNullOrEmpty(_projectid)) throw new Exception("Invalid project id");

        if (!Int32.TryParse(_projectid, out _projectIdInt)) throw new Exception("Project id not valid int");
        if (!Int32.TryParse(_suiteid, out _suiteIdInt)) throw new Exception("Suite id not valid int");

        // we should add validation for project and suite id
    }
}
