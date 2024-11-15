# Brainfuse Tutoring Data Sync

## Installation
### Clone Repo
`git clone`
<a name="update-settings"></a>
### Update Settings

Create appsettings files from examples:
```powershell
Copy-Item -Path "appsettings.example.json" -Destination "appsettings.json"
Copy-Item -Path "appsettings.Development.example.json" -Destination "appsettings.Development.json"
```

Add encrypted Brainfuse API key (stored in Keeper) using [MCCC.Extensions.Configuration.Encryption](https://github.com/montgomery-county-community-college/MCCC.Extensions.Configuration.Encryption).


## Usage

* UpdateMode
  * Flag for whether or not the data should be written to the ODS database
* UseProxy
  * Flag for whether or not to use `127.0.0.1:8888` as a proxy (Fiddler)
* ConnectionStrings
  * Colleague
  * Colleague connection string to switch between Prod and a Test DB
  * CROA
  * ODS connection string to switch 
* McccTutorCourseMappingFile
  * Location of an Excel file to translate Brainfuse subjects to Colleague Courses
* ReportLookBackPeriod
  * Number of days to load
  * Ex. 1 - If this is set to 1 and run on 11/15/2024, it would pull data for 11/14/2024
  * Ex. 2 - If this is set to 5 and run on 11/15/2024, it owuld pull data for 11/10/2024 thru 11/14/2024
* BrainfuseApiSettings
  * ThreadCount
    * Number of concurrent calls. Brainfuse will refuse too many calls per second.
  * PageSize
    * Number of results per call on mass calls.
  * BrainfuseApiKey
    * See [Update Settings](#update-settings)