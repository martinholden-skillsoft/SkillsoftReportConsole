# SkillsoftReportConsole
This simple console application illustrates calling the [UD_SubmitReport](https://documentation.skillsoft.com/en_us/skillport/8_0/olsa/index.htm#40247.htm) function to generate a [summary_catalog](https://documentation.skillsoft.com/en_us/skillport/8_0/ah/index.htm#45936.htm) report.

The report that is run is defined in the code in:

~~~~C#
//--------------------------------------------------------------------------------------------
//Define report settings here to ease changes
string scopingUserId = "admin";
reportFormat reportFormat = reportFormat.CSV;
string reportFilename = "report." + reportFormat.ToString();

string reportLanguage = "en_US"; //en_US only supported value
int reportRetainperiod = 3; //1 - 1 hour, 2 - 8 hours, 3 - 24 hours.

//For details of report names and parameters see https://documentation.skillsoft.com/en_us/skillport/8_0/ah/35465.htm
string reportName = "summary_catalog";
//Define report parameters
List<MapItem> paramList = new List<MapItem>();
paramList.Add(new MapItem() { key = "asset_category", value = "1,2,3,4,5,21" });
paramList.Add(new MapItem() { key = "display_options", value = "all" });


//--------------------------------------------------------------------------------------------
~~~~

You need to specify the OLSA details to run:

~~~~~
SkillsoftReportConsole.exe --help

Usage:
 SkillsoftReportConsole.exe endpoint customerid sharedsecret
  - endpoint     : The full URI to the endpoint. For example: https://evaltls01.skillwsa.com/olsa/services/Olsa (string, required)
  - customerid   : The Skillsoft customerid (string, required)
  - sharedsecret : The Skillsoft sharedsecret (string, required)

~~~~~

