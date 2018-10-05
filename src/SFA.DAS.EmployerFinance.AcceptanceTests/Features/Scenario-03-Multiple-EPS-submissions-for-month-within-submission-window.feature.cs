﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.3.0.0
//      SpecFlow Generator Version:2.3.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SFA.DAS.EmployerFinance.AcceptanceTests.Features
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.3.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("HMRC-Scenario-03-Multiple-EPS-submissions-for-month-within-submission-window")]
    public partial class HMRC_Scenario_03_Multiple_EPS_Submissions_For_Month_Within_Submission_WindowFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Scenario-03-Multiple-EPS-submissions-for-month-within-submission-window.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "HMRC-Scenario-03-Multiple-EPS-submissions-for-month-within-submission-window", null, ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Month-03-submission-Multiple-submission-month")]
        public virtual void Month_03_Submission_Multiple_Submission_Month()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Month-03-submission-Multiple-submission-month", ((string[])(null)));
#line 3
this.ScenarioSetup(scenarioInfo);
#line 4
 testRunner.Given("We have an account with a paye scheme", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Id",
                        "LevyDueYtd",
                        "Payroll_Year",
                        "Payroll_Month",
                        "English_Fraction",
                        "SubmissionDate"});
            table1.AddRow(new string[] {
                        "999000301",
                        "10000",
                        "17-18",
                        "1",
                        "1",
                        "2017-05-15"});
            table1.AddRow(new string[] {
                        "999000302",
                        "20000",
                        "17-18",
                        "2",
                        "1",
                        "2017-06-15"});
            table1.AddRow(new string[] {
                        "999000303",
                        "35000",
                        "17-18",
                        "3",
                        "1",
                        "2017-07-15"});
            table1.AddRow(new string[] {
                        "999000304",
                        "25000",
                        "17-18",
                        "3",
                        "1",
                        "2017-07-16"});
            table1.AddRow(new string[] {
                        "999000305",
                        "30000",
                        "17-18",
                        "3",
                        "1",
                        "2017-07-17"});
#line 5
 testRunner.And("Hmrc return the following submissions for paye scheme", ((string)(null)), table1, "And ");
#line 12
 testRunner.When("we refresh levy data for 5 paye scheme", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 13
 testRunner.And("All the transaction lines in this scenario have had their transaction date update" +
                    "d to their created date", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 14
 testRunner.Then("we should see a level 1 screen with a balance of 33000 on the 07/2017", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 15
 testRunner.And("we should see a level 1 screen with a total levy of 11000 on the 07/2017", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 16
 testRunner.And("we should see a level 2 screen with a levy declared of 10000 on the 07/2017", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 17
 testRunner.And("we should see a level 2 screen with a top up of 1000 on the 07/2017", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Month-04-submission-Month-after-multiple-submissions")]
        public virtual void Month_04_Submission_Month_After_Multiple_Submissions()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Month-04-submission-Month-after-multiple-submissions", ((string[])(null)));
#line 19
this.ScenarioSetup(scenarioInfo);
#line 20
 testRunner.Given("We have an account with a paye scheme", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Id",
                        "LevyDueYtd",
                        "Payroll_Year",
                        "Payroll_Month",
                        "English_Fraction",
                        "SubmissionDate"});
            table2.AddRow(new string[] {
                        "999000306",
                        "10000",
                        "17-18",
                        "1",
                        "1",
                        "2017-05-15"});
            table2.AddRow(new string[] {
                        "999000307",
                        "20000",
                        "17-18",
                        "2",
                        "1",
                        "2017-06-15"});
            table2.AddRow(new string[] {
                        "999000308",
                        "35000",
                        "17-18",
                        "3",
                        "1",
                        "2017-07-15"});
            table2.AddRow(new string[] {
                        "999000309",
                        "25000",
                        "17-18",
                        "3",
                        "1",
                        "2017-07-16"});
            table2.AddRow(new string[] {
                        "999000310",
                        "30000",
                        "17-18",
                        "3",
                        "1",
                        "2017-07-17"});
            table2.AddRow(new string[] {
                        "999000311",
                        "40000",
                        "17-18",
                        "4",
                        "1",
                        "2017-08-17"});
#line 21
 testRunner.And("Hmrc return the following submissions for paye scheme", ((string)(null)), table2, "And ");
#line 29
 testRunner.When("we refresh levy data for account id 25 paye scheme", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 30
 testRunner.And("All the transaction lines in this scenario have had their transaction date update" +
                    "d to their created date", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 31
 testRunner.Then("we should see a level 1 screen with a balance of 44000 on the 08/2017", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 32
 testRunner.And("we should see a level 1 screen with a total levy of 11000 on the 08/2017", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 33
 testRunner.And("we should see a level 2 screen with a levy declared of 10000 on the 08/2017", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 34
 testRunner.And("we should see a level 2 screen with a top up of 1000 on the 08/2017", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
